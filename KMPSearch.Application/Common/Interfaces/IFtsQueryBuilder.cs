namespace KMPSearch.Application.Common.Interfaces;

public interface IFtsQueryBuilder
{
    /// <summary>
    /// Builds a SQL Server Full-Text Search CONTAINS query string from user input.
    /// Supports prefix matching with wildcards and advanced FTS operators (AND, OR, NEAR).
    /// </summary>
    /// <param name="userInput">The raw search query from the user</param>
    /// <returns>A properly formatted and escaped FTS query string</returns>
    string BuildContainsQuery(string userInput);
}
