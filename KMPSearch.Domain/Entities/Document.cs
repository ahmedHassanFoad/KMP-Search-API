using KMPSearch.Domain.Common;

namespace KMPSearch.Domain.Entities;

public class Document : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Category { get; set; } = string.Empty;
    public string[] Tags { get; set; } = Array.Empty<string>();
    public string FilePath { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string MimeType { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid CreatedBy { get; set; }
    public bool IsDeleted { get; set; }

    // Navigation properties
    public Department Department { get; set; } = null!;
}
