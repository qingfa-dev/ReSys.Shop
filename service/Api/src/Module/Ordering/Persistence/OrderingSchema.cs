namespace Module.Ordering.Persistence;

// Context: Schema and table name constants for the Ordering module — referenced by EF Core configurations and migrations
// @CAT-10 Boundary: Persistence → Database — schema namespace for all Ordering module tables
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