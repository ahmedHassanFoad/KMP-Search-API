using KMPSearch.Application.Common.Interfaces;
using KMPSearch.Domain.Common;
using KMPSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace KMPSearch.Infrastructure.Persistence;

public class SearchDbContext : DbContext, ISearchDbContext
{
    public SearchDbContext(DbContextOptions<SearchDbContext> options) : base(options)
    {
    }

    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<SearchQuery> SearchQueries => Set<SearchQuery>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SearchDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Auto-update timestamps for modified entities
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
