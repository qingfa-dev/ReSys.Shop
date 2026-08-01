using System.Reflection;

using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxons.Services.Hierarchy;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Services.Hierarchy;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonHierarchy")]
public class TaxonHierarchyValidationTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<TaxonHierarchyService>> _loggerMock;
    private readonly TaxonHierarchyService _service;

    public TaxonHierarchyValidationTests()
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

    private static T InvokePrivateStaticMethod<T>(string methodName, params object?[] args)
    {
        var method = typeof(TaxonHierarchyService).GetMethod(methodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        if (method == null) throw new InvalidOperationException($"Method {methodName} not found.");
        return (T)method.Invoke(null, args)!;
    }

    [Fact(DisplayName = "ValidateDescendant: Should return success when not a descendant")]
    public async Task ValidateDescendant_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var child1 = TaxonMethod.Create(taxonomy.Id, root.Id, "C1", "C1", null, 0, "c1", null, null, null, false, null, null, false, null, null).Value;
        var child2 = TaxonMethod.Create(taxonomy.Id, root.Id, "C2", "C2", null, 1, "c2", null, null, null, false, null, null, false, null, null).Value;
        
        root.Lft = 1; root.Rgt = 6;
        child1.Lft = 2; child1.Rgt = 3;
        child2.Lft = 4; child2.Rgt = 5;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, child1, child2);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _service.ValidateDescendantAsync(child1.Id, child2.Id, ct: ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "ValidateDescendant: Should return failure when circular parenting detected")]
    public async Task ValidateDescendant_ShouldReturnFailure_WhenCircular()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var child1 = TaxonMethod.Create(taxonomy.Id, root.Id, "C1", "C1", null, 0, "c1", null, null, null, false, null, null, false, null, null).Value;
        var grandchild = TaxonMethod.Create(taxonomy.Id, child1.Id, "G1", "G1", null, 0, "g1", null, null, null, false, null, null, false, null, null).Value;
        
        root.Lft = 1; root.Rgt = 6;
        child1.Lft = 2; child1.Rgt = 5;
        grandchild.Lft = 3; grandchild.Rgt = 4;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, child1, grandchild);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _service.ValidateDescendantAsync(child1.Id, grandchild.Id, ct: ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.CircularParenting.Code);
    }

    [Fact(DisplayName = "ValidateDescendant: Should return failure when taxons from different taxonomies")]
    public async Task ValidateDescendant_ShouldReturnFailure_WhenDifferentTaxonomies()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxo1 = TaxonomyMethod.Create("T1", "T1", 0).Value;
        var taxo2 = TaxonomyMethod.Create("T2", "T2", 0).Value;
        var root1 = TaxonMethod.Create(taxo1.Id, null, "R1", "R1", null, 0, "r1", null, null, null, false, null, null, false, null, null).Value;
        var root2 = TaxonMethod.Create(taxo2.Id, null, "R2", "R2", null, 0, "r2", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().AddRange(taxo1, taxo2);
        _dbContext.Set<Taxon>().AddRange(root1, root2);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _service.ValidateDescendantAsync(root1.Id, root2.Id, ct: ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.ParentTaxonomyMismatch.Code);
    }

    [Fact(DisplayName = "ValidateDescendant: Should return failure when taxon not found")]
    public async Task ValidateDescendant_ShouldReturnFailure_WhenNotFound()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;

        // Act
        var result = await _service.ValidateDescendantAsync(Guid.NewGuid(), Guid.NewGuid(), ct: ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "ValidateHierarchy: Should return success for healthy hierarchy")]
    public async Task ValidateHierarchy_ShouldReturnSuccess_WhenHealthy()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        root.Lft = 1; root.Rgt = 2; root.Depth = 0;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(root);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _service.ValidateHierarchyAsync(taxonomy.Id, ct: ct);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "ValidateHierarchy: Should return failure when overlapping boundaries detected")]
    public async Task ValidateHierarchy_ShouldReturnFailure_WhenOverlapping()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var child = TaxonMethod.Create(taxonomy.Id, root.Id, "C1", "C1", null, 0, "c1", null, null, null, false, null, null, false, null, null).Value;
        
        root.Lft = 1; root.Rgt = 4;
        child.Lft = 1; child.Rgt = 3; // Duplicate Lft=1

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(root, child);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _service.ValidateHierarchyAsync(taxonomy.Id, ct: ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Taxon.Hierarchy.OverlappingBoundaries");
    }

    [Fact(DisplayName = "ValidateHierarchy: Should return failure when invalid nested set (Lft >= Rgt)")]
    public async Task ValidateHierarchy_ShouldReturnFailure_WhenInvalidNestedSet()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var root = TaxonMethod.Create(taxonomy.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        
        root.Lft = 5; root.Rgt = 5; // Invalid

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(root);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _service.ValidateHierarchyAsync(taxonomy.Id, ct: ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Taxon.Hierarchy.InvalidNestedSet");
    }

    [Fact(DisplayName = "ValidateHierarchy: Should return failure when no root node (cycle)")]
    public async Task ValidateHierarchy_ShouldReturnFailure_WhenNoRoot()
    {
        // Arrange
        var ct = TestContext.Current.CancellationToken;
        var taxonomy = TaxonomyMethod.Create("Cat", "Cat", 0).Value;
        var taxonA = TaxonMethod.Create(taxonomy.Id, null, "A", "A", null, 0, "a", null, null, null, false, null, null, false, null, null).Value;
        var taxonB = TaxonMethod.Create(taxonomy.Id, null, "B", "B", null, 1, "b", null, null, null, false, null, null, false, null, null).Value;
        
        // Create a cycle: A -> B -> A
        taxonA.Lft = 1; taxonA.Rgt = 2;
        taxonB.Lft = 3; taxonB.Rgt = 4;
        
        // Set ParentIds manually to bypass validation during creation
        typeof(Taxon).GetProperty("ParentId")!.SetValue(taxonA, taxonB.Id);
        typeof(Taxon).GetProperty("ParentId")!.SetValue(taxonB, taxonA.Id);

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().AddRange(taxonA, taxonB);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        // Act
        var result = await _service.ValidateHierarchyAsync(taxonomy.Id, ct: ct);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Taxon.NoRoot");
    }

    [Fact(DisplayName = "VerifyStructuralIntegrity: Should detect boundary violations")]
    public void VerifyStructuralIntegrity_ShouldDetectBoundaryViolations()
    {
        // Arrange
        var taxoId = Guid.NewGuid();
        var root = TaxonMethod.Create(taxoId, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        var child = TaxonMethod.Create(taxoId, root.Id, "Child", "Child", null, 0, "child", null, null, null, false, null, null, false, null, null).Value;
        
        root.Lft = 1; root.Rgt = 4;
        child.Lft = 0; child.Rgt = 5; // Outside parent

        var taxons = new List<Taxon> { root, child };

        // Act
        var result = InvokePrivateStaticMethod<Result>("VerifyStructuralIntegrity", taxons);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be("Taxon.Hierarchy.BoundaryViolation");
    }

    [Fact(DisplayName = "VerifyStructuralIntegrity: Should return ok for empty list")]
    public void VerifyStructuralIntegrity_ShouldReturnOk_WhenEmpty()
    {
        var result = InvokePrivateStaticMethod<Result>("VerifyStructuralIntegrity", new List<Taxon>());

        result.IsSuccess.Should().BeTrue();
    }
}
