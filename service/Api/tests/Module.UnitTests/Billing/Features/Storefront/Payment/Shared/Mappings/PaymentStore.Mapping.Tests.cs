using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Storefront.Payment.Shared.Mappings;
using Module.Billing.Features.Storefront.Payment.Shared.Models;
using PaymentCapture = Module.Billing.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentStoreMapping")]
public class PaymentStoreMappingTests
{
    [Fact(DisplayName = "MapToStoreDetail: Should map entity to store detail response")]
    public void MapToStoreDetail_ShouldMapEntityToDetail()
    {
        var payment = CreatePayment();

        var response = payment.MapToStoreDetail<StorePaymentDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(payment.Id);
        response.Amount.Should().Be(payment.Amount);
        response.OrderId.Should().Be(payment.OrderId);
        response.PaymentMethodId.Should().Be(payment.PaymentMethodId.GetValueOrDefault());
        response.CreatedAtUtc.Should().Be(payment.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(payment.ModifiedAtUtc);
    }

    [Fact(DisplayName = "MapToStoreDetail: Should map ClientSecret from IntentClientSecret")]
    public void MapToStoreDetail_ShouldMapClientSecret()
    {
        var payment = CreatePayment(p => p.IntentClientSecret = "secret_456");

        var response = payment.MapToStoreDetail<StorePaymentDetailResponse>();

        response.ClientSecret.Should().Be("secret_456");
    }

    [Fact(DisplayName = "MapToStoreDetail: Should set Currency from payment")]
    public void MapToStoreDetail_ShouldSetCurrencyFromPayment()
    {
        var payment = CreatePayment();

        var response = payment.MapToStoreDetail<StorePaymentDetailResponse>();

        response.Currency.Should().Be(PaymentConstant.Defaults.Currency);
    }

    [Fact(DisplayName = "MapToStoreListItem: Should map entity to store list item response")]
    public void MapToStoreListItem_ShouldMapEntityToList()
    {
        var payment = CreatePayment();

        var response = payment.MapToStoreListItem<StorePaymentListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(payment.Id);
        response.Amount.Should().Be(payment.Amount);
        response.OrderId.Should().Be(payment.OrderId);
        response.PaymentMethodId.Should().Be(payment.PaymentMethodId.GetValueOrDefault());
        response.Currency.Should().Be(PaymentConstant.Defaults.Currency);
    }

    [Fact(DisplayName = "MapToStoreDetail: Should handle null optional fields")]
    public void MapToStoreDetail_WhenOptionalFieldsNull_ShouldMapCorrectly()
    {
        var payment = CreatePayment(p =>
        {
            p.ModifiedAtUtc = null;
            p.IntentClientSecret = null;
        });

        var response = payment.MapToStoreDetail<StorePaymentDetailResponse>();

        response.Id.Should().Be(payment.Id);
        response.ClientSecret.Should().BeNull();
        response.ModifiedAtUtc.Should().BeNull();
    }

    [Fact(DisplayName = "MapToStoreDetail: Should map PaymentStatus from payment")]
    public void MapToStoreDetail_ShouldMapPaymentStatus()
    {
        var payment = CreatePayment(p => p.PaymentStatus = "requires_action");

        var response = payment.MapToStoreDetail<StorePaymentDetailResponse>();

        response.PaymentStatus.Should().Be("requires_action");
    }

    private static PaymentCapture CreatePayment(Action<PaymentCapture>? configure = null)
    {
        var payment = new PaymentCapture
        {
            Id = Guid.NewGuid(),
            Amount = 49.99m,
            State = PaymentRecordState.Checkout,
            PaymentMethodId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            IntentClientSecret = null,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            ModifiedAtUtc = DateTimeOffset.UtcNow,
        };
        configure?.Invoke(payment);
        return payment;
    }
}
