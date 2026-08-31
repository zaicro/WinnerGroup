namespace FunEvents.Infrastructure.Sql;

public static class DependencyInjection
{
    public static void AddInfraestructureSQL(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<FunEventsDbContext>(options =>
           options.UseSqlServer(connectionString,
               builder => builder.MigrationsAssembly(typeof(FunEventsDbContext).Assembly.FullName)));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IEventRepository, EventRepository>();
        services.AddScoped<IReservationRepository, ReservationRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
    }
}
