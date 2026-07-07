using FluentValidation;using FluentValidation.TestHelper;
using Module.Ordering.Domain.Adjustments;
namespace Module.UnitTests.Ordering.Domain.Adjustments;
[Trait("Category","Unit")][Trait("Module","Ordering")][Trait("Feature","Validators")][Trait("Entity","Adjustment")]
public class AdjustmentValidationLabelTests
{
    sealed class M{public string? L{get;set;}}sealed class V:AbstractValidator<M>{public V()=>RuleFor(x=>x.L).ApplyLabelRules();}
    [Theory][InlineData("")][InlineData(" ")][InlineData(null)]public void WhenEmpty_ShouldHaveError(string? l)=>new V().TestValidate(new M{L=l}).ShouldHaveValidationErrorFor(x=>x.L);
    [Fact]public void WhenValid_ShouldPass()=>new V().TestValidate(new M{L="Discount"}).ShouldNotHaveValidationErrorFor(x=>x.L);
}
