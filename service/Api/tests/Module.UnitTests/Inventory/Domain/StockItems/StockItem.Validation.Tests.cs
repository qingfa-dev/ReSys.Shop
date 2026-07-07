using Module.Inventory.Domain.StockLocations.StockItems;

namespace Module.UnitTests.Inventory.Domain.StockItems;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockItem")]
public class StockItemValidationCountOnHandTests
{
    private sealed class TestModel { public int CountOnHand { get; set; } }
    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.CountOnHand).ApplyCountOnHandRules();
    }

    [Fact(DisplayName = "CountOnHand: Should fail when negative")]
    public void ApplyCountOnHandRules_WhenNegative_ShouldHaveError()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CountOnHand = -1 });

        result.ShouldHaveValidationErrorFor(x => x.CountOnHand)
            .WithErrorCode(StockItemResult.Errors.NegativeCountOnHand.Code);
    }

    [Fact(DisplayName = "CountOnHand: Should pass when zero")]
    public void ApplyCountOnHandRules_WhenZero_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CountOnHand = 0 });

        result.ShouldNotHaveValidationErrorFor(x => x.CountOnHand);
    }

    [Fact(DisplayName = "CountOnHand: Should pass when positive")]
    public void ApplyCountOnHandRules_WhenPositive_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { CountOnHand = 10 });

        result.ShouldNotHaveValidationErrorFor(x => x.CountOnHand);
    }
}

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockItem")]
public class StockItemValidationBackorderableTests
{
    private sealed class TestModel { public bool Backorderable { get; set; } }
    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.Backorderable).ApplyBackorderableRules();
    }

    [Fact(DisplayName = "Backorderable: Should pass when true")]
    public void ApplyBackorderableRules_WhenTrue_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Backorderable = true });

        result.ShouldNotHaveValidationErrorFor(x => x.Backorderable);
    }

    [Fact(DisplayName = "Backorderable: Should pass when false")]
    public void ApplyBackorderableRules_WhenFalse_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Backorderable = false });

        result.ShouldNotHaveValidationErrorFor(x => x.Backorderable);
    }
}
