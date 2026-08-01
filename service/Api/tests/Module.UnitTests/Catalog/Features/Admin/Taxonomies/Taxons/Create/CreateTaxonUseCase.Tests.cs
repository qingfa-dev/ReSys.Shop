using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxons.Create;
using Module.Catalog.Features.Admin.Taxons.Services.Hierarchy.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonCreate")]
public class CreateTaxonTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ITaxonHierarchyService> _hierarchyServiceMock;
    private readonly CreateTaxon.CommandHandler _handler;

    public CreateTaxonTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        Mock<ICurrentUser> currentUserMock = new();
        currentUserMock.Setup(x => x.UserName).Returns("admin");

        _hierarchyServiceMock = new Mock<ITaxonHierarchyService>();
        _hierarchyServiceMock.Setup(x => x.RebuildHierarchyAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        Mock<ILogger<CreateTaxon.CommandHandler>> loggerMock = new();

        _handler = new CreateTaxon.CommandHandler(_dbContext, _hierarchyServiceMock.Object, loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create taxon successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateTaxon.Request
        {
            Name = "Shirts",
            Slug = "shirts",
            Position = 0,
            TaxonomyId = taxonomy.Id,
        };

        var result = await _handler.Handle(new CreateTaxon.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("shirts");

        var persisted = await _dbContext.Set<Taxon>().FirstOrDefaultAsync(x => x.Name == "shirts", cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.TaxonomyId.Should().Be(taxonomy.Id);

        _hierarchyServiceMock.Verify(x => x.RebuildHierarchyAsync(taxonomy.Id, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonomyNotFound()
    {
        var result = await _handler.Handle(new CreateTaxon.Command(new CreateTaxon.Request()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);

        _hierarchyServiceMock.Verify(x => x.RebuildHierarchyAsync(It.IsAny<Guid>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should return failure when parent taxon invalid")]
    public async Task Handle_ShouldReturnFailure_WhenParentNotFound()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateTaxon.Request
        {
            Name = "Shirts",
            ParentId = Guid.NewGuid(),
            TaxonomyId = taxonomy.Id,
        };

        var result = await _handler.Handle(new CreateTaxon.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.InvalidParent.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when parent from different taxonomy")]
    public async Task Handle_ShouldReturnFailure_WhenParentTaxonomyMismatch()
    {
        var taxonomy = TaxonomyMethod.Create("Cat 1", "Cat 1", 0).Value;
        var otherTaxonomy = TaxonomyMethod.Create("Cat 2", "Cat 2", 0).Value;
        var otherRoot = TaxonMethod.Create(otherTaxonomy.Id, null, "Other Root", "Other Root", null, 0, "other-root", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().AddRange(taxonomy, otherTaxonomy);
        _dbContext.Set<Taxon>().Add(otherRoot);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateTaxon.Request
        {
            Name = "Shirts",
            ParentId = otherRoot.Id,
            TaxonomyId = taxonomy.Id,
        };

        var result = await _handler.Handle(new CreateTaxon.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.ParentTaxonomyMismatch.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when sibling with same name exists")]
    public async Task Handle_ShouldReturnFailure_WhenDuplicateName()
    {
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var parent = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var sibling = TaxonMethod.Create(taxonomy.Id, parent.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(parent, sibling);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateTaxon.Request
        {
            Name = "Shirts",
            ParentId = parent.Id,
            TaxonomyId = taxonomy.Id,
        };

        var result = await _handler.Handle(new CreateTaxon.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.DuplicateName.Code);
    }

    [Fact(DisplayName = "Handler: Should create child taxon under valid parent")]
    public async Task Handle_ShouldReturnSuccess_WhenWithValidParent()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var parent = TaxonMethod.Create(taxonomy.Id, null, "Clothing", "Clothing", null, 0, "clothing", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(parent);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateTaxon.Request
        {
            Name = "T-Shirts",
            Presentation = "T-Shirts",
            Slug = "t-shirts",
            ParentId = parent.Id,
            Position = 1,
            TaxonomyId = taxonomy.Id,
        };

        var result = await _handler.Handle(new CreateTaxon.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<Taxon>()
            .FirstOrDefaultAsync(x => x.Name == "t_shirts", cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.ParentId.Should().Be(parent.Id);
        persisted.Position.Should().Be(1);

        _hierarchyServiceMock.Verify(x => x.RebuildHierarchyAsync(taxonomy.Id, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
