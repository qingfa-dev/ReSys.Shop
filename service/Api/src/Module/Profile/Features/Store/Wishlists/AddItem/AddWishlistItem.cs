using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Shared.Mappings;

namespace Module.Profile.Features.Store.Wishlists.AddItem;

/// <summary>Adds a product to a wishlist.</summary>
public static partial class AddWishlistItem
{
    public sealed record Command(Guid UserId, Guid Id, Request Request) : ICommand<Response>;

    /// <summary>Handles adding a product to a wishlist.</summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Adds a product to a wishlist.</summary>
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

            // Create: Add the product variant to the wishlist
            var addResult = wishlist.AddItem(command.Request.VariantId, command.Request.Quantity);
            if (addResult.IsFailure)
                return addResult.Errors;

            // Log: Record the item addition via persistence
            await dbContext.SaveChangesAsync(cancellationToken);

            // Transform: Map updated wishlist to response DTO
            return wishlist.MapToSimple<Response>();
        }
    }
}
