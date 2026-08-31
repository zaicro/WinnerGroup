using FunEvents.Application.Features.Reservation.Models.DTOs;

namespace FunEvents.Application.Features.Reservation.Models.Commands;

public class UpdateReservationCommand : IRequest<Response<ReservationDto>>
{
    [JsonProperty("code")]
    public string Code { get; set; } = null!;

    [JsonProperty("quantity")]
    public int Quantity { get; set; }

    [JsonProperty("status")]
    public OptionDto Status { get; set; } = null!;
}
