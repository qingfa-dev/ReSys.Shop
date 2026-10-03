using Shared.Security.Identity.Domain.Permissions;

namespace Module.Customer.Features.Shared;

public static partial class ProfileFeature
{
    public static class Storefront
    {
        public static class Profiles
        {
            public static class Get
            {
                public const string Route = "api/storefront/customer";
                public const string Description = "Retrieve the authenticated user's profile";
                public const string Summary = "Get profile";
            }

            public static class Create
            {
                public const string Route = "api/storefront/customer";
                public const string Description = "Create a new profile for the authenticated user";
                public const string Summary = "Create profile";
            }

            public static class Update
            {
                public const string Route = "api/storefront/customer";
                public const string Description = "Update the authenticated user's profile";
                public const string Summary = "Update profile";
            }

            public static class Delete
            {
                public const string Route = "api/storefront/customer";
                public const string Description = "Deactivate the authenticated user's profile";
                public const string Summary = "Delete profile";
            }
        }

        public static class Addresses
        {
            public static class Create
            {
                public const string Route = "api/storefront/customer/addresses";
                public const string Description = "Create a new address";
                public const string Summary = "Create address";
            }

            public static class GetAll
            {
                public const string Route = "api/storefront/customer/addresses";
                public const string Description = "Retrieve all user addresses";
                public const string Summary = "Get all addresses";
            }

            public static class GetById
            {
                public const string Route = "api/storefront/customer/addresses/{id:guid}";
                public const string Description = "Retrieve an address by identifier";
                public const string Summary = "Get address by ID";
            }

            public static class Update
            {
                public const string Route = "api/storefront/customer/addresses/{id:guid}";
                public const string Description = "Update an existing address";
                public const string Summary = "Update address";
            }

            public static class Delete
            {
                public const string Route = "api/storefront/customer/addresses/{id:guid}";
                public const string Description = "Delete an address";
                public const string Summary = "Delete address";
            }

            public static class GetDefault
            {
                public const string Route = "api/storefront/customer/addresses/default";
                public const string Description = "Retrieve the user's default address";
                public const string Summary = "Get default address";
            }
        }

        public static class NotificationPreferences
        {
        public static class Get
            {
                public const string Route = "api/storefront/customer/notification-preferences";
                public const string Description = "Retrieve the authenticated user's profile";
                public const string Summary = "Get profile";
                public static PermissionMetadata Permission => ProfileFeatureMetadata.UserProfile.Read;
            }

        public static class Update
            {
                public const string Route = "api/storefront/customer/notification-preferences";
                public const string Description = "Update the authenticated user's profile";
                public const string Summary = "Update profile";
                public static PermissionMetadata Permission => ProfileFeatureMetadata.UserProfile.Update;
            }
        }

        public static class Wishlists
        {
            public static class GetAll
            {
                public const string Route = "api/storefront/customer/wishlists";
                public const string Description = "List the authenticated user's wishlists";
                public const string Summary = "List wishlists";
            }

            public static class GetById
            {
                public const string Route = "api/storefront/customer/wishlists/{id:guid}";
                public const string Description = "Retrieve a wishlist by identifier";
                public const string Summary = "Get wishlist by ID";
            }

        public static class Create
            {
                public const string Route = "api/storefront/customer/wishlists";
                public const string Description = "Create a new profile for the authenticated user";
                public const string Summary = "Create profile";
                public static PermissionMetadata Permission => ProfileFeatureMetadata.UserProfile.Create;
            }

            public static class Update
            {
                public const string Route = "api/storefront/customer/wishlists/{id:guid}";
                public const string Description = "Update a wishlist's name, privacy, or default flag";
                public const string Summary = "Update wishlist";
            }

            public static class Delete
            {
                public const string Route = "api/storefront/customer/wishlists/{id:guid}";
                public const string Description = "Soft-delete a wishlist";
                public const string Summary = "Delete wishlist";
            }

            public static class AddItem
            {
                public const string Route = "api/storefront/customer/wishlists/{id:guid}/items";
                public const string Description = "Add a variant to a wishlist";
                public const string Summary = "Add item to wishlist";
            }

            public static class RemoveItem
            {
                public const string Route = "api/storefront/customer/wishlists/{id:guid}/items/{itemId:guid}";
                public const string Description = "Remove an item from a wishlist";
                public const string Summary = "Remove wishlist item";
            }
        }
    }
}
