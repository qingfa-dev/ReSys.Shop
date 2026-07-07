using Module.Payment.Domain.Payments;

namespace Module.UnitTests.Payment.Domain.Payments;

[Trait("Category","Unit")][Trait("Module","Payment")][Trait("Entity","Payment")]
public class PaymentValidationTests
{
    private sealed class M { public decimal Amount { get; set; } }
    private sealed class V : AbstractValidator<M>
    {
        public V() => RuleFor(x => x.Amount).ApplyAmountRules();
    }

    [Fact]
    public void ApplyAmountRules_WhenZero_ShouldFail()
    {
        new V().TestValidate(new M { Amount = 0 }).ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ApplyAmountRules_WhenNegative_ShouldFail()
    {
        new V().TestValidate(new M { Amount = -10 }).ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ApplyAmountRules_WhenPositive_ShouldPass()
    {
        new V().TestValidate(new M { Amount = 50 }).ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
}
