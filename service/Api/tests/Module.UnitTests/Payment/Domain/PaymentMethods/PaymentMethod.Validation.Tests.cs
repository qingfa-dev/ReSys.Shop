using Module.Payment.Domain.PaymentMethods;

namespace Module.UnitTests.Payment.Domain.PaymentMethods;

[Trait("Category","Unit")][Trait("Module","Payment")][Trait("Entity","PaymentMethod")]
public class PaymentMethodValidationTests
{
    #region ApplyNameRules
    private sealed class NameM { public string? Name { get; set; } }
    private sealed class NameV : AbstractValidator<NameM>
    {
        public NameV() => RuleFor(x => x.Name).ApplyNameRules();
    }

    [Fact]
    public void ApplyNameRules_WhenNull_ShouldFail()
    {
        new NameV().TestValidate(new NameM { Name = null }).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ApplyNameRules_WhenEmpty_ShouldFail()
    {
        new NameV().TestValidate(new NameM { Name = "" }).ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void ApplyNameRules_WhenValid_ShouldPass()
    {
        new NameV().TestValidate(new NameM { Name = "Credit Card" }).ShouldNotHaveValidationErrorFor(x => x.Name);
    }
    #endregion

    #region ApplyCodeRules
    private sealed class CodeM { public string? Code { get; set; } }
    private sealed class CodeV : AbstractValidator<CodeM>
    {
        public CodeV() => RuleFor(x => x.Code).ApplyCodeRules();
    }

    [Fact]
    public void ApplyCodeRules_WhenNull_ShouldFail()
    {
        new CodeV().TestValidate(new CodeM { Code = null }).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void ApplyCodeRules_WhenEmpty_ShouldFail()
    {
        new CodeV().TestValidate(new CodeM { Code = "" }).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void ApplyCodeRules_WhenTooLong_ShouldFail()
    {
        new CodeV().TestValidate(new CodeM { Code = new string('a', 51) }).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void ApplyCodeRules_WhenInvalidPattern_ShouldFail()
    {
        new CodeV().TestValidate(new CodeM { Code = "invalid code!" }).ShouldHaveValidationErrorFor(x => x.Code);
    }

    [Fact]
    public void ApplyCodeRules_WhenValid_ShouldPass()
    {
        new CodeV().TestValidate(new CodeM { Code = "CC" }).ShouldNotHaveValidationErrorFor(x => x.Code);
    }
    #endregion

    #region ApplyProviderKeyRules
    private sealed class ProviderKeyM { public string? ProviderKey { get; set; } }
    private sealed class ProviderKeyV : AbstractValidator<ProviderKeyM>
    {
        public ProviderKeyV() => RuleFor(x => x.ProviderKey).ApplyProviderKeyRules();
    }

    [Fact]
    public void ApplyProviderKeyRules_WhenNull_ShouldFail()
    {
        new ProviderKeyV().TestValidate(new ProviderKeyM { ProviderKey = null }).ShouldHaveValidationErrorFor(x => x.ProviderKey);
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

    #region ApplyDescriptionRules
    private sealed class DescriptionM { public string? Description { get; set; } }
    private sealed class DescriptionV : AbstractValidator<DescriptionM>
    {
        public DescriptionV() => RuleFor(x => x.Description).ApplyDescriptionRules();
    }

    [Fact]
    public void ApplyDescriptionRules_WhenTooLong_ShouldFail()
    {
        new DescriptionV().TestValidate(new DescriptionM { Description = new string('a', 1001) }).ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ApplyDescriptionRules_WhenNull_ShouldPass()
    {
        new DescriptionV().TestValidate(new DescriptionM { Description = null }).ShouldNotHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void ApplyDescriptionRules_WhenValid_ShouldPass()
    {
        new DescriptionV().TestValidate(new DescriptionM { Description = "Test description" }).ShouldNotHaveValidationErrorFor(x => x.Description);
    }
    #endregion

    #region ApplyDisplayOnRules
    private sealed class DisplayOnM { public DisplayOn DisplayOn { get; set; } }
    private sealed class DisplayOnV : AbstractValidator<DisplayOnM>
    {
        public DisplayOnV() => RuleFor(x => x.DisplayOn).ApplyDisplayOnRules();
    }

    [Fact]
    public void ApplyDisplayOnRules_WhenInvalid_ShouldFail()
    {
        new DisplayOnV().TestValidate(new DisplayOnM { DisplayOn = (DisplayOn)99 }).ShouldHaveValidationErrorFor(x => x.DisplayOn);
    }

    [Fact]
    public void ApplyDisplayOnRules_WhenBoth_ShouldPass()
    {
        new DisplayOnV().TestValidate(new DisplayOnM { DisplayOn = DisplayOn.Both }).ShouldNotHaveValidationErrorFor(x => x.DisplayOn);
    }

    [Fact]
    public void ApplyDisplayOnRules_WhenFrontend_ShouldPass()
    {
        new DisplayOnV().TestValidate(new DisplayOnM { DisplayOn = DisplayOn.Frontend }).ShouldNotHaveValidationErrorFor(x => x.DisplayOn);
    }

    [Fact]
    public void ApplyDisplayOnRules_WhenBackend_ShouldPass()
    {
        new DisplayOnV().TestValidate(new DisplayOnM { DisplayOn = DisplayOn.Backend }).ShouldNotHaveValidationErrorFor(x => x.DisplayOn);
    }
    #endregion

    #region ApplyPositionRules
    private sealed class PositionM { public int Position { get; set; } }
    private sealed class PositionV : AbstractValidator<PositionM>
    {
        public PositionV() => RuleFor(x => x.Position).ApplyPositionRules();
    }

    [Fact]
    public void ApplyPositionRules_WhenNegative_ShouldFail()
    {
        new PositionV().TestValidate(new PositionM { Position = -1 }).ShouldHaveValidationErrorFor(x => x.Position);
    }

    [Fact]
    public void ApplyPositionRules_WhenExceedsMax_ShouldFail()
    {
        new PositionV().TestValidate(new PositionM { Position = 10000 }).ShouldHaveValidationErrorFor(x => x.Position);
    }

    [Fact]
    public void ApplyPositionRules_WhenZero_ShouldPass()
    {
        new PositionV().TestValidate(new PositionM { Position = 0 }).ShouldNotHaveValidationErrorFor(x => x.Position);
    }

    [Fact]
    public void ApplyPositionRules_WhenValid_ShouldPass()
    {
        new PositionV().TestValidate(new PositionM { Position = 5 }).ShouldNotHaveValidationErrorFor(x => x.Position);
    }
    #endregion

    #region ApplyPresentationRules
    private sealed class PresentationM { public string? Presentation { get; set; } }
    private sealed class PresentationV : AbstractValidator<PresentationM>
    {
        public PresentationV() => RuleFor(x => x.Presentation).ApplyPresentationRules();
    }

    [Fact]
    public void ApplyPresentationRules_WhenTooLong_ShouldFail()
    {
        new PresentationV().TestValidate(new PresentationM { Presentation = new string('a', 501) }).ShouldHaveValidationErrorFor(x => x.Presentation);
    }

    [Fact]
    public void ApplyPresentationRules_WhenNull_ShouldPass()
    {
        new PresentationV().TestValidate(new PresentationM { Presentation = null }).ShouldNotHaveValidationErrorFor(x => x.Presentation);
    }

    [Fact]
    public void ApplyPresentationRules_WhenValid_ShouldPass()
    {
        new PresentationV().TestValidate(new PresentationM { Presentation = "Pay with Card" }).ShouldNotHaveValidationErrorFor(x => x.Presentation);
    }
    #endregion
}
