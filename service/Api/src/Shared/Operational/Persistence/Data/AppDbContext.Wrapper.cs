using System.Data;

using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Transactions;

namespace Shared.Operational.Persistence.Data;

/// <summary>
/// Defines the contract for the application database context.
/// Provides access to entity sets, change tracking, and transaction support.
/// </summary>
public interface IApplicationDbContext
{
    /// <summary>
    /// Gets whether the current database provider supports transactions.
    /// Returns false for in-memory provider, true for relational providers.
    /// </summary>
    bool SupportsTransactions { get; }

    /// <summary>
    /// Begins a database transaction at the specified isolation level.
    /// Returns a NoOpTransaction when the provider does not support transactions.
    /// </summary>
    /// <param name="isolationLevel">The isolation level for the transaction.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>An <see cref="IDatabaseTransaction"/> instance.</returns>
    Task<IDatabaseTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a DbSet for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity for which a set should be returned.</typeparam>
    /// <returns>A set for the given entity type.</returns>
    [SuppressMessage("Naming", "CA1716:Identifiers should not match keywords", Justification = "Matches EF Core DbContext.Set<TEntity>()")]
    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    /// <summary>
    /// Saves all changes made in this context to the database asynchronously.
    /// </summary>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
    /// <returns>A task that represents the asynchronous save operation. The task result contains the number of state entries written to the database.</returns>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
