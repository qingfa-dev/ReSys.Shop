using Module.Payment.Domain.PaymentMethods;

namespace Module.UnitTests.Payment.Domain.PaymentMethods;

[Trait("Category","Unit")][Trait("Module","Payment")][Trait("Entity","PaymentMethod")]
public class PaymentMethodValidationTests
{
    private sealed class M { public string? Name { get; set; } }
    private sealed class V : AbstractValidator<M>
    {
        public V() => RuleFor(x => x.Name).ApplyNameRules();
    }

    [Fact]
    public void ApplyNameRules_WhenNull_ShouldFail()
    {
        new V().TestValidate(new M { Name = null }).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ApplyNameRules_WhenEmpty_ShouldFail()
    {
        new V().TestValidate(new M { Name = "" }).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ApplyNameRules_WhenValid_ShouldPass()
    {
        new V().TestValidate(new M { Name = "Credit Card" }).ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
