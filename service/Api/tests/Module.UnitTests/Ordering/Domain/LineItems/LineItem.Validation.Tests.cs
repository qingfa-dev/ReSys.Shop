using Module.Ordering.Domain.LineItems;
namespace Module.UnitTests.Ordering.Domain.LineItems;
[Trait("Category","Unit")][Trait("Module","Ordering")][Trait("Feature","Validators")][Trait("Entity","LineItem")]
public class LineItemValidationQuantityTests
{
    sealed class M{public int Q{get;set;}}sealed class V:AbstractValidator<M>{public V()=>RuleFor(x=>x.Q).ApplyQuantityRules();}
    [Fact]public void WhenZero_ShouldHaveError()=>new V().TestValidate(new M{Q=0}).ShouldHaveValidationErrorFor(x=>x.Q).WithErrorCode(LineItemResult.Errors.QuantityExceedsMax.Code);
    [Fact]public void WhenOne_ShouldPass()=>new V().TestValidate(new M{Q=1}).ShouldNotHaveValidationErrorFor(x=>x.Q);
}
