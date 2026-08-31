using FunEvents.Application.Features.Reservation.Models.DTOs;

namespace FunEvents.Application.Features.Reservation.Models.Queries;

public class GetByUserQuery : IRequest<Response<List<ReservationDto>>>
{
    [JsonProperty("userName")]
    public string UserName { get; set; } = null!;
}
