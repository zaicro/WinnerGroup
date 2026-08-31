namespace FunEvents.Application.Features.Reservation.Models.DTOs;

public class ReservationDto
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
    public OptionDto Channel { get; set; } = null!;

    [JsonProperty("status")]
    public OptionDto Status { get; set; } = null!;

    [JsonProperty("createdAt")]
    public DateTime CreatedAt { get; set; }
}
