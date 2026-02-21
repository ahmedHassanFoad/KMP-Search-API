namespace KMPSearch.Application.DTOs;

public class FacetsResponse
{
    public List<FacetItem> Categories { get; set; } = new();
    public List<DepartmentFacet> Departments { get; set; } = new();
    public List<FacetItem> Tags { get; set; } = new();
    public DateRangeFacet DateRange { get; set; } = new();
}

public class FacetItem
{
    public string Value { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DepartmentFacet
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class DateRangeFacet
{
    public DateTime? Min { get; set; }
    public DateTime? Max { get; set; }
}
