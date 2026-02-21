using KMPSearch.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace KMPSearch.Infrastructure.Services;

public class FtsValidator : IFtsValidator
{
    private readonly ISearchDbContext _context;

    public FtsValidator(ISearchDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsFtsEnabledAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var sql = @"
                SELECT COUNT(*) 
                FROM sys.fulltext_indexes 
                WHERE object_id = OBJECT_ID('Documents')";

            var dbContext = (DbContext)_context;
            var result = await dbContext.Database.ExecuteSqlRawAsync(
                "SELECT 1 WHERE EXISTS (" + sql + ")", 
                cancellationToken);

            // Alternative approach: use a query that returns a value
            var connection = dbContext.Database.GetDbConnection();
            await using var command = connection.CreateCommand();
            command.CommandText = sql;
            
            if (connection.State != System.Data.ConnectionState.Open)
            {
                await connection.OpenAsync(cancellationToken);
            }

            var count = await command.ExecuteScalarAsync(cancellationToken);
            return count != null && Convert.ToInt32(count) > 0;
        }
        catch
        {
            // If we can't check, assume FTS is not available
            return false;
        }
    }
}
