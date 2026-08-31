using FunEvents.Application.Features.Reservation.Models.Commands;
using FunEvents.Application.Features.Reservation.Models.DTOs;

namespace FunEvents.Application.Features.Reservation.Services;

public interface IUpdateReservationService
{
    Task<ReservationDto> UpdateReservationAsync(UpdateReservationCommand request, CancellationToken cancellationToken);
}
