using Module.Payment.Domain.PaymentMethods;

namespace Module.UnitTests.Payment.Domain.PaymentMethods;

[Trait("Category","Unit")][Trait("Module","Payment")][Trait("Entity","PaymentMethod")]
public class PaymentMethodExtensionsTests
{
    [Fact]
    public void Create_WithValidParams_ShouldReturnPaymentMethod()
    {
        var result = PaymentMethodExtensions.Create("Credit Card", "CC", "CreditCard");

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Credit Card");
        result.Value.Code.Should().Be("CC");
        result.Value.ProviderKey.Should().Be("CreditCard");
        result.Value.Active.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenInactive_ShouldActivate()
    {
        var method = PaymentMethodExtensions.Create("Test", null, "TestProvider").Value;
        method.Active = false;

        var result = method.Activate();

        result.IsSuccess.Should().BeTrue();
        method.Active.Should().BeTrue();
    }

    [Fact]
    public void Activate_WhenAlreadyActive_ShouldFail()
    {
        var method = PaymentMethodExtensions.Create("Test", null, "TestProvider").Value;

        var result = method.Activate();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentMethodResult.Errors.AlreadyActive);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivate()
    {
        var method = PaymentMethodExtensions.Create("Test", null, "TestProvider").Value;

        var result = method.Deactivate();

        result.IsSuccess.Should().BeTrue();
        method.Active.Should().BeFalse();
    }

    [Fact]
    public void Deactivate_WhenAlreadyInactive_ShouldFail()
    {
        var method = PaymentMethodExtensions.Create("Test", null, "TestProvider").Value;
        method.Active = false;

        var result = method.Deactivate();

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Should().Be(PaymentMethodResult.Errors.AlreadyInactive);
    }

    [Fact]
    public void UpdatePreferences_ShouldReplacePreferences()
    {
        var method = PaymentMethodExtensions.Create("Test", null, "TestProvider").Value;
        var prefs = new Dictionary<string, string> { ["key1"] = "value1" };

        var result = method.UpdatePreferences(prefs);

        result.IsSuccess.Should().BeTrue();
        method.Preferences.Should().ContainKey("key1");
        method.Preferences["key1"].Should().Be("value1");
    }
}
