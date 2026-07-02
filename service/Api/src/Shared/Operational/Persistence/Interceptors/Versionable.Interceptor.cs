using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Shared.Application.Domain.Concerns.Versionable;

namespace Shared.Operational.Persistence.Interceptors;

/// <summary>
/// Interceptor that automatically increments the version property for optimistic concurrency.
/// Targets entities that implement the <see cref="IVersionable"/> interface.
/// </summary>
public class VersionableInterceptor : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        // Contract: pre=eventData!=null, post=true

        // Guard: Ensure database context is available to prevent null reference
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, cancellationToken);

        // Map: Resolve all versionable entries from the change tracker for processing
        IEnumerable<EntityEntry<IVersionable>> entries = eventData.Context.ChangeTracker.Entries<IVersionable>();

        foreach (EntityEntry<IVersionable> entry in entries)
        {
            if (entry.State == EntityState.Modified)
            {
                // Update: Increment version number to enforce optimistic concurrency
                entry.Entity.Version++;
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
