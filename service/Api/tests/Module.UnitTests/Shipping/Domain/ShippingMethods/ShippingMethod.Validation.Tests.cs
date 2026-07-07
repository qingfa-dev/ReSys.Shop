using Module.Shipping.Domain.ShippingMethods;
namespace Module.UnitTests.Shipping.Domain.ShippingMethods;
[Trait("Category","Unit")][Trait("Module","Shipping")][Trait("Entity","ShippingMethod")]
public class ShippingMethodValidationTests
{
    sealed class M{public string? N{get;set;}public string? C{get;set;}public string? T{get;set;}}
    sealed class V:AbstractValidator<M>{public V(){RuleFor(x=>x.N).ApplyNameRules();RuleFor(x=>x.C).ApplyCodeRules();RuleFor(x=>x.T).ApplyTrackingUrlRules();}}
    [Fact]public void ApplyNameRules_Empty_ShouldHaveError()=>new V().TestValidate(new M{N="",C="x",T="x"}).ShouldHaveValidationErrorFor(x=>x.N);
    [Fact]public void ApplyNameRules_Valid_ShouldPass()=>new V().TestValidate(new M{N="Standard",C="x",T="x"}).ShouldNotHaveValidationErrorFor(x=>x.N);
    [Fact]public void ApplyCodeRules_Valid_ShouldPass()=>new V().TestValidate(new M{N="x",C="CODE",T="x"}).ShouldNotHaveValidationErrorFor(x=>x.C);
    [Fact]public void ApplyTrackingUrlRules_Valid_ShouldPass()=>new V().TestValidate(new M{N="x",C="x",T="https://track.com/:tracking"}).ShouldNotHaveValidationErrorFor(x=>x.T);
}
