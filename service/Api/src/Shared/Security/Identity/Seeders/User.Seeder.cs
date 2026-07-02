using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using Shared.Operational.Persistence.Data;
using Shared.Operational.Persistence.Seeders;
using Shared.Security.Identity.Domain.Roles;
using Shared.Security.Identity.Domain.Users;
using Shared.Security.Identity.Domain.Users.Roles;

namespace Shared.Security.Identity.Seeders;

public sealed class UserSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;

    public override int Order => 40;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasUsers = await HasDataAsync<User>(cancellationToken);
        if (hasUsers)
        {
            return Result.Ok();
        }

        var adminRole = await Context.Set<Role>().FirstAsync(r => r.Name == RoleConstant.Defaults.Admin, cancellationToken);
        var managerRole = await Context.Set<Role>().FirstAsync(r => r.Name == RoleConstant.Defaults.Manager, cancellationToken);
        var userRole = await Context.Set<Role>().FirstAsync(r => r.Name == RoleConstant.Defaults.User, cancellationToken);

        var hasher = new PasswordHasher<User>();

        var adminUser = CreateUser("admin", "admin@resys.shop", "Admin", "User", hasher, "Admin@123!",
            dateOfBirth: new DateTimeOffset(1985, 6, 15, 0, 0, 0, TimeSpan.Zero),
            phoneNumber: "+12025550101");
        var managers = new[]
        {
            CreateUser("manager1", "manager1@resys.shop", "Manager", "One", hasher, "Manager@123!",
                dateOfBirth: new DateTimeOffset(1990, 3, 10, 0, 0, 0, TimeSpan.Zero),
                phoneNumber: "+12025550201"),
            CreateUser("manager2", "manager2@resys.shop", "Manager", "Two", hasher, "Manager@123!",
                dateOfBirth: new DateTimeOffset(1991, 7, 22, 0, 0, 0, TimeSpan.Zero),
                phoneNumber: "+12025550202"),
            CreateUser("manager3", "manager3@resys.shop", "Manager", "Three", hasher, "Manager@123!",
                dateOfBirth: new DateTimeOffset(1992, 11, 5, 0, 0, 0, TimeSpan.Zero),
                phoneNumber: "+12025550203"),
        };
        var users = new[]
        {
            CreateUser("user1", "user1@resys.shop", "User", "One", hasher, "User@123!",
                dateOfBirth: new DateTimeOffset(1995, 1, 15, 0, 0, 0, TimeSpan.Zero),
                phoneNumber: "+12025550301"),
            CreateUser("user2", "user2@resys.shop", "User", "Two", hasher, "User@123!",
                dateOfBirth: new DateTimeOffset(1996, 4, 20, 0, 0, 0, TimeSpan.Zero),
                phoneNumber: "+12025550302"),
            CreateUser("user3", "user3@resys.shop", "User", "Three", hasher, "User@123!",
                dateOfBirth: new DateTimeOffset(1997, 8, 30, 0, 0, 0, TimeSpan.Zero),
                phoneNumber: "+12025550303"),
        };

        var allUsers = new[] { adminUser }.Concat(managers).Concat(users).ToArray();
        Context.Set<User>().AddRange(allUsers);

        Context.Set<UserRole>().Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
        Context.Set<UserRole>().AddRange(managers.Select(m => new UserRole { UserId = m.Id, RoleId = managerRole.Id }));
        Context.Set<UserRole>().AddRange(users.Select(u => new UserRole { UserId = u.Id, RoleId = userRole.Id }));

        await Context.SaveChangesAsync(cancellationToken);

        return Result.Ok();
    }

    private static User CreateUser(
        string userName,
        string email,
        string firstName,
        string lastName,
        PasswordHasher<User> hasher,
        string password,
        DateTimeOffset? dateOfBirth = null,
        string? phoneNumber = null)
    {
        Result<User> result = UserMethod.Create(userName, email, firstName, lastName);

        if (result.IsFailure)
        {
            throw new InvalidOperationException(
                $"Failed to create user '{userName}': {result.Errors[0].Code} - {result.Errors[0].Message}");
        }

        User user = result.Value;
        user.NormalizedUserName = userName.ToUpperInvariant();
        user.NormalizedEmail = email.ToUpperInvariant();
        user.EmailConfirmed = true;
        user.PhoneNumber = phoneNumber;
        user.PhoneNumberConfirmed = true;
        user.IsActive = true;
        user.DateOfBirth = dateOfBirth;
        user.SecurityStamp = Guid.NewGuid().ToString();
        user.ConcurrencyStamp = Guid.NewGuid().ToString();
        user.CreatedAtUtc = UtcNow;
        user.CreatedBy = "System";
        user.PasswordHash = hasher.HashPassword(user, password);

        return user;
    }
}
