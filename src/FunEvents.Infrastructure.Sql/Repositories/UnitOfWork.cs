namespace FunEvents.Infrastructure.Sql.Repositories;

internal sealed class UnitOfWork(FunEventsDbContext context) : IUnitOfWork
{
    private readonly FunEventsDbContext _context = context ?? throw new ArgumentNullException(nameof(context));
    private IDbContextTransaction? _transaction;

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        _transaction ??= await _context.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            var result = await _context.SaveChangesAsync(cancellationToken);

            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();

            _transaction = null;

            return result;
        }
        catch
        {
            await RollBackAsync(cancellationToken);
            throw;
        }
    }

    public async Task RollBackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is null)
        {
            _context.ChangeTracker.Clear();
            return;
        }

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();

        _transaction = null;

        _context.ChangeTracker.Clear();
    }
}