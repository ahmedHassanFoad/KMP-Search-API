using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace KMPSearch.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
}
