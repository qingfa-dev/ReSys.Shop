namespace Module.Ordering.Backgrounds;

public static class CartExpiryJobConstants
{
    public static class Defaults
    {
        public const int AfterDays = 7;
    }

    public static class Scheduler
    {
        public const string JobId = "cart-expiry";
        public const string CronExpression = "0 * * * *";
    }
}
