using KMPSearch.Application.Common.Interfaces;
using KMPSearch.Application.Common.Models;
using KMPSearch.Application.DTOs;
using KMPSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

namespace KMPSearch.Infrastructure.Services;

public class SearchService : ISearchService
{
    private readonly ISearchDbContext _context;
    private readonly IHighlightService _highlightService;
    private readonly IFtsQueryBuilder _ftsQueryBuilder;

    public SearchService(
        ISearchDbContext context, 
        IHighlightService highlightService,
        IFtsQueryBuilder ftsQueryBuilder)
    {
        _context = context;
        _highlightService = highlightService;
        _ftsQueryBuilder = ftsQueryBuilder;
    }

    public async Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var hasSearchQuery = !string.IsNullOrWhiteSpace(request.Query);
            
            // Track search query for autocomplete
            if (hasSearchQuery)
            {
                await TrackSearchQueryAsync(request.Query, cancellationToken);
            }

            List<SearchResultItem> results;
            int totalCount;
            SearchFacets facets;

            if (hasSearchQuery)
            {
                // Use FTS for search queries
                var ftsResults = await ExecuteFtsSearchAsync(request, cancellationToken);
                totalCount = ftsResults.TotalCount;
                
                // Map to search results with highlighting and FTS rank
                results = ftsResults.Documents.Select(d => MapToSearchResult(d, request.Query, d.FtsRank)).ToList();
                
                // Calculate facets from FTS results
                facets = await CalculateFtsFacetsAsync(request.Query, request.Filters, cancellationToken);
            }
            else
            {
                // No search query - use regular LINQ with filters only
                var linqResults = await ExecuteFilterOnlySearchAsync(request, cancellationToken);
                totalCount = linqResults.TotalCount;
                
                // Map without FTS rank
                results = linqResults.Documents.Select(d => MapToSearchResult(d, null, null)).ToList();
                
                // Calculate facets from filtered query
                facets = await CalculateLinqFacetsAsync(request.Filters, cancellationToken);
            }

            var response = new SearchResponse
            {
                Results = results,
                Pagination = new PaginationInfo
                {
                    Page = request.Page,
                    PageSize = request.PageSize,
                    TotalResults = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
                },
                Facets = facets
            };

            return Result<SearchResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<SearchResponse>.Failure($"Search failed: {ex.Message}", 500);
        }
    }

    private async Task<(List<DocumentFtsResult> Documents, int TotalCount)> ExecuteFtsSearchAsync(
        SearchRequest request, 
        CancellationToken cancellationToken)
    {
        var ftsQuery = _ftsQueryBuilder.BuildContainsQuery(request.Query);
        
        if (string.IsNullOrEmpty(ftsQuery))
        {
            return (new List<DocumentFtsResult>(), 0);
        }

        var sortField = request.Sort?.Field?.ToLower() ?? "relevance";
        var sortOrder = request.Sort?.Order?.ToLower() ?? "desc";

        // Build the ORDER BY clause
        var orderByClause = sortField switch
        {
            "relevance" => "ft.RANK DESC",
            "date" => sortOrder == "asc" ? "d.CreatedAt ASC" : "d.CreatedAt DESC",
            "title" => sortOrder == "asc" ? "d.Title ASC" : "d.Title DESC",
            _ => "ft.RANK DESC"
        };

        // Build filter WHERE clauses
        var whereConditions = new List<string> { "d.IsDeleted = 0" };
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@ftsQuery", ftsQuery),
            new SqlParameter("@skip", (request.Page - 1) * request.PageSize),
            new SqlParameter("@pageSize", request.PageSize)
        };

        if (request.Filters != null)
        {
            if (request.Filters.Categories != null && request.Filters.Categories.Length > 0)
            {
                var categoryParams = string.Join(",", request.Filters.Categories.Select((_, i) => $"@category{i}"));
                whereConditions.Add($"d.Category IN ({categoryParams})");
                for (int i = 0; i < request.Filters.Categories.Length; i++)
                {
                    parameters.Add(new SqlParameter($"@category{i}", request.Filters.Categories[i]));
                }
            }

            if (request.Filters.DepartmentIds != null && request.Filters.DepartmentIds.Length > 0)
            {
                var deptParams = string.Join(",", request.Filters.DepartmentIds.Select((_, i) => $"@dept{i}"));
                whereConditions.Add($"d.DepartmentId IN ({deptParams})");
                for (int i = 0; i < request.Filters.DepartmentIds.Length; i++)
                {
                    parameters.Add(new SqlParameter($"@dept{i}", request.Filters.DepartmentIds[i]));
                }
            }

            if (request.Filters.DateRange != null)
            {
                if (request.Filters.DateRange.From.HasValue)
                {
                    whereConditions.Add("d.CreatedAt >= @fromDate");
                    parameters.Add(new SqlParameter("@fromDate", request.Filters.DateRange.From.Value));
                }

                if (request.Filters.DateRange.To.HasValue)
                {
                    whereConditions.Add("d.CreatedAt <= @toDate");
                    parameters.Add(new SqlParameter("@toDate", request.Filters.DateRange.To.Value));
                }
            }

            if (request.Filters.Tags != null && request.Filters.Tags.Length > 0)
            {
                for (int i = 0; i < request.Filters.Tags.Length; i++)
                {
                    whereConditions.Add($"d.Tags LIKE @tag{i}");
                    parameters.Add(new SqlParameter($"@tag{i}", $"%{request.Filters.Tags[i]}%"));
                }
            }
        }

        var whereSql = string.Join(" AND ", whereConditions);

        // Get total count
        var countSql = $@"
            SELECT COUNT(*)
            FROM Documents d
            INNER JOIN CONTAINSTABLE(Documents, (Title, Description, Tags), @ftsQuery) ft ON d.Id = ft.[KEY]
            WHERE {whereSql}";

        var dbContext = (DbContext)_context;
        var connection = dbContext.Database.GetDbConnection();
        
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        int totalCount;
        await using (var countCommand = connection.CreateCommand())
        {
            countCommand.CommandText = countSql;
            foreach (var param in parameters.Where(p => p.ParameterName == "@ftsQuery" || 
                                                        p.ParameterName.StartsWith("@category") ||
                                                        p.ParameterName.StartsWith("@dept") ||
                                                        p.ParameterName.StartsWith("@fromDate") ||
                                                        p.ParameterName.StartsWith("@toDate") ||
                                                        p.ParameterName.StartsWith("@tag")))
            {
                var cmdParam = countCommand.CreateParameter();
                cmdParam.ParameterName = param.ParameterName;
                cmdParam.Value = param.Value;
                countCommand.Parameters.Add(cmdParam);
            }
            
            totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));
        }

        // Get paginated results
        var sql = $@"
            SELECT 
                d.Id, d.Title, d.Description, d.Category, d.Tags, d.FilePath, d.FileSize, d.MimeType,
                d.DepartmentId, d.CreatedBy, d.IsDeleted, d.CreatedAt, d.UpdatedAt,
                ft.RANK as FtsRank,
                dep.Name as DepartmentName
            FROM Documents d
            INNER JOIN CONTAINSTABLE(Documents, (Title, Description, Tags), @ftsQuery) ft ON d.Id = ft.[KEY]
            LEFT JOIN Departments dep ON d.DepartmentId = dep.Id
            WHERE {whereSql}
            ORDER BY {orderByClause}
            OFFSET @skip ROWS
            FETCH NEXT @pageSize ROWS ONLY";

        var documents = new List<DocumentFtsResult>();
        
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = sql;
            foreach (var param in parameters)
            {
                var cmdParam = command.CreateParameter();
                cmdParam.ParameterName = param.ParameterName;
                cmdParam.Value = param.Value;
                command.Parameters.Add(cmdParam);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                documents.Add(new DocumentFtsResult
                {
                    Id = reader.GetGuid(reader.GetOrdinal("Id")),
                    Title = reader.GetString(reader.GetOrdinal("Title")),
                    Description = reader.IsDBNull(reader.GetOrdinal("Description")) 
                        ? null 
                        : reader.GetString(reader.GetOrdinal("Description")),
                    Category = reader.GetString(reader.GetOrdinal("Category")),
                    Tags = reader.GetString(reader.GetOrdinal("Tags")),
                    FilePath = reader.GetString(reader.GetOrdinal("FilePath")),
                    FileSize = reader.GetInt64(reader.GetOrdinal("FileSize")),
                    MimeType = reader.GetString(reader.GetOrdinal("MimeType")),
                    DepartmentId = reader.GetGuid(reader.GetOrdinal("DepartmentId")),
                    CreatedBy = reader.GetGuid(reader.GetOrdinal("CreatedBy")),
                    IsDeleted = reader.GetBoolean(reader.GetOrdinal("IsDeleted")),
                    CreatedAt = reader.GetDateTime(reader.GetOrdinal("CreatedAt")),
                    UpdatedAt = reader.IsDBNull(reader.GetOrdinal("UpdatedAt")) 
                        ? null 
                        : reader.GetDateTime(reader.GetOrdinal("UpdatedAt")),
                    FtsRank = reader.GetInt32(reader.GetOrdinal("FtsRank")),
                    DepartmentName = reader.IsDBNull(reader.GetOrdinal("DepartmentName")) 
                        ? string.Empty 
                        : reader.GetString(reader.GetOrdinal("DepartmentName"))
                });
            }
        }

        return (documents, totalCount);
    }

    private async Task<(List<Document> Documents, int TotalCount)> ExecuteFilterOnlySearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken)
    {
        var query = _context.Documents
            .Include(d => d.Department)
            .Where(d => !d.IsDeleted);

        // Apply filters
        if (request.Filters != null)
        {
            if (request.Filters.Categories != null && request.Filters.Categories.Length > 0)
            {
                query = query.Where(d => request.Filters.Categories.Contains(d.Category));
            }

            if (request.Filters.DepartmentIds != null && request.Filters.DepartmentIds.Length > 0)
            {
                query = query.Where(d => request.Filters.DepartmentIds.Contains(d.DepartmentId));
            }

            if (request.Filters.DateRange != null)
            {
                if (request.Filters.DateRange.From.HasValue)
                {
                    query = query.Where(d => d.CreatedAt >= request.Filters.DateRange.From.Value);
                }

                if (request.Filters.DateRange.To.HasValue)
                {
                    query = query.Where(d => d.CreatedAt <= request.Filters.DateRange.To.Value);
                }
            }

            if (request.Filters.Tags != null && request.Filters.Tags.Length > 0)
            {
                foreach (var tag in request.Filters.Tags)
                {
                    var tagToSearch = tag;
                    query = query.Where(d => d.Tags.Contains(tagToSearch));
                }
            }
        }

        var totalCount = await query.CountAsync(cancellationToken);

        // Apply sorting
        var sortField = request.Sort?.Field?.ToLower() ?? "date";
        var sortOrder = request.Sort?.Order?.ToLower() ?? "desc";

        if (sortField == "date")
        {
            query = sortOrder == "asc"
                ? query.OrderBy(d => d.CreatedAt)
                : query.OrderByDescending(d => d.CreatedAt);
        }
        else if (sortField == "title")
        {
            query = sortOrder == "asc"
                ? query.OrderBy(d => d.Title)
                : query.OrderByDescending(d => d.Title);
        }
        else
        {
            query = query.OrderByDescending(d => d.CreatedAt);
        }

        var skip = (request.Page - 1) * request.PageSize;
        var documents = await query
            .Skip(skip)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return (documents, totalCount);
    }

    public async Task<Result<SuggestionResponse>> GetSuggestionsAsync(string query, int limit = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Result<SuggestionResponse>.Success(new SuggestionResponse());
            }

            var suggestions = new List<SuggestionItem>();
            var ftsQuery = _ftsQueryBuilder.BuildContainsQuery(query);

            if (!string.IsNullOrEmpty(ftsQuery))
            {
                // Get suggestions from search history using FTS
                var historySql = $@"
                    SELECT TOP({limit}) sq.QueryText, sq.SearchCount, ft.RANK
                    FROM SearchQueries sq
                    INNER JOIN CONTAINSTABLE(SearchQueries, QueryText, @ftsQuery) ft ON sq.Id = ft.[KEY]
                    ORDER BY ft.RANK DESC, sq.SearchCount DESC";

                var historySuggestions = await ExecuteFtsSuggestionsQueryAsync(
                    historySql, ftsQuery, "query", limit, cancellationToken);
                suggestions.AddRange(historySuggestions);

                // Get suggestions from document titles using FTS
                var titleSql = $@"
                    SELECT TOP({limit}) d.Title, COUNT(*) as Cnt, MAX(ft.RANK) as RANK
                    FROM Documents d
                    INNER JOIN CONTAINSTABLE(Documents, Title, @ftsQuery) ft ON d.Id = ft.[KEY]
                    WHERE d.IsDeleted = 0
                    GROUP BY d.Title
                    ORDER BY MAX(ft.RANK) DESC, COUNT(*) DESC";

                var titleSuggestions = await ExecuteFtsSuggestionsQueryAsync(
                    titleSql, ftsQuery, "title", limit, cancellationToken, useAggregateCount: true);
                suggestions.AddRange(titleSuggestions);

                // Get suggestions from tags using FTS
                var tagSql = $@"
                    SELECT TOP({limit}) d.Tags, COUNT(*) as Cnt, MAX(ft.RANK) as RANK
                    FROM Documents d
                    INNER JOIN CONTAINSTABLE(Documents, Tags, @ftsQuery) ft ON d.Id = ft.[KEY]
                    WHERE d.IsDeleted = 0
                    GROUP BY d.Tags
                    ORDER BY MAX(ft.RANK) DESC, COUNT(*) DESC";

                var tagResults = await ExecuteFtsSuggestionsQueryAsync(
                    tagSql, ftsQuery, "tag-raw", limit, cancellationToken, useAggregateCount: true);
                
                // Extract individual tags from CSV strings
                foreach (var tagResult in tagResults)
                {
                    var individualTags = tagResult.Text.Split(',', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var tag in individualTags)
                    {
                        var trimmedTag = tag.Trim();
                        if (trimmedTag.Contains(query, StringComparison.OrdinalIgnoreCase))
                        {
                            var existing = suggestions.FirstOrDefault(s => s.Type == "tag" && s.Text.Equals(trimmedTag, StringComparison.OrdinalIgnoreCase));
                            if (existing == null)
                            {
                                suggestions.Add(new SuggestionItem
                                {
                                    Text = trimmedTag,
                                    Type = "tag",
                                    Count = tagResult.Count
                                });
                            }
                            else
                            {
                                existing.Count += tagResult.Count;
                            }
                        }
                    }
                }
            }

            // Return top suggestions by count
            var topSuggestions = suggestions
                .OrderByDescending(s => s.Count)
                .Take(limit)
                .ToList();

            var response = new SuggestionResponse { Suggestions = topSuggestions };
            return Result<SuggestionResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<SuggestionResponse>.Failure($"Failed to get suggestions: {ex.Message}", 500);
        }
    }

    private async Task<List<SuggestionItem>> ExecuteFtsSuggestionsQueryAsync(
        string sql,
        string ftsQuery,
        string suggestionType,
        int limit,
        CancellationToken cancellationToken,
        bool useAggregateCount = false)
    {
        var suggestions = new List<SuggestionItem>();
        var dbContext = (DbContext)_context;
        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        
        var param = command.CreateParameter();
        param.ParameterName = "@ftsQuery";
        param.Value = ftsQuery;
        command.Parameters.Add(param);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var text = reader.GetString(0);
            var count = useAggregateCount 
                ? reader.GetInt32(1) 
                : (suggestionType == "query" ? reader.GetInt32(1) : 1);

            suggestions.Add(new SuggestionItem
            {
                Text = text,
                Type = suggestionType,
                Count = count
            });
        }

        return suggestions;
    }

    public async Task<Result<FacetsResponse>> GetFacetsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var documents = _context.Documents.Where(d => !d.IsDeleted);

            // Category facets
            var categories = await documents
                .GroupBy(d => d.Category)
                .Select(g => new FacetItem
                {
                    Value = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(f => f.Count)
                .ToListAsync(cancellationToken);

            // Department facets
            var departments = await documents
                .Include(d => d.Department)
                .GroupBy(d => new { d.DepartmentId, d.Department.Name })
                .Select(g => new DepartmentFacet
                {
                    Id = g.Key.DepartmentId,
                    Name = g.Key.Name,
                    Count = g.Count()
                })
                .OrderByDescending(f => f.Count)
                .ToListAsync(cancellationToken);

            // Tag facets - flatten tags array and group
            var allDocuments = await documents.ToListAsync(cancellationToken);
            var tags = allDocuments
                .SelectMany(d => d.Tags)
                .GroupBy(tag => tag)
                .Select(g => new FacetItem
                {
                    Value = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(f => f.Count)
                .ToList();

            // Date range facet
            var dateRange = new DateRangeFacet
            {
                Min = await documents.MinAsync(d => (DateTime?)d.CreatedAt, cancellationToken),
                Max = await documents.MaxAsync(d => (DateTime?)d.CreatedAt, cancellationToken)
            };

            var response = new FacetsResponse
            {
                Categories = categories,
                Departments = departments,
                Tags = tags,
                DateRange = dateRange
            };

            return Result<FacetsResponse>.Success(response);
        }
        catch (Exception ex)
        {
            return Result<FacetsResponse>.Failure($"Failed to get facets: {ex.Message}", 500);
        }
    }

    private SearchResultItem MapToSearchResult(Document document, string? query, int? ftsRank = null)
    {
        var hasQuery = !string.IsNullOrWhiteSpace(query);
        double score;

        if (ftsRank.HasValue)
        {
            // Normalize FTS RANK to 0-1 range (typical RANK values are 0-1000)
            score = Math.Min(ftsRank.Value / 1000.0, 1.0);
        }
        else
        {
            // No FTS rank - set score to 0
            score = 0.0;
        }

        return new SearchResultItem
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            Category = document.Category,
            Tags = document.Tags,
            DepartmentName = document.Department.Name,
            CreatedAt = document.CreatedAt,
            Highlights = hasQuery ? new SearchHighlights
            {
                Title = _highlightService.HighlightMatches(document.Title, query!),
                Description = document.Description != null
                    ? _highlightService.HighlightMatches(document.Description, query!)
                    : null
            } : null,
            Score = score
        };
    }

    private SearchResultItem MapToSearchResult(DocumentFtsResult document, string? query, int? ftsRank)
    {
        var hasQuery = !string.IsNullOrWhiteSpace(query);
        double score;

        if (ftsRank.HasValue)
        {
            // Normalize FTS RANK to 0-1 range (typical RANK values are 0-1000)
            score = Math.Min(ftsRank.Value / 1000.0, 1.0);
        }
        else
        {
            score = 0.0;
        }

        // Convert Tags from CSV string to array
        var tagsArray = document.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(t => t.Trim())
            .ToArray();

        return new SearchResultItem
        {
            Id = document.Id,
            Title = document.Title,
            Description = document.Description,
            Category = document.Category,
            Tags = tagsArray,
            DepartmentName = document.DepartmentName,
            CreatedAt = document.CreatedAt,
            Highlights = hasQuery ? new SearchHighlights
            {
                Title = _highlightService.HighlightMatches(document.Title, query!),
                Description = document.Description != null
                    ? _highlightService.HighlightMatches(document.Description, query!)
                    : null
            } : null,
            Score = score
        };
    }

    private async Task<SearchFacets> CalculateFtsFacetsAsync(
        string query,
        SearchFilters? filters,
        CancellationToken cancellationToken)
    {
        var ftsQuery = _ftsQueryBuilder.BuildContainsQuery(query);
        
        if (string.IsNullOrEmpty(ftsQuery))
        {
            return new SearchFacets { Categories = new List<FacetItem>() };
        }

        // Build filter WHERE clauses
        var whereConditions = new List<string> { "d.IsDeleted = 0" };
        var parameters = new List<SqlParameter>
        {
            new SqlParameter("@ftsQuery", ftsQuery)
        };

        if (filters != null)
        {
            if (filters.Categories != null && filters.Categories.Length > 0)
            {
                var categoryParams = string.Join(",", filters.Categories.Select((_, i) => $"@category{i}"));
                whereConditions.Add($"d.Category IN ({categoryParams})");
                for (int i = 0; i < filters.Categories.Length; i++)
                {
                    parameters.Add(new SqlParameter($"@category{i}", filters.Categories[i]));
                }
            }

            if (filters.DepartmentIds != null && filters.DepartmentIds.Length > 0)
            {
                var deptParams = string.Join(",", filters.DepartmentIds.Select((_, i) => $"@dept{i}"));
                whereConditions.Add($"d.DepartmentId IN ({deptParams})");
                for (int i = 0; i < filters.DepartmentIds.Length; i++)
                {
                    parameters.Add(new SqlParameter($"@dept{i}", filters.DepartmentIds[i]));
                }
            }

            if (filters.DateRange != null)
            {
                if (filters.DateRange.From.HasValue)
                {
                    whereConditions.Add("d.CreatedAt >= @fromDate");
                    parameters.Add(new SqlParameter("@fromDate", filters.DateRange.From.Value));
                }

                if (filters.DateRange.To.HasValue)
                {
                    whereConditions.Add("d.CreatedAt <= @toDate");
                    parameters.Add(new SqlParameter("@toDate", filters.DateRange.To.Value));
                }
            }

            if (filters.Tags != null && filters.Tags.Length > 0)
            {
                for (int i = 0; i < filters.Tags.Length; i++)
                {
                    whereConditions.Add($"d.Tags LIKE @tag{i}");
                    parameters.Add(new SqlParameter($"@tag{i}", $"%{filters.Tags[i]}%"));
                }
            }
        }

        var whereSql = string.Join(" AND ", whereConditions);

        var sql = $@"
            SELECT TOP(10) d.Category, COUNT(*) as Count
            FROM Documents d
            INNER JOIN CONTAINSTABLE(Documents, (Title, Description, Tags), @ftsQuery) ft ON d.Id = ft.[KEY]
            WHERE {whereSql}
            GROUP BY d.Category
            ORDER BY COUNT(*) DESC";

        var categories = new List<FacetItem>();
        var dbContext = (DbContext)_context;
        var connection = dbContext.Database.GetDbConnection();

        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using (var command = connection.CreateCommand())
        {
            command.CommandText = sql;
            foreach (var param in parameters)
            {
                var cmdParam = command.CreateParameter();
                cmdParam.ParameterName = param.ParameterName;
                cmdParam.Value = param.Value;
                command.Parameters.Add(cmdParam);
            }

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                categories.Add(new FacetItem
                {
                    Value = reader.GetString(0),
                    Count = reader.GetInt32(1)
                });
            }
        }

        return new SearchFacets { Categories = categories };
    }

    private async Task<SearchFacets> CalculateLinqFacetsAsync(
        SearchFilters? filters,
        CancellationToken cancellationToken)
    {
        var query = _context.Documents.Where(d => !d.IsDeleted);

        if (filters != null)
        {
            if (filters.Categories != null && filters.Categories.Length > 0)
            {
                query = query.Where(d => filters.Categories.Contains(d.Category));
            }

            if (filters.DepartmentIds != null && filters.DepartmentIds.Length > 0)
            {
                query = query.Where(d => filters.DepartmentIds.Contains(d.DepartmentId));
            }

            if (filters.DateRange != null)
            {
                if (filters.DateRange.From.HasValue)
                {
                    query = query.Where(d => d.CreatedAt >= filters.DateRange.From.Value);
                }

                if (filters.DateRange.To.HasValue)
                {
                    query = query.Where(d => d.CreatedAt <= filters.DateRange.To.Value);
                }
            }

            if (filters.Tags != null && filters.Tags.Length > 0)
            {
                foreach (var tag in filters.Tags)
                {
                    var tagToSearch = tag;
                    query = query.Where(d => d.Tags.Contains(tagToSearch));
                }
            }
        }

        var categories = await query
            .GroupBy(d => d.Category)
            .Select(g => new FacetItem
            {
                Value = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(f => f.Count)
            .Take(10)
            .ToListAsync(cancellationToken);

        return new SearchFacets { Categories = categories };
    }

    private async Task TrackSearchQueryAsync(string queryText, CancellationToken cancellationToken)
    {
        try
        {
            var existingQuery = await _context.SearchQueries
                .FirstOrDefaultAsync(sq => sq.QueryText == queryText, cancellationToken);

            if (existingQuery != null)
            {
                existingQuery.SearchCount++;
                existingQuery.LastSearchedAt = DateTime.UtcNow;
            }
            else
            {
                var newQuery = new SearchQuery
                {
                    QueryText = queryText,
                    SearchCount = 1,
                    LastSearchedAt = DateTime.UtcNow
                };
                _context.SearchQueries.Add(newQuery);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            // Silently fail search tracking - not critical
        }
    }
}
