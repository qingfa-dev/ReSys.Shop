using Module.Ordering.Domain.Adjustments;

namespace Module.UnitTests.Ordering.Domain.Adjustments;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Entity", "Adjustment")]
public class AdjustmentMethodTests
{
    [Fact(DisplayName = "Create: valid params returns success with correct properties")]
    public void Create_ShouldReturnAdjustment()
    {
        var adjustableId = Guid.NewGuid();
        var sourceId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var result = AdjustmentMethod.Create(
            "Shipping Fee", 10.5m, adjustableId, "Order", sourceId, "Shipping", orderId);

        result.IsSuccess.Should().BeTrue();
        var a = result.Value;
        a.Label.Should().Be("Shipping Fee");
        a.Amount.Should().Be(10.5m);
        a.DisplayAmount.Should().Be("10.50");
        a.State.Should().Be("open");
        a.Eligible.Should().BeTrue();
        a.Mandatory.Should().BeFalse();
        a.AdjustableId.Should().Be(adjustableId);
        a.SourceId.Should().Be(sourceId);
        a.OrderId.Should().Be(orderId);
        a.CreatedAtUtc.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact(DisplayName = "Close: when open transitions to closed")]
    public void Close_WhenOpen_ShouldSucceed()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "Shipping", Guid.NewGuid()).Value;

        var result = a.Close();

        result.IsSuccess.Should().BeTrue();
        a.State.Should().Be("closed");
    }

    [Fact(DisplayName = "Close: when closed returns AlreadyClosed error")]
    public void Close_WhenClosed_ShouldFail()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "Shipping", Guid.NewGuid()).Value;
        a.Close();

        var result = a.Close();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(AdjustmentResult.Errors.AlreadyClosed);
    }

    [Fact(DisplayName = "Open: when closed transitions to open")]
    public void Open_WhenClosed_ShouldSucceed()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "Shipping", Guid.NewGuid()).Value;
        a.Close();

        var result = a.Open();

        result.IsSuccess.Should().BeTrue();
        a.State.Should().Be("open");
    }

    [Fact(DisplayName = "Open: when open returns AlreadyOpen error")]
    public void Open_WhenOpen_ShouldFail()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "Shipping", Guid.NewGuid()).Value;

        var result = a.Open();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(AdjustmentResult.Errors.AlreadyOpen);
    }

    [Fact(DisplayName = "MarkIneligible: when eligible sets Eligible to false")]
    public void MarkIneligible_WhenEligible_ShouldSetIneligible()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "Shipping", Guid.NewGuid()).Value;

        var result = a.MarkIneligible();

        result.IsSuccess.Should().BeTrue();
        a.Eligible.Should().BeFalse();
    }

    [Fact(DisplayName = "MarkIneligible: when already ineligible is no-op")]
    public void MarkIneligible_WhenAlreadyIneligible_ShouldBeNoOp()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "Shipping", Guid.NewGuid()).Value;
        a.MarkIneligible();

        var result = a.MarkIneligible();

        result.IsSuccess.Should().BeTrue();
        a.Eligible.Should().BeFalse();
    }

    [Fact(DisplayName = "MarkEligible: when ineligible sets Eligible to true")]
    public void MarkEligible_WhenIneligible_ShouldSetEligible()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "Shipping", Guid.NewGuid()).Value;
        a.MarkIneligible();

        var result = a.MarkEligible();

        result.IsSuccess.Should().BeTrue();
        a.Eligible.Should().BeTrue();
    }

    [Fact(DisplayName = "MarkEligible: when already eligible is no-op")]
    public void MarkEligible_WhenAlreadyEligible_ShouldBeNoOp()
    {
        var a = AdjustmentMethod.Create("X", 5, Guid.NewGuid(), "Order", Guid.NewGuid(), "Shipping", Guid.NewGuid()).Value;

        var result = a.MarkEligible();

        result.IsSuccess.Should().BeTrue();
        a.Eligible.Should().BeTrue();
    }
}
