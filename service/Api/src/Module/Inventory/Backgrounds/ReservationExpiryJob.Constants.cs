namespace Module.Inventory.Backgrounds;

public static class ReservationExpiryJobConstants
{
    public static class Defaults
    {
        public const int SweepIntervalSeconds = 60;
    }

    public static class Scheduler
    {
        public const string JobId = "reservation-expiry";
        public const string CronExpression = "*/1 * * * *";
    }
}
