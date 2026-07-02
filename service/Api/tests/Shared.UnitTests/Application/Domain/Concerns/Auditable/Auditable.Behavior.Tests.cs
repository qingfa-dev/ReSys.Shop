using Shared.Application.Domain.Concerns.Auditable;

namespace Shared.UnitTests.Application.Domain.Concerns.Auditable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class AuditableBehaviorTests
{
    private sealed class TestAuditable : IAuditable
    {
        public DateTimeOffset CreatedAtUtc { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? ModifiedAtUtc { get; set; }
        public string? ModifiedBy { get; set; }
    }

    [Fact(DisplayName = "Create should set CreatedAtUtc and ModifiedAtUtc to current time")]
    public void Create_ShouldSetTimestamps()
    {
        var entity = new TestAuditable();
        DateTimeOffset before = DateTimeOffset.UtcNow;

        AuditableBehavior.Create(entity);

        entity.CreatedAtUtc.Should().BeOnOrAfter(before);
        entity.CreatedAtUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
        entity.ModifiedAtUtc.Should().BeOnOrAfter(before);
        entity.ModifiedAtUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Fact(DisplayName = "Create should use provided timestamp when specified")]
    public void Create_WithExplicitTime_ShouldUseProvidedTimestamp()
    {
        var entity = new TestAuditable();
        DateTimeOffset specifiedTime = DateTimeOffset.UtcNow.AddDays(-5);

        AuditableBehavior.Create(entity, specifiedTime);

        entity.CreatedAtUtc.Should().Be(specifiedTime);
        entity.ModifiedAtUtc.Should().Be(specifiedTime);
    }

    [Fact(DisplayName = "CreateBy should set timestamps and creator info")]
    public void CreateBy_ShouldSetTimestampsAndUser()
    {
        var entity = new TestAuditable();
        var by = "test-user";
        DateTimeOffset before = DateTimeOffset.UtcNow;

        AuditableBehavior.CreateBy(entity, by);

        entity.CreatedAtUtc.Should().BeOnOrAfter(before);
        entity.CreatedAtUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
        entity.CreatedBy.Should().Be(by);
        entity.ModifiedAtUtc.Should().BeOnOrAfter(before);
        entity.ModifiedAtUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
        entity.ModifiedBy.Should().Be(by);
    }

    [Fact(DisplayName = "CreateBy with explicit values should use provided values")]
    public void CreateBy_WithExplicitValues_ShouldUseProvidedValues()
    {
        var entity = new TestAuditable();
        var by = "test-user";
        DateTimeOffset specifiedTime = DateTimeOffset.UtcNow.AddDays(-5);

        AuditableBehavior.CreateBy(entity, by, specifiedTime);

        entity.CreatedAtUtc.Should().Be(specifiedTime);
        entity.CreatedBy.Should().Be(by);
        entity.ModifiedAtUtc.Should().Be(specifiedTime);
        entity.ModifiedBy.Should().Be(by);
    }

    [Fact(DisplayName = "Touch should update ModifiedAtUtc to current time")]
    public void Touch_ShouldUpdateModifiedAtUtc_WhenCalled()
    {
        var entity = new TestAuditable();
        DateTimeOffset before = DateTimeOffset.UtcNow;

        AuditableBehavior.Touch(entity);

        entity.ModifiedAtUtc.Should().NotBeNull();
        entity.ModifiedAtUtc.Should().BeOnOrAfter(before);
        entity.ModifiedAtUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
    }

    [Fact(DisplayName = "Touch should update ModifiedAtUtc with specified time")]
    public void Touch_ShouldUpdateModifiedAtUtcWithSpecifiedTime_WhenProvided()
    {
        var entity = new TestAuditable();
        DateTimeOffset specifiedTime = DateTimeOffset.UtcNow.AddDays(-1);

        AuditableBehavior.Touch(entity, specifiedTime);

        entity.ModifiedAtUtc.Should().Be(specifiedTime);
    }

    [Fact(DisplayName = "TouchBy should update ModifiedAtUtc and ModifiedBy")]
    public void TouchBy_ShouldUpdateModifiedAtAndModifiedBy_WhenCalled()
    {
        var entity = new TestAuditable();
        var user = "test-user";
        DateTimeOffset before = DateTimeOffset.UtcNow;

        AuditableBehavior.TouchBy(entity, user, null);

        entity.ModifiedAtUtc.Should().NotBeNull();
        entity.ModifiedAtUtc.Should().BeOnOrAfter(before);
        entity.ModifiedBy.Should().Be(user);
    }

    [Fact(DisplayName = "TouchBy should update with specified user and time")]
    public void TouchBy_ShouldUpdateWithSpecifiedValues_WhenProvided()
    {
        var entity = new TestAuditable();
        var user = "test-user";
        DateTimeOffset specifiedTime = DateTimeOffset.UtcNow.AddDays(-1);

        AuditableBehavior.TouchBy(entity, user, specifiedTime);

        entity.ModifiedAtUtc.Should().Be(specifiedTime);
        entity.ModifiedBy.Should().Be(user);
    }
}
