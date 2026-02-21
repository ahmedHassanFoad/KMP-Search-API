using KMPSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KMPSearch.Infrastructure.Persistence.Configurations;

public class SearchQueryConfiguration : IEntityTypeConfiguration<SearchQuery>
{
    public void Configure(EntityTypeBuilder<SearchQuery> builder)
    {
        builder.ToTable("SearchQueries");

        builder.HasKey(sq => sq.Id);

        builder.Property(sq => sq.QueryText)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(sq => sq.SearchCount)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(sq => sq.LastSearchedAt)
            .IsRequired();

        // Index for fast autocomplete lookups
        builder.HasIndex(sq => sq.QueryText);
        builder.HasIndex(sq => sq.SearchCount);
    }
}
