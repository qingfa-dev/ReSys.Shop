using Module.Customer.Domain;
using Module.Customer.Domain.Wishlists;
using Module.Customer.Features.Storefront.Wishlists.GetById;

namespace Module.UnitTests.Profile.Features.Store.Wishlists.GetById;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "WishlistGetById")]
public class GetWishlistByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetWishlistById.QueryHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public GetWishlistByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new GetWishlistById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return wishlist with items")]
    public async Task Handle_ShouldReturnWishlistWithItems()
    {
        var wishlist = WishlistExtensions.Create("My List", _userId, isPrivate: false).Value;
        wishlist.AddItem(Guid.NewGuid(), 2);
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetWishlistById.Query(_userId, wishlist.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("My List");
        result.Value.WishedItems.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handle: Should return NotFound when wishlist does not exist")]
    public async Task Handle_ShouldFail_WhenNotFound()
    {
        var result = await _handler.Handle(new GetWishlistById.Query(_userId, Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(WishlistResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should return NotFound when wishlist belongs to another user")]
    public async Task Handle_ShouldFail_WhenNotOwnedByUser()
    {
        var otherUserId = Guid.NewGuid();
        var wishlist = WishlistExtensions.Create("Other's List", otherUserId, isPrivate: false).Value;
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetWishlistById.Query(_userId, wishlist.Id), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(WishlistResult.Failure.NotFound.Code);
    }
}
