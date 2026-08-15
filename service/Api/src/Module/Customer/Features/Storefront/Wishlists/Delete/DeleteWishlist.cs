using Module.Customer.Domain.Wishlists;
using Module.Customer.Features.Storefront.Wishlists.Shared.Mappings;

namespace Module.Customer.Features.Storefront.Wishlists.Delete;

/// <summary>Deletes a wishlist.</summary>
public static partial class DeleteWishlist
{
    public sealed record Command(Parameters Parameters) : ICommand<Response>;

    /// <summary>Handles the deletion of a wishlist.</summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Deletes a wishlist.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Load: Fetch the wishlist from persistence
            var wishlist = await dbContext.Set<Wishlist>()
                .FirstOrDefaultAsync(
                    w => w.Id == command.Parameters.Id && w.UserId == command.Parameters.UserId && !w.IsDeleted,
                    cancellationToken);

            // Validate: Confirm wishlist exists
            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            // Update: Mark wishlist as soft-deleted with audit metadata
            wishlist.IsDeleted = true;
            wishlist.DeletedAtUtc = DateTimeOffset.UtcNow;
            wishlist.DeletedBy = command.Parameters.DeletedBy ?? command.Parameters.UserId.ToString();

            // Log: Record the deletion via persistence
            await dbContext.SaveChangesAsync(cancellationToken);

            // Transform: Map deleted wishlist to response DTO
            return wishlist.MapToSimple<Response>();
        }
    }
}
