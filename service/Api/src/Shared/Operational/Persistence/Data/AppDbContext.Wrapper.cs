using System.Diagnostics.CodeAnalysis;

using Microsoft.EntityFrameworkCore;

namespace Shared.Operational.Persistence.Data;

/// <summary>
/// Defines the contract for the application database context.
/// Provides access to entity sets and change tracking.
/// </summary>
public interface IApplicationDbContext
{
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
