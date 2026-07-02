using Shared.Application.Domain.Concerns.SoftDeletable;

namespace Shared.UnitTests.Application.Domain.Concerns.SoftDeletable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class SoftDeletableBehaviorTests
{
    private sealed class TestSoftDeletable : ISoftDeletable
    {
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAtUtc { get; set; }
        public string? DeletedBy { get; set; }
    }

    [Fact(DisplayName = "Delete should set IsDeleted to true and DeletedAtUtc to current time")]
    public void Delete_ShouldSetIsDeletedAndTimestamp()
    {
        var entity = new TestSoftDeletable();
        DateTimeOffset before = DateTimeOffset.UtcNow;

        SoftDeletableBehavior.Delete(entity);

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAtUtc.Should().NotBeNull();
        entity.DeletedAtUtc.Should().BeOnOrAfter(before);
        entity.DeletedAtUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Fact(DisplayName = "Delete should use provided timestamp when specified")]
    public void Delete_WithExplicitTime_ShouldUseProvidedTimestamp()
    {
        var entity = new TestSoftDeletable();
        DateTimeOffset specifiedTime = DateTimeOffset.UtcNow.AddDays(-5);

        SoftDeletableBehavior.Delete(entity, specifiedTime);

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedAtUtc.Should().Be(specifiedTime);
    }

    [Fact(DisplayName = "DeleteBy should set IsDeleted, DeletedBy, and DeletedAtUtc")]
    public void DeleteBy_ShouldSetAllDeletionFields()
    {
        var entity = new TestSoftDeletable();
        var by = "test-user";
        DateTimeOffset before = DateTimeOffset.UtcNow;

        SoftDeletableBehavior.DeleteBy(entity, by);

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedBy.Should().Be(by);
        entity.DeletedAtUtc.Should().NotBeNull();
        entity.DeletedAtUtc.Should().BeOnOrAfter(before);
        entity.DeletedAtUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Fact(DisplayName = "DeleteBy should use provided timestamp when specified")]
    public void DeleteBy_WithExplicitTime_ShouldUseProvidedTimestamp()
    {
        var entity = new TestSoftDeletable();
        var by = "test-user";
        DateTimeOffset specifiedTime = DateTimeOffset.UtcNow.AddDays(-5);

        SoftDeletableBehavior.DeleteBy(entity, by, specifiedTime);

        entity.IsDeleted.Should().BeTrue();
        entity.DeletedBy.Should().Be(by);
        entity.DeletedAtUtc.Should().Be(specifiedTime);
    }

    [Fact(DisplayName = "Restore should clear all deletion tracking fields")]
    public void Restore_ShouldClearDeletionFields()
    {
        var entity = new TestSoftDeletable
        {
            IsDeleted = true,
            DeletedAtUtc = DateTimeOffset.UtcNow,
            DeletedBy = "test-user"
        };

        SoftDeletableBehavior.Restore(entity);

        entity.IsDeleted.Should().BeFalse();
        entity.DeletedAtUtc.Should().BeNull();
        entity.DeletedBy.Should().BeNull();
    }
}
