using Module.Profile.Domain;
using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Update;
using Module.UnitTests.Identity.Fixtures;

namespace Module.UnitTests.Profile.Features.Store.Wishlists.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "WishlistUpdate")]
public class UpdateWishlistTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly UpdateWishlist.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateWishlistTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _currentUserMock = IdentityMocks.CreateCurrentUserMock(_userId);
        _handler = new UpdateWishlist.CommandHandler(_dbContext, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should update wishlist name and privacy")]
    public async Task Handle_ShouldUpdateNameAndPrivacy()
    {
        var wishlist = WishlistExtensions.Create("Original", _userId, isPrivate: false).Value;
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new UpdateWishlist.Command(wishlist.Id, new UpdateWishlist.Request
        {
            Name = "Updated",
            IsPrivate = true,
            IsDefault = null
        }), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated");
        result.Value.IsPrivate.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should return Unauthorized when user not authenticated")]
    public async Task Handle_ShouldFail_WhenNotAuthenticated()
    {
        _currentUserMock.Setup(x => x.UserId).Returns((string?)null);

        var result = await _handler.Handle(new UpdateWishlist.Command(Guid.NewGuid(), new UpdateWishlist.Request
        {
            Name = "Test"
        }), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(WishlistResult.Failure.AuthRequired.Code);
    }

    [Fact(DisplayName = "Handle: Should return NotFound when wishlist does not exist")]
    public async Task Handle_ShouldFail_WhenNotFound()
    {
        var result = await _handler.Handle(new UpdateWishlist.Command(Guid.NewGuid(), new UpdateWishlist.Request
        {
            Name = "Test"
        }), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(WishlistResult.Failure.NotFound.Code);
    }

    [Fact(DisplayName = "Handle: Should update only name when other fields are null")]
    public async Task Handle_ShouldUpdateOnlyName()
    {
        var wishlist = WishlistExtensions.Create("Original", _userId, isPrivate: false).Value;
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new UpdateWishlist.Command(wishlist.Id, new UpdateWishlist.Request
        {
            Name = "New Name",
            IsPrivate = null,
            IsDefault = null
        }), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New Name");
        result.Value.IsPrivate.Should().BeFalse();
    }
}
