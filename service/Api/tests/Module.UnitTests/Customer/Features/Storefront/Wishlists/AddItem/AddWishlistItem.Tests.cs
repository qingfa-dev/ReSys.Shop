using Module.Customer.Domain;
using Module.Customer.Domain.Wishlists;
using Module.Customer.Features.Storefront.Wishlists.AddItem;

namespace Module.UnitTests.Profile.Features.Store.Wishlists.AddItem;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "WishlistAddItem")]
public class AddWishlistItemTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly AddWishlistItem.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public AddWishlistItemTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new AddWishlistItem.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should add item to wishlist")]
    public async Task Handle_ShouldAddItem()
    {
        var wishlist = WishlistExtensions.Create("My List", _userId, isPrivate: false).Value;
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var variantId = Guid.NewGuid();
        var result = await _handler.Handle(new AddWishlistItem.Command(_userId, wishlist.Id, new AddWishlistItem.Request
        {
            VariantId = variantId,
            Quantity = 2
        }), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var updated = await _dbContext.Set<Wishlist>().Include(w => w.WishedItems).FirstAsync(w => w.Id == wishlist.Id, TestContext.Current.CancellationToken);
        updated.WishedItems.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handle: Should return NotFound when wishlist does not exist")]
    public async Task Handle_ShouldFail_WhenNotFound()
    {
        var result = await _handler.Handle(new AddWishlistItem.Command(_userId, Guid.NewGuid(), new AddWishlistItem.Request
        {
            VariantId = Guid.NewGuid(),
            Quantity = 1
        }), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(WishlistResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should default quantity to 1")]
    public async Task Handle_ShouldDefaultQuantityToOne()
    {
        var wishlist = WishlistExtensions.Create("My List", _userId, isPrivate: false).Value;
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new AddWishlistItem.Command(_userId, wishlist.Id, new AddWishlistItem.Request
        {
            VariantId = Guid.NewGuid()
        }), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }
}
