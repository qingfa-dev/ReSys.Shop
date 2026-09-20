namespace Shared.Application.Domain.Concerns.Auditable;

/// <summary>
/// Provides shared behaviors for auditable entities.
/// </summary>
public static class AuditableBehavior
{
    public static void Create(IAuditable entity, DateTimeOffset? atUtc = null)
    {
        DateTimeOffset now = atUtc ?? DateTimeOffset.UtcNow;
        entity.CreatedAtUtc = now;
        entity.ModifiedAtUtc = now;
    }

    public static void CreateBy(IAuditable entity, string by, DateTimeOffset? atUtc = null)
    {
        DateTimeOffset now = atUtc ?? DateTimeOffset.UtcNow;
        entity.CreatedAtUtc = now;
        entity.CreatedBy = by;
        entity.ModifiedAtUtc = now;
        entity.ModifiedBy = by;
    }

    public static void Touch(IAuditable entity, DateTimeOffset? atUtc = null)
    {
        entity.ModifiedAtUtc = atUtc ?? DateTimeOffset.UtcNow;
    }

    public static void TouchBy(IAuditable entity, string by, DateTimeOffset? atUtc)
    {
        entity.ModifiedAtUtc = atUtc ?? DateTimeOffset.UtcNow;
        entity.ModifiedBy = by;
    }
}
