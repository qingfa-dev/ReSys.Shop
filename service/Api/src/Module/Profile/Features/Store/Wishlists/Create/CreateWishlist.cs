using Module.Profile.Domain;
using Module.Profile.Domain.Wishlists;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Wishlists.Create;

/// <summary>Creates a new wishlist for the authenticated user.</summary>
public static partial class CreateWishlist
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates authentication, creates the wishlist domain entity, and persists it.</summary>
        /// <param name="command">The command containing wishlist name and privacy settings.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the created wishlist details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated, post=wishlist persisted, throws=DbUpdateException
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return WishlistResult.Failure.AuthRequired;

            // Create: Build wishlist domain entity
            var createResult = WishlistExtensions.Create(
                name: command.Request.Name,
                userId: Guid.Parse(currentUser.UserId),
                isPrivate: command.Request.IsPrivate);

            if (createResult.IsFailure)
                return createResult.Errors;

            var wishlist = createResult.Value;
            dbContext.Set<Wishlist>().Add(wishlist);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Created(new Response
            {
                Id = wishlist.Id,
                Name = wishlist.Name,
                IsPrivate = wishlist.IsPrivate,
                IsDefault = wishlist.IsDefault,
                Token = wishlist.Token,
                ItemCount = 0
            });
        }
    }
}
