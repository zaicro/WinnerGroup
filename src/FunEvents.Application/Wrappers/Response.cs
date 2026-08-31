using Newtonsoft.Json;

namespace FunEvents.Application.Wrappers;

public class Response<T>
{
    [JsonProperty("succeeded")]
    public bool Succeeded { get; set; }

    [JsonProperty("message")]
    public string Message { get; set; } = string.Empty;

    [JsonProperty("errors")]
    public List<string> Errors { get; set; } = [];

    [JsonProperty("data")]
    public T? Data { get; set; }

    public static Response<T> Success()
    {
        return new Response<T>
        {
            Succeeded = true
        };
    }

    public static Response<T> Success(T data)
    {
        return new Response<T>
        {
            Succeeded = true,
            Data = data
        };
    }

    public static Response<T> Success(T data, string message)
    {
        return new Response<T>
        {
            Succeeded = true,
            Message = message,
            Data = data
        };
    }

    public static Response<T> Failure(string message)
    {
        return new Response<T>
        {
            Succeeded = false,
            Message = message
        };
    }

    public static Response<T> Failure(
        string message,
        List<string> errors)
    {
        return new Response<T>
        {
            Succeeded = false,
            Message = message,
            Errors = errors
        };
    }
}