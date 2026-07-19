using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Shared.Mappings;

namespace Module.Profile.Features.Store.Wishlists.AddItem;

public static partial class AddWishlistItem
{
    public sealed record Command(Guid UserId, Guid Id, Request Request) : ICommand<Response>;

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

            var addResult = wishlist.AddItem(command.Request.VariantId, command.Request.Quantity);
            if (addResult.IsFailure)
                return addResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return wishlist.MapToSimple<Response>();
        }
    }
}
