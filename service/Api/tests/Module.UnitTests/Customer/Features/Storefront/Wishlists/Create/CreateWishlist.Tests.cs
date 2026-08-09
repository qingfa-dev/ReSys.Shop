using Module.Customer.Domain;
using Module.Customer.Domain.Wishlists;
using Module.Customer.Features.Storefront.Wishlists.Create;

namespace Module.UnitTests.Profile.Features.Store.Wishlists.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "WishlistCreate")]
public class CreateWishlistTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly CreateWishlist.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public CreateWishlistTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new CreateWishlist.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should create wishlist successfully")]
    public async Task Handle_ShouldCreateWishlist()
    {
        var result = await _handler.Handle(new CreateWishlist.Command(_userId, new CreateWishlist.Request
        {
            Name = "My Wishlist",
            IsPrivate = false
        }), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Name.Should().Be("My Wishlist");
        result.Value.IsPrivate.Should().BeFalse();
    }

    [Fact(DisplayName = "Handle: Should create private wishlist")]
    public async Task Handle_ShouldCreatePrivateWishlist()
    {
        var result = await _handler.Handle(new CreateWishlist.Command(_userId, new CreateWishlist.Request
        {
            Name = "Private List",
            IsPrivate = true
        }), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.IsPrivate.Should().BeTrue();
    }
}
