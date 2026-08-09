namespace Module.Identity.Features.Shared;

public static partial class IdentityFeature
{
    public static class Tags
    {
        public static readonly string[] User = ["User"];
        public static readonly string[] Role = ["Role"];
        public static readonly string[] Permission = ["Permission"];

        public static readonly string[] Authentication = ["Authentication"];
        public static readonly string[] Authorization = ["Authorization"];
    }
}