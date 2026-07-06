using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy.Abstractions;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Update;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonUpdate")]
public class UpdateTaxonTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ITaxonHierarchyService> _hierarchyServiceMock;
    private readonly Mock<ILogger<UpdateTaxon.CommandHandler>> _loggerMock;
    private readonly UpdateTaxon.CommandHandler _handler;

    public UpdateTaxonTests()
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

        _loggerMock = new Mock<ILogger<UpdateTaxon.CommandHandler>>();

        _handler = new UpdateTaxon.CommandHandler(_dbContext, _hierarchyServiceMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update taxon successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var taxonomy = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateTaxon.Request
        {
            Name = "New Shirts",
            Slug = "new-shirts",
            Position = 1
        };

        var result = await _handler.Handle(new UpdateTaxon.Command(taxonomy.Id, taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("new shirts");

        var persisted = await _dbContext.Set<Taxon>().FindAsync([taxon.Id], TestContext.Current.CancellationToken);
        persisted!.Name.Should().Be("new shirts");

        _hierarchyServiceMock.Verify(x => x.RebuildHierarchyAsync(taxonomy.Id, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonomyNotFound()
    {
        var result = await _handler.Handle(new UpdateTaxon.Command(Guid.NewGuid(), Guid.NewGuid(), new UpdateTaxon.Request()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxon not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonNotFound()
    {
        var taxonomy = TaxonomyExtensions.Create("Cat", "Cat", 0).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new UpdateTaxon.Command(taxonomy.Id, Guid.NewGuid(), new UpdateTaxon.Request()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when root taxon locked")]
    public async Task Handle_ShouldReturnFailure_WhenRootLocked()
    {
        var taxonomy = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var other = TaxonMethod.Create(taxonomy.Id, null, "Other", "Other", null, 1, "other", null, null, null, false, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, other);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateTaxon.Request
        {
            Name = "New Root",
            ParentId = other.Id
        };

        var result = await _handler.Handle(new UpdateTaxon.Command(taxonomy.Id, root.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.RootLock.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when self-parenting")]
    public async Task Handle_ShouldReturnFailure_WhenSelfParenting()
    {
        var taxonomy = TaxonomyExtensions.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, root.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateTaxon.Request
        {
            ParentId = taxon.Id
        };

        var result = await _handler.Handle(new UpdateTaxon.Command(taxonomy.Id, taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.SelfParenting.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when sibling with same name exists")]
    public async Task Handle_ShouldReturnFailure_WhenDuplicateName()
    {
        var taxonomy = TaxonomyExtensions.Create("Cat", "Cat", 0).Value;
        var parent = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, parent.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var sibling = TaxonMethod.Create(taxonomy.Id, parent.Id, "Other", "Other", null, 2, "other", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(parent, taxon, sibling);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateTaxon.Request
        {
            Name = "Other"
        };

        var result = await _handler.Handle(new UpdateTaxon.Command(taxonomy.Id, taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.DuplicateName.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when descendant validation fails during parent change")]
    public async Task Handle_ShouldReturnFailure_WhenDescendantValidationFails()
    {
        var taxonomy = TaxonomyExtensions.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, root.Id, "Shirts", "Shirts", null, 1, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var other = TaxonMethod.Create(taxonomy.Id, root.Id, "Other", "Other", null, 2, "other", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, taxon, other);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _hierarchyServiceMock.Setup(x => x.ValidateDescendantAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(TaxonResult.Errors.CircularParenting);

        var request = new UpdateTaxon.Request
        {
            ParentId = other.Id
        };

        var result = await _handler.Handle(new UpdateTaxon.Command(taxonomy.Id, taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.CircularParenting.Code);
    }
}
