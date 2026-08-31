namespace FunEvents.Infrastructure.Sql.Repositories;

internal sealed class UserRepository(FunEventsDbContext context) : IUserRepository
{
    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
    {
        var table = new TbUser
        {
            Username = user.Username,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            PasswordHash = user.PasswordHash
        };

        await context.Set<TbUser>()
            .AddAsync(table, cancellationToken);
    }

    public async Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        return await context.Set<TbUser>()
            .AnyAsync(
                x => x.Username == username &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        return await context.Set<TbUser>()
            .AnyAsync(
                x => x.Email == email &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<User>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tables = await context.Set<TbUser>()
            .ToListAsync(cancellationToken);

        return [.. tables.Select(MapToDomain)];
    }

    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var table = await context.Set<TbUser>()
            .FirstOrDefaultAsync(
                x => x.Username == username &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        return table is null
            ? null
            : MapToDomain(table);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var table = await context.Set<TbUser>()
            .FirstOrDefaultAsync(
                x => x.Email == email &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        return table is null
            ? null
            : MapToDomain(table);
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        var table = await context.Set<TbUser>()
            .FirstOrDefaultAsync(
                x => x.Username == user.Username &&
                     x.IsActive &&
                     !x.IsDeleted,
                cancellationToken);

        if (table is null)
            return;

        table.Name = user.Name;
        table.Phone = user.Phone;

        context.Set<TbUser>().Update(table);
    }

    private static User MapToDomain(TbUser table)
    {
        return new User(
            table.Id,
            table.Username,
            table.Name,
            table.Email,
            table.Phone,
            table.PasswordHash);
    }
}