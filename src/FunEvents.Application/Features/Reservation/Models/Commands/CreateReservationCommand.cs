using FunEvents.Application.Features.Reservation.Models.DTOs;

namespace FunEvents.Application.Features.Reservation.Models.Commands;

public class CreateReservationCommand : IRequest<Response<ReservationDto>>
{
    [JsonProperty("code")]
    public string Code { get; set; } = null!;

    [JsonProperty("eventCode")]
    public string EventCode { get; set; } = null!;

    [JsonProperty("userName")]
    public string UserName { get; set; } = null!;

    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("channel")]
    public int Channel { get; set; }
}
