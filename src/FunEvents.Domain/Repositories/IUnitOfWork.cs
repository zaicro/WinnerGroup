public interface IUnitOfWork
{
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task<int> CommitAsync(CancellationToken cancellationToken = default);

    Task RollBackAsync(CancellationToken cancellationToken = default);
}
