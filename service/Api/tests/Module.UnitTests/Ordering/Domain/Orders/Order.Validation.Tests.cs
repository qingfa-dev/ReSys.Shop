using Module.Ordering.Domain.Orders;
namespace Module.UnitTests.Ordering.Domain.Orders;
[Trait("Category","Unit")][Trait("Module","Ordering")][Trait("Feature","Validators")][Trait("Entity","Order")]
public class OrderValidationCheckoutStateTests
{
    private sealed class M { public CheckoutState State { get; set; } }
    private sealed class V : AbstractValidator<M> { public V() => RuleFor(x=>x.State).ApplyCheckoutStateTransitionRules(); }
    [Fact] public void ApplyCheckoutStateTransitionRules_WhenValid_ShouldPass() =>
        new V().TestValidate(new M{State=CheckoutState.Confirm}).ShouldNotHaveValidationErrorFor(x=>x.State);
}
