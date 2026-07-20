using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

using PaymentMethod = Module.Payment.Domain.PaymentMethods.PaymentMethod;

namespace Module.Payment.Features.Admin.Payments.Shared.Mappings;

public static class PaymentModelMapping
{
    public static T MapToDetail<T>(this PaymentCapture payment) where T : Models.PaymentDetailResponse, new()
    {
        return new T
        {
            Id = payment.Id,
            Number = payment.Number,
            Amount = payment.Amount,
            Currency = string.Empty,
            OrderId = payment.OrderId,
            PaymentMethodId = payment.PaymentMethodId.GetValueOrDefault(),
            State = payment.State.ToString(),
            ResponseCode = payment.ResponseCode,
            PaymentMethodName = payment.PaymentMethod?.Name,
            ClientSecret = payment.IntentClientSecret,
            CreatedAtUtc = payment.CreatedAtUtc,
            ModifiedAtUtc = payment.ModifiedAtUtc,
            CreatedBy = payment.CreatedBy,
            ModifiedBy = payment.ModifiedBy
        };
    }

    public static T MapToListItem<T>(this PaymentCapture payment) where T : Models.PaymentListItemResponse, new()
    {
        return new T
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Currency = string.Empty,
            OrderId = payment.OrderId,
            PaymentMethodId = payment.PaymentMethodId.GetValueOrDefault(),
            State = payment.State.ToString()
        };
    }
}