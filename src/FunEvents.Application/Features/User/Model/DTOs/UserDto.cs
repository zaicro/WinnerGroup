namespace FunEvents.Application.Features.User.Model.DTOs;

public class UserDto
{
    [JsonProperty("username")]
    public string Username { get; set; } = null!;

    [JsonProperty("name")]
    public string Name { get; set; } = null!;

    [JsonProperty("email")]
    public string Email { get; set; } = null!;

    [JsonProperty("phone")]
    public string Phone { get; set; } = null!;
}
