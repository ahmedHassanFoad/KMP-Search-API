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
- **HTTP Requests** - `TEST_REQUESTS.http` (VS Code REST Client)
- **CURL Commands** - `CURL_COMMANDS.txt` (Windows PowerShell)
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
- SQL Server (LocalDB, Express, or full instance)
- SQL Server Management Studio (optional, for running seed script)

## 🚀 Setup Instructions

### Option A: Using EF Core Migrations (Recommended)

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

4. **Apply migrations (creates database)**
   ```bash
   dotnet ef database update --project KMPSearch.Infrastructure --startup-project KMPSearch.API
   ```

5. **Seed the database**
   
   Run the seed script using sqlcmd or SSMS:
   ```bash
   sqlcmd -S . -d KMPSearchDb -i SeedDatabase.sql
   ```
   
   Or using SSMS: Open `SeedDatabase.sql` and execute against KMPSearchDb

6. **Run the application**
   ```bash
   dotnet run --project KMPSearch.API
   ```

7. **Access Swagger UI**
   
   Open browser: [http://localhost:5082/swagger](http://localhost:5082/swagger)

### Option B: Using Standalone SQL Script

1. **Create database and run seed script**
   ```bash
   sqlcmd -S . -i SeedDatabase.sql
   ```

2. **Follow steps 1, 2, 6, and 7 from Option A**

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

**Test Results:** See [POSTMAN_TEST_RESULTS.md](POSTMAN_TEST_RESULTS.md) for detailed test execution report with expected responses and performance metrics.

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

The service uses **SQL Server LIKE queries** with optimizations:

- Case-insensitive search
- Partial word matching
- Search across title, description, and tags
- Simple relevance scoring:
  - Title match: +10 points
  - Description match: +5 points
  - Tag match: +3 points

**Future Enhancement:** 
The database is structured to support SQL Server Full-Text Search (FTS). To enable:

```sql
-- Create full-text catalog
CREATE FULLTEXT CATALOG DocumentSearchCatalog AS DEFAULT;

-- Create full-text index
CREATE FULLTEXT INDEX ON Documents(Title, Description)
KEY INDEX PK_Documents
ON DocumentSearchCatalog
WITH CHANGE_TRACKING AUTO;
```

Then update `SearchService.cs` to use `CONTAINSTABLE` or `FREETEXTTABLE` for better relevance ranking.

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

1. **Update connection string** - Use secure credentials
2. **Enable HTTPS** - Configure SSL certificates
3. **Add authentication** - Implement JWT/OAuth if required
4. **Configure logging** - Add Serilog or similar
5. **Enable CORS** - If consumed by web clients
6. **Add rate limiting** - Prevent API abuse
7. **Enable FTS** - For better search performance
8. **Add caching** - Redis for search results/facets

## ⚡ Performance Tips

- Indexed fields: Category, DepartmentId, IsDeleted, CreatedAt, QueryText
- Page size capped at 100
- Soft deletes (IsDeleted) for data retention
- Efficient tag storage (comma-separated, converted at app layer)
- Search query tracking is non-blocking

## 🐛 Troubleshooting

**Database connection fails:**
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure Windows Authentication is enabled

**Migration fails:**
- Ensure KMPSearchDb database doesn't already exist or drop it first
- Check SQL Server permissions

**No search results:**
- Verify database is seeded (run SeedDatabase.sql)
- Check IsDeleted = 0 in Documents table
- Try broader search terms

**Port conflict (5082 in use):**
- Update port in launchSettings.json
- Update README examples

## 📄 License

This is a technical assignment project.

## 👥 Contact

For questions or issues, contact the development team.

---

**Version:** 1.0.0  
**Last Updated:** February 21, 2026
