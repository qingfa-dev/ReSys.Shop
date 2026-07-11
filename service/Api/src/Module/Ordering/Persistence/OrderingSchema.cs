namespace Module.Ordering.Persistence;

public static class OrderingSchema
{
    public const string Name = "ordering";

    public static class TableNames
    {
        public const string Orders = "orders";
        public const string LineItems = "line_items";
        public const string Adjustments = "adjustments";
    }
}
