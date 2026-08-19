using Module.Customer.Domain.Wishlists;
using Module.Customer.Features.Storefront.Shared.Mappings;

namespace Module.Customer.Features.Storefront.Wishlists.Create;

/// <summary>Creates a new wishlist.</summary>
public static partial class CreateWishlist
{
    public sealed record Command(Guid UserId, Request Request) : ICommand<Response>;

    /// <summary>Handles the creation of a new wishlist.</summary>
    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Creates a new wishlist.</summary>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Validate: Build and validate new wishlist via domain factory
            var createResult = WishlistExtensions.Create(
                name: command.Request.Name,
                userId: command.UserId,
                isPrivate: command.Request.IsPrivate);

            if (createResult.IsFailure)
                return createResult.Errors;

            // Create: Persist the new wishlist entity
            var wishlist = createResult.Value;
            dbContext.Set<Wishlist>().Add(wishlist);
            // Log: Record the wishlist creation via persistence
            await dbContext.SaveChangesAsync(cancellationToken);

            // Transform: Map new wishlist to response DTO
            return Result<Response>.Created(wishlist.MapToSimple<Response>());
        }
    }
}
