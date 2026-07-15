using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Restore;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Restore;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonRestore")]
public class RestoreTaxonTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ITaxonHierarchyService> _hierarchyServiceMock;
    private readonly Mock<ILogger<RestoreTaxon.CommandHandler>> _loggerMock;
    private readonly RestoreTaxon.CommandHandler _handler;

    public RestoreTaxonTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _hierarchyServiceMock = new Mock<ITaxonHierarchyService>();
        _hierarchyServiceMock.Setup(x => x.RebuildHierarchyAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _loggerMock = new Mock<ILogger<RestoreTaxon.CommandHandler>>();

        _handler = new RestoreTaxon.CommandHandler(_dbContext, _hierarchyServiceMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should restore taxon successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var parent = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, parent.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;
        taxon.Delete();

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(parent, taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RestoreTaxon.Command(taxonomy.Id, taxon.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Taxon>().FindAsync([taxon.Id], TestContext.Current.CancellationToken);
        persisted!.IsDeleted.Should().BeFalse();

        _hierarchyServiceMock.Verify(x => x.RebuildHierarchyAsync(taxonomy.Id, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonomyNotFound()
    {
        var result = await _handler.Handle(new RestoreTaxon.Command(Guid.NewGuid(), Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxon not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RestoreTaxon.Command(taxonomy.Id, Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should restore non-deleted taxon gracefully (idempotent)")]
    public async Task Handle_ShouldReturnSuccess_WhenAlreadyActive()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var parent = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, parent.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(parent, taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RestoreTaxon.Command(taxonomy.Id, taxon.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Taxon>().FindAsync([taxon.Id], TestContext.Current.CancellationToken);
        persisted!.IsDeleted.Should().BeFalse();

        _hierarchyServiceMock.Verify(x => x.RebuildHierarchyAsync(taxonomy.Id, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
