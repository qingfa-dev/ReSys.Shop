using Module.Profile.Domain.Wishlists;

namespace Module.Profile.Features.Store.Wishlists.Update;

/// <summary>Updates an existing wishlist for the authenticated user.</summary>
public static partial class UpdateWishlist
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates ownership and applies name, privacy, and default flag updates.</summary>
        /// <param name="command">The command containing the wishlist ID and update data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the updated wishlist details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated && wishlist owned by user, post=wishlist updated, throws=DbUpdateException
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return WishlistResult.Failure.AuthRequired;

            // Load: Find the wishlist with items, owned by current user
            var wishlist = await dbContext.Set<Wishlist>()
                .Include(w => w.WishedItems)
                .FirstOrDefaultAsync(
                    w => w.Id == command.Id && w.UserId == Guid.Parse(currentUser.UserId) && !w.IsDeleted,
                    cancellationToken);

            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            // Update: Apply changes to wishlist domain entity
            var updateResult = wishlist.Update(
                name: command.Request.Name,
                isPrivate: command.Request.IsPrivate,
                isDefault: command.Request.IsDefault);

            if (updateResult.IsFailure)
                return updateResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = wishlist.Id,
                Name = wishlist.Name,
                IsPrivate = wishlist.IsPrivate,
                IsDefault = wishlist.IsDefault,
                Token = wishlist.Token,
                ItemCount = wishlist.WishedItems.Count
            };
        }
    }
}
