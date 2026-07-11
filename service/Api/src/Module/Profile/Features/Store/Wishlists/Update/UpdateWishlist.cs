using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Shared.Mappings;

namespace Module.Profile.Features.Store.Wishlists.Update;

public static partial class UpdateWishlist
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(currentUser.UserId))
                return WishlistResult.Failure.AuthRequired;

            var wishlist = await dbContext.Set<Wishlist>()
                .Include(w => w.WishedItems)
                .FirstOrDefaultAsync(
                    w => w.Id == command.Id && w.UserId == Guid.Parse(currentUser.UserId) && !w.IsDeleted,
                    cancellationToken);

            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            var updateResult = wishlist.Update(
                name: command.Request.Name,
                isPrivate: command.Request.IsPrivate,
                isDefault: command.Request.IsDefault);

            if (updateResult.IsFailure)
                return updateResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return wishlist.MapToSimple<Response>();
        }
    }
}
