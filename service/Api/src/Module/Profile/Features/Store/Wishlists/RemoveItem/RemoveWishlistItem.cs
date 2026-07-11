using Microsoft.EntityFrameworkCore;

using Module.Profile.Domain.Wishlists;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Wishlists.RemoveItem;

/// <summary>Removes an item from a wishlist for the authenticated user.</summary>
public static partial class RemoveWishlistItem
{
    public sealed record Command(Guid Id, Guid ItemId) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates ownership, removes the specified item, and persists the change.</summary>
        /// <param name="command">The command containing the wishlist ID and item ID to remove.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the updated wishlist summary or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated && wishlist owned by user, post=item removed from wishlist,
            //           throws=DbUpdateException
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

            // Remove: Delete the specified item from the wishlist
            var removeResult = wishlist.RemoveItem(command.ItemId);
            if (removeResult.IsFailure)
                return removeResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response(wishlist.Id, wishlist.Name);
        }
    }
}
