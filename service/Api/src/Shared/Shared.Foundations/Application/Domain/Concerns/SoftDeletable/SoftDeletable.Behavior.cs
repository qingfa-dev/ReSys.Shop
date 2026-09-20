namespace Shared.Application.Domain.Concerns.SoftDeletable;

/// <summary>
/// Provides shared behaviors for soft-deletable entities.
/// </summary>
// Contract: pre=entity!=null, post=Restore clears all deletion tracking (IsDeleted=false, DeletedAtUtc=null, DeletedBy=null)
public static class SoftDeletableBehavior
{
    // Delete: Mark entity as deleted with timestamp
    public static void Delete(ISoftDeletable entity, DateTimeOffset? atUtc = null)
    {
        entity.IsDeleted = true;
        entity.DeletedAtUtc = atUtc ?? DateTimeOffset.UtcNow;
    }

    // DeleteBy: Mark entity as deleted with user context
    public static void DeleteBy(ISoftDeletable entity, string by, DateTimeOffset? atUtc = null)
    {
        entity.IsDeleted = true;
        entity.DeletedBy = by;
        entity.DeletedAtUtc = atUtc ?? DateTimeOffset.UtcNow;
    }

    // Restore: Clear deletion tracking entirely
    public static void Restore(ISoftDeletable entity)
    {
        entity.IsDeleted = false;
        entity.DeletedAtUtc = null;
        entity.DeletedBy = null;
    }
}
