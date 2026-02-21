namespace KMPSearch.Infrastructure.Services;

/// <summary>
/// Internal model for capturing FTS query results with RANK scores.
/// Used to retrieve both Document data and FTS relevance score in a single query.
/// </summary>
internal class DocumentFtsResult
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Tags { get; set; } = string.Empty;  // CSV string from database
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid CreatedBy { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // FTS-specific fields
    public int FtsRank { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
}
