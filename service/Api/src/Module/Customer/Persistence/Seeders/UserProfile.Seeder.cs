using Module.Customer.Domain;

using Shared.Security.Identity.Domain.Users;

namespace Module.Customer.Persistence.Seeders;

public sealed class UserProfileSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    private static readonly DateTimeOffset UtcNow = DateTimeOffset.UtcNow;

    public override int Order => 50;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasProfiles = await HasDataAsync<UserProfile>(cancellationToken);
        if (hasProfiles)
        {
            return Result.Ok();
        }

        var users = await Context.Set<User>().ToListAsync(cancellationToken);

        var profiles = users.Select(u =>
        {
            Result<UserProfile> result = UserProfileMethod.Create(u.FirstName,
                u.LastName!,
                u.Email!,
                u.PhoneNumber,
                u.Id);

            if (result.IsFailure)
            {
                throw new InvalidOperationException(
                    $"Failed to create profile for user '{u.Id}': {result.Errors.FirstOrDefault().Message}");
            }

            UserProfile profile = result.Value;
            profile.CreatedAtUtc = UtcNow;
            profile.CreatedBy = "System";
            return profile;
        });

        Context.Set<UserProfile>().AddRange(profiles);

        await SaveChangesWithIdempotencyAsync(cancellationToken);

        return Result.Ok();
    }
}