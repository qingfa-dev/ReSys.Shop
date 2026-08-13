using PaymentCapture = Module.Billing.Domain.PaymentCaptures.PaymentCapture;

using Module.Billing.Services.Provider;
using Module.Billing.Features.Storefront.Payment.Shared.Models;

namespace Module.Billing.Features.Storefront.Payment.Shared.Mappings;

// Map: PaymentCapture/PaymentGatewayResponse → storefront response DTOs
public static class PaymentStoreMapping
{
    public static T MapToStoreDetail<T>(this PaymentGatewayResponse response) where T : StorePaymentDetailResponse, new()
    {
        return new T
        {
            ClientSecret = response.SetupIntentClientSecret,
        };
    }


    public static T MapToStoreDetail<T>(this PaymentCapture payment) where T : StorePaymentDetailResponse, new()
    {
        return new T
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Currency = payment.Currency,
            OrderId = payment.OrderId,
            PaymentMethodId = payment.PaymentMethodId.GetValueOrDefault(),
            State = payment.State.ToString(),
            ClientSecret = payment.IntentClientSecret,
            CheckoutUrl = payment.CheckoutUrl,
            ResponseCode = payment.ResponseCode,
            PaymentStatus = payment.PaymentStatus,
            CreatedAtUtc = payment.CreatedAtUtc,
            ModifiedAtUtc = payment.ModifiedAtUtc,
        };
    }

    public static T MapToStoreListItem<T>(this PaymentCapture payment) where T : StorePaymentListItemResponse, new()
    {
        return new T
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Currency = payment.Currency,
            OrderId = payment.OrderId,
            PaymentMethodId = payment.PaymentMethodId.GetValueOrDefault(),
            State = payment.State.ToString(),
        };
    }
}
