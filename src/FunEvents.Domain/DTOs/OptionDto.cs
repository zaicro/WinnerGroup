namespace FunEvents.Domain.DTOs;

public class OptionDto
{
    [JsonProperty("code")]
    public int Code { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; } = null!;
}
