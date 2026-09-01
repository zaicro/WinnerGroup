using FunEvents.Application.Contracts;

namespace FunEvents.Api;

public sealed class HttpIdempotencyKeyProvider : IIdempotencyKeyProvider
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public HttpIdempotencyKeyProvider(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? Get()
    {
        return _httpContextAccessor
            .HttpContext?
            .Request
            .Headers["Idempotency-Key"]
            .FirstOrDefault();
    }
}
