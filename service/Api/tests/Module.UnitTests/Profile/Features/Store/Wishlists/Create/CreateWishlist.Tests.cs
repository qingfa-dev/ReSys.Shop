using Module.Profile.Domain;
using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Create;
using Module.UnitTests.Identity.Fixtures;

namespace Module.UnitTests.Profile.Features.Store.Wishlists.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "WishlistCreate")]
public class CreateWishlistTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly CreateWishlist.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateWishlistTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = IdentityMocks.CreateCurrentUserMock(_userId);
        _handler = new CreateWishlist.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should create wishlist successfully")]
    public async Task Handle_ShouldCreateWishlist()
    {
        var result = await _handler.Handle(new CreateWishlist.Command(new CreateWishlist.Request
        {
            Name = "My Wishlist",
            IsPrivate = false
        }), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Name.Should().Be("My Wishlist");
        result.Value.IsPrivate.Should().BeFalse();
    }

    [Fact(DisplayName = "Handle: Should return Unauthorized when user not authenticated")]
    public async Task Handle_ShouldFail_WhenNotAuthenticated()
    {
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        var result = await _handler.Handle(new CreateWishlist.Command(new CreateWishlist.Request
        {
            Name = "Test",
            IsPrivate = false
        }), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(WishlistResult.Failure.AuthRequired.Code);
    }

    [Fact(DisplayName = "Handle: Should create private wishlist")]
    public async Task Handle_ShouldCreatePrivateWishlist()
    {
        var result = await _handler.Handle(new CreateWishlist.Command(new CreateWishlist.Request
        {
            Name = "Private List",
            IsPrivate = true
        }), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPrivate.Should().BeTrue();
    }
}
