using Shared.Application.Models.Errors;

namespace Module.Payment.Infrastructure.Gateways.Bogus;

/// <summary>Error factory for BogusGateway — mirrors the StripeGatewayResult.Errors pattern.</summary>
public static class BogusGatewayResult
{
    public static class Errors
    {
        public static Error CardDeclined => Error.BadRequest(
            code: "Bogus.CardDeclined",
            message: "Card was declined by issuer.");

        public static Error InsufficientFunds => Error.BadRequest(
            code: "Bogus.InsufficientFunds",
            message: "Insufficient funds on the card.");

        public static Error UnknownCard => Error.BadRequest(
            code: "Bogus.UnknownCard",
            message: "Unknown test card number.");
    }
}
