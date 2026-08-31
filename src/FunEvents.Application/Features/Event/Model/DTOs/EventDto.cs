namespace FunEvents.Application.Features.Event.Model.DTOs;

public class EventDto
{
    [JsonProperty("code")]
    public string Code { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("eventDate")]
    public DateTime EventDate { get; set; }

    [JsonProperty("capacity")]
    public int Capacity { get; set; }

    [JsonProperty("status")]
    public OptionDto Status { get; set; } = null!;
}
