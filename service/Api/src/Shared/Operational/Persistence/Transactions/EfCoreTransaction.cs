using Microsoft.EntityFrameworkCore.Storage;

namespace Shared.Operational.Persistence.Transactions;

public sealed class EfCoreTransaction : IDatabaseTransaction
{
    private readonly IDbContextTransaction _transaction;

    public EfCoreTransaction(IDbContextTransaction transaction)
    {
        _transaction = transaction;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default) =>
        _transaction.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default) =>
        _transaction.RollbackAsync(cancellationToken);

    public async ValueTask DisposeAsync()
    {
        await _transaction.DisposeAsync();
    }
}
