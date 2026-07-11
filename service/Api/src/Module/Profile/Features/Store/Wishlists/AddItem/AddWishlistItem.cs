using Module.Profile.Domain.Wishlists;

namespace Module.Profile.Features.Store.Wishlists.AddItem;

/// <summary>Adds an item to an existing wishlist for the authenticated user.</summary>
public static partial class AddWishlistItem
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates ownership, adds the variant to the wishlist, and persists the change.</summary>
        /// <param name="command">The command containing the wishlist ID, variant ID, and quantity.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the updated wishlist details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated && wishlist owned by user, post=item added to wishlist,
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

            // Add: Append variant to wishlist items
            var addResult = wishlist.AddItem(command.Request.VariantId, command.Request.Quantity);
            if (addResult.IsFailure)
                return addResult.Errors;

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
