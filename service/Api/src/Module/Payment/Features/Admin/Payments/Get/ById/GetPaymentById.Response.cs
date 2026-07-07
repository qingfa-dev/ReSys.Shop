using Module.Payment.Domain.Payments;

namespace Module.Payment.Features.Admin.Payments.Get.ById;

public static partial class GetPaymentById
{
    public class Response
    {
        public Guid Id { get; init; }
        public string Number { get; init; } = string.Empty;
        public decimal Amount { get; init; }
        public PaymentState State { get; init; }
        public string? ResponseCode { get; init; }
        public string? IntentClientSecret { get; init; }
        public Guid OrderId { get; init; }
        public string? OrderNumber { get; init; }
        public Guid PaymentMethodId { get; init; }
        public string? PaymentMethodName { get; init; }
        public DateTimeOffset CreatedAtUtc { get; init; }
        public DateTimeOffset? ModifiedAtUtc { get; init; }
    }
}
