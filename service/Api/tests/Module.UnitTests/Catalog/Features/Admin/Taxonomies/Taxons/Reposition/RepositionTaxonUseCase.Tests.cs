using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Reposition;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Reposition;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonReposition")]
public class RepositionTaxonTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ITaxonHierarchyService> _hierarchyServiceMock;
    private readonly Mock<ILogger<RepositionTaxon.CommandHandler>> _loggerMock;
    private readonly RepositionTaxon.CommandHandler _handler;

    public RepositionTaxonTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _hierarchyServiceMock = new Mock<ITaxonHierarchyService>();
        _hierarchyServiceMock.Setup(x => x.ValidateDescendantAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());
        _hierarchyServiceMock.Setup(x => x.RebuildHierarchyAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _loggerMock = new Mock<ILogger<RepositionTaxon.CommandHandler>>();

        _handler = new RepositionTaxon.CommandHandler(_dbContext, _hierarchyServiceMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should reposition taxon successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, root.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var otherParent = TaxonMethod.Create(taxonomy.Id, root.Id, "Clothes", "Clothes", null, 2, "clothes", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, taxon, otherParent);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RepositionTaxon.Request
        {
            ParentId = otherParent.Id,
            Position = 10
        };

        var result = await _handler.Handle(new RepositionTaxon.Command(taxonomy.Id, taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Taxon>().FindAsync([taxon.Id], TestContext.Current.CancellationToken);
        persisted!.ParentId.Should().Be(otherParent.Id);
        persisted.Position.Should().Be(10);

        _hierarchyServiceMock.Verify(x => x.RebuildHierarchyAsync(taxonomy.Id, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return success early when same parent and position")]
    public async Task Handle_ShouldReturnSuccess_WhenNoChange()
    {
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, root.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RepositionTaxon.Request { ParentId = root.Id, Position = 1 };

        var result = await _handler.Handle(new RepositionTaxon.Command(taxonomy.Id, taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        _hierarchyServiceMock.Verify(x => x.RebuildHierarchyAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonomyNotFound()
    {
        var result = await _handler.Handle(new RepositionTaxon.Command(Guid.NewGuid(), Guid.NewGuid(), new RepositionTaxon.Request()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxon not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonNotFound()
    {
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RepositionTaxon.Command(taxonomy.Id, Guid.NewGuid(), new RepositionTaxon.Request()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when root locked")]
    public async Task Handle_ShouldReturnFailure_WhenRootLocked()
    {
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(root);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new RepositionTaxon.Command(taxonomy.Id, root.Id, new RepositionTaxon.Request()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.RootLock.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when parent not found")]
    public async Task Handle_ShouldReturnFailure_WhenParentNotFound()
    {
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, root.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RepositionTaxon.Request { ParentId = Guid.NewGuid() };

        _hierarchyServiceMock.Setup(x => x.ValidateDescendantAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TaxonResult.Errors.NotFound);

        var result = await _handler.Handle(new RepositionTaxon.Command(taxonomy.Id, taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when parent taxonomy mismatch")]
    public async Task Handle_ShouldReturnFailure_WhenParentTaxonomyMismatch()
    {
        var taxonomy = TaxonomyMethod.Create("Cat 1", "Cat 1", 0).Value;
        var root1 = TaxonMethod.Create(taxonomy.Id, null, "Root 1", "Root 1", null, 0, "root-1", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, root1.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;

        var otherTaxonomy = TaxonomyMethod.Create("Cat 2", "Cat 2", 0).Value;
        var root2 = TaxonMethod.Create(otherTaxonomy.Id, null, "Root 2", "Root 2", null, 0, "root-2", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().AddRange(taxonomy, otherTaxonomy);
        _dbContext.Set<Taxon>().AddRange(root1, taxon, root2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RepositionTaxon.Request { ParentId = root2.Id };

        _hierarchyServiceMock.Setup(x => x.ValidateDescendantAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TaxonResult.Errors.ParentTaxonomyMismatch);

        var result = await _handler.Handle(new RepositionTaxon.Command(taxonomy.Id, taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.ParentTaxonomyMismatch.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when self-parenting")]
    public async Task Handle_ShouldReturnFailure_WhenSelfParenting()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, root.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RepositionTaxon.Request
        {
            ParentId = taxon.Id,
            Position = 0
        };

        var result = await _handler.Handle(new RepositionTaxon.Command(taxonomy.Id, taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.SelfParenting.Code);
    }
}
