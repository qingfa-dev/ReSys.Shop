using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Data;

namespace Shared.Operational.Persistence.Seeders;

/// <summary>
/// Provides a base implementation for data seeders with access to ApplicationDbContext.
/// </summary>
/// <param name="context">The application database context.</param>
public abstract class AbstractDataSeeder(IApplicationDbContext context) : IDataSeeder
{
    /// <summary>
    /// Gets the application database context.
    /// </summary>
    protected IApplicationDbContext Context { get; } = context;

    /// <inheritdoc />
    public abstract int Order { get; }

    /// <inheritdoc />
    public abstract Task<Result> SeedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Verifies if a table already contains data for the specified entity type.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity to check.</typeparam>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True if the table contains one or more records; otherwise, false.</returns>
    protected async Task<bool> HasDataAsync<TEntity>(CancellationToken cancellationToken) where TEntity : class
    {
        // Contract: pre=cancellationToken!=null

        // Check: Verify if any records exist for the specified entity type
        return await Context.Set<TEntity>().AnyAsync(cancellationToken);
    }
}
