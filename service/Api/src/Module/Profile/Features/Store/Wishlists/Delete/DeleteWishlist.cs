using Module.Profile.Domain.Wishlists;

namespace Module.Profile.Features.Store.Wishlists.Delete;

/// <summary>Soft-deletes a wishlist belonging to the authenticated user.</summary>
public static partial class DeleteWishlist
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates ownership, then marks the wishlist as deleted with audit timestamps.</summary>
        /// <param name="command">The command containing the wishlist ID to delete.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the deleted wishlist summary or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=user authenticated, post=wishlist.IsDeleted==true, throws=DbUpdateException
            // Check: Ensure user is authenticated
            if (string.IsNullOrEmpty(currentUser.UserId))
                return WishlistResult.Failure.AuthRequired;

            // Load: Find the wishlist owned by the current user
            var wishlist = await dbContext.Set<Wishlist>()
                .FirstOrDefaultAsync(
                    w => w.Id == command.Id && w.UserId == Guid.Parse(currentUser.UserId) && !w.IsDeleted,
                    cancellationToken);

            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            // Soft Delete: Mark as deleted with audit trail
            wishlist.IsDeleted = true;
            wishlist.DeletedAtUtc = DateTimeOffset.UtcNow;
            wishlist.DeletedBy = currentUser.UserName;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response(wishlist.Id, wishlist.Name);
        }
    }
}
