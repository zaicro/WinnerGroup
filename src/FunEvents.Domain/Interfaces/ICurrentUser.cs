namespace FunEvents.Domain.Interfaces
{
    public interface ICurrentUser
    {
        string? UserName { get; }

        string? ClientId { get; }

        bool IsAuthenticated { get; }
    }
}
