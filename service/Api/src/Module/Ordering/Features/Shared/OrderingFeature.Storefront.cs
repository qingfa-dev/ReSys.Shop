namespace Module.Ordering.Features.Shared;

public static partial class OrderingFeature
{
    // Context: Storefront API route and description metadata for Ordering module — consumed by Carter endpoints and OpenAPI
    public static class Storefront
    {
        public const string Route = "api/storefront";

        public static class Cart
        {
            public const string BaseRoute = $"{Route}/cart";

            public static class Get
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve the current user's shopping cart";
                public const string Summary = "Get shopping cart";
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new shopping cart";
                public const string Summary = "Create cart";
            }

            public static class Associate
            {
                public const string Route = $"{BaseRoute}/associate";
                public const string Description = "Associate a guest cart with the current user";
                public const string Summary = "Associate cart with user";
            }

            public static class AddItem
            {
                public const string Route = $"{BaseRoute}/items";
                public const string Description = "Add an item to the shopping cart";
                public const string Summary = "Add to cart";
            }

            public static class RemoveItem
            {
                public const string Route = $"{BaseRoute}/items/{{lineItemId:guid}}";
                public const string Description = "Remove a line item from the cart";
                public const string Summary = "Remove cart item";
            }

            public static class UpdateItemQuantity
            {
                public const string Route = $"{BaseRoute}/items/{{lineItemId:guid}}";
                public const string Description = "Update the quantity of a cart line item";
                public const string Summary = "Update cart item quantity";
            }

            public static class Empty
            {
                public const string Route = $"{BaseRoute}/empty";
                public const string Description = "Remove all items from the cart";
                public const string Summary = "Empty cart";
            }

            public static class Delete
            {
                public const string Route = BaseRoute;
                public const string Description = "Delete the shopping cart";
                public const string Summary = "Delete cart";
            }

            public static class Checkout
            {
                public const string Route = $"{BaseRoute}/checkout";
                public const string Description = "Create an order from the current cart";
                public const string Summary = "Checkout cart";
            }

            public static class Update
            {
                public const string Route = BaseRoute;
                public const string Description = "Update cart checkout details (email, addresses, special instructions)";
                public const string Summary = "Update checkout";
            }

            public static class Validate
            {
                public const string Route = $"{BaseRoute}/validate";
                public const string Description = "Validate the current checkout state";
                public const string Summary = "Validate checkout";
            }

            public static class SelectShippingRate
            {
                public const string Route = $"{BaseRoute}/shipping-rate";
                public const string Description = "Select a shipping rate for the order";
                public const string Summary = "Select shipping rate";
            }
        }

        public static class Orders
        {
            public const string BaseRoute = $"{Route}/orders";

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve an order by identifier (customer facing)";
                public const string Summary = "Get customer order";
            }

            public static class Cancel
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/cancel";
                public const string Description = "Cancel an order";
                public const string Summary = "Cancel order";
            }

            public static class List
            {
                public const string Route = BaseRoute;
                public const string Description = "List current user's orders";
                public const string Summary = "List orders";
            }
        }
    }
}