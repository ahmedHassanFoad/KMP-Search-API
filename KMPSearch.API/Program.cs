using KMPSearch.Application;
using KMPSearch.Infrastructure;
using KMPSearch.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Add Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "KMP Search API",
        Version = "v1",
        Description = "Full-text search microservice for Knowledge Management Platform"
    });

    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

// Add Application layer services
builder.Services.AddApplication();

// Add Infrastructure layer services
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Run migrations and seed database on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SearchDbContext>();
        
        // Apply pending migrations
        await context.Database.MigrateAsync();
        
        Console.WriteLine("Database migrations applied successfully");

        // Validate FTS is enabled
        var ftsValidator = services.GetRequiredService<KMPSearch.Application.Common.Interfaces.IFtsValidator>();
        var isFtsEnabled = await ftsValidator.IsFtsEnabledAsync();
        
        if (!isFtsEnabled)
        {
            throw new InvalidOperationException(
                "SQL Server Full-Text Search is not configured on the Documents table.\n" +
                "Please ensure:\n" +
                "1. SQL Server Full-Text Search feature is installed\n" +
                "2. Run the 'AddFullTextSearch' migration: dotnet ef database update\n" +
                "3. Verify FTS catalog and index exist:\n" +
                "   SELECT * FROM sys.fulltext_catalogs;\n" +
                "   SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('Documents');");
        }

        Console.WriteLine("Full-Text Search validation passed");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"An error occurred while migrating the database: {ex.Message}");
        throw; // Re-throw to prevent app from starting with invalid configuration
    }
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "KMP Search API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

Console.WriteLine("KMP Search API is running...");
Console.WriteLine($"Swagger UI: {(app.Environment.IsDevelopment() ? "http://localhost:5082/swagger" : "N/A")}");

app.Run();
