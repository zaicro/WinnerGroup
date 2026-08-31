namespace FunEvents.Infrastructure.Sql.Repositories;

internal sealed class ReservationRepository(FunEventsDbContext context) : IReservationRepository
{
    public async Task AddAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        var table = new TbReservation
        {
            ReservationCode = reservation.ReservationCode,
            UserId = reservation.UserId,
            EventId = reservation.EventId,
            Quantity = reservation.Quantity,
            Channel = (int)reservation.Channel,
            Status = (int)reservation.Status
        };

        await context.Set<TbReservation>()
            .AddAsync(table, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string reservationCode, CancellationToken cancellationToken = default)
    {
        return await context.Set<TbReservation>()
            .AnyAsync(
                x => x.ReservationCode == reservationCode &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task<Reservation?> GetByCodeAsync(string reservationCode, CancellationToken cancellationToken = default)
    {
        var table = await context.Set<TbReservation>()
            .FirstOrDefaultAsync(
                x => x.ReservationCode == reservationCode &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        return table is null
            ? null
            : MapToDomain(table);
    }

    public async Task<IReadOnlyCollection<Reservation>> GetByEventAsync(int eventId, CancellationToken cancellationToken = default)
    {
        var tables = await context.Set<TbReservation>()
            .Where(x => x.EventId == eventId &&
                        x.IsActive &&
                        !x.IsDeleted)
            .ToListAsync(cancellationToken);

        return [.. tables.Select(MapToDomain)];
    }

    public async Task<IReadOnlyCollection<Reservation>> GetByUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var tables = await context.Set<TbReservation>()
            .Where(x => x.UserId == userId &&
                        x.IsActive &&
                        !x.IsDeleted)
            .ToListAsync(cancellationToken);

        return [.. tables.Select(MapToDomain)];
    }

    public async Task<IReadOnlyCollection<Reservation>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tables = await context.Set<TbReservation>()
            .ToListAsync(cancellationToken);

        return [.. tables.Select(MapToDomain)];
    }

    public async Task UpdateAsync(Reservation reservation, CancellationToken cancellationToken = default)
    {
        var table = await context.Set<TbReservation>()
            .FirstOrDefaultAsync(
                x => x.ReservationCode == reservation.ReservationCode &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        if (table is null)
            return;

        table.UserId = reservation.UserId;
        table.EventId = reservation.EventId;
        table.Quantity = reservation.Quantity;
        table.Channel = (int)reservation.Channel;
        table.Status = (int)reservation.Status;

        context.Set<TbReservation>().Update(table);
    }

    private static Reservation MapToDomain(TbReservation table)
    {
        if (!Enum.IsDefined(
                typeof(ReservationChannel),
                table.Channel))
        {
            throw new InvalidOperationException(
                $"Invalid reservation channel: {table.Channel}.");
        }

        if (!Enum.IsDefined(
                typeof(ReservationStatus),
                table.Status))
        {
            throw new InvalidOperationException(
                $"Invalid reservation status: {table.Status}.");
        }

        return new Reservation(
            table.ReservationCode,
            table.UserId,
            table.EventId,
            table.Quantity,
            (ReservationChannel)table.Channel,
            (ReservationStatus)table.Status);
    }
}