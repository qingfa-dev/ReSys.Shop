namespace Shared.Application.Domain.Concerns.Auditable;

/// <summary>
/// Contains constraints for auditing properties.
/// </summary>
public static class AuditableConstant
{
    public static class Constraints
    {
        public const int MaxCreatedByLength = 100;
        public const int MaxModifiedByLength = 100;
    }
}
