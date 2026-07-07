namespace Module.Ordering.Domain.Orders;

public static class PaymentRecordExtensions
{
    public static PaymentRecord Create(decimal amount, string state, bool isStoreCredit = false)
    {
        return new PaymentRecord(amount, state, isStoreCredit);
    }

    public static bool IsFailed(this PaymentRecord payment)
    {
        return payment.State is "failed" or "invalid";
    }

    public static bool IsCompleted(this PaymentRecord payment)
    {
        return payment.State == "completed";
    }

    public static bool IsPending(this PaymentRecord payment)
    {
        return payment.State == "pending";
    }
}
