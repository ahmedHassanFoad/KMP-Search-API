using KMPSearch.Domain.Common;

namespace KMPSearch.Domain.Entities;

public class Department : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public ICollection<Document> Documents { get; set; } = new List<Document>();
}
