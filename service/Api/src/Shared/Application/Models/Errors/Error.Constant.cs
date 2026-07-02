namespace Shared.Application.Models.Errors;

public static class ErrorConstant
{
    public static class Constraints
    {
        public const int MaxCodeLength = 256;
        public const int MaxMessageLength = 2048;
    }

    public static class DefaultValues
    {
        public const string Code = "General.Unexpected";
        public const string Message = "An unexpected error occurred.";

        public const int Type = ErrorType.Unexpected;
    }

    public static class Patterns
    {
        public const string Code =
            @"^[A-Z][A-Za-z0-9]*(\.[A-Z][A-Za-z0-9]*)+$";
    }

    public static class Metadata
    {
        public const string Field = "Field";
        public const string Resource = "Resource";
        public const string ResourceId = "ResourceId";
        public const string AttemptedValue = "AttemptedValue";
    }
}