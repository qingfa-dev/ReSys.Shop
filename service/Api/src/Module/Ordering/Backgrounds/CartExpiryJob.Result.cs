namespace Module.Ordering.Backgrounds;

// Contract: Success and error factories for CartExpiryJob background sweep operations
public static class CartExpiryJobResult
{
    public static class Success
    {
        /// <summary>Cart-expiry job completed with the given number of expired drafts.</summary>
        public static string Expired(int count) => $"Cart-expiry job completed: {count} drafts expired.";
    }

    public static class Errors
    {
        #region Existence
        /// <summary>No expired draft carts were found to process.</summary>
        public static Error NotFound => Error.NotFound(
            code: "CartExpiry.NotFound",
            message: "No expired draft carts found.");
        #endregion
    }
}