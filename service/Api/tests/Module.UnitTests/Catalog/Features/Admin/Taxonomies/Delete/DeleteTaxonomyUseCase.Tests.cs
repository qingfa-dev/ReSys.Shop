using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Delete;
using Module.Catalog.Features.Admin.Taxons.Delete;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonomyDelete")]
public class DeleteTaxonomyTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly DeleteTaxonomy.CommandHandler _handler;

    public DeleteTaxonomyTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxonomy).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _senderMock = new Mock<ISender>();
        _senderMock.Setup(x => x.Send(It.IsAny<DeleteTaxon.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok(TaxonResult.Success.Deleted));

        _handler = new DeleteTaxonomy.CommandHandler(_dbContext, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete taxonomy successfully when no taxons")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var entity = TaxonomyMethod.Create("Categories", "Presentation", 0).Value;
        _dbContext.Set<Taxonomy>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteTaxonomy.Command(entity.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Taxonomy>().FindAsync([entity.Id], TestContext.Current.CancellationToken);
        persisted!.IsDeleted.Should().BeTrue();

        _senderMock.Verify(x => x.Send(It.IsAny<DeleteTaxon.Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var result = await _handler.Handle(new DeleteTaxonomy.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy has non-root taxons")]
    public async Task Handle_ShouldReturnFailure_WhenHasTaxons()
    {
        var entity = TaxonomyMethod.Create("Categories", "Presentation", 0).Value;
        var root = TaxonMethod.Create(entity.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var child = TaxonMethod.Create(entity.Id, root.Id, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;

        entity.Taxons.Add(root);
        entity.Taxons.Add(child);

        _dbContext.Set<Taxonomy>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteTaxonomy.Command(entity.Id), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.HasTaxons.Code);
    }

    [Fact(DisplayName = "Handler: Should delete root taxon via sender when root exists in DB")]
    public async Task Handle_ShouldDeleteRootTaxonViaSender_WhenRootExists()
    {
        var ct = TestContext.Current.CancellationToken;
        var entity = TaxonomyMethod.Create("Categories", "Presentation", 0).Value;
        var root = TaxonMethod.Create(entity.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(entity);
        _dbContext.Set<Taxon>().Add(root);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        var result = await _handler.Handle(new DeleteTaxonomy.Command(entity.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Taxonomy>().FindAsync([entity.Id], ct);
        persisted!.IsDeleted.Should().BeTrue();

        _senderMock.Verify(x => x.Send(
            It.Is<DeleteTaxon.Command>(c => c.Id == root.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
