using KMPSearch.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KMPSearch.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.Description)
            .HasMaxLength(2000);

        builder.Property(d => d.Category)
            .IsRequired();

        builder.Property(d => d.FilePath)
            .IsRequired();

        builder.Property(d => d.MimeType)
            .IsRequired();

        builder.Property(d => d.CreatedBy)
            .IsRequired();

        builder.Property(d => d.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Convert Tags array to comma-separated string for storage
        builder.Property(d => d.Tags)
            .HasConversion(
                v => string.Join(",", v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

        // Indexes for performance
        builder.HasIndex(d => d.Category);
        builder.HasIndex(d => d.DepartmentId);
        builder.HasIndex(d => d.IsDeleted);
        builder.HasIndex(d => d.CreatedBy);
        builder.HasIndex(d => d.CreatedAt);

        // Relationship
        builder.HasOne(d => d.Department)
            .WithMany(dept => dept.Documents)
            .HasForeignKey(d => d.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
