using Module.Payment.Domain.PaymentCaptures;

namespace Module.UnitTests.Payment.Domain.Payments;

[Trait("Category","Unit")][Trait("Module","Payment")][Trait("Entity","Payment")]
public class PaymentValidationTests
{
    #region ApplyAmountRules
    private sealed class AmountM { public decimal Amount { get; set; } }
    private sealed class AmountV : AbstractValidator<AmountM>
    {
        public AmountV() => RuleFor(x => x.Amount).ApplyAmountRules();
    }

    [Fact]
    public void ApplyAmountRules_WhenZero_ShouldFail()
    {
        new AmountV().TestValidate(new AmountM { Amount = 0 }).ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ApplyAmountRules_WhenNegative_ShouldFail()
    {
        new AmountV().TestValidate(new AmountM { Amount = -10 }).ShouldHaveValidationErrorFor(x => x.Amount);
    }

    [Fact]
    public void ApplyAmountRules_WhenPositive_ShouldPass()
    {
        new AmountV().TestValidate(new AmountM { Amount = 50 }).ShouldNotHaveValidationErrorFor(x => x.Amount);
    }
    #endregion

    #region ApplyNumberRules
    private sealed class NumberM { public string? Number { get; set; } }
    private sealed class NumberV : AbstractValidator<NumberM>
    {
        public NumberV() => RuleFor(x => x.Number).ApplyNumberRules();
    }

    [Fact]
    public void ApplyNumberRules_WhenEmpty_ShouldFail()
    {
        new NumberV().TestValidate(new NumberM { Number = "" }).ShouldHaveValidationErrorFor(x => x.Number);
    }

    [Fact]
    public void ApplyNumberRules_WhenTooLong_ShouldFail()
    {
        new NumberV().TestValidate(new NumberM { Number = new string('a', 51) }).ShouldHaveValidationErrorFor(x => x.Number);
    }

    [Fact]
    public void ApplyNumberRules_WhenValid_ShouldPass()
    {
        new NumberV().TestValidate(new NumberM { Number = "PAY-20260713-ABC123" }).ShouldNotHaveValidationErrorFor(x => x.Number);
    }
    #endregion

    #region ApplyOrderIdRules
    private sealed class OrderIdM { public Guid OrderId { get; set; } }
    private sealed class OrderIdV : AbstractValidator<OrderIdM>
    {
        public OrderIdV() => RuleFor(x => x.OrderId).ApplyOrderIdRules();
    }

    [Fact]
    public void ApplyOrderIdRules_WhenEmpty_ShouldFail()
    {
        new OrderIdV().TestValidate(new OrderIdM { OrderId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.OrderId);
    }

    [Fact]
    public void ApplyOrderIdRules_WhenValid_ShouldPass()
    {
        new OrderIdV().TestValidate(new OrderIdM { OrderId = Guid.NewGuid() }).ShouldNotHaveValidationErrorFor(x => x.OrderId);
    }
    #endregion

    #region ApplyPaymentMethodIdRules
    private sealed class PaymentMethodIdM { public Guid PaymentMethodId { get; set; } }
    private sealed class PaymentMethodIdV : AbstractValidator<PaymentMethodIdM>
    {
        public PaymentMethodIdV() => RuleFor(x => x.PaymentMethodId).ApplyPaymentMethodIdRules();
    }

    [Fact]
    public void ApplyPaymentMethodIdRules_WhenEmpty_ShouldFail()
    {
        new PaymentMethodIdV().TestValidate(new PaymentMethodIdM { PaymentMethodId = Guid.Empty }).ShouldHaveValidationErrorFor(x => x.PaymentMethodId);
    }

    [Fact]
    public void ApplyPaymentMethodIdRules_WhenValid_ShouldPass()
    {
        new PaymentMethodIdV().TestValidate(new PaymentMethodIdM { PaymentMethodId = Guid.NewGuid() }).ShouldNotHaveValidationErrorFor(x => x.PaymentMethodId);
    }
    #endregion

    #region ApplyProviderKeyRules
    private sealed class ProviderKeyM { public string? ProviderKey { get; set; } }
    private sealed class ProviderKeyV : AbstractValidator<ProviderKeyM>
    {
        public ProviderKeyV() => RuleFor(x => x.ProviderKey).ApplyProviderKeyRules();
    }

    [Fact]
    public void ApplyProviderKeyRules_WhenEmpty_ShouldFail()
    {
        new ProviderKeyV().TestValidate(new ProviderKeyM { ProviderKey = "" }).ShouldHaveValidationErrorFor(x => x.ProviderKey);
    }

    [Fact]
    public void ApplyProviderKeyRules_WhenTooLong_ShouldFail()
    {
        new ProviderKeyV().TestValidate(new ProviderKeyM { ProviderKey = new string('a', 51) }).ShouldHaveValidationErrorFor(x => x.ProviderKey);
    }

    [Fact]
    public void ApplyProviderKeyRules_WhenValid_ShouldPass()
    {
        new ProviderKeyV().TestValidate(new ProviderKeyM { ProviderKey = "stripe" }).ShouldNotHaveValidationErrorFor(x => x.ProviderKey);
    }
    #endregion

    #region ApplyResponseCodeRules
    private sealed class ResponseCodeM { public string? ResponseCode { get; set; } }
    private sealed class ResponseCodeV : AbstractValidator<ResponseCodeM>
    {
        public ResponseCodeV() => RuleFor(x => x.ResponseCode).ApplyResponseCodeRules();
    }

    [Fact]
    public void ApplyResponseCodeRules_WhenTooLong_ShouldFail()
    {
        new ResponseCodeV().TestValidate(new ResponseCodeM { ResponseCode = new string('a', 256) }).ShouldHaveValidationErrorFor(x => x.ResponseCode);
    }

    [Fact]
    public void ApplyResponseCodeRules_WhenNull_ShouldPass()
    {
        new ResponseCodeV().TestValidate(new ResponseCodeM { ResponseCode = null }).ShouldNotHaveValidationErrorFor(x => x.ResponseCode);
    }

    [Fact]
    public void ApplyResponseCodeRules_WhenValid_ShouldPass()
    {
        new ResponseCodeV().TestValidate(new ResponseCodeM { ResponseCode = "ch_123" }).ShouldNotHaveValidationErrorFor(x => x.ResponseCode);
    }
    #endregion

    #region ApplyAvsResponseRules
    private sealed class AvsResponseM { public string? AvsResponse { get; set; } }
    private sealed class AvsResponseV : AbstractValidator<AvsResponseM>
    {
        public AvsResponseV() => RuleFor(x => x.AvsResponse).ApplyAvsResponseRules();
    }

    [Fact]
    public void ApplyAvsResponseRules_WhenTooLong_ShouldFail()
    {
        new AvsResponseV().TestValidate(new AvsResponseM { AvsResponse = new string('a', 256) }).ShouldHaveValidationErrorFor(x => x.AvsResponse);
    }

    [Fact]
    public void ApplyAvsResponseRules_WhenNull_ShouldPass()
    {
        new AvsResponseV().TestValidate(new AvsResponseM { AvsResponse = null }).ShouldNotHaveValidationErrorFor(x => x.AvsResponse);
    }
    #endregion

    #region ApplyCvvCodeRules
    private sealed class CvvCodeM { public string? CvvCode { get; set; } }
    private sealed class CvvCodeV : AbstractValidator<CvvCodeM>
    {
        public CvvCodeV() => RuleFor(x => x.CvvCode).ApplyCvvCodeRules();
    }

    [Fact]
    public void ApplyCvvCodeRules_WhenTooLong_ShouldFail()
    {
        new CvvCodeV().TestValidate(new CvvCodeM { CvvCode = new string('a', 11) }).ShouldHaveValidationErrorFor(x => x.CvvCode);
    }

    [Fact]
    public void ApplyCvvCodeRules_WhenNull_ShouldPass()
    {
        new CvvCodeV().TestValidate(new CvvCodeM { CvvCode = null }).ShouldNotHaveValidationErrorFor(x => x.CvvCode);
    }
    #endregion

    #region ApplyCvvMessageRules
    private sealed class CvvMessageM { public string? CvvMessage { get; set; } }
    private sealed class CvvMessageV : AbstractValidator<CvvMessageM>
    {
        public CvvMessageV() => RuleFor(x => x.CvvMessage).ApplyCvvMessageRules();
    }

    [Fact]
    public void ApplyCvvMessageRules_WhenTooLong_ShouldFail()
    {
        new CvvMessageV().TestValidate(new CvvMessageM { CvvMessage = new string('a', 256) }).ShouldHaveValidationErrorFor(x => x.CvvMessage);
    }

    [Fact]
    public void ApplyCvvMessageRules_WhenNull_ShouldPass()
    {
        new CvvMessageV().TestValidate(new CvvMessageM { CvvMessage = null }).ShouldNotHaveValidationErrorFor(x => x.CvvMessage);
    }
    #endregion

    #region ApplyIntentClientSecretRules
    private sealed class ClientSecretM { public string? ClientSecret { get; set; } }
    private sealed class ClientSecretV : AbstractValidator<ClientSecretM>
    {
        public ClientSecretV() => RuleFor(x => x.ClientSecret).ApplyIntentClientSecretRules();
    }

    [Fact]
    public void ApplyIntentClientSecretRules_WhenTooLong_ShouldFail()
    {
        new ClientSecretV().TestValidate(new ClientSecretM { ClientSecret = new string('a', 501) }).ShouldHaveValidationErrorFor(x => x.ClientSecret);
    }

    [Fact]
    public void ApplyIntentClientSecretRules_WhenNull_ShouldPass()
    {
        new ClientSecretV().TestValidate(new ClientSecretM { ClientSecret = null }).ShouldNotHaveValidationErrorFor(x => x.ClientSecret);
    }
    #endregion

    #region ApplySourceTypeRules
    private sealed class SourceTypeM { public string? SourceType { get; set; } }
    private sealed class SourceTypeV : AbstractValidator<SourceTypeM>
    {
        public SourceTypeV() => RuleFor(x => x.SourceType).ApplySourceTypeRules();
    }

    [Fact]
    public void ApplySourceTypeRules_WhenEmpty_ShouldFail()
    {
        new SourceTypeV().TestValidate(new SourceTypeM { SourceType = "" }).ShouldHaveValidationErrorFor(x => x.SourceType);
    }

    [Fact]
    public void ApplySourceTypeRules_WhenTooLong_ShouldFail()
    {
        new SourceTypeV().TestValidate(new SourceTypeM { SourceType = new string('a', 101) }).ShouldHaveValidationErrorFor(x => x.SourceType);
    }

    [Fact]
    public void ApplySourceTypeRules_WhenValid_ShouldPass()
    {
        new SourceTypeV().TestValidate(new SourceTypeM { SourceType = "card" }).ShouldNotHaveValidationErrorFor(x => x.SourceType);
    }
    #endregion

    #region ApplyCurrencyRules
    private sealed class CurrencyM { public string? Currency { get; set; } }
    private sealed class CurrencyV : AbstractValidator<CurrencyM>
    {
        public CurrencyV() => RuleFor(x => x.Currency).ApplyCurrencyRules();
    }

    [Fact]
    public void ApplyCurrencyRules_WhenEmpty_ShouldFail()
    {
        new CurrencyV().TestValidate(new CurrencyM { Currency = "" }).ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void ApplyCurrencyRules_WhenTooShort_ShouldFail()
    {
        new CurrencyV().TestValidate(new CurrencyM { Currency = "US" }).ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void ApplyCurrencyRules_WhenTooLong_ShouldFail()
    {
        new CurrencyV().TestValidate(new CurrencyM { Currency = "USDD" }).ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Fact]
    public void ApplyCurrencyRules_WhenValid_ShouldPass()
    {
        new CurrencyV().TestValidate(new CurrencyM { Currency = "USD" }).ShouldNotHaveValidationErrorFor(x => x.Currency);
    }
    #endregion
}
