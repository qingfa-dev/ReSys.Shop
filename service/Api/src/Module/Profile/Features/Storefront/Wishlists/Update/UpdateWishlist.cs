using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Storefront.Wishlists.Shared.Mappings;

namespace Module.Profile.Features.Storefront.Wishlists.Update;

/// <summary>Updates a wishlist's details.</summary>
public static partial class UpdateWishlist
{
    public sealed record Command(Guid UserId, Guid Id, Request Request) : ICommand<Response>;

    /// <summary>Handles the update of a wishlist's details.</summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Updates a wishlist's details.</summary>
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

            // Update: Apply name, privacy, and default settings to the wishlist
            var updateResult = wishlist.Update(
                name: command.Request.Name,
                isPrivate: command.Request.IsPrivate,
                isDefault: command.Request.IsDefault);

            if (updateResult.IsFailure)
                return updateResult.Errors;

            // Log: Record the update via persistence
            await dbContext.SaveChangesAsync(cancellationToken);

            // Transform: Map updated wishlist to response DTO
            return wishlist.MapToSimple<Response>();
        }
    }
}
