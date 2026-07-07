using Module.Profile.Domain;
using Module.Profile.Domain.Wishlists;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Wishlists.Create;

public static partial class CreateWishlist
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(currentUser.UserId))
                return WishlistResult.Failure.AuthRequired;

            var createResult = WishlistExtensions.Create(
                name: command.Request.Name,
                userId: Guid.Parse(currentUser.UserId),
                isPrivate: command.Request.IsPrivate);

            if (createResult.IsFailure)
                return createResult.Errors;

            var wishlist = createResult.Value;
            dbContext.Set<Wishlist>().Add(wishlist);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = wishlist.Id,
                Name = wishlist.Name,
                IsPrivate = wishlist.IsPrivate,
                IsDefault = wishlist.IsDefault,
                Token = wishlist.Token,
                ItemCount = 0
            };
        }
    }
}
