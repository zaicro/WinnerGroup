namespace FunEvents.Domain.Repositories;

public interface IReservationRepository
{
    Task AddAsync(Reservation reservation, string idempotencyKey, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string reservationCode, CancellationToken cancellationToken = default);

    Task<Reservation?> GetByCodeAsync(string reservationCode, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Reservation>> GetByEventAsync(int eventId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Reservation>> GetByUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Reservation>> GetAllAsync(CancellationToken cancellationToken = default);

    Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default);
}