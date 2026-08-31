using FunEvents.Application.Features.Reservation.Models.DTOs;

namespace FunEvents.Application.Features.Reservation.Models.Queries;

public class GetByEventQuery : IRequest<Response<List<ReservationDto>>>
{
    [JsonProperty("eventCode")]
    public string EventCode { get; set; } = null!;
}
