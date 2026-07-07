using FluentAssertions;
using Module.Promotions.Domain.Promotions;

namespace Module.UnitTests.Promotions.Domain.Promotions;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "PromotionExtensions")]
public class PromotionExtensionTests
{
    [Fact(DisplayName = "Create: Should set properties when name provided")]
    public void Create_ShouldSetProperties_WhenNameProvided()
    {
        var result = PromotionExtensions.Create("Summer Sale");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Summer Sale");
        result.Value.Id.Should().NotBe(Guid.Empty);
        result.Value.Kind.Should().Be(PromotionKind.CouponCode);
        result.Value.Active.Should().BeTrue();
        result.Value.MatchPolicy.Should().Be(MatchPolicy.All);
        result.Value.Position.Should().Be(0);
    }

    [Fact(DisplayName = "Create: Should use explicit id when provided")]
    public void Create_ShouldUseExplicitId_WhenProvided()
    {
        var id = Guid.NewGuid();
        var result = PromotionExtensions.Create("Test", id: id);

        result.Value.Id.Should().Be(id);
    }

    [Fact(DisplayName = "Update: Should update only provided fields")]
    public void Update_ShouldUpdateOnlyProvidedFields()
    {
        var result = PromotionExtensions.Create("Original", code: "OLD", description: "Old desc");
        var promotion = result.Value;

        promotion.Update(name: "Updated").IsSuccess.Should().BeTrue();

        promotion.Name.Should().Be("Updated");
        promotion.Code.Should().Be("OLD");
        promotion.Description.Should().Be("Old desc");
    }

    [Fact(DisplayName = "Update: Should make no changes when all null")]
    public void Update_ShouldMakeNoChanges_WhenAllNull()
    {
        var result = PromotionExtensions.Create("Original", code: "CODE");
        var promotion = result.Value;

        promotion.Update().IsSuccess.Should().BeTrue();

        promotion.Name.Should().Be("Original");
        promotion.Code.Should().Be("CODE");
    }

    [Fact(DisplayName = "Activate: Should set active when inactive")]
    public void Activate_ShouldSetActive_WhenInactive()
    {
        var result = PromotionExtensions.Create("Test", active: false);
        var promotion = result.Value;

        var actResult = promotion.Activate();

        actResult.IsSuccess.Should().BeTrue();
        promotion.Active.Should().BeTrue();
    }

    [Fact(DisplayName = "Activate: Should return failure when already active")]
    public void Activate_ShouldReturnFailure_WhenAlreadyActive()
    {
        var result = PromotionExtensions.Create("Test", active: true);
        var promotion = result.Value;

        var actResult = promotion.Activate();

        actResult.IsSuccess.Should().BeFalse();
        actResult.Failures.Should().Contain(f => f.Code == "Promotion.AlreadyActive");
    }

    [Fact(DisplayName = "Deactivate: Should set inactive when active")]
    public void Deactivate_ShouldSetInactive_WhenActive()
    {
        var result = PromotionExtensions.Create("Test", active: true);
        var promotion = result.Value;

        var deactResult = promotion.Deactivate();

        deactResult.IsSuccess.Should().BeTrue();
        promotion.Active.Should().BeFalse();
    }

    [Fact(DisplayName = "Deactivate: Should return failure when already inactive")]
    public void Deactivate_ShouldReturnFailure_WhenAlreadyInactive()
    {
        var result = PromotionExtensions.Create("Test", active: false);
        var promotion = result.Value;

        var deactResult = promotion.Deactivate();

        deactResult.IsSuccess.Should().BeFalse();
        deactResult.Failures.Should().Contain(f => f.Code == "Promotion.AlreadyInactive");
    }

    [Fact(DisplayName = "Delete: Should mark soft delete")]
    public void Delete_ShouldMarkSoftDelete()
    {
        var result = PromotionExtensions.Create("Test");
        var promotion = result.Value;

        var delResult = promotion.Delete("admin");

        delResult.IsSuccess.Should().BeTrue();
        promotion.IsDeleted.Should().BeTrue();
        promotion.DeletedAtUtc.Should().NotBeNull();
        promotion.DeletedBy.Should().Be("admin");
    }

    [Fact(DisplayName = "Delete: Should be idempotent when already deleted")]
    public void Delete_ShouldBeIdempotent_WhenAlreadyDeleted()
    {
        var result = PromotionExtensions.Create("Test");
        var promotion = result.Value;

        promotion.Delete("admin");
        var deletedAt = promotion.DeletedAtUtc;

        var delResult = promotion.Delete("admin");

        delResult.IsSuccess.Should().BeTrue();
        promotion.DeletedAtUtc.Should().Be(deletedAt);
    }

    [Fact(DisplayName = "IsActive: Should return true when active and not expired")]
    public void IsActive_ShouldReturnTrue_WhenActiveAndNotExpired()
    {
        var result = PromotionExtensions.Create("Test", active: true,
            startsAtUtc: DateTimeOffset.UtcNow.AddDays(-1),
            expiresAtUtc: DateTimeOffset.UtcNow.AddDays(1));

        result.Value.IsActive().Should().BeTrue();
    }

    [Fact(DisplayName = "IsActive: Should return false when inactive")]
    public void IsActive_ShouldReturnFalse_WhenInactive()
    {
        var result = PromotionExtensions.Create("Test", active: false);

        result.Value.IsActive().Should().BeFalse();
    }

    [Fact(DisplayName = "IsActive: Should return false when deleted")]
    public void IsActive_ShouldReturnFalse_WhenDeleted()
    {
        var result = PromotionExtensions.Create("Test", active: true);
        var promotion = result.Value;
        promotion.Delete("admin");

        promotion.IsActive().Should().BeFalse();
    }

    [Fact(DisplayName = "IsActive: Should return false when before start date")]
    public void IsActive_ShouldReturnFalse_WhenBeforeStartDate()
    {
        var result = PromotionExtensions.Create("Test", active: true,
            startsAtUtc: DateTimeOffset.UtcNow.AddDays(1));

        result.Value.IsActive().Should().BeFalse();
    }

    [Fact(DisplayName = "IsActive: Should return false when after expiry")]
    public void IsActive_ShouldReturnFalse_WhenAfterExpiry()
    {
        var result = PromotionExtensions.Create("Test", active: true,
            expiresAtUtc: DateTimeOffset.UtcNow.AddDays(-1));

        result.Value.IsActive().Should().BeFalse();
    }

    [Fact(DisplayName = "IsActive: Should handle null date boundaries")]
    public void IsActive_ShouldHandleNullDateBoundaries()
    {
        var noStart = PromotionExtensions.Create("Test", active: true, expiresAtUtc: DateTimeOffset.UtcNow.AddDays(1));
        noStart.Value.IsActive().Should().BeTrue();

        var noEnd = PromotionExtensions.Create("Test", active: true, startsAtUtc: DateTimeOffset.UtcNow.AddDays(-1));
        noEnd.Value.IsActive().Should().BeTrue();

        var bothNull = PromotionExtensions.Create("Test", active: true);
        bothNull.Value.IsActive().Should().BeTrue();
    }
}
