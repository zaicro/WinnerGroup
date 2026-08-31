using FunEvents.Logging.Domain;
using FunEvents.Logging.Providers.Log4Net;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace FunEvents.Logging;

/// <summary>
/// Provides dependency injection registration for logging services.
/// </summary>
[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    /// <summary>
    /// Registers the logging services and their implementations.
    /// </summary>
    /// <param name="services">
    /// The service collection to which the logging services are added.
    /// </param>
    /// <returns>
    /// The same service collection with logging services registered.
    /// </returns>
    public static IServiceCollection AddLoggingServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ILogger, Log4NetLogger>();

        return services;
    }
}