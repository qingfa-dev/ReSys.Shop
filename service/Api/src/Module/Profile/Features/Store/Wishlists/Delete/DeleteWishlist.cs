using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Shared.Mappings;

namespace Module.Profile.Features.Store.Wishlists.Delete;

public static partial class DeleteWishlist
{
    public sealed record Command(Guid UserId, Guid Id, string? DeletedBy = null) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var wishlist = await dbContext.Set<Wishlist>()
                .FirstOrDefaultAsync(
                    w => w.Id == command.Id && w.UserId == command.UserId && !w.IsDeleted,
                    cancellationToken);

            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            wishlist.IsDeleted = true;
            wishlist.DeletedAtUtc = DateTimeOffset.UtcNow;
            wishlist.DeletedBy = command.DeletedBy ?? command.UserId.ToString();

            await dbContext.SaveChangesAsync(cancellationToken);

            return wishlist.MapToSimple<Response>();
        }
    }
}
