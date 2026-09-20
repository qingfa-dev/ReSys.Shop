namespace Shared.Application.Domain.Concerns.Parameterizable;

public static class ParameterizableConstant
{
    public static class Constraints
    {
        public const int MaxNameLength = 255;
        public const int MaxPresentationLength = 255;
    }

    public static class Defaults
    {
        public const string Empty = "";

        public static class Normalization
        {
            public const char Separator = '-';
        }
    }
}