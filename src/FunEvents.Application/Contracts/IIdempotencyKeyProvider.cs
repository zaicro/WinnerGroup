namespace FunEvents.Application.Contracts;

public interface IIdempotencyKeyProvider
{
    string? Get();
}
