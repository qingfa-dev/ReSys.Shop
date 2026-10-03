namespace Module.Inventory.Backgrounds;

public static class ReservationExpiryJobResult
{
    public static class Success
    {
        public static string Expired(int count) => $"Expired {count} stock reservations and restored stock.";
    }

    public static class Errors
    {
        public static Error SweepFailed => Error.Conflict(
            code: "ReservationExpiry.SweepFailed",
            message: "Reservation expiry sweep failed.");
    }
}