using Module.Profile.Domain;
using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Storefront.Wishlists.Update;

namespace Module.UnitTests.Profile.Features.Store.Wishlists.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "WishlistUpdate")]
public class UpdateWishlistTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UpdateWishlist.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public UpdateWishlistTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new UpdateWishlist.CommandHandler(_dbContext);
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

        var result = await _handler.Handle(new UpdateWishlist.Command(_userId, wishlist.Id, new UpdateWishlist.Request
        {
            Name = "Updated",
            IsPrivate = true,
            IsDefault = null
        }), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated");
        result.Value.IsPrivate.Should().BeTrue();
    }

    [Fact(DisplayName = "Handle: Should return NotFound when wishlist does not exist")]
    public async Task Handle_ShouldFail_WhenNotFound()
    {
        var result = await _handler.Handle(new UpdateWishlist.Command(_userId, Guid.NewGuid(), new UpdateWishlist.Request
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

        var result = await _handler.Handle(new UpdateWishlist.Command(_userId, wishlist.Id, new UpdateWishlist.Request
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
