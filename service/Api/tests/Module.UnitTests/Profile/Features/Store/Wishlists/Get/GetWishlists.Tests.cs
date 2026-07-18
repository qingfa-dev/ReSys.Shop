using Module.Profile.Domain;
using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Get;
using Module.UnitTests.Identity.Fixtures;

namespace Module.UnitTests.Profile.Features.Store.Wishlists.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "WishlistGet")]
public class GetWishlistsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly GetWishlists.PagedQueryHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public GetWishlistsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = IdentityMocks.CreateCurrentUserMock(_userId);
        _handler = new GetWishlists.PagedQueryHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should return paginated wishlists")]
    public async Task Handle_ShouldReturnWishlists()
    {
        var wishlist = WishlistExtensions.Create("Test List", _userId, isPrivate: false).Value;
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetWishlists.Query(new GetWishlists.Parameters()), TestContext.Current.CancellationToken);

        result.Items.Should().HaveCount(1);
        result.Items.First().Name.Should().Be("Test List");
        result.TotalCount.Should().Be(1);
    }

    [Fact(DisplayName = "Handle: Should return empty list when no wishlists")]
    public async Task Handle_ShouldReturnEmpty_WhenNoWishlists()
    {
        var result = await _handler.Handle(new GetWishlists.Query(new GetWishlists.Parameters()), TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact(DisplayName = "Handle: Should not return deleted wishlists")]
    public async Task Handle_ShouldNotReturnDeleted()
    {
        var wishlist = WishlistExtensions.Create("Deleted", _userId, isPrivate: false).Value;
        wishlist.IsDeleted = true;
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetWishlists.Query(new GetWishlists.Parameters()), TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handle: Should return empty when user not authenticated")]
    public async Task Handle_ShouldReturnEmpty_WhenNotAuthenticated()
    {
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        var result = await _handler.Handle(new GetWishlists.Query(new GetWishlists.Parameters()), TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }
}
