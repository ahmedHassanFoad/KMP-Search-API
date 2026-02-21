namespace KMPSearch.Application.Common.Interfaces;

public interface IFtsValidator
{
    /// <summary>
    /// Checks if SQL Server Full-Text Search is enabled on the Documents table.
    /// </summary>
    /// <returns>True if FTS index exists on Documents table, otherwise false</returns>
    Task<bool> IsFtsEnabledAsync(CancellationToken cancellationToken = default);
}
