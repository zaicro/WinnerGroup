using FunEvents.Application.Features.Event.Model.DTOs;

namespace FunEvents.Application.Features.Event.Model.Commands;

public class CreateEventCommand : IRequest<Response<EventDto>>
{
    [JsonProperty("code")]
    public string Code { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("eventDate")]
    public DateTime EventDate { get; set; }

    [JsonProperty("capacity")]
    public int Capacity { get; set; }
}
