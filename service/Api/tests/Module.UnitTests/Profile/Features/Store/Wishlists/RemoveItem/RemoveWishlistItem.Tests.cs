using Module.Profile.Domain;
using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.RemoveItem;
using Module.UnitTests.Identity.Fixtures;

namespace Module.UnitTests.Profile.Features.Store.Wishlists.RemoveItem;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "WishlistRemoveItem")]
public class RemoveWishlistItemTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly RemoveWishlistItem.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public RemoveWishlistItemTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = IdentityMocks.CreateCurrentUserMock(_userId);
        _handler = new RemoveWishlistItem.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should remove item from wishlist")]
    public async Task Handle_ShouldRemoveItem()
    {
        var wishlist = WishlistExtensions.Create("My List", _userId, isPrivate: false).Value;
        var variantId = Guid.NewGuid();
        wishlist.AddItem(variantId, 1);
        var itemId = wishlist.WishedItems.First().Id;
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RemoveWishlistItem.Command(wishlist.Id, itemId), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.WishedItems.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should return Unauthorized when user not authenticated")]
    public async Task Handle_ShouldFail_WhenNotAuthenticated()
    {
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        var result = await _handler.Handle(new RemoveWishlistItem.Command(Guid.NewGuid(), Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(WishlistResult.Failure.AuthRequired.Code);
    }

    [Fact(DisplayName = "Handle: Should return NotFound when wishlist does not exist")]
    public async Task Handle_ShouldFail_WhenNotFound()
    {
        var result = await _handler.Handle(new RemoveWishlistItem.Command(Guid.NewGuid(), Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(WishlistResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return error when item not found in wishlist")]
    public async Task Handle_ShouldFail_WhenItemNotFound()
    {
        var wishlist = WishlistExtensions.Create("My List", _userId, isPrivate: false).Value;
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RemoveWishlistItem.Command(wishlist.Id, Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(WishlistResult.Failure.ItemNotFound.Code);
    }
}
