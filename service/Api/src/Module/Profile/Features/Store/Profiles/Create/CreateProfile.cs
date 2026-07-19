using Module.Profile.Domain;
using Module.Profile.Features.Store.Profiles.Shared.Mappings;

using Shared.Application.Contracts.Profile;
using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Profiles.Create;

/// <summary>Creates a new user profile for an existing identity user.</summary>
public static partial class CreateProfile
{
    /// <param name="UserId">The unique identifier of the user.</param>
    /// <param name="Request">The request containing profile details.</param>
    public sealed record Command(Guid UserId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates user existence, enforces one-profile-per-user, and persists via shared mapping.</summary>
        /// <param name="command">The command containing the user ID and profile data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the created profile details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var request = command.Request;

            // Check: Target user must exist in identity store before profile creation.
            var user = await dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

            if (user is null)
                return UserProfileResult.Failure.UserNotFound;

            // Check: Enforce one profile per user — duplicate detection.
            var existing = await dbContext.Set<UserProfile>()
                .AnyAsync(p => p.UserId == command.UserId, cancellationToken);

            if (existing)
                return UserProfileResult.Failure.AlreadyExists;

            // Create: Build UserProfile from validated request via shared domain mapping.
            var profile = request.MapToDomain();
            profile.UserId = user.Id;

            dbContext.Set<UserProfile>().Add(profile);
            await dbContext.SaveChangesAsync(cancellationToken);
            return profile.MapToDetail<Response>();
        }
    }
}

public sealed class CreateUserProfileCommandHandler(
    IApplicationDbContext dbContext)
    : ICommandHandler<CreateUserProfileCommand, CreateUserProfileResult>
{
    public async Task<Result<CreateUserProfileResult>> Handle(
        CreateUserProfileCommand command,
        CancellationToken cancellationToken)
    {
        var inner = new CreateProfile.CommandHandler(dbContext);
        var result = await inner.Handle(
            new CreateProfile.Command(command.UserId, new CreateProfile.Request
            {
                FirstName = command.FirstName,
                LastName = command.LastName ?? string.Empty,
                Email = command.Email
            }), cancellationToken);

        return result.IsSuccess
            ? new CreateUserProfileResult { ProfileId = result.Value.Id }
            : result.Errors;
    }
}