using FunEvents.Application.Features.User.Model.DTOs;

namespace FunEvents.Application.Features.User.Model.Queries;

public class GetByEmailQuery : IRequest<Response<UserDto>>
{
    [JsonProperty("email")]
    public string Email { get; set; } = null!;
}
