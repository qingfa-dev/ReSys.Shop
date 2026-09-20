namespace Shared.Security.Authorization.Attributes;

/// <summary>
/// Convenience attribute. Decorate endpoints like:
///   [HasPermission(PermissionStore.Todos.Create)]
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="HasPermissionAttribute"/>.
/// </remarks>
/// <param name="permission">The required permission name.</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
public class HasPermissionAttribute(string permission) : Microsoft.AspNetCore.Authorization.AuthorizeAttribute(policy: permission)
{
}