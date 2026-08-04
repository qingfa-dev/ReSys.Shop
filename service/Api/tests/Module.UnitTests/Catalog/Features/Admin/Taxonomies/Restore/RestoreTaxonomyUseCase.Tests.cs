using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Restore;
using Module.Catalog.Features.Admin.Taxons.Restore;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Restore;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonomyRestore")]
public class RestoreTaxonomyTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly RestoreTaxonomy.CommandHandler _handler;

    public RestoreTaxonomyTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxonomy).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _senderMock = new Mock<ISender>();
        _senderMock.Setup(x => x.Send(It.IsAny<RestoreTaxon.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new RestoreTaxonomy.CommandHandler(_dbContext, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should restore taxonomy successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var entity = TaxonomyMethod.Create("Categories", "Presentation", 0).Value;
        entity.Delete();
        _dbContext.Set<Taxonomy>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RestoreTaxonomy.Command(entity.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Taxonomy>().FindAsync([entity.Id], TestContext.Current.CancellationToken);
        persisted!.IsDeleted.Should().BeFalse();
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(new RestoreTaxonomy.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should restore root taxon via sender when root exists")]
    public async Task Handle_ShouldRestoreRootTaxonViaSender_WhenRootExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var entity = TaxonomyMethod.Create("Categories", "Presentation", 0).Value;
        entity.Delete();
        _dbContext.Set<Taxonomy>().Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        var root = TaxonMethod.Create(entity.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        root.Lft = 1; root.Rgt = 2;
        _dbContext.Set<Taxon>().Add(root);
        await _dbContext.SaveChangesAsync(ct);

        _senderMock.Invocations.Clear();

        var result = await _handler.Handle(new RestoreTaxonomy.Command(entity.Id), ct);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Taxonomy>().FindAsync([entity.Id], ct);
        persisted!.IsDeleted.Should().BeFalse();

        _senderMock.Verify(x => x.Send(
            It.Is<RestoreTaxon.Command>(c => c.Id == root.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should not call sender when no root taxon exists")]
    public async Task Handle_ShouldNotCallSender_WhenNoRootTaxon()
    {
        var ct = TestContext.Current.CancellationToken;
        var entity = TaxonomyMethod.Create("Categories", "Presentation", 0).Value;
        entity.Delete();
        _dbContext.Set<Taxonomy>().Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        _senderMock.Invocations.Clear();

        var result = await _handler.Handle(new RestoreTaxonomy.Command(entity.Id), ct);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Taxonomy>().FindAsync([entity.Id], ct);
        persisted!.IsDeleted.Should().BeFalse();

        _senderMock.Verify(x => x.Send(It.IsAny<RestoreTaxon.Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
