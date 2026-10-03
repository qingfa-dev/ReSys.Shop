namespace Module.Location.Features.Shared;

public static partial class LocationFeature
{
    public static class Storefront
    {
        public static class Countries
        {
            public static readonly string[] Tags = ["Country", "Location"];

            public static class GetAll
            {
                public const string Route = "api/storefront/location/countries";
                public const string Description = "Fetch all countries with optional filtering";
                public const string Summary = "Retrieve all countries";
            }

            public static class GetById
            {
                public const string Route = "api/storefront/location/countries/{id:guid}";
                public const string Description = "Retrieve a specific country by its unique identifier";
                public const string Summary = "Get country by UUID";
            }

            public static class GetByIso
            {
                public const string Route = "api/storefront/location/countries/by-iso/{isoCode:regex(^[A-Za-z]{{2,3}}$)}";
                public const string Description = "Find a country using its ISO 3166-1 alpha-2 or alpha-3 code";
                public const string Summary = "Find country by ISO code";
            }
        }

        public static class States
        {
            public static readonly string[] Tags = ["State", "Location"];

            public static class GetAll
            {
                public const string Route = "api/storefront/location/states";
                public const string Description = "Fetch all states with optional filtering";
                public const string Summary = "Retrieve all states";
            }

            public static class GetById
            {
                public const string Route = "api/storefront/location/states/{id:guid}";
                public const string Description = "Retrieve a specific state by its unique identifier";
                public const string Summary = "Get state by UUID";
            }

            public static class GetByIso
            {
                public const string Route = "api/storefront/location/states/by-iso/{isoCode:regex(^[A-Za-z0-9]{{1,5}}$)}";
                public const string Description = "Find a state using its ISO 3166-2 subdivision code";
                public const string Summary = "Find state by ISO code";
            }
        }
    }
}
