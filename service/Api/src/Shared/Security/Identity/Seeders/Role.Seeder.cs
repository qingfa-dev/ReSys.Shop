using Shared.Operational.Persistence.Data;
using Shared.Operational.Persistence.Seeders;
using Shared.Security.Identity.Domain.Permissions;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Roles.Claims;

namespace Shared.Security.Identity.Seeders;

public sealed class RoleSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;

    public override int Order => 30;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        bool hasRoles = await HasDataAsync<Role>(cancellationToken);
        if (hasRoles)
        {
            return Result.Ok();
        }

        Role adminRole = CreateRole(RoleConstant.Defaults.Admin, "Default Admin role with system-level permissions");
        Role managerRole = CreateRole(RoleConstant.Defaults.Manager, "Default Manager role with operational management permissions");
        Role userRole = CreateRole(RoleConstant.Defaults.User, "Default User role with read-only customer permissions");

        Context.Set<Role>().AddRange([adminRole, managerRole, userRole]);

        Context.Set<RoleClaim>()
            .AddRange(CreateRoleClaims(adminRole.Id, RoleConstant.RolePermissions.Admin));
        Context.Set<RoleClaim>()
            .AddRange(CreateRoleClaims(managerRole.Id, RoleConstant.RolePermissions.Manager));
        Context.Set<RoleClaim>()
            .AddRange(CreateRoleClaims(userRole.Id, RoleConstant.RolePermissions.User));

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private static Role CreateRole(string name, string description)
    {
        Result<Role> result = RoleMethod.Create(name, description);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to create role '{name}': {result.Errors[0].Code} - {result.Errors[0].Message}");
        }

        Role role = result.Value;
        role.NormalizedName = name.ToUpperInvariant();
        role.IsSystem = true;
        role.CreatedAtUtc = UtcNow;
        role.CreatedBy = "System";
        role.ConcurrencyStamp = Guid.NewGuid().ToString();

        return role;
    }

    private static IEnumerable<RoleClaim> CreateRoleClaims(Guid roleId, IReadOnlyList<PermissionMetadata> permissions)
    {
        return permissions.Select(p => new RoleClaim
        {
            RoleId = roleId,
            ClaimType = PermissionMetadataConstant.ClaimType,
            ClaimValue = p.Identifier,
        });
    }
}
