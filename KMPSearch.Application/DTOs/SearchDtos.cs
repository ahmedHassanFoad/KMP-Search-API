namespace KMPSearch.Application.DTOs;

public class SearchRequest
{
    public string Query { get; set; } = string.Empty;
    public SearchFilters? Filters { get; set; }
    public SortOptions? Sort { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchFilters
{
    public string[]? Categories { get; set; }
    public Guid[]? DepartmentIds { get; set; }
    public DateRangeFilter? DateRange { get; set; }
    public string[]? Tags { get; set; }
}

public class DateRangeFilter
{
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
}

public class SortOptions
{
    public string Field { get; set; } = "relevance";
    public string Order { get; set; } = "desc";
}

public class SearchResponse
{
    public List<SearchResultItem> Results { get; set; } = new();
    public PaginationInfo Pagination { get; set; } = new();
    public SearchFacets? Facets { get; set; }
}

public class SearchResultItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string DepartmentName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public SearchHighlights? Highlights { get; set; }
    public double Score { get; set; }
}

public class SearchHighlights
{
    public string? Title { get; set; }
    public string? Description { get; set; }
}

public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalResults { get; set; }
    public int TotalPages { get; set; }
}

public class SearchFacets
{
    public List<FacetItem> Categories { get; set; } = new();
}
