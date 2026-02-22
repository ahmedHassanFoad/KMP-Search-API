# KMP Search Microservice

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-8.0-512BD4)](https://docs.microsoft.com/aspnet/core)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-FTS-CC2927?logo=microsoft-sql-server)](https://www.microsoft.com/sql-server)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

Full-text search microservice for the Knowledge Management Platform (KMP) providing search capabilities against document metadata.

**Repository:** https://github.com/ahmedHassanFoad/KMP-Search-API

## 🎯 Features

- **Full-Text Search** - Search across document titles, descriptions, and tags
- **Advanced Filtering** - Filter by categories, departments, date ranges, and tags
- **Multiple Sort Options** - Sort by relevance, date, or title
- **Autocomplete Suggestions** - Smart suggestions from search history, titles, and tags
- **Filter Facets** - Get available filter options with counts
- **Pagination** - Efficient pagination with configurable page sizes
- **Highlighting** - Search term highlighting in results with `<em>` tags
- **Relevance Scoring** - Results ranked by relevance score (0-1)

## 📁 Testing Documentation

Complete test suite included:
- **Postman Collection** - `KMP-Search-API.postman_collection.json` (31 requests, 42 assertions)
- **Test Results** - `POSTMAN_TEST_RESULTS.md` (detailed execution report)
- **Test Scenarios** - `TEST_SCENARIOS.md` (20 complete scenarios)

## 🏗️ Architecture

Clean Architecture with 4 layers:

```
KMP-Search/
├── KMPSearch.Domain/           # Business entities (no dependencies)
├── KMPSearch.Application/      # Business logic & interfaces
├── KMPSearch.Infrastructure/   # Data access & services
└── KMPSearch.API/             # REST API controllers
```

**Technology Stack:**
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core 9.0
- SQL Server (KMPSearchDb database)
- FluentValidation
- Swagger/OpenAPI

## 📋 Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- SQL Server (LocalDB, Express, or full instance) **with Full-Text Search feature installed**
- SQL Server Management Studio (optional, for running seed script)
- sqlcmd command-line tool (included with SQL Server)

## 🚀 Setup Instructions

### ⚠️ Important: Verify Full-Text Search (FTS) is Installed

Before proceeding, verify SQL Server has Full-Text Search installed:

```sql
-- Run this query in SSMS or sqlcmd
SELECT SERVERPROPERTY('IsFullTextInstalled') AS IsFullTextInstalled;
-- Result should be 1 (installed)
```

**If FTS is not installed:**
1. Run SQL Server Installation Center
2. Choose "Installation" → "New SQL Server stand-alone installation"
3. Select "Add features to an existing instance"
4. Check **"Full-Text and Semantic Extractions for Search"**
5. Complete installation wizard

### 📍 SQL Server Instance Note

The default connection string uses the **default SQL Server instance** (Server=`.`):
- If you're using a **named instance** like `DESKTOP-XXX\SQLEXPRESS`, update the connection string in `appsettings.json`
- To find your instance name, run: `sqlcmd -L`

### Setup Approach (EF Core Migrations + SQL Seed Script)

1. **Clone/Download the repository**
   ```bash
   cd C:\Users\{YourUser}\Desktop\KMP-Search
   ```

2. **Restore NuGet packages**
   ```bash
   dotnet restore
   ```

3. **Update connection string** (if needed)
   
   Edit `KMPSearch.API/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=.;Database=KMPSearchDb;..."
   }
   ```
   
   **For named instances** (e.g., SQL Express):
   ```json
   "DefaultConnection": "Server=.\\SQLEXPRESS;Database=KMPSearchDb;..."
   ```

4. **Apply migrations (creates database schema with Full-Text Search)**
   ```bash
   dotnet ef database update --project KMPSearch.Infrastructure --startup-project KMPSearch.API
   ```
   
   This creates:
   - Database tables (Departments, Documents, SearchQueries)
   - Indexes for optimized queries
   - **Full-Text Catalog** (DocumentCatalog)
   - **Full-Text Index** on Documents (Title, Description, Tags)
   
   ⚠️ **Note:** The Full-Text Index creation uses `suppressTransaction: true` because SQL Server doesn't allow FTS operations inside transactions.

5. **Seed the database with test data**
   
   Run the seed script using sqlcmd:
   ```bash
   sqlcmd -S . -d KMPSearchDb -i SeedDatabase.sql
   ```
   
   Or using SSMS: Open `SeedDatabase.sql` and execute against KMPSearchDb.
   
   This populates:
   - 5 Departments (IT, HR, Finance, Operations, Legal)
   - 26 Documents with realistic content
   - 10 Pre-seeded search queries for autocomplete
   
   **Why SQL script instead of EF migration?**
   - Easier to maintain and modify test data
   - Can be re-run to reset data
   - Avoids bloating migration files with seed data
   - Better separation of schema (migrations) vs data (scripts)

6. **Run the application**
   ```bash
   dotnet run --project KMPSearch.API
   ```

7. **Access Swagger UI**
   
   Open browser: [http://localhost:5082/swagger](http://localhost:5082/swagger)

### Verifying Full-Text Search is Working

After setup, verify FTS is operational:

```sql
-- 1. Check Full-Text Catalog exists
SELECT name, is_default FROM sys.fulltext_catalogs;
-- Expected: DocumentCatalog (is_default = 1)

-- 2. Check Full-Text Index on Documents table
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    name AS IndexName
FROM sys.fulltext_indexes;
-- Expected: Documents table with FT index

-- 3. Test FTS query
USE KMPSearchDb;
SELECT Title, Description 
FROM Documents 
WHERE CONTAINS((Title, Description, Tags), 'security');
-- Should return documents with 'security' in content
```

### Troubleshooting Full-Text Search

**Error: "CREATE FULLTEXT INDEX statement cannot be used inside a user transaction"**
- ✅ Already fixed - Migration uses `suppressTransaction: true`
- If you see this, ensure you're using the latest migration files

**No results from FTS queries:**
- Wait a few seconds after seeding for FTS indexing to complete
- Check index population status:
  ```sql
  SELECT OBJECTPROPERTY(OBJECT_ID('Documents'), 'TableFullTextPopulateStatus');
  -- 0 = Idle (ready), 1 = Full population in progress
  ```

**Database not visible in SSMS:**
- You may be connected to a **different SQL Server instance**
- The database is on the instance specified in connection string (default: `.`)
- In SSMS, connect to the correct instance (use Server name from connection string)

## 🔌 API Endpoints

### 1. Full-Text Search
```http
POST /api/v1/search
Content-Type: application/json

{
  "query": "annual report",
  "filters": {
    "categories": ["Finance"],
    "departmentIds": ["guid-here"],
    "dateRange": {
      "from": "2024-01-01",
      "to": "2024-12-31"
    },
    "tags": ["quarterly"]
  },
  "sort": {
    "field": "relevance",
    "order": "desc"
  },
  "page": 1,
  "pageSize": 20
}
```

**Sort Fields:** `relevance`, `date`, `title`  
**Sort Orders:** `asc`, `desc`

### 2. Autocomplete Suggestions
```http
GET /api/v1/search/suggestions?q=ann&limit=10
```

Returns suggestions from:
- Previous search queries
- Document titles
- Document tags

### 3. Filter Facets
```http
GET /api/v1/search/facets
```

Returns available filters with counts:
- Categories
- Departments
- Tags
- Date range (min/max)

### 4. Health Check
```http
GET /api/health
```

## 📊 Sample Data

The seed script includes:

- **5 Departments:**
  - Information Technology
  - Human Resources
  - Finance
  - Operations
  - Legal

- **26 Documents** across categories:
  - IT (5 documents)
  - HR (6 documents)
  - Finance (6 documents)
  - Operations (5 documents)
  - Legal (5 documents)
  - Engineering (1 document)

- **Document Date Range:** 2020 - 2026

- **Sample Tags:** annual, quarterly, budget, policy, compliance, planning, security, etc.

## 🧪 Testing

### Using Swagger UI

1. Navigate to [http://localhost:5082/swagger](http://localhost:5082/swagger)
2. Expand any endpoint
3. Click "Try it out"
4. Enter request parameters
5. Click "Execute"

### Using HTTP file

Open `KMPSearch.http` in VS Code with REST Client extension and send requests directly.

### Using Postman Collection

The repository includes a complete Postman collection with 31 pre-configured test requests and automated assertions.

**Import Collection:**
1. Open Postman (Desktop or Web)
2. Click **Import** → Select `KMP-Search-API.postman_collection.json`
3. Collection appears in your workspace

**Run All Tests:**
1. Click on collection name
2. Click **Run** button
3. Click **Run KMP Search API**
4. View automated test results (42 assertions)

**What's Included:**
- ✅ Health check endpoint
- ✅ Basic search (3 requests)
- ✅ Filtered search (5 requests)
- ✅ Sorting options (5 requests)
- ✅ Pagination (3 requests)
- ✅ Boolean operators (5 requests)
- ✅ Autocomplete suggestions (4 requests)
- ✅ Facets endpoint (1 request)
- ✅ Error handling (2 requests)
- ✅ Example responses saved
- ✅ Test scripts with assertions


### Sample Test Scenarios

1. **Test full-text search:**
   - Search for "annual report"
   - Search for "budget"
   - Search for "policy"

2. **Test filtering:**
   - Filter by category: "Finance"
   - Filter by date range: 2024-01-01 to 2024-12-31
   - Filter by tags: ["annual"]

3. **Test sorting:**
   - Sort by relevance (default)
   - Sort by date (newest first)
   - Sort by title (A-Z)

4. **Test pagination:**
   - Page 1, size 10
   - Page 2, size 10

5. **Test autocomplete:**
   - Query: "ann" (should suggest "annual report", "annual")
   - Query: "bud" (should suggest "budget")

6. **Test facets:**
   - Get all available categories with counts
   - Get all departments with document counts
   - Get popular tags

## 🔍 Search Implementation

The service uses **SQL Server Full-Text Search (FTS)** for optimized, enterprise-grade search:

### Full-Text Search Features
- **CONTAINSTABLE** queries for relevance-ranked results
- **Automatic index maintenance** with CHANGE_TRACKING AUTO
- **Multi-column search** across Title, Description, and Tags
- **Language-aware indexing** (LANGUAGE 1033 = US English)
- **Word and phrase matching** with natural language processing
- **Relevance scoring** (0.0 to 1.0) based on term frequency and proximity

### Search Architecture

**Full-Text Catalog:** `DocumentCatalog`
- Default catalog for all FTS indexes
- Automatically managed by SQL Server

**Full-Text Index:** On `Documents` table
- Indexed columns: Title, Description, Tags
- Key Index: Primary key (Id)
- Update method: Automatic change tracking

### Search Query Processing

```csharp
// Simplified from SearchService.cs
var ftsQuery = BuildFtsQuery(searchTerm); // e.g., "security AND policy"

var results = await context.Documents
    .FromSqlRaw(@"
        SELECT d.*, ft.RANK
        FROM Documents d
        INNER JOIN CONTAINSTABLE(Documents, (Title, Description, Tags), {0}) AS ft
        ON d.Id = ft.[KEY]
        WHERE d.IsDeleted = 0
    ", ftsQuery)
    .ToListAsync();
```

### Supported Search Operators

- **AND:** `security AND policy`
- **OR:** `budget OR financial`
- **NOT:** `annual NOT report`
- **Phrase:** `"employee handbook"`
- **Prefix:** `secur*` (matches security, secure, etc.)
- **NEAR:** `annual NEAR report` (words close together)

### Search Query Tracking

All searches are automatically tracked in `SearchQueries` table:
- Increments `SearchCount` for existing queries
- Updates `LastSearchedAt` timestamp
- Powers autocomplete suggestions
- Enables search analytics

## 📝 Project Structure

```
KMP-Search/
├── SeedDatabase.sql              # Comprehensive seed script
├── KMPSearch.http               # HTTP test file
├── README.md                    # This file
├── KMPSearch.sln               # Solution file
│
├── KMPSearch.Domain/
│   ├── Common/
│   │   └── BaseEntity.cs       # Base entity with Guid v7 IDs
│   └── Entities/
│       ├── Document.cs         # Document entity
│       ├── Department.cs       # Department entity
│       └── SearchQuery.cs      # Search history entity
│
├── KMPSearch.Application/
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   ├── ISearchDbContext.cs
│   │   │   └── ISearchService.cs
│   │   └── Models/
│   │       └── Result.cs       # Result pattern
│   ├── DTOs/
│   │   ├── SearchDtos.cs       # Search request/response
│   │   ├── SuggestionDtos.cs   # Autocomplete
│   │   └── FacetDtos.cs        # Facets
│   ├── Validators/
│   │   └── SearchRequestValidator.cs
│   └── DependencyInjection.cs
│
├── KMPSearch.Infrastructure/
│   ├── Persistence/
│   │   ├── SearchDbContext.cs
│   │   ├── Configurations/
│   │   │   ├── DocumentConfiguration.cs
│   │   │   ├── DepartmentConfiguration.cs
│   │   │   └── SearchQueryConfiguration.cs
│   │   └── Migrations/
│   ├── Services/
│   │   ├── SearchService.cs    # Core search logic
│   │   └── HighlightService.cs # Text highlighting
│   └── DependencyInjection.cs
│
└── KMPSearch.API/
    ├── Controllers/
    │   ├── BaseController.cs   # Base with /api/v1 routing
    │   ├── SearchController.cs # Search endpoints
    │   └── HealthController.cs
    ├── Properties/
    │   └── launchSettings.json # Port 5082 config
    ├── appsettings.json        # Configuration
    └── Program.cs              # App startup

```

## 🔐 Security Notes

- No authentication required (per assignment spec)
- Connection string uses Windows Authentication
- SQL injection protected via EF Core parameterization
- Input validation with FluentValidation
- Max page size: 100 (prevents excessive data retrieval)

## 🚢 Deployment Considerations

For production deployment:

1. **Update connection string** - Use secure credentials (not Windows Auth)
2. **Enable HTTPS** - Configure SSL certificates
3. **Add authentication** - Implement JWT/OAuth if required
4. **Configure logging** - Add Serilog or Application Insights
5. **Enable CORS** - If consumed by web clients from different domains
6. **Add rate limiting** - Prevent API abuse
7. **Verify FTS is installed** - Required for search functionality
8. **Add caching** - Redis for search results/facets/autocomplete
9. **Monitor FTS performance** - Use SQL Server Query Store
10. **Schedule FTS index optimization** - Periodic REORGANIZE/REBUILD for large datasets

### Full-Text Search Production Setup

**Index Optimization:**
```sql
-- Reorganize FTS index (online operation)
ALTER FULLTEXT INDEX ON Documents START INCREMENTAL POPULATION;

-- Full index rebuild (for major changes)
ALTER FULLTEXT INDEX ON Documents START FULL POPULATION;
```

**Monitoring Queries:**
```sql
-- Check index fragment status
SELECT * FROM sys.dm_fts_index_keywords(DB_ID('KMPSearchDb'), OBJECT_ID('Documents'));

-- Monitor population status
SELECT OBJECTPROPERTY(OBJECT_ID('Documents'), 'TableFullTextPopulateStatus');
```

## ⚡ Performance Tips

- **Full-Text Search enabled** - Uses CONTAINSTABLE for optimized queries
- **Automatic index updates** - CHANGE_TRACKING AUTO keeps FTS index current
- **Database indexes** on frequently queried fields:
  - Category, DepartmentId, IsDeleted, CreatedAt (Documents table)
  - QueryText, SearchCount (SearchQueries table)
- **Page size capped at 100** - Prevents excessive data retrieval
- **Soft deletes** (IsDeleted) for data retention and faster cleanup
- **Efficient tag storage** - Comma-separated, converted at application layer
- **Search query tracking** - Non-blocking async operations
- **FTS catalog auto-managed** - SQL Server handles index optimization
- **Connection pooling** - EF Core manages database connections efficiently

### FTS Performance Optimization

For large datasets (>100K documents):
1. **Monitor index fragmentation** - Rebuild if >30% fragmented
2. **Consider partitioning** - Split Documents table by date/department
3. **Use stoplist** - Exclude common words (the, a, an) from indexing
4. **Adjust max query length** - Default 100, increase for complex queries
5. **Enable query statistics** - Track slow FTS queries via DMVs

## 🐛 Troubleshooting

**Database connection fails:**
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure Windows Authentication is enabled
- **Verify you're connecting to the correct SQL Server instance:**
  - Run `sqlcmd -L` to list available instances
  - If using SQL Express, your instance might be `.\SQLEXPRESS` instead of `.`
  - Update connection string: `Server=.\SQLEXPRESS;Database=KMPSearchDb;...`

**Database not visible in SSMS Object Explorer:**
- You may be viewing a **different SQL Server instance**
- The database exists on the instance in the connection string (check appsettings.json)
- Example: If connection uses `Server=.` but SSMS is connected to `.\SQLEXPRESS`, you won't see it
- **Solution:** Connect to the correct instance in SSMS (same as connection string)

**Migration fails with Full-Text error:**
- Error: "CREATE FULLTEXT INDEX statement cannot be used inside a user transaction"
- **Solution:** Already fixed in migration - uses `suppressTransaction: true`
- If still occurring, ensure you have the latest migration files
- Verify Full-Text Search is installed: `SELECT SERVERPROPERTY('IsFullTextInstalled')`

**Migration succeeds but Full-Text Search doesn't work:**
- Check if Full-Text is installed: `SELECT SERVERPROPERTY('IsFullTextInstalled')`
- If result is 0, install FTS feature via SQL Server setup
- Verify catalog exists: `SELECT * FROM sys.fulltext_catalogs WHERE name = 'DocumentCatalog'`
- Verify index exists: `SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Documents')`

**No search results:**
- Verify database is seeded (run SeedDatabase.sql)
- Check IsDeleted = 0 in Documents table
- Try broader search terms
- **For FTS:** Wait 10-30 seconds after seeding for index population
- Check FTS index status: `SELECT OBJECTPROPERTY(OBJECT_ID('Documents'), 'TableFullTextPopulateStatus')`

**Port conflict (5082 in use):**
- Update port in launchSettings.json
- Update README examples


**Version:** 1.0.0  
**Last Updated:** February 22, 2026  
**Full-Text Search:** Enabled ✅
