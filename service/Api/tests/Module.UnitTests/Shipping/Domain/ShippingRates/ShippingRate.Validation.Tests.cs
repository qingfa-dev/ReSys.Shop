using Module.Shipping.Domain.ShippingRates;
namespace Module.UnitTests.Shipping.Domain.ShippingRates;
[Trait("Category","Unit")][Trait("Module","Shipping")][Trait("Entity","ShippingRate")]
public class ShippingRateValidationTests
{
    sealed class M{public string N{get;set;}=null!;public decimal C{get;set;}public string? D{get;set;}}
    sealed class V:AbstractValidator<M>{public V(){RuleFor(x=>x.N).ApplyNameRules();RuleFor(x=>x.C).ApplyCostRules();RuleFor(x=>x.D).ApplyDeliveryRangeRules();}}
    [Fact]public void ApplyNameRules_Empty_ShouldHaveError()=>new V().TestValidate(new M{N="",C=10,D="x"}).ShouldHaveValidationErrorFor(x=>x.N);
    [Fact]public void ApplyNameRules_Valid_ShouldPass()=>new V().TestValidate(new M{N="Standard",C=10,D="x"}).ShouldNotHaveValidationErrorFor(x=>x.N);
    [Fact]public void ApplyCostRules_Zero_ShouldHaveError()=>new V().TestValidate(new M{N="x",C=0,D="x"}).ShouldHaveValidationErrorFor(x=>x.C);
    [Fact]public void ApplyCostRules_Negative_ShouldHaveError()=>new V().TestValidate(new M{N="x",C=-5,D="x"}).ShouldHaveValidationErrorFor(x=>x.C);
    [Fact]public void ApplyCostRules_Positive_ShouldPass()=>new V().TestValidate(new M{N="x",C=10,D="x"}).ShouldNotHaveValidationErrorFor(x=>x.C);
    [Fact]public void ApplyDeliveryRangeRules_Valid_ShouldPass()=>new V().TestValidate(new M{N="x",C=10,D="3-5 days"}).ShouldNotHaveValidationErrorFor(x=>x.D);
}
