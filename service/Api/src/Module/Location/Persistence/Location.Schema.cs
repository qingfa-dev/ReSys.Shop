using Module.Location.Domain.Countries;
using Module.Location.Domain.States;

namespace Module.Location.Persistence;

public static class LocationSchema
{
    public static string Name => "Location".ToSnakeCase()!;

    public static class TableNames
    {
        public static string Countries => nameof(Country).ToSnakeCase();
        public static string States => nameof(State).ToSnakeCase();
    }
}
