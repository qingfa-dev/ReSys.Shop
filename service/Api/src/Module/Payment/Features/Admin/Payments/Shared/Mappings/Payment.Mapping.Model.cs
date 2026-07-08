using PaymentRecord = Module.Payment.Domain.Payments.PaymentRecord;

namespace Module.Payment.Features.Admin.Payments.Shared.Mappings;

public static class PaymentModelMapping
{
    public static T MapToDetail<T>(this PaymentRecord payment) where T : Models.PaymentDetailResponse, new()
    {
        return new T
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Currency = string.Empty,
            OrderId = payment.OrderId,
            PaymentMethodId = payment.PaymentMethodId,
            State = payment.State.ToString(),
            ClientSecret = payment.IntentClientSecret,
            CreatedAtUtc = payment.CreatedAtUtc,
            ModifiedAtUtc = payment.ModifiedAtUtc,
            CreatedBy = payment.CreatedBy,
            ModifiedBy = payment.ModifiedBy
        };
    }

    public static T MapToListItem<T>(this PaymentRecord payment) where T : Models.PaymentListItemResponse, new()
    {
        return new T
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Currency = string.Empty,
            OrderId = payment.OrderId,
            PaymentMethodId = payment.PaymentMethodId,
            State = payment.State.ToString()
        };
    }
}
