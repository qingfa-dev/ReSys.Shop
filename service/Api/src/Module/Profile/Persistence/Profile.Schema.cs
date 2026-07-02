using Shared.Governance.Conventions;

namespace Module.Profile.Persistence;

public static class ProfileSchema
{
    public static string Name => "Profile".ToSnakeCase()!;

    public static class TableNames
    {
        public static string Profiles => "UserProfiles".ToSnakeCase()!;
        public static string Addresses => "Addresses".ToSnakeCase()!;
        public static string Wishlists => "Wishlists".ToSnakeCase()!;
        public static string WishedItems => "WishedItems".ToSnakeCase()!;
    }
}