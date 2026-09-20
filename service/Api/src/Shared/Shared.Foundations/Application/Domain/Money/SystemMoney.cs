namespace Shared.Application.Domain.Money;

public readonly record struct SystemMoney(decimal Amount, string CurrencyCode)
{
    public override string ToString() => $"{Amount:F2} {CurrencyCode}";
}
