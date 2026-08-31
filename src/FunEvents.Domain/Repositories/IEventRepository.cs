namespace FunEvents.Domain.Repositories;

public interface IEventRepository
{
    Task AddAsync(Event @event, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<Event?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task UpdateAsync(Event @event, CancellationToken cancellationToken = default);
}