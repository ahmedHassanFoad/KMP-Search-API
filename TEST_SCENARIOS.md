# KMP Search API - Test Scenarios

## Base URL
```
http://localhost:5082/api/v1
```

## Swagger UI
```
http://localhost:5082/swagger
```

---

## Test Scenarios

### 1. Basic Full-Text Search

**Description:** Search for documents containing "report"  
**Expected:** Returns documents with "report" in title, description, or tags with highlights

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"query":"report","page":1,"pageSize":5}'
```

**CURL (Windows cmd/PowerShell):**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"query\":\"report\",\"page\":1,\"pageSize\":5}"
```

**Expected Response:**
- Status: 200 OK
- Results with `<em>report</em>` highlights in title/description
- Relevance scores (0.0 to 1.0)
- Pagination info

---

### 2. Search with Partial Word Matching

**Description:** Search using prefix "bud" to match "budget"  
**Expected:** Returns documents containing words starting with "bud"

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"query":"bud","page":1,"pageSize":10}'
```

**CURL:**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"query\":\"bud\",\"page\":1,\"pageSize\":10}"
```

**Expected:** Documents with "budget", "budgeting", etc.

---

### 3. Search with Category Filter

**Description:** Search "policy" in Finance category only  
**Expected:** Only Finance department policy documents

**PowerShell:**
```powershell
$body = @{
    query = "policy"
    filters = @{
        categories = @("Finance")
    }
    page = 1
    pageSize = 10
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body $body
```

**CURL:**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"query\":\"policy\",\"filters\":{\"categories\":[\"Finance\"]},\"page\":1,\"pageSize\":10}"
```

**Expected:** Only Finance category results

---

### 4. Search with Multiple Filters (AND Logic)

**Description:** Search with category AND date range filters  
**Expected:** Documents matching ALL filter criteria

**PowerShell:**
```powershell
$body = @{
    query = "report"
    filters = @{
        categories = @("Finance", "HR")
        dateRange = @{
            from = "2025-01-01"
            to = "2026-12-31"
        }
    }
    sort = @{
        field = "relevance"
        order = "desc"
    }
    page = 1
    pageSize = 20
} | ConvertTo-Json -Depth 5

Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body $body
```

**CURL:**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"query\":\"report\",\"filters\":{\"categories\":[\"Finance\",\"HR\"],\"dateRange\":{\"from\":\"2025-01-01\",\"to\":\"2026-12-31\"}},\"sort\":{\"field\":\"relevance\",\"order\":\"desc\"},\"page\":1,\"pageSize\":20}"
```

**Expected:** Finance or HR documents from 2025-2026

---

### 5. Sort by Date (Newest First)

**Description:** Get latest documents sorted by creation date  
**Expected:** Results ordered by CreatedAt descending

**PowerShell:**
```powershell
$body = @{
    query = "annual"
    sort = @{
        field = "date"
        order = "desc"
    }
    page = 1
    pageSize = 10
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body $body
```

**CURL:**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"query\":\"annual\",\"sort\":{\"field\":\"date\",\"order\":\"desc\"},\"page\":1,\"pageSize\":10}"
```

**Expected:** Documents ordered by newest first

---

### 6. Sort by Title (Alphabetical)

**Description:** Sort results alphabetically by title  
**Expected:** A-Z ordering

**PowerShell:**
```powershell
$body = @{
    query = "policy"
    sort = @{
        field = "title"
        order = "asc"
    }
    page = 1
    pageSize = 10
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body $body
```

**CURL:**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"query\":\"policy\",\"sort\":{\"field\":\"title\",\"order\":\"asc\"},\"page\":1,\"pageSize\":10}"
```

**Expected:** Alphabetically sorted titles

---

### 7. Pagination Test

**Description:** Test pagination with page 2  
**Expected:** Second page of results with correct pagination metadata

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"query":"annual","page":2,"pageSize":5}'
```

**CURL:**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"query\":\"annual\",\"page\":2,\"pageSize\":5}"
```

**Expected:** 
- Page 2 results
- Pagination: `{"page":2,"pageSize":5,"totalResults":X,"totalPages":Y}`

---

### 8. Empty Search (No Query)

**Description:** Search without query text  
**Expected:** All documents with default sorting

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"page":1,"pageSize":10}'
```

**CURL:**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"page\":1,\"pageSize\":10}"
```

**Expected:** All non-deleted documents

---

### 9. Advanced Boolean Search (FTS Operators)

**Description:** Use FTS boolean operators (AND, OR, NEAR)  
**Expected:** Combined search logic

**PowerShell:**
```powershell
# Search for documents with both "budget" AND "planning"
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"query":"budget AND planning","page":1,"pageSize":10}'
```

**CURL:**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"query\":\"budget AND planning\",\"page\":1,\"pageSize\":10}"
```

**Additional Boolean Examples:**
```powershell
# OR operator
'{"query":"finance OR budget","page":1,"pageSize":10}'

# NEAR operator (words close together)
'{"query":"annual NEAR report","page":1,"pageSize":10}'

# Combination
'{"query":"(budget OR planning) AND 2024","page":1,"pageSize":10}'
```

---

### 10. Autocomplete Suggestions

**Description:** Get suggestions for "bud"  
**Expected:** Suggestions from queries, titles, and tags

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search/suggestions?q=bud&limit=5" -Method GET
```

**CURL:**
```bash
curl.exe "http://localhost:5082/api/v1/search/suggestions?q=bud&limit=5"
```

**Expected Response:**
```json
{
  "suggestions": [
    {"text": "budget", "type": "query", "count": 32},
    {"text": "budget", "type": "tag", "count": 3},
    {"text": "Q4 Budget Report 2024", "type": "title", "count": 1}
  ]
}
```

---

### 11. Suggestions with Different Prefixes

**PowerShell:**
```powershell
# Annual reports
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search/suggestions?q=ann&limit=10" -Method GET

# Compliance
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search/suggestions?q=comp&limit=10" -Method GET

# Security
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search/suggestions?q=sec&limit=10" -Method GET
```

**CURL:**
```bash
curl.exe "http://localhost:5082/api/v1/search/suggestions?q=ann&limit=10"
curl.exe "http://localhost:5082/api/v1/search/suggestions?q=comp&limit=10"
curl.exe "http://localhost:5082/api/v1/search/suggestions?q=sec&limit=10"
```

---

### 12. Get All Facets

**Description:** Retrieve all available filters  
**Expected:** Categories, departments, tags, and date range

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search/facets" -Method GET
```

**CURL:**
```bash
curl.exe "http://localhost:5082/api/v1/search/facets"
```

**Expected Response:**
```json
{
  "categories": [
    {"value": "Finance", "count": 6},
    {"value": "HR", "count": 6}
  ],
  "departments": [
    {"id": "guid", "name": "Finance", "count": 6}
  ],
  "tags": [
    {"value": "annual", "count": 12},
    {"value": "2024", "count": 10}
  ],
  "dateRange": {
    "min": "2025-02-21T17:24:37.32",
    "max": "2026-02-21T17:24:37.37"
  }
}
```

---

### 13. Search by Tag

**Description:** Search for documents with specific tag  
**Expected:** Documents tagged with the search term

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"query":"quarterly","page":1,"pageSize":10}'
```

**CURL:**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"query\":\"quarterly\",\"page\":1,\"pageSize\":10}"
```

**Expected:** Documents with "quarterly" tag

---

### 14. Filter by Multiple Tags

**Description:** Apply tag filters (AND logic)  
**Expected:** Documents having ALL specified tags

**PowerShell:**
```powershell
$body = @{
    query = ""
    filters = @{
        tags = @("annual", "2024")
    }
    page = 1
    pageSize = 10
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body $body
```

**CURL:**
```bash
curl.exe -X POST "http://localhost:5082/api/v1/search" -H "Content-Type: application/json" -d "{\"filters\":{\"tags\":[\"annual\",\"2024\"]},\"page\":1,\"pageSize\":10}"
```

**Expected:** Only documents with both "annual" AND "2024" tags

---

### 15. Filter by Department ID

**Description:** Get documents from specific department  
**Expected:** Department-specific results

**First, get department IDs from facets:**
```powershell
$facets = Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search/facets" -Method GET
$financeId = $facets.departments | Where-Object {$_.name -eq "Finance"} | Select-Object -ExpandProperty id

# Then search by department
$body = @{
    query = ""
    filters = @{
        departmentIds = @($financeId)
    }
    page = 1
    pageSize = 10
} | ConvertTo-Json

Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body $body
```

---

### 16. Relevance Score Verification

**Description:** Verify relevance scoring is working  
**Expected:** Higher scores for better matches

**PowerShell:**
```powershell
# Exact title match should score higher
$result = Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"query":"Budget Report","page":1,"pageSize":10}'
$result.results | Select-Object title, score | Format-Table
```

**Expected:** 
- Documents with "Budget Report" in title have higher scores
- Scores decrease for partial matches

---

### 17. Highlight Verification

**Description:** Verify search terms are highlighted  
**Expected:** `<em>` tags around matched terms

**PowerShell:**
```powershell
$result = Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"query":"compliance","page":1,"pageSize":3}'
$result.results | ForEach-Object { 
    Write-Host "Title: $($_.highlights.title)"
    Write-Host "Description: $($_.highlights.description)"
    Write-Host "---"
}
```

**Expected:** Output with `<em>compliance</em>` tags

---

### 18. Complex Multi-Filter Query

**Description:** Combine all filter types  
**Expected:** Results matching ALL criteria

**PowerShell:**
```powershell
$body = @{
    query = "report"
    filters = @{
        categories = @("Finance", "Legal")
        tags = @("annual")
        dateRange = @{
            from = "2025-01-01"
            to = "2026-12-31"
        }
    }
    sort = @{
        field = "relevance"
        order = "desc"
    }
    page = 1
    pageSize = 20
} | ConvertTo-Json -Depth 5

Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body $body
```

**Expected:** Finance OR Legal documents with "annual" tag from 2025-2026 containing "report"

---

### 19. Error Handling - Invalid Suggestion Query

**Description:** Test validation on suggestions endpoint  
**Expected:** 400 Bad Request

**PowerShell:**
```powershell
try {
    Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search/suggestions?q=&limit=5" -Method GET
} catch {
    Write-Host "Status: $($_.Exception.Response.StatusCode)"
    Write-Host "Error: $($_.ErrorDetails.Message)"
}
```

**Expected:** 400 error with message about required 'q' parameter

---

### 20. Performance Test - Large Page Size

**Description:** Request maximum results  
**Expected:** Efficient response with pagination

**PowerShell:**
```powershell
Measure-Command {
    $result = Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"query":"","page":1,"pageSize":100}'
    Write-Host "Returned: $($result.results.Count) documents"
}
```

**Expected:** Fast response (< 200ms typically) with 27 documents (total in DB)

---

## Quick Test Script (PowerShell)

Run all basic tests at once:

```powershell
# Quick validation script
Write-Host "=== Testing KMP Search API ===" -ForegroundColor Cyan

# Test 1: Basic Search
Write-Host "`n1. Basic Search (report)..." -ForegroundColor Yellow
$search1 = Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"query":"report","page":1,"pageSize":5}'
Write-Host "   ✓ Returned $($search1.results.Count) results" -ForegroundColor Green

# Test 2: Suggestions
Write-Host "`n2. Autocomplete Suggestions (bud)..." -ForegroundColor Yellow
$suggest = Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search/suggestions?q=bud&limit=5" -Method GET
Write-Host "   ✓ Returned $($suggest.suggestions.Count) suggestions" -ForegroundColor Green

# Test 3: Facets
Write-Host "`n3. Get Facets..." -ForegroundColor Yellow
$facets = Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search/facets" -Method GET
Write-Host "   ✓ Categories: $($facets.categories.Count)" -ForegroundColor Green
Write-Host "   ✓ Departments: $($facets.departments.Count)" -ForegroundColor Green
Write-Host "   ✓ Tags: $($facets.tags.Count)" -ForegroundColor Green

# Test 4: Filtered Search
Write-Host "`n4. Filtered Search (Finance category)..." -ForegroundColor Yellow
$filter = @{query="policy";filters=@{categories=@("Finance")};page=1;pageSize=10} | ConvertTo-Json
$search2 = Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body $filter
Write-Host "   ✓ Returned $($search2.results.Count) Finance documents" -ForegroundColor Green

# Test 5: Pagination
Write-Host "`n5. Pagination Test..." -ForegroundColor Yellow
$page1 = Invoke-RestMethod -Uri "http://localhost:5082/api/v1/search" -Method POST -ContentType "application/json" -Body '{"query":"","page":1,"pageSize":5}'
Write-Host "   ✓ Page 1: $($page1.pagination.page)/$($page1.pagination.totalPages)" -ForegroundColor Green
Write-Host "   ✓ Total Results: $($page1.pagination.totalResults)" -ForegroundColor Green

Write-Host "`n=== All Tests Passed! ===" -ForegroundColor Green
```

---

## Expected Database State

- **Total Documents:** 27
- **Departments:** 5
- **Categories:** Finance, HR, Legal, Operations, IT, Engineering
- **Common Tags:** annual, 2024, policy, report, compliance, quarterly, budget

---

## Notes

1. **FTS Features Used:**
   - Prefix matching with wildcard (*)
   - Boolean operators (AND, OR, NEAR)
   - RANK-based relevance scoring
   - Multi-field search (Title, Description, Tags)

2. **Response Headers:**
   - `X-Total-Count`: Total matching documents
   - `X-Page`: Current page number
   - `X-Page-Size`: Results per page
   - `X-Total-Pages`: Total number of pages

3. **Performance:**
   - Search queries: ~50-150ms
   - Suggestions: ~30-80ms
   - Facets: ~20-50ms

---

## Troubleshooting

If you get errors:

1. **500 Internal Server Error on Suggestions:**
   - Ensure FTS index exists on SearchQueries table
   - Run: `sqlcmd -S . -d KMPSearchDb -Q "SELECT name FROM sys.fulltext_indexes WHERE object_id IN (OBJECT_ID('Documents'), OBJECT_ID('SearchQueries'))"`

2. **No Results:**
   - Check if seed data exists: `sqlcmd -S . -d KMPSearchDb -Q "SELECT COUNT(*) FROM Documents"`

3. **Connection Errors:**
   - Verify API is running: `http://localhost:5082/api/health`
   - Check port 5082 is not in use by another process
