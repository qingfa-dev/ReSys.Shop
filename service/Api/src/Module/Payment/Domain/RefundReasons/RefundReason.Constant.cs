namespace Module.Payment.Domain.RefundReasons;

public static class RefundReasonConstant
{
    public static class Constraints
    {
        public const int MaxNameLength = 255;
        public const int MaxCodeLength = 50;
    }

    public static class Defaults
    {
        public const bool Active = true;
    }
}