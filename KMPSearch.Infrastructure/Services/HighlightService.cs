namespace KMPSearch.Infrastructure.Services;

public interface IHighlightService
{
    string HighlightMatches(string text, string query);
}

public class HighlightService : IHighlightService
{
    public string HighlightMatches(string text, string query)
    {
        if (string.IsNullOrWhiteSpace(text) || string.IsNullOrWhiteSpace(query))
            return text;

        // Split query into individual terms
        var terms = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var result = text;
        foreach (var term in terms)
        {
            if (string.IsNullOrWhiteSpace(term))
                continue;

            // Find case-insensitive matches and wrap with <em> tags
            var pattern = System.Text.RegularExpressions.Regex.Escape(term);
            result = System.Text.RegularExpressions.Regex.Replace(
                result,
                pattern,
                match => $"<em>{match.Value}</em>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return result;
    }
}
