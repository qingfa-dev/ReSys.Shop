using FluentAssertions;
using Module.Promotions.Domain.PromotionRules;

namespace Module.UnitTests.Promotions.Domain.PromotionRules;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Domain", "PromotionRuleExtensions")]
public class PromotionRuleExtensionTests
{
    [Fact(DisplayName = "Create: Should set properties")]
    public void Create_ShouldSetProperties()
    {
        var result = PromotionRuleExtensions.Create("ItemTotal", new Dictionary<string, string> { ["amount_min"] = "100" }, Guid.NewGuid());

        result.IsSuccess.Should().BeTrue();
        result.Value.Type.Should().Be("ItemTotal");
        result.Value.Preferences.Should().ContainKey("amount_min");
        result.Value.PromotionId.Should().NotBe(Guid.Empty);
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Create: Should use explicit id when provided")]
    public void Create_ShouldUseExplicitId_WhenProvided()
    {
        var id = Guid.NewGuid();
        var result = PromotionRuleExtensions.Create("Type", id: id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(id);
    }

    [Fact(DisplayName = "Create: Should use empty dict when preferences null")]
    public void Create_ShouldUseEmptyDict_WhenPreferencesNull()
    {
        var result = PromotionRuleExtensions.Create("Type");

        result.IsSuccess.Should().BeTrue();
        result.Value.Preferences.Should().NotBeNull();
        result.Value.Preferences.Should().BeEmpty();
    }

    [Fact(DisplayName = "Update: Should update type only")]
    public void Update_ShouldUpdateTypeOnly()
    {
        var result = PromotionRuleExtensions.Create("OldType", new Dictionary<string, string> { ["key"] = "val" });
        var rule = result.Value;

        rule.Update(type: "NewType").IsSuccess.Should().BeTrue();

        rule.Type.Should().Be("NewType");
        rule.Preferences.Should().ContainKey("key");
    }

    [Fact(DisplayName = "Update: Should replace preferences when provided")]
    public void Update_ShouldReplacePreferences_WhenProvided()
    {
        var result = PromotionRuleExtensions.Create("Type", new Dictionary<string, string> { ["old"] = "val" });
        var rule = result.Value;

        rule.Update(preferences: new Dictionary<string, string> { ["new"] = "value" }).IsSuccess.Should().BeTrue();

        rule.Preferences.Should().NotContainKey("old");
        rule.Preferences.Should().ContainKey("new");
    }

    [Fact(DisplayName = "Update: Should keep preferences when null")]
    public void Update_ShouldKeepPreferences_WhenNull()
    {
        var result = PromotionRuleExtensions.Create("Type", new Dictionary<string, string> { ["key"] = "val" });
        var rule = result.Value;

        rule.Update(type: "NewType", preferences: null).IsSuccess.Should().BeTrue();

        rule.Preferences.Should().ContainKey("key");
    }

    [Fact(DisplayName = "SetPreference: Should add new key")]
    public void SetPreference_ShouldAddNewKey()
    {
        var result = PromotionRuleExtensions.Create("Type");
        var rule = result.Value;

        rule.SetPreference("new_key", "new_val").IsSuccess.Should().BeTrue();

        rule.Preferences["new_key"].Should().Be("new_val");
    }

    [Fact(DisplayName = "SetPreference: Should overwrite existing")]
    public void SetPreference_ShouldOverwriteExisting()
    {
        var result = PromotionRuleExtensions.Create("Type", new Dictionary<string, string> { ["key"] = "old" });
        var rule = result.Value;

        rule.SetPreference("key", "new").IsSuccess.Should().BeTrue();

        rule.Preferences["key"].Should().Be("new");
    }

    [Fact(DisplayName = "RemovePreference: Should remove existing")]
    public void RemovePreference_ShouldRemoveExisting()
    {
        var result = PromotionRuleExtensions.Create("Type", new Dictionary<string, string> { ["key"] = "val" });
        var rule = result.Value;

        rule.RemovePreference("key").IsSuccess.Should().BeTrue();

        rule.Preferences.Should().NotContainKey("key");
    }

    [Fact(DisplayName = "RemovePreference: Should be no-op when key missing")]
    public void RemovePreference_ShouldBeNoOp_WhenKeyMissing()
    {
        var result = PromotionRuleExtensions.Create("Type", new Dictionary<string, string> { ["key"] = "val" });
        var rule = result.Value;

        rule.RemovePreference("nonexistent").IsSuccess.Should().BeTrue();

        rule.Preferences.Should().ContainKey("key");
        rule.Preferences.Count.Should().Be(1);
    }
}
