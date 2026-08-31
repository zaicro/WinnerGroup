using FunEvents.Application.Features.Event.Model.DTOs;

namespace FunEvents.Application.Features.Event.Model.Queries;

public class GetByCodeQuery : IRequest<Response<EventDto>>
{
    [JsonProperty("code")]
    public string Code { get; set; } = null!;
}
