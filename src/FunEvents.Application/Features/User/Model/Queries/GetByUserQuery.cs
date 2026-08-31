using FunEvents.Application.Features.User.Model.DTOs;

namespace FunEvents.Application.Features.User.Model.Queries;

public class GetByUserQuery : IRequest<Response<UserDto>>
{
    [JsonProperty("user")]
    public string User { get; set; } = null!;
}
