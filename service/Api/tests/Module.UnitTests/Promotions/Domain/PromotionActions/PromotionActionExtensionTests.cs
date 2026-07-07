using FluentAssertions;
using Module.Promotions.Domain.PromotionActions;

namespace Module.UnitTests.Promotions.Domain.PromotionActions;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "PromotionActionExtensions")]
public class PromotionActionExtensionTests
{
    [Fact(DisplayName = "Create: Should set properties")]
    public void Create_ShouldSetProperties()
    {
        var result = PromotionActionExtensions.Create("CreateAdjustment", promotionId: Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("CreateAdjustment");
        result.Value.CalculatorType.Should().BeNull();
        result.Value.Preferences.Should().BeEmpty();
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Create: Should set calculator type when provided")]
    public void Create_ShouldSetCalculatorType_WhenProvided()
    {
        var result = PromotionActionExtensions.Create("CreateAdjustment", calculatorType: "FlatRate");

        result.IsSuccess.Should().BeTrue();
        result.Value.CalculatorType.Should().Be("FlatRate");
    }

    [Fact(DisplayName = "Update: Should update selected fields")]
    public void Update_ShouldUpdateSelectedFields()
    {
        var result = PromotionActionExtensions.Create("OldType", calculatorType: "FlatRate");
        var action = result.Value;

        action.Update(type: "NewType").IsSuccess.Should().BeTrue();

        action.Type.Should().Be("NewType");
        action.CalculatorType.Should().Be("FlatRate");
    }

    [Fact(DisplayName = "Update: Should keep calculator type when null")]
    public void Update_ShouldKeepCalculatorType_WhenNull()
    {
        var result = PromotionActionExtensions.Create("CreateAdjustment", calculatorType: "FlatRate");
        var action = result.Value;

        action.Update(calculatorType: null).IsSuccess.Should().BeTrue();

        action.CalculatorType.Should().Be("FlatRate");
    }

    [Fact(DisplayName = "SetPreference: Should add and overwrite")]
    public void SetPreference_ShouldAddAndOverwrite()
    {
        var result = PromotionActionExtensions.Create("CreateAdjustment");
        var action = result.Value;

        action.SetPreference("amount", "10").IsSuccess.Should().BeTrue();
        action.Preferences["amount"].Should().Be("10");

        action.SetPreference("amount", "20").IsSuccess.Should().BeTrue();
        action.Preferences["amount"].Should().Be("20");
    }

    [Fact(DisplayName = "RemovePreference: Should remove and no-op")]
    public void RemovePreference_ShouldRemoveAndNoOp()
    {
        var result = PromotionActionExtensions.Create("CreateAdjustment",
            preferences: new Dictionary<string, string> { ["key"] = "val" });
        var action = result.Value;

        action.RemovePreference("key").IsSuccess.Should().BeTrue();
        action.Preferences.Should().BeEmpty();

        action.RemovePreference("nonexistent").IsSuccess.Should().BeTrue();
        action.Preferences.Should().BeEmpty();
    }
}
