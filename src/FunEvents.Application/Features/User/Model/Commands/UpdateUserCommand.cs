using FunEvents.Application.Features.User.Model.DTOs;

namespace FunEvents.Application.Features.User.Model.Commands;

public class UpdateUserCommand : IRequest<Response<UserDto>>
{
    [JsonProperty("userName")]
    public string UserName { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("phone")]
    public string Phone { get; set; } = null!;
}
