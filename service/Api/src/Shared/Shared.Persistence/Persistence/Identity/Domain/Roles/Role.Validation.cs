using FluentValidation;

using Shared.Security.Identity.Domain.Users;

namespace Shared.Security.Identity.Domain.Roles;

/// <summary>
/// Provides FluentValidation extension methods for Role domain rules,
/// consistent with the pattern established in <see cref="UserValidation"/>.
/// </summary>
public static class RoleValidation
{
    /// <summary>
    /// Validates a role name. When <paramref name="required"/> is true, enforces NotEmpty + MaxLength.
    /// When false, only enforces MaxLength for non-empty values.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ApplyRoleNameRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder,
        bool required = true)
    {
        if (required)
        {
            return ruleBuilder
                .NotEmpty()
                .WithErrorCode(RoleResult.Failure.NameRequired.Code)
                .WithMessage(RoleResult.Failure.NameRequired.Message)
                .MaximumLength(RoleConstant.Constraints.Name.MaxLength)
                .WithErrorCode(RoleResult.Failure.NameTooLong.Code)
                .WithMessage(RoleResult.Failure.NameTooLong.Message);
        }

        return ruleBuilder
            .MaximumLength(RoleConstant.Constraints.Name.MaxLength)
            .WithErrorCode(RoleResult.Failure.NameTooLong.Code)
            .WithMessage(RoleResult.Failure.NameTooLong.Message);
    }

    /// <summary>
    /// Validates a role description. Always optional — only validates MaxLength when a value is provided.
    /// No NotEmpty rule since description is inherently optional.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ApplyRoleDescriptionRules<T>(
        this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .MaximumLength(RoleConstant.Constraints.Description.MaxLength)
            .WithErrorCode(RoleResult.Failure.DescriptionTooLong.Code)
            .WithMessage(RoleResult.Failure.DescriptionTooLong.Message);
    }
}
