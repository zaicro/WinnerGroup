using FunEvents.Application.Features.User.Model.DTOs;

namespace FunEvents.Application.Features.User.Model.Commands;

public class CreateUserCommand : IRequest<Response<UserDto>>
{
    [JsonProperty("username")]
    public string Username { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("email")]
    public string Email { get; set; } = null!;

    [JsonProperty("phone")]
    public string Phone { get; set; } = null!;

    [JsonProperty("password")]
    public string Password { get; set; } = null!;

    [JsonProperty("confirmedPassword")]
    public string ConfirmedPassword { get; set; } = null!;
}
