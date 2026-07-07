namespace Module.Promotions.Domain.Actions;
/// <summary>Represents a Action Result.</summary>

public static class ActionResult
{
    public static class Errors
    {
        public static Error OrderRequired => Error.Validation(
            code: "Action.Order.Required",
            description: "Order is required to perform the action.");
    }
}