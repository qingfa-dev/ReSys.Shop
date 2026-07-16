using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Delete;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonDelete")]
public class DeleteTaxonTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ITaxonHierarchyService> _hierarchyServiceMock;
    private readonly Mock<IAutoClassificationService> _autoClassificationServiceMock;
    private readonly Mock<ILogger<DeleteTaxon.CommandHandler>> _loggerMock;
    private readonly DeleteTaxon.CommandHandler _handler;

    public DeleteTaxonTests()
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

        _autoClassificationServiceMock = new Mock<IAutoClassificationService>();
        _autoClassificationServiceMock.Setup(x => x.RegenerateForTaxonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _loggerMock = new Mock<ILogger<DeleteTaxon.CommandHandler>>();

        _handler = new DeleteTaxon.CommandHandler(_dbContext, _hierarchyServiceMock.Object, _autoClassificationServiceMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete taxon successfully and rebuild hierarchy")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var parent = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, parent.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(parent, taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteTaxon.Command(taxonomy.Id, taxon.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Taxon>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == taxon.Id, TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.IsDeleted.Should().BeTrue();

        _hierarchyServiceMock.Verify(x => x.RebuildHierarchyAsync(taxonomy.Id, null, It.IsAny<CancellationToken>()), Times.Once);
        _autoClassificationServiceMock.Verify(x => x.RegenerateForTaxonAsync(taxon.Id, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should call auto-classification when taxon is automatic")]
    public async Task Handle_ShouldCallAutoClassification_WhenAutomatic()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var parent = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, parent.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, true, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(parent, taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteTaxon.Command(taxonomy.Id, taxon.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        _autoClassificationServiceMock.Verify(x => x.RegenerateForTaxonAsync(taxon.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonomyNotFound()
    {
        var result = await _handler.Handle(new DeleteTaxon.Command(Guid.NewGuid(), Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxon not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonNotFound()
    {
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteTaxon.Command(taxonomy.Id, Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxon has children")]
    public async Task Handle_ShouldReturnFailure_WhenHasChildren()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var parent = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, parent.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var child = TaxonMethod.Create(taxonomy.Id, taxon.Id, "T-Shirts", "T-Shirts", null, 0, "t-shirts", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(parent, taxon, child);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteTaxon.Command(taxonomy.Id, taxon.Id), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.HasChildren.Code);
    }
}
