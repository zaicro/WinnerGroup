using FunEvents.Application.Features.Event.Services;
using FunEvents.Application.Features.Event.Services.Impl;
using FunEvents.Application.Features.Reservation.Services;
using FunEvents.Application.Features.Reservation.Services.Impl;
using FunEvents.Application.Features.User.Services;
using FunEvents.Application.Features.User.Services.Impl;

namespace FunEvents.Application;

public static class DependencyInjection
{
    public static void AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(config => config.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

        services.AddScoped<ICreateEventService, CreateEventService>();
        services.AddScoped<IGetEventService, GetEventService>();
        services.AddScoped<IUpdateEventService, UpdateEventService>();

        services.AddScoped<ICreateReservationService, CreateReservationService>();
        services.AddScoped<IGetReservationService, GetReservationService>();
        services.AddScoped<IUpdateReservationService, UpdateReservationService>();

        services.AddScoped<ICreateUserService, CreateUserService>();
        services.AddScoped<IGetUserService, GetUserService>();
        services.AddScoped<IUpdateUserService, UpdateUserService>();
    }
}
