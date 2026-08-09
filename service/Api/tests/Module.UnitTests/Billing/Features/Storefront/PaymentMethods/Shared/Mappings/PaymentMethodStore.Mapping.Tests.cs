using Module.Billing.Domain.PaymentMethods;
using PaymentRecord = Module.Billing.Domain.PaymentMethods.PaymentMethod;
using Module.Billing.Features.Storefront.PaymentMethods.Shared.Mappings;
using Module.Billing.Features.Storefront.PaymentMethods.Shared.Models;

namespace Module.UnitTests.Payment.Features.Storefront.PaymentMethods.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentMethodStoreMapping")]
public class PaymentMethodStoreMappingTests
{
    [Fact(DisplayName = "MapToStoreListItem: Should map PaymentMethod to store list item")]
    public void MapToStoreListItem_ShouldMapEntityToList()
    {
        var method = CreatePaymentMethod();

        var response = method.MapToStoreListItem<StorePaymentMethodListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(method.Id);
        response.Name.Should().Be(method.Name);
        response.Code.Should().Be(method.Code);
        response.Description.Should().Be(method.Description);
        response.ProviderKey.Should().Be(method.ProviderKey);
        response.AutoCapture.Should().Be(method.AutoCapture);
        response.DisplayOn.Should().Be(method.DisplayOn);
        response.Position.Should().Be(method.Position);
        response.Presentation.Should().Be(method.Presentation);
        response.Active.Should().Be(method.Active);
    }

    [Fact(DisplayName = "MapToStoreListItem: Should handle null optional fields")]
    public void MapToStoreListItem_WhenOptionalFieldsNull_ShouldMapCorrectly()
    {
        var method = CreatePaymentMethod(m =>
        {
            m.Code = null;
            m.Description = null;
            m.Presentation = null;
            m.Settings = [];
            m.Preferences = [];
        });

        var response = method.MapToStoreListItem<StorePaymentMethodListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(method.Id);
        response.Name.Should().Be(method.Name);
        response.Code.Should().BeNull();
        response.Description.Should().BeNull();
    }

    private static PaymentRecord CreatePaymentMethod(Action<PaymentRecord>? configure = null)
    {
        var result = PaymentMethodMethod.Create(
            "Credit Card", "CC", "stripe", true, DisplayOn.Both,
            new Dictionary<string, string> { ["key"] = "val" }, "Test method");
        var method = result.Value;
        configure?.Invoke(method);
        return method;
    }
}
