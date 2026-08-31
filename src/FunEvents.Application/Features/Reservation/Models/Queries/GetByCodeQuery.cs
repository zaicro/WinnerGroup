using FunEvents.Application.Features.Reservation.Models.DTOs;

namespace FunEvents.Application.Features.Reservation.Models.Queries;

public class GetByCodeQuery : IRequest<Response<ReservationDto>>
{
    [JsonProperty("code")]
    public string Code { get; set; } = null!;
}
