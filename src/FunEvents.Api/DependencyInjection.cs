using FunEvents.Application;
using FunEvents.Application.Contracts;
using FunEvents.Infrastructure.Sql;
using FunEvents.Logging;

namespace FunEvents.Api;

internal static class DependencyInjection
{
    public static IServiceCollection AddProjectServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.SetMinimumLevel(LogLevel.Trace);
        });

        services.AddLoggingServices();

        services.AddInfraestructureSQL(configuration.GetConnectionString("DefaultConnection"));

        services.AddApplication();

        services.AddHttpContextAccessor();
        services.AddScoped<IIdempotencyKeyProvider, HttpIdempotencyKeyProvider>();

        return services;
    }
}