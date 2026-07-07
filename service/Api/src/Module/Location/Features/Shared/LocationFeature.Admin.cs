using Shared.Security.Identity.Domain.Permissions;

namespace Module.Location.Features.Shared;

/// <summary>Contains route and permission metadata for the Location feature.</summary>
public static partial class LocationFeature
{
    /// <summary>Admin-facing configuration for countries and states CRUD operations.</summary>
    public static class Admin
    {
        public const string Route = "api/locations";

        /// <summary>Country-related endpoint configuration for admin operations.</summary>
        public static class Countries
        {
            public static readonly string[] Tags = ["Country", "Location"];

            public const string CountryRoute = $"{Route}/countries";

            /// <summary>Create country endpoint configuration.</summary>
            public static class Create
            {
                public const string Route = CountryRoute;
                public const string Description = "Add a new country to the system";
                public const string Summary = "Create a new country";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.Countries.Create;
            }

            /// <summary>Get all countries endpoint configuration.</summary>
            public static class GetAll
            {
                public const string Route = CountryRoute;
                public const string Description = "Fetch all countries with optional filtering";
                public const string Summary = "Retrieve all countries";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.Countries.List;
            }

            /// <summary>Get country by ID endpoint configuration.</summary>
            public static class GetById
            {
                public const string Route = $"{CountryRoute}/{{id:guid}}";
                public const string Description = "Retrieve a specific country by its unique identifier";
                public const string Summary = "Get country by UUID";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.Countries.Read;
            }

            /// <summary>Get country by ISO code endpoint configuration.</summary>
            public static class GetByIso
            {
                public const string Route = $"{CountryRoute}/by-iso/{{isoCode}}";
                public const string Description = "Find a country using its ISO 3166-1 alpha-2 or alpha-3 code";
                public const string Summary = "Find country by ISO code";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.Countries.Read;
            }

            /// <summary>Update country endpoint configuration.</summary>
            public static class Update
            {
                public const string Route = $"{CountryRoute}/{{id:guid}}";
                public const string Description = "Modify an existing country's details";
                public const string Summary = "Update country information";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.Countries.Update;
            }

            /// <summary>Delete country endpoint configuration.</summary>
            public static class Delete
            {
                public const string Route = $"{CountryRoute}/{{id:guid}}";
                public const string Description = "Remove a country from the system permanently";
                public const string Summary = "Remove a country";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.Countries.Delete;
            }
        }

        /// <summary>State-related endpoint configuration for admin operations.</summary>
        public static class States
        {
            public static readonly string[] Tags = ["State", "Location"];

            public const string StateRoute = $"{Route}/states";

            /// <summary>Create state endpoint configuration.</summary>
            public static class Create
            {
                public const string Route = StateRoute;
                public const string Description = "Add a new state to the system";
                public const string Summary = "Create a new state";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.States.Create;
            }

            /// <summary>Get all states endpoint configuration.</summary>
            public static class GetAll
            {
                public const string Route = StateRoute;
                public const string Description = "Fetch all states with optional filtering";
                public const string Summary = "Retrieve all states";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.States.List;
            }

            /// <summary>Get state by ID endpoint configuration.</summary>
            public static class GetById
            {
                public const string Route = $"{StateRoute}/{{id:guid}}";
                public const string Description = "Retrieve a specific state by its unique identifier";
                public const string Summary = "Get state by UUID";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.States.Read;
            }

            /// <summary>Get state by ISO code endpoint configuration.</summary>
            public static class GetByIso
            {
                public const string Route = $"{StateRoute}/by-iso/{{isoCode}}";
                public const string Description = "Find a state using its ISO 3166-2 subdivision code";
                public const string Summary = "Find state by ISO code";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.States.Read;
            }

            /// <summary>Update state endpoint configuration.</summary>
            public static class Update
            {
                public const string Route = $"{StateRoute}/{{id:guid}}";
                public const string Description = "Modify an existing state's details";
                public const string Summary = "Update state information";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.States.Update;
            }

            /// <summary>Delete state endpoint configuration.</summary>
            public static class Delete
            {
                public const string Route = $"{StateRoute}/{{id:guid}}";
                public const string Description = "Remove a state from the system permanently";
                public const string Summary = "Remove a state";
                /// <summary>Required permission for this endpoint.</summary>
                public static PermissionMetadata Permission => LocationFeatureMetadata.States.Delete;
            }
        }
    }
}