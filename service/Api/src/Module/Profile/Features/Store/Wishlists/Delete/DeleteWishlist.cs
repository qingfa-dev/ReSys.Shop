using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Shared.Mappings;

namespace Module.Profile.Features.Store.Wishlists.Delete;

/// <summary>Deletes a wishlist.</summary>
public static partial class DeleteWishlist
{
    public sealed record Command(Guid UserId, Guid Id, string? DeletedBy = null) : ICommand<Response>;

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
                    w => w.Id == command.Id && w.UserId == command.UserId && !w.IsDeleted,
                    cancellationToken);

            // Validate: Confirm wishlist exists
            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            // Update: Mark wishlist as soft-deleted with audit metadata
            wishlist.IsDeleted = true;
            wishlist.DeletedAtUtc = DateTimeOffset.UtcNow;
            wishlist.DeletedBy = command.DeletedBy ?? command.UserId.ToString();

            // Log: Record the deletion via persistence
            await dbContext.SaveChangesAsync(cancellationToken);

            // Transform: Map deleted wishlist to response DTO
            return wishlist.MapToSimple<Response>();
        }
    }
}
