using KMPSearch.Application.Common.Models;
using KMPSearch.Application.DTOs;

namespace KMPSearch.Application.Common.Interfaces;

public interface ISearchService
{
    Task<Result<SearchResponse>> SearchAsync(SearchRequest request, CancellationToken cancellationToken = default);
    Task<Result<SuggestionResponse>> GetSuggestionsAsync(string query, int limit = 10, CancellationToken cancellationToken = default);
    Task<Result<FacetsResponse>> GetFacetsAsync(CancellationToken cancellationToken = default);
}
