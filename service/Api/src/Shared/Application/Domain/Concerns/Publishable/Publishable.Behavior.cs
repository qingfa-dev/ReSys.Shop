namespace Shared.Application.Domain.Concerns.Publishable;

public static class PublishableBehavior
{
    public static void Publish(IPublishable entity, DateTimeOffset? atUtc = null)
    {
        DateTimeOffset now = atUtc ?? DateTimeOffset.UtcNow;
        entity.IsPublished = true;
        entity.PublishedAtUtc = now;
    }

    public static void Unpublish(IPublishable entity)
    {
        entity.IsPublished = false;
        entity.PublishedAtUtc = null;
    }

    public static bool IsActive(IPublishable entity, DateTimeOffset? atUtc = null)
    {
        DateTimeOffset atUtcValue = atUtc ?? DateTimeOffset.UtcNow;
        return entity.IsPublished && entity.PublishedAtUtc.HasValue && entity.PublishedAtUtc.Value <= atUtcValue;
    }
}
