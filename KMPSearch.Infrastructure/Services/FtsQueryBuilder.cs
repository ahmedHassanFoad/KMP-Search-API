using KMPSearch.Application.Common.Interfaces;
using System.Text;
using System.Text.RegularExpressions;

namespace KMPSearch.Infrastructure.Services;

public class FtsQueryBuilder : IFtsQueryBuilder
{
    private static readonly string[] AdvancedOperators = { "AND", "OR", "NEAR", "NOT" };
    private static readonly char[] SpecialChars = { '"', '*', '&', '|', '!', '(', ')', '[', ']', '<', '>', '~' };

    public string BuildContainsQuery(string userInput)
    {
        if (string.IsNullOrWhiteSpace(userInput))
        {
            return string.Empty;
        }

        var trimmedInput = userInput.Trim();

        // Check if input contains advanced FTS operators
        var hasAdvancedOperators = AdvancedOperators.Any(op => 
            trimmedInput.Contains($" {op} ", StringComparison.OrdinalIgnoreCase));

        if (hasAdvancedOperators)
        {
            // User is using advanced syntax - preserve structure, just escape special chars
            return EscapeAdvancedQuery(trimmedInput);
        }

        // Check if it's a phrase query (wrapped in quotes)
        if (trimmedInput.StartsWith('"') && trimmedInput.EndsWith('"') && trimmedInput.Length > 2)
        {
            // Return as phrase query - remove outer quotes, escape inner content, re-wrap
            var phraseContent = trimmedInput.Substring(1, trimmedInput.Length - 2);
            return $"\"{EscapeSpecialCharacters(phraseContent)}\"";
        }

        // Simple query - split into words and add prefix wildcards
        return BuildPrefixQuery(trimmedInput);
    }

    private string BuildPrefixQuery(string input)
    {
        // Split by whitespace
        var words = input.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return string.Empty;
        }

        var queryParts = new List<string>();

        foreach (var word in words)
        {
            var cleanWord = EscapeSpecialCharacters(word);
            
            if (string.IsNullOrWhiteSpace(cleanWord))
            {
                continue;
            }

            // Add wildcard for prefix matching (e.g., "annu" becomes "annu*")
            // Don't add wildcard if word already ends with *
            if (!cleanWord.EndsWith('*'))
            {
                queryParts.Add($"\"{cleanWord}*\"");
            }
            else
            {
                queryParts.Add($"\"{cleanWord}\"");
            }
        }

        if (queryParts.Count == 0)
        {
            return string.Empty;
        }

        // Join with AND operator for multiple words
        return string.Join(" AND ", queryParts);
    }

    private string EscapeAdvancedQuery(string query)
    {
        // For advanced queries, we need to preserve operators but escape literals
        // This is a simplified approach - split by operators and escape each part
        
        var result = new StringBuilder();
        var currentToken = new StringBuilder();
        var inQuotes = false;

        for (int i = 0; i < query.Length; i++)
        {
            char c = query[i];

            if (c == '"')
            {
                inQuotes = !inQuotes;
                currentToken.Append(c);
            }
            else if (c == ' ' && !inQuotes)
            {
                // Check if we're at an operator boundary
                var token = currentToken.ToString().Trim();
                if (!string.IsNullOrEmpty(token))
                {
                    if (IsOperator(token))
                    {
                        result.Append(token.ToUpper());
                    }
                    else
                    {
                        result.Append(EscapeToken(token));
                    }
                }
                result.Append(' ');
                currentToken.Clear();
            }
            else
            {
                currentToken.Append(c);
            }
        }

        // Handle last token
        var lastToken = currentToken.ToString().Trim();
        if (!string.IsNullOrEmpty(lastToken))
        {
            if (IsOperator(lastToken))
            {
                result.Append(lastToken.ToUpper());
            }
            else
            {
                result.Append(EscapeToken(lastToken));
            }
        }

        return result.ToString().Trim();
    }

    private bool IsOperator(string token)
    {
        return AdvancedOperators.Contains(token, StringComparer.OrdinalIgnoreCase);
    }

    private string EscapeToken(string token)
    {
        // If token is already quoted, escape inner content
        if (token.StartsWith('"') && token.EndsWith('"') && token.Length > 2)
        {
            var inner = token.Substring(1, token.Length - 2);
            return $"\"{EscapeSpecialCharacters(inner)}\"";
        }

        // Otherwise, escape and quote
        return $"\"{EscapeSpecialCharacters(token)}\"";
    }

    private string EscapeSpecialCharacters(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }

        var result = new StringBuilder(text.Length);

        foreach (char c in text)
        {
            // Escape special FTS characters by replacing them with space
            // Or remove them entirely depending on requirements
            if (SpecialChars.Contains(c))
            {
                // For most special chars, we'll just skip them
                // The exception is * which we handle separately for wildcards
                if (c == '*')
                {
                    result.Append(c);
                }
                // Skip other special characters
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}
