using Module.Billing.Domain.PaymentCaptures;

namespace Module.Billing.Features.Admin.Shared.Mappings;

public static class PaymentRecordMapping
{
    public static PaymentCapture MapToDomain<T>(this T parameters) where T : Models.PaymentParameters
    {
        return PaymentCaptureMethod.Create(
            parameters.Amount,
            parameters.PaymentMethodId,
            parameters.OrderId).Value;
    }
}

public static class PaymentModelMapping
{
    public static T MapToDetail<T>(this PaymentCapture payment) where T : Models.PaymentDetailResponse, new()
    {
        return new T
        {
            Id = payment.Id,
            Number = payment.Number,
            Amount = payment.Amount,
            Currency = payment.Currency,
            OrderId = payment.OrderId,
            PaymentMethodId = payment.PaymentMethodId.GetValueOrDefault(),
            State = payment.State,
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
            Currency = payment.Currency,
            OrderId = payment.OrderId,
            PaymentMethodId = payment.PaymentMethodId.GetValueOrDefault(),
            State = payment.State
        };
    }
}
