using Microsoft.EntityFrameworkCore;

using Module.Profile.Domain.Wishlists;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Wishlists.Delete;

public static partial class DeleteWishlist
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, ICurrentUser currentUser)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(currentUser.UserId))
                return WishlistResult.Failure.AuthRequired;

            var wishlist = await dbContext.Set<Wishlist>()
                .FirstOrDefaultAsync(
                    w => w.Id == command.Id && w.UserId == Guid.Parse(currentUser.UserId) && !w.IsDeleted,
                    cancellationToken);

            if (wishlist is null)
                return WishlistResult.Failure.NotFound;

            wishlist.IsDeleted = true;
            wishlist.DeletedAtUtc = DateTimeOffset.UtcNow;
            wishlist.DeletedBy = currentUser.UserName;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response(wishlist.Id, wishlist.Name);
        }
    }
}
