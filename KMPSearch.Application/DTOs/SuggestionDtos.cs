namespace KMPSearch.Application.DTOs;

public class SuggestionResponse
{
    public List<SuggestionItem> Suggestions { get; set; } = new();
}

public class SuggestionItem
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int Count { get; set; }
}
