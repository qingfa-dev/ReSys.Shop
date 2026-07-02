using Shared.Application.Domain.Concerns.Auditable;

namespace Shared.Security.Identity.Domain.Roles;

public static class RoleMethod
{
    #region Create
    public static Result<Role> Create(string name, string? description = default)
    {
        // Validate: Check required fields
        if (string.IsNullOrWhiteSpace(name))
            return RoleResult.Failure.NameRequired;
        if (name.Length > RoleConstant.Constraints.Name.MaxLength)
            return RoleResult.Failure.NameTooLong;
        if (description is not null && description.Length > RoleConstant.Constraints.Description.MaxLength)
            return RoleResult.Failure.DescriptionTooLong;

        // Create: Instantiate role with optional description
        Role entity = new() { Name = name };
        if (description is not null)
            entity.Description = description;

        AuditableBehavior.Create(entity);

        // Map: Return created role
        return entity;
    }
    #endregion

    #region Update
    public static Result<Role> Update(this Role entity, string? name = default, string? description = default)
    {
        // Validate
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.IsSystem)
            return RoleResult.Failure.SystemRoleProtected;

        bool isChanged = false;

        // Update name
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return RoleResult.Failure.NameRequired;
            if (name.Length > RoleConstant.Constraints.Name.MaxLength)
                return RoleResult.Failure.NameTooLong;

            if (name != entity.Name)
            {
                entity.Name = name;
                isChanged = true;
            }
        }

        // Update description
        if (description is not null)
        {
            if (description.Length > RoleConstant.Constraints.Description.MaxLength)
                return RoleResult.Failure.DescriptionTooLong;

            if (description != entity.Description)
            {
                entity.Description = description;
                isChanged = true;
            }
        }

        // Audit only when changed
        if (isChanged)
            AuditableBehavior.Touch(entity);

        return entity;
    }
    #endregion

    #region Delete
    public static Result<Role> Delete(Role entity)
    {
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.IsSystem)
            return RoleResult.Failure.SystemRoleProtected;
        if (entity.UserRoles.Count > 0)
            return RoleResult.Failure.CannotDeleteRoleWithUsers;

        return Result<Role>.NoContent();
    }
    #endregion
}
