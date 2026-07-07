using Microsoft.EntityFrameworkCore;

using Module.Profile.Domain.Wishlists;

using Shared.Security.Identity.Domain.Users;

namespace Module.Profile.Features.Store.Wishlists.AddItem;

public static partial class AddWishlistItem
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

            var addResult = wishlist.AddItem(command.Request.VariantId, command.Request.Quantity);
            if (addResult.IsFailure)
                return addResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = wishlist.Id,
                Name = wishlist.Name,
                IsPrivate = wishlist.IsPrivate,
                IsDefault = wishlist.IsDefault,
                Token = wishlist.Token,
                ItemCount = wishlist.WishedItems.Count
            };
        }
    }
}
