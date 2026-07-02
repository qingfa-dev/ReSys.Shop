using Shared.Application.Domain.Concerns.Auditable;

namespace Shared.Security.Identity.Domain.Roles;

public static class RoleMethod
{
    #region Create
    public static Result<Role> Create(string name, Optional<string> description = default)
    {
        // Validate: Check required fields
        if (string.IsNullOrWhiteSpace(name))
            return RoleResult.Failure.NameRequired;
        if (name.Length > RoleConstant.Constraints.Name.MaxLength)
            return RoleResult.Failure.NameTooLong;
        if (description.HasValue && description.Value.Length > RoleConstant.Constraints.Description.MaxLength)
            return RoleResult.Failure.DescriptionTooLong;

        // Create: Instantiate role with optional description
        Role entity = new() { Name = name };
        description.Apply(x => entity.Description = x);

        AuditableBehavior.Create(entity);

        // Map: Return created role
        return entity;
    }
    #endregion

    #region Update
    public static Result<Role> Update(this Role entity, Optional<string> name = default, Optional<string> description = default)
    {
        // Validate
        ArgumentNullException.ThrowIfNull(entity);

        if (entity.IsSystem)
            return RoleResult.Failure.SystemRoleProtected;

        bool isChanged = false;

        // Update name
        if (name.HasValue)
        {
            string newName = name.Value;
            if (string.IsNullOrWhiteSpace(newName))
                return RoleResult.Failure.NameRequired;
            if (newName.Length > RoleConstant.Constraints.Name.MaxLength)
                return RoleResult.Failure.NameTooLong;

            isChanged |= name.ApplyIfChanged(entity.Name!, x => entity.Name = x);
        }

        // Update description
        if (description.HasValue)
        {
            string newDescription = description.Value;
            if (newDescription.Length > RoleConstant.Constraints.Description.MaxLength)
                return RoleResult.Failure.DescriptionTooLong;

            isChanged |= description.ApplyIfChanged(entity.Description!, x => entity.Description = newDescription);
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
