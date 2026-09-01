namespace FunEvents.Infrastructure.Sql.Repositories;

internal sealed class EventRepository(FunEventsDbContext context) : IEventRepository
{
    public async Task AddAsync(Event @event, CancellationToken cancellationToken = default)
    {
        var table = new TbEvent
        {
            Code = @event.Code,
            Name = @event.Name,
            EventDate = @event.EventDate,
            Capacity = @event.Capacity,
            AvailableCapacity = @event.AvailableCapacity,
            Status = 1
        };

        await context.Set<TbEvent>()
            .AddAsync(table, cancellationToken);
    }

    public async Task<bool> ExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await context.Set<TbEvent>()
            .AnyAsync(
                x => x.Code == code &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tables = await context.Set<TbEvent>()
            .ToListAsync(cancellationToken);

        return [.. tables.Select(MapToDomain)];
    }

    public async Task<Event?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var table = await context.Set<TbEvent>()
            .FirstOrDefaultAsync(
                x => x.Code == code &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        return table is null
            ? null
            : MapToDomain(table);
    }

    public async Task UpdateAsync(Event @event, CancellationToken cancellationToken = default)
    {
        var table = await context.Set<TbEvent>()
            .FirstOrDefaultAsync(
                x => x.Code == @event.Code &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        if (table is null)
            return;

        table.Name = @event.Name;
        table.EventDate = @event.EventDate;
        table.Capacity = @event.Capacity;

        context.Set<TbEvent>().Update(table);
    }

    private static Event MapToDomain(TbEvent table)
    {
        return new Event(
            table.Id,
            table.Code,
            table.Name,
            table.EventDate,
            table.Capacity,
            table.AvailableCapacity);
    }
}