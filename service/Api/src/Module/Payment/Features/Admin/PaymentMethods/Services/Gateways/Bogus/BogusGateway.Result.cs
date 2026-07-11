using Module.Payment.Domain.Gateways;

namespace Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Bogus;

public static class BogusGatewayResult
{
    public static class Errors
    {
        public static Error CardDeclined => Error.BadRequest(
            GatewayConstants.ErrorCodes.Bogus.CardDeclined,
            "Card was declined by issuer.");

        public static Error InsufficientFunds => Error.BadRequest(
            GatewayConstants.ErrorCodes.Bogus.InsufficientFunds,
            "Insufficient funds on the card.");

        public static Error UnknownCard => Error.BadRequest(
            GatewayConstants.ErrorCodes.Bogus.UnknownCard,
            "Unknown test card number.");
    }
}
