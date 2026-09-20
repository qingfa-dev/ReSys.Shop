namespace Shared.Application.Domain.Concerns.DisplayMoney;

public static class DisplayMoney
{
    public static decimal RoundToTwoPlaces(decimal amount) => Math.Round(amount, 2, MidpointRounding.AwayFromZero);
}
