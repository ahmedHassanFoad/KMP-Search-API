using KMPSearch.Application.Common.Interfaces;
using KMPSearch.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace KMPSearch.API.Controllers;

/// <summary>
/// Search endpoints for full-text document search
/// </summary>
public class SearchController : BaseController
{
    private readonly ISearchService _searchService;

    public SearchController(ISearchService searchService)
    {
        _searchService = searchService;
    }

    /// <summary>
    /// Execute full-text search with filters and sorting
    /// </summary>
    /// <param name="request">Search request with query, filters, sorting, and pagination</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results with pagination and facets</returns>
    [HttpPost]
    [ProducesResponseType(typeof(SearchResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> Search(
        [FromBody] SearchRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _searchService.SearchAsync(request, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });

        // Add pagination headers
        Response.Headers.Append("X-Total-Count", result.Data!.Pagination.TotalResults.ToString());
        Response.Headers.Append("X-Page", result.Data.Pagination.Page.ToString());
        Response.Headers.Append("X-Page-Size", result.Data.Pagination.PageSize.ToString());
        Response.Headers.Append("X-Total-Pages", result.Data.Pagination.TotalPages.ToString());

        return Ok(result.Data);
    }

    /// <summary>
    /// Get autocomplete suggestions based on query prefix
    /// </summary>
    /// <param name="q">Query prefix</param>
    /// <param name="limit">Maximum number of suggestions (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of suggestions from search history, titles, and tags</returns>
    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(SuggestionResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetSuggestions(
        [FromQuery] string q,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { error = "Query parameter 'q' is required" });
        }

        if (limit < 1 || limit > 100)
        {
            return BadRequest(new { error = "Limit must be between 1 and 100" });
        }

        var result = await _searchService.GetSuggestionsAsync(q, limit, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });

        return Ok(result.Data);
    }

    /// <summary>
    /// Get available filter facets for search refinement
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Facets with counts for categories, departments, tags, and date range</returns>
    [HttpGet("facets")]
    [ProducesResponseType(typeof(FacetsResponse), 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetFacets(CancellationToken cancellationToken)
    {
        var result = await _searchService.GetFacetsAsync(cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.StatusCode, new { error = result.ErrorMessage });

        return Ok(result.Data);
    }
}
