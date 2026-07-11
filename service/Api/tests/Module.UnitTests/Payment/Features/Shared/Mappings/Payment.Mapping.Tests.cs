using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Features.Admin.Payments.Shared.Mappings;
using Module.Payment.Features.Admin.Payments.Shared.Models;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.UnitTests.Payment.Features.Shared.Mappings;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "PaymentMapping")]
public class PaymentMappingTests
{
    [Fact(DisplayName = "ToDomain: Should map request to domain entity")]
    public void ToDomain_ShouldMapRequestToEntity()
    {
        var request = new PaymentRequest { Amount = 99.99m, OrderId = Guid.NewGuid(), PaymentMethodId = Guid.NewGuid() };

        var payment = request.MapToDomain();

        payment.Should().NotBeNull();
        payment.Amount.Should().Be(request.Amount);
        payment.PaymentMethodId.Should().Be(request.PaymentMethodId);
        payment.OrderId.Should().Be(request.OrderId);
        payment.State.Should().Be(PaymentRecordState.Checkout);
        payment.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "ToDomain: Should return null when amount is zero or negative")]
    public void ToDomain_WhenAmountIsZero_ShouldReturnNull()
    {
        var request = new PaymentRequest { Amount = 0, OrderId = Guid.NewGuid(), PaymentMethodId = Guid.NewGuid() };

        var payment = request.MapToDomain();

        payment.Should().BeNull();
    }

    [Fact(DisplayName = "ToDetail: Should map entity to detail response")]
    public void ToDetail_ShouldMapEntityToDetail()
    {
        var payment = CreatePayment();

        var response = payment.MapToDetail<PaymentDetailResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(payment.Id);
        response.Amount.Should().Be(payment.Amount);
        response.OrderId.Should().Be(payment.OrderId);
        response.PaymentMethodId.Should().Be(payment.PaymentMethodId);
        response.CreatedAtUtc.Should().Be(payment.CreatedAtUtc);
        response.ModifiedAtUtc.Should().Be(payment.ModifiedAtUtc);
    }

    [Fact(DisplayName = "ToDetail: Should map ClientSecret from IntentClientSecret")]
    public void ToDetail_ShouldMapClientSecret()
    {
        var payment = CreatePayment(p => p.IntentClientSecret = "secret_123");

        var response = payment.MapToDetail<PaymentDetailResponse>();

        response.ClientSecret.Should().Be("secret_123");
    }

    [Fact(DisplayName = "ToDetail: Should set Currency to empty string")]
    public void ToDetail_ShouldSetCurrencyToEmpty()
    {
        var payment = CreatePayment();

        var response = payment.MapToDetail<PaymentDetailResponse>();

        response.Currency.Should().BeEmpty();
    }

    [Fact(DisplayName = "ToListItem: Should map entity to list item response")]
    public void ToListItem_ShouldMapEntityToList()
    {
        var payment = CreatePayment();

        var response = payment.MapToListItem<PaymentListItemResponse>();

        response.Should().NotBeNull();
        response.Id.Should().Be(payment.Id);
        response.Amount.Should().Be(payment.Amount);
        response.OrderId.Should().Be(payment.OrderId);
        response.PaymentMethodId.Should().Be(payment.PaymentMethodId);
    }

    [Fact(DisplayName = "ToListItem: Should handle null auditable fields")]
    public void ToListItem_WhenModifiedAtNull_ShouldMapCorrectly()
    {
        var payment = CreatePayment(p =>
        {
            p.ModifiedAtUtc = null;
            p.CreatedBy = null;
            p.ModifiedBy = null;
        });

        var response = payment.MapToListItem<PaymentListItemResponse>();

        response.Id.Should().Be(payment.Id);
    }

    private static PaymentCapture CreatePayment(Action<PaymentCapture>? configure = null)
    {
        var payment = new PaymentCapture
        {
            Id = Guid.NewGuid(),
            Amount = 99.99m,
            State = PaymentRecordState.Checkout,
            PaymentMethodId = Guid.NewGuid(),
            OrderId = Guid.NewGuid(),
            IntentClientSecret = null,
            CreatedAtUtc = DateTimeOffset.UtcNow.AddDays(-1),
            ModifiedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "admin",
            ModifiedBy = "admin",
        };
        configure?.Invoke(payment);
        return payment;
    }
}
