namespace Shared.Application.Domain.Concerns.SoftDeletable;

/// <summary>
/// Contains constraints for soft delete properties.
/// </summary>
public static class SoftDeletableConstant
{
    public static class Constraints
    {
        public const int MaxDeletedByLength = 100;
    }
}
