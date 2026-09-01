using FunEvents.Application.Features.Reservation.Models.Commands;
using FunEvents.Application.Features.Reservation.Models.DTOs;

namespace FunEvents.Application.Features.Reservation.Services;

public interface ICreateReservationService
{
    Task<ReservationDto> CreateReservationAsync(CreateReservationCommand request, string idempotencyKey, CancellationToken cancellationToken);
}
