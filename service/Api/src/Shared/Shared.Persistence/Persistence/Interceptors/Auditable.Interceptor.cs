using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Shared.Application.Domain.Concerns.Auditable;
using Shared.Application.Systems.SystemDateTimes;

namespace Shared.Operational.Persistence.Interceptors;

/// <summary>
/// Interceptor that automatically sets auditing properties (CreatedAtUtc, CreatedBy, ModifiedAtUtc, ModifiedBy).
/// Targets entities that implement the <see cref="IAuditable"/> interface.
/// </summary>
public class AuditableInterceptor(
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

        // Map: Resolve all auditable entries from the change tracker for processing
        IEnumerable<EntityEntry<IAuditable>> entries = eventData.Context.ChangeTracker.Entries<IAuditable>();

        // Compute: Determine current user identity for auditing (username > userid > empty)
        string by = currentUser.UserName ?? currentUser.UserId ?? string.Empty;

        // Update: Assign audit metadata based on entity state (Added or Modified)
        foreach (EntityEntry<IAuditable> entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                // Update: Set creation metadata for new entity
                AuditableBehavior.CreateBy(
                    entity: entry.Entity,
                    by: by,
                    atUtc: systemDateTime.UtcNow);
            }
            else if (entry.State == EntityState.Modified)
            {
                // Update: Set modification metadata for existing entity
                AuditableBehavior.TouchBy(
                    entity: entry.Entity,
                    by: by,
                    atUtc: systemDateTime.UtcNow);
            }
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
