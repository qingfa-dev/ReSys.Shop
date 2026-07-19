using Module.Profile.Domain;
using Module.Profile.Domain.Wishlists;
using Module.Profile.Features.Store.Wishlists.Delete;

namespace Module.UnitTests.Profile.Features.Store.Wishlists.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Profile")]
[Trait("Feature", "WishlistDelete")]
public class DeleteWishlistTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DeleteWishlist.CommandHandler _handler;
    private readonly Guid _userId = Guid.NewGuid();

    public DeleteWishlistTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(UserProfile).Assembly];

        _dbContext = new ApplicationDbContext(options);
        _handler = new DeleteWishlist.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handle: Should soft-delete wishlist")]
    public async Task Handle_ShouldSoftDelete()
    {
        var wishlist = WishlistExtensions.Create("To Delete", _userId, isPrivate: false).Value;
        _dbContext.Set<Wishlist>().Add(wishlist);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteWishlist.Command(_userId, wishlist.Id, "testuser"), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var deleted = await _dbContext.Set<Wishlist>().FindAsync(wishlist.Id);
        deleted.Should().NotBeNull();
        deleted!.IsDeleted.Should().BeTrue();
        deleted.DeletedAtUtc.Should().NotBeNull();
        deleted.DeletedBy.Should().Be("testuser");
    }

    [Fact(DisplayName = "Handle: Should return NotFound when wishlist does not exist")]
    public async Task Handle_ShouldFail_WhenNotFound()
    {
        var result = await _handler.Handle(new DeleteWishlist.Command(_userId, Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(WishlistResult.Failure.NotFound.Code);
    }
}
