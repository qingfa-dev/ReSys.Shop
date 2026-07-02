using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Shared.Application.Domain.Concerns.SoftDeletable;
using Shared.Application.Systems.SystemDateTimes;

namespace Shared.Operational.Persistence.Interceptors;

/// <summary>
/// Interceptor that automatically sets DeletedAt and DeletedBy when soft-deleting entities.
/// Monitors the IsDeleted property for state changes.
/// </summary>
public class SoftDeletableInterceptor(
    ICurrentUser currentUser,
    ISystemDateTime systemDateTime) : SaveChangesInterceptor
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

        // Map: Resolve all soft-deletable entries from the change tracker for processing
        IEnumerable<EntityEntry<ISoftDeletable>> entries = eventData.Context.ChangeTracker.Entries<ISoftDeletable>();

        // Compute: Determine current user identity for auditing (username > userid > empty)
        string by = currentUser.UserName ?? currentUser.UserId ?? string.Empty;

        // Update: Assign deletion metadata based on entity state transitions
        foreach (EntityEntry<ISoftDeletable> entry in entries)
        {
            if (entry.State != EntityState.Modified) continue;

            // Check: Detect if IsDeleted property was toggled in this transaction
            PropertyEntry<ISoftDeletable, bool> isDeletedProperty = entry.Property(x => x.IsDeleted);

            if (isDeletedProperty.IsModified)
            {
                if (entry.Entity.IsDeleted)
                {
                    // Update: Set deletion metadata for Active→Deleted transition
                    SoftDeletableBehavior.DeleteBy(
                        entity: entry.Entity,
                        by: by,
                        atUtc: systemDateTime.UtcNow);
                }
                else
                {
                    // Recover: Clear deletion metadata for Deleted→Active transition (restore)
                    SoftDeletableBehavior.Restore(entry.Entity);
                }
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
