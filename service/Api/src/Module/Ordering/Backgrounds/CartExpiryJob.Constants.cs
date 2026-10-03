namespace Module.Ordering.Backgrounds;

// Initialize: Configuration constants for the CartExpiryJob — sweep interval, job identifier, and cron schedule
public static class CartExpiryJobConstants
{
    public static class Defaults
    {
        // Initialize: Carts inactive for 7+ days are eligible for expiry
        public const int AfterDays = 7;
    }

    public static class Scheduler
    {
        // Initialize: Unique Hangfire job identifier for management and monitoring
        public const string JobId = "cart-expiry";
        // Initialize: Run every hour on the hour via Hangfire recurring job
        public const string CronExpression = "0 * * * *";
    }
}