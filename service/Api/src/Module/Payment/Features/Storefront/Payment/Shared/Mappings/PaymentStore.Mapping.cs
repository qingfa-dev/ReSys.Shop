using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

using Module.Payment.Services.Provider;
using Module.Payment.Features.Storefront.Payment.Shared.Models;

namespace Module.Payment.Features.Storefront.Payment.Shared.Mappings;

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
            Currency = string.Empty,
            OrderId = payment.OrderId,
            PaymentMethodId = payment.PaymentMethodId.GetValueOrDefault(),
            State = payment.State.ToString(),
            ClientSecret = payment.IntentClientSecret,
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
            Currency = string.Empty,
            OrderId = payment.OrderId,
            PaymentMethodId = payment.PaymentMethodId.GetValueOrDefault(),
            State = payment.State.ToString(),
        };
    }
}
