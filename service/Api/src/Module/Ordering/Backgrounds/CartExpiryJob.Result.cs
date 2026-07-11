namespace Module.Ordering.Backgrounds;

public static class CartExpiryJobResult
{
    public static class Success
    {
        public static string Expired(int count) => $"Cart-expiry job completed: {count} drafts expired.";
    }

    public static class Errors
    {
        public static Error NotFound => Error.NotFound(
            code: "CartExpiry.NotFound",
            message: "No expired draft carts found.");
    }
}
