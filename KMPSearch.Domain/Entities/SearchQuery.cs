using KMPSearch.Domain.Common;

namespace KMPSearch.Domain.Entities;

public class SearchQuery : BaseEntity
{
    public string QueryText { get; set; } = string.Empty;
    public int SearchCount { get; set; }
    public DateTime LastSearchedAt { get; set; }

    public SearchQuery()
    {
        LastSearchedAt = DateTime.UtcNow;
        SearchCount = 1;
    }
}
