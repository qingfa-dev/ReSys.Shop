using FluentValidation;
using FluentValidation.TestHelper;

using Module.Inventory.Domain.StockLocations;

namespace Module.UnitTests.Inventory.Domain.StockLocations;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "Validators")]
[Trait("Entity", "StockLocation")]
public class StockLocationValidationNameTests
{
    private sealed class TestModel { public string? Name { get; set; } }

    private sealed class TestValidator : AbstractValidator<TestModel>
    {
        public TestValidator() => RuleFor(x => x.Name).ApplyNameRules();
    }

    [Theory(DisplayName = "Name: Should fail when empty")]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void ApplyNameRules_WhenEmpty_ShouldHaveError(string? name)
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = name });

        result.ShouldHaveValidationErrorFor(x => x.Name)
            .WithErrorCode(StockLocationResult.Errors.NameRequired.Code);
    }

    [Fact(DisplayName = "Name: Should pass with valid name")]
    public void ApplyNameRules_WhenValid_ShouldPass()
    {
        var validator = new TestValidator();
        var result = validator.TestValidate(new TestModel { Name = "Warehouse" });

        result.ShouldNotHaveValidationErrorFor(x => x.Name);
    }
}
