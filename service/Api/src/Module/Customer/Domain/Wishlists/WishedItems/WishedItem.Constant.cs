// Policy: Quantity constraints prevent negative or unrealistically large order amounts
namespace Module.Customer.Domain.Wishlists.WishedItems;

public static class WishedItemConstant
{
    public static class Defaults
    {
        public const int Quantity = 1;
    }

    public static class Constraints
    {
        public const int MinQuantity = 1;
        public const int MaxQuantity = 999;
    }
}