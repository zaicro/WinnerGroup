using FunEvents.Application.Features.Reservation.Models.DTOs;

namespace FunEvents.Application.Features.Reservation.Models.Queries;

public class GetAllQuery : IRequest<Response<List<ReservationDto>>>
{
}
