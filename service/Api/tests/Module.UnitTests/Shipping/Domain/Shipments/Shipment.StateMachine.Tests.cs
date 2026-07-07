using Module.Shipping.Domain.Shipments;
namespace Module.UnitTests.Shipping.Domain.Shipments;
[Trait("Category","Unit")][Trait("Module","Shipping")][Trait("Entity","Shipment")]
public class ShipmentStateMachineTests
{
    [Fact]
    public void Pend_FromReady_ShouldTransitionToPending()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        var r = s.Pend();
        r.IsSuccess.Should().BeTrue();
        s.State.Should().Be(ShipmentState.Pending);
    }
    [Fact]
    public void Pend_FromPending_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var r = s.Pend();
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Pend_FromShipped_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        s.Ship("TRACK");
        var r = s.Pend();
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Pend_FromCanceled_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Cancel();
        var r = s.Pend();
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Resume_FromCanceled_ShouldTransitionToPending()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Cancel();
        var r = s.Resume();
        r.IsSuccess.Should().BeTrue();
        s.State.Should().Be(ShipmentState.Pending);
    }
    [Fact]
    public void Resume_FromPending_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        var r = s.Resume();
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Resume_FromReady_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        var r = s.Resume();
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void Resume_FromShipped_ShouldFail()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        s.Ship("TRACK");
        var r = s.Resume();
        r.IsFailure.Should().BeTrue();
    }
    [Fact]
    public void DetermineState_WhenCanceled_ReturnsCanceled()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.State = ShipmentState.Canceled;
        s.DetermineState().Should().Be("canceled");
    }
    [Fact]
    public void DetermineState_WhenPending_ReturnsPending()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.DetermineState().Should().Be("pending");
    }
    [Fact]
    public void IsBackordered_ShouldReturnFalse()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.IsBackordered().Should().BeFalse();
    }
    [Fact]
    public void IsTracked_WithTracking_ShouldReturnTrue()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Tracking = "TRACK123";
        s.IsTracked().Should().BeTrue();
    }
    [Fact]
    public void IsTracked_WithoutTracking_ShouldReturnFalse()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.IsTracked().Should().BeFalse();
    }
    [Fact]
    public void IsShippable_WhenReadyAndTracked_ShouldReturnTrue()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.Ready();
        s.Tracking = "TRACK123";
        s.IsShippable().Should().BeTrue();
    }
    [Fact]
    public void IsShippable_WhenPending_ShouldReturnFalse()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.IsShippable().Should().BeFalse();
    }
    [Fact]
    public void IsDigital_ShouldReturnFalse()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.IsDigital().Should().BeFalse();
    }
    [Fact]
    public void GetTaxTotal_ShouldSumIncludedAndAdditional()
    {
        var s = ShipmentExtensions.Create(Guid.NewGuid(), Guid.NewGuid()).Value;
        s.IncludedTaxTotal = 1.5m;
        s.AdditionalTaxTotal = 2.5m;
        s.GetTaxTotal().Should().Be(4.0m);
    }
}
