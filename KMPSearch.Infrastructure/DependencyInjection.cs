using KMPSearch.Application.Common.Interfaces;
using KMPSearch.Infrastructure.Persistence;
using KMPSearch.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KMPSearch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        services.AddDbContext<SearchDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(SearchDbContext).Assembly.FullName)));

        // Register ISearchDbContext
        services.AddScoped<ISearchDbContext>(provider =>
            provider.GetRequiredService<SearchDbContext>());

        // Register services
        services.AddScoped<ISearchService, SearchService>();
        services.AddSingleton<IHighlightService, HighlightService>();
        services.AddSingleton<IFtsQueryBuilder, FtsQueryBuilder>();
        services.AddScoped<IFtsValidator, FtsValidator>();

        return services;
    }
}
