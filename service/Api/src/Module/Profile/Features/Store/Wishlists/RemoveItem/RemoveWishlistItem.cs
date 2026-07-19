using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Shared.Mappings;

namespace Module.Profile.Features.Store.Wishlists.RemoveItem;

public static partial class RemoveWishlistItem
{
    public sealed record Command(Guid UserId, Guid Id, Guid ItemId) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var wishlist = await dbContext.Set<Wishlist>()
                .Include(w => w.WishedItems)
                .FirstOrDefaultAsync(
                    w => w.Id == command.Id && w.UserId == command.UserId && !w.IsDeleted,
                    cancellationToken);

            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            var removeResult = wishlist.RemoveItem(command.ItemId);
            if (removeResult.IsFailure)
                return removeResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return wishlist.MapToSimple<Response>();
        }
    }
}
