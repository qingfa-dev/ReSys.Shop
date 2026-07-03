using System.Reflection;

using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonHierarchy")]
public class TaxonHierarchyInternalTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<TaxonHierarchyService>> _loggerMock;
    private readonly TaxonHierarchyService _service;

    public TaxonHierarchyInternalTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<TaxonHierarchyService>>();

        _service = new TaxonHierarchyService(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<T> InvokePrivateMethodAsync<T>(string methodName, params object?[] args)
    {
        var method = typeof(TaxonHierarchyService).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (method == null) throw new InvalidOperationException($"Method {methodName} not found.");
        
        var task = (Task<T>)method.Invoke(_service, args)!;
        return await task;
    }

    [Fact(DisplayName = "GetTaxonomyOrFailure: Should return taxonomy when exists")]
    public async Task GetTaxonomyOrFailure_ShouldReturnTaxonomy_WhenExists()
    {
        // Arrange
        var taxonomy = TaxonomyExtensions.Create("Test", "Test", 0).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await InvokePrivateMethodAsync<Result<Taxonomy>>("GetTaxonomyOrFailureAsync", taxonomy.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(taxonomy.Id);
    }

    [Fact(DisplayName = "GetTaxonomyOrFailure: Should return failure when not found")]
    public async Task GetTaxonomyOrFailure_ShouldReturnFailure_WhenNotFound()
    {
        // Act
        var result = await InvokePrivateMethodAsync<Result<Taxonomy>>("GetTaxonomyOrFailureAsync", Guid.NewGuid(), CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "GetTaxonOrFailure: Should return taxon when exists in taxonomy")]
    public async Task GetTaxonOrFailure_ShouldReturnTaxon_WhenExists()
    {
        // Arrange
        var taxo = TaxonomyExtensions.Create("T", "T", 0).Value;
        var taxon = TaxonExtensions.Create(taxo.Id, null, "T1", "T1", null, 0, "t1", null, null, null, false, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxo);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await InvokePrivateMethodAsync<Result<Taxon>>("GetTaxonOrFailureAsync", taxon.Id, taxo.Id, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(taxon.Id);
    }

    [Fact(DisplayName = "GetTaxonOrFailure: Should return failure when mismatch or not found")]
    public async Task GetTaxonOrFailure_ShouldReturnFailure_WhenMismatch()
    {
        // Arrange
        var taxo = TaxonomyExtensions.Create("T", "T", 0).Value;
        var otherTaxoId = Guid.NewGuid();
        var taxon = TaxonExtensions.Create(taxo.Id, null, "T1", "T1", null, 0, "t1", null, null, null, false, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxo);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await InvokePrivateMethodAsync<Result<Taxon>>("GetTaxonOrFailureAsync", taxon.Id, otherTaxoId, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "LoadTaxonTree: Should load full tree top-down")]
    public async Task LoadTaxonTree_ShouldLoadFullTree()
    {
        // Arrange
        var taxo = TaxonomyExtensions.Create("T", "T", 0).Value;
        var root = TaxonExtensions.Create(taxo.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var child = TaxonExtensions.Create(taxo.Id, root.Id, "Child", "Child", null, 0, "child", null, null, null, false, null, null, false, null, null).Value;
        
        root.Lft = 1; root.Rgt = 4;
        child.Lft = 2; child.Rgt = 3;

        _dbContext.Set<Taxonomy>().Add(taxo);
        _dbContext.Set<Taxon>().AddRange(root, child);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await InvokePrivateMethodAsync<Result<List<Taxon>>>("LoadTaxonTreeAsync", taxo.Id, null, true, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Id.Should().Be(root.Id);
        result.Value[1].Id.Should().Be(child.Id);
    }

    [Fact(DisplayName = "LoadTaxonTree: Should load anchored subtree")]
    public async Task LoadTaxonTree_ShouldLoadSubtree()
    {
        // Arrange
        var taxo = TaxonomyExtensions.Create("T", "T", 0).Value;
        var root = TaxonExtensions.Create(taxo.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var branch = TaxonExtensions.Create(taxo.Id, root.Id, "Branch", "Branch", null, 0, "branch", null, null, null, false, null, null, false, null, null).Value;
        var leaf = TaxonExtensions.Create(taxo.Id, branch.Id, "Leaf", "Leaf", null, 0, "leaf", null, null, null, false, null, null, false, null, null).Value;
        var other = TaxonExtensions.Create(taxo.Id, root.Id, "Other", "Other", null, 1, "other", null, null, null, false, null, null, false, null, null).Value;

        root.Lft = 1; root.Rgt = 8;
        branch.Lft = 2; branch.Rgt = 5;
        leaf.Lft = 3; leaf.Rgt = 4;
        other.Lft = 6; other.Rgt = 7;

        _dbContext.Set<Taxonomy>().Add(taxo);
        _dbContext.Set<Taxon>().AddRange(root, branch, leaf, other);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act: Load subtree starting from 'branch'
        var result = await InvokePrivateMethodAsync<Result<List<Taxon>>>("LoadTaxonTreeAsync", taxo.Id, branch.Id, true, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value.Select(x => x.Id).Should().Contain([branch.Id, leaf.Id]);
        result.Value.Select(x => x.Id).Should().NotContain(root.Id);
        result.Value.Select(x => x.Id).Should().NotContain(other.Id);
    }
}
