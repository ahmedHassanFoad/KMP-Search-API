using KMPSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KMPSearch.Application.Common.Interfaces;

public interface ISearchDbContext
{
    DbSet<Document> Documents { get; }
    DbSet<Department> Departments { get; }
    DbSet<SearchQuery> SearchQueries { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
