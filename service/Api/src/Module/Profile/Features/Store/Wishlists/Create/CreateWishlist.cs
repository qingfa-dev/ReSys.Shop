using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Shared.Mappings;

namespace Module.Profile.Features.Store.Wishlists.Create;

public static partial class CreateWishlist
{
    public sealed record Command(Guid UserId, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            var createResult = WishlistExtensions.Create(
                name: command.Request.Name,
                userId: command.UserId,
                isPrivate: command.Request.IsPrivate);

            if (createResult.IsFailure)
                return createResult.Errors;

            var wishlist = createResult.Value;
            dbContext.Set<Wishlist>().Add(wishlist);
            await dbContext.SaveChangesAsync(cancellationToken);

            return Result<Response>.Created(wishlist.MapToSimple<Response>());
        }
    }
}
