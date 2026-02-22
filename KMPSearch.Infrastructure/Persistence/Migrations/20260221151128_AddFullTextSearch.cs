using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace KMPSearch.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddFullTextSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create Full-Text Catalog (must be outside transaction)
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'DocumentCatalog')
                BEGIN
                    CREATE FULLTEXT CATALOG DocumentCatalog AS DEFAULT;
                END
            ", suppressTransaction: true);

            // Create Full-Text Index on Documents table
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Documents'))
                BEGIN
                    CREATE FULLTEXT INDEX ON Documents(Title LANGUAGE 1033, Description LANGUAGE 1033, Tags LANGUAGE 1033)
                    KEY INDEX PK_Documents
                    ON DocumentCatalog
                    WITH CHANGE_TRACKING AUTO;
                END
            ", suppressTransaction: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop Full-Text Index
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Documents'))
                BEGIN
                    DROP FULLTEXT INDEX ON Documents;
                END
            ", suppressTransaction: true);

            // Drop Full-Text Catalog (must be outside transaction)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT 1 FROM sys.fulltext_catalogs WHERE name = 'DocumentCatalog')
                BEGIN
                    DROP FULLTEXT CATALOG DocumentCatalog;
                END
            ", suppressTransaction: true);
        }
    }
}
