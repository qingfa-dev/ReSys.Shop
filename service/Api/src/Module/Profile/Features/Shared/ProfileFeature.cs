using Shared.Security.Identity.Domain.Permissions;

namespace Module.Profile.Features.Shared;

public static class ProfileFeature
{
    public static class Tags
    {
        public static readonly string[] Address = ["Address"];
        public static readonly string[] Profile = ["Profile"];
        public static readonly string[] NotificationPreferences = ["NotificationPreferences"];
        public static readonly string[] Wishlist = ["Wishlist"];
    }

    public static class Store
    {
        private const string StoreRoute = "api/store/profiles";

        public static class Profile
        {
            private const string BaseRoute = $"{StoreRoute}/profiles";

            public static class Get
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve the authenticated user's profile";
                public const string Summary = "Get profile";
            }

            public static class GetAll
            {
                public const string Route = $"{BaseRoute}/all";
                public const string Description = "Retrieve all user profiles with pagination and filtering";
                public const string Summary = "Get all profiles";
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new profile for the authenticated user";
                public const string Summary = "Create profile";
            }

            public static class Update
            {
                public const string Route = BaseRoute;
                public const string Description = "Update the authenticated user's profile";
                public const string Summary = "Update profile";
            }

            public static class Delete
            {
                public const string Route = BaseRoute;
                public const string Description = "Deactivate the authenticated user's profile";
                public const string Summary = "Delete profile";
            }
        }

        public static class Addresses
        {
            private const string BaseRoute = $"{StoreRoute}/addresses";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new address";
                public const string Summary = "Create address";
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all user addresses";
                public const string Summary = "Get all addresses";
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve an address by identifier";
                public const string Summary = "Get address by ID";
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update an existing address";
                public const string Summary = "Update address";
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete an address";
                public const string Summary = "Delete address";
            }
        }

        public static class NotificationPreferences
        {
            private const string BaseRoute = $"{StoreRoute}/notification-preferences";

            public static class Get
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve the authenticated user's notification preferences";
                public const string Summary = "Get notification preferences";
            }

            public static class Update
            {
                public const string Route = BaseRoute;
                public const string Description = "Update the authenticated user's notification preferences";
                public const string Summary = "Update notification preferences";
            }
        }

        public static class Wishlists
        {
            private const string BaseRoute = $"{StoreRoute}/wishlists";

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "List the authenticated user's wishlists";
                public const string Summary = "List wishlists";
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve a wishlist by identifier";
                public const string Summary = "Get wishlist by ID";
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new wishlist";
                public const string Summary = "Create wishlist";
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update a wishlist's name, privacy, or default flag";
                public const string Summary = "Update wishlist";
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Soft-delete a wishlist";
                public const string Summary = "Delete wishlist";
            }

            public static class AddItem
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/items";
                public const string Description = "Add a variant to a wishlist";
                public const string Summary = "Add item to wishlist";
            }

            public static class RemoveItem
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}/items/{{itemId:guid}}";
                public const string Description = "Remove an item from a wishlist";
                public const string Summary = "Remove wishlist item";
            }
        }
    }

    public static class Admin
    {
        private const string AdminStore = "api/profiles";

        public static class Profiles
        {
            private const string BaseRoute = $"{AdminStore}/profiles";

            public static class Get
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve the authenticated user's profile";
                public const string Summary = "Get profile";
            }

            public static class GetAll
            {
                public const string Route = $"{BaseRoute}/all";
                public const string Description = "Retrieve all user profiles with pagination and filtering";
                public const string Summary = "Get all profiles";
                public static readonly PermissionMetadata Permission = new("Profile", "Admin", "Profiles", "GetAll");
            }

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new profile for the authenticated user";
                public const string Summary = "Create profile";
            }

            public static class Update
            {
                public const string Route = BaseRoute;
                public const string Description = "Update the authenticated user's profile";
                public const string Summary = "Update profile";
            }

            public static class Delete
            {
                public const string Route = BaseRoute;
                public const string Description = "Deactivate the authenticated user's profile";
                public const string Summary = "Delete profile";
            }
        }

        public static class Addresses
        {
            private const string BaseRoute = $"{AdminStore}/addresses";

            public static class Create
            {
                public const string Route = BaseRoute;
                public const string Description = "Create a new address";
                public const string Summary = "Create address";
            }

            public static class GetAll
            {
                public const string Route = BaseRoute;
                public const string Description = "Retrieve all user addresses";
                public const string Summary = "Get all addresses";
            }

            public static class GetById
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Retrieve an address by identifier";
                public const string Summary = "Get address by ID";
            }

            public static class Update
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Update an existing address";
                public const string Summary = "Update address";
            }

            public static class Delete
            {
                public const string Route = $"{BaseRoute}/{{id:guid}}";
                public const string Description = "Delete an address";
                public const string Summary = "Delete address";
            }
        }
    }
}