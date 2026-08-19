using Module.Customer.Domain.Wishlists;
using Module.Customer.Features.Storefront.Shared.Mappings;

namespace Module.Customer.Features.Storefront.Wishlists.RemoveItem;

/// <summary>Removes a product from a wishlist.</summary>
public static partial class RemoveWishlistItem
{
    public sealed record Command(Guid UserId, Guid Id, Guid ItemId) : ICommand<Response>;

    /// <summary>Handles removing a product from a wishlist.</summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Removes a product from a wishlist.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch the wishlist with items from persistence
            var wishlist = await dbContext.Set<Wishlist>()
                .Include(w => w.WishedItems)
                .FirstOrDefaultAsync(
                    w => w.Id == command.Id && w.UserId == command.UserId && !w.IsDeleted,
                    cancellationToken);

            // Validate: Confirm wishlist exists
            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            // Remove: Remove the specified item from the wishlist
            var removeResult = wishlist.RemoveItem(command.ItemId);
            if (removeResult.IsFailure)
                return removeResult.Errors;

            // Log: Record the removal via persistence
            await dbContext.SaveChangesAsync(cancellationToken);

            // Transform: Map updated wishlist to response DTO
            return wishlist.MapToSimple<Response>();
        }
    }
}
