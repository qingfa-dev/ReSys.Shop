using Shared.Application.Domain.Concerns.Publishable;

namespace Shared.UnitTests.Application.Domain.Concerns.Publishable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class PublishableBehaviorTests
{
    private sealed class TestPublishable : IPublishable
    {
        public bool IsPublished { get; set; }
        public DateTimeOffset? PublishedAtUtc { get; set; }
    }

    [Fact(DisplayName = "Publish should set IsPublished to true and PublishedAtUtc to current time")]
    public void Publish_ShouldSetIsPublishedAndTimestamp()
    {
        var entity = new TestPublishable();
        DateTimeOffset before = DateTimeOffset.UtcNow;

        PublishableBehavior.Publish(entity);

        entity.IsPublished.Should().BeTrue();
        entity.PublishedAtUtc.Should().NotBeNull();
        entity.PublishedAtUtc.Should().BeOnOrAfter(before);
        entity.PublishedAtUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Fact(DisplayName = "Publish should use provided timestamp when specified")]
    public void Publish_WithExplicitTime_ShouldUseProvidedTimestamp()
    {
        var entity = new TestPublishable();
        DateTimeOffset specifiedTime = DateTimeOffset.UtcNow.AddDays(-5);

        PublishableBehavior.Publish(entity, specifiedTime);

        entity.IsPublished.Should().BeTrue();
        entity.PublishedAtUtc.Should().Be(specifiedTime);
    }

    [Fact(DisplayName = "Unpublish should set IsPublished to false and PublishedAtUtc to null")]
    public void Unpublish_ShouldClearPublishedState()
    {
        var entity = new TestPublishable
        {
            IsPublished = true,
            PublishedAtUtc = DateTimeOffset.UtcNow
        };

        PublishableBehavior.Unpublish(entity);

        entity.IsPublished.Should().BeFalse();
        entity.PublishedAtUtc.Should().BeNull();
    }

    [Fact(DisplayName = "IsActive should return true when published and within schedule")]
    public void IsActive_PublishedAndInSchedule_ShouldReturnTrue()
    {
        var entity = new TestPublishable
        {
            IsPublished = true,
            PublishedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
        };

        PublishableBehavior.IsActive(entity).Should().BeTrue();
    }

    [Fact(DisplayName = "IsActive should return false when not published")]
    public void IsActive_NotPublished_ShouldReturnFalse()
    {
        var entity = new TestPublishable
        {
            IsPublished = false,
            PublishedAtUtc = DateTimeOffset.UtcNow.AddDays(-1)
        };

        PublishableBehavior.IsActive(entity).Should().BeFalse();
    }

    [Fact(DisplayName = "IsActive should return false when PublishedAtUtc is in the future")]
    public void IsActive_FuturePublishDate_ShouldReturnFalse()
    {
        var entity = new TestPublishable
        {
            IsPublished = true,
            PublishedAtUtc = DateTimeOffset.UtcNow.AddDays(1)
        };

        PublishableBehavior.IsActive(entity).Should().BeFalse();
    }

    [Fact(DisplayName = "IsActive should return false when PublishedAtUtc is null")]
    public void IsActive_NullPublishedAt_ShouldReturnFalse()
    {
        var entity = new TestPublishable
        {
            IsPublished = true,
            PublishedAtUtc = null
        };

        PublishableBehavior.IsActive(entity).Should().BeFalse();
    }

    [Fact(DisplayName = "IsActive with explicit atUtc should check against provided time")]
    public void IsActive_WithExplicitTime_ShouldCheckAgainstProvidedTime()
    {
        var entity = new TestPublishable
        {
            IsPublished = true,
            PublishedAtUtc = DateTimeOffset.UtcNow.AddDays(-5)
        };

        DateTimeOffset checkTime = DateTimeOffset.UtcNow.AddDays(-10);

        PublishableBehavior.IsActive(entity, checkTime).Should().BeFalse();
    }
}
