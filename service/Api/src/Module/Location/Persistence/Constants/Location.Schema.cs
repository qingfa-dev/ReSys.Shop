using Module.Location.Domain.Countries;
using Module.Location.Domain.States;

using Shared.Governance.Conventions;

namespace Module.Location.Persistence.Constants;

public static class LocationSchema
{
    public static string Name => "Locations".ToSnakeCase()!;

    public static class TableNames
    {
        public static string Countries => nameof(Country).ToSnakeCase();
        public static string States => nameof(State).ToSnakeCase();
    }
}
