using FunEvents.Application.Features.Reservation.Models.DTOs;
using FunEvents.Application.Features.Reservation.Models.Queries;

namespace FunEvents.Application.Features.Reservation.Services;

public interface IGetReservationService
{
    Task<List<ReservationDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<ReservationDto> GetByCodeAsync(GetByCodeQuery request, CancellationToken cancellationToken);

    Task<List<ReservationDto>> GetByEventAsync(GetByEventQuery request, CancellationToken cancellationToken);

    Task<List<ReservationDto>> GetByUserAsync(GetByUserQuery request, CancellationToken cancellationToken);
}
