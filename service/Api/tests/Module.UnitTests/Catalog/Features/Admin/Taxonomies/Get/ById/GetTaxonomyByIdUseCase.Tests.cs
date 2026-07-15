using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Get.ById;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonomyGetById")]
public class GetTaxonomyByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetTaxonomyById.QueryHandler _handler;

    public GetTaxonomyByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxonomy).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetTaxonomyById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return taxonomy when found")]
    public async Task Handle_ShouldReturnSuccess_WhenFound()
    {
        // Arrange
        var entity = TaxonomyMethod.Create("Categories", "Presentation", 0).Value;
        _dbContext.Set<Taxonomy>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetTaxonomyById.Query(entity.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(entity.Id);
        result.Value.Name.Should().Be(entity.Name);
    }

    [Fact(DisplayName = "Handler: Should return failure when not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        // Act
        var result = await _handler.Handle(new GetTaxonomyById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy is soft-deleted")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonomyIsSoftDeleted()
    {
        var entity = TaxonomyMethod.Create("Deleted", "Deleted", 0).Value;
        entity.Delete();
        _dbContext.Set<Taxonomy>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetTaxonomyById.Query(entity.Id), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);
    }
}
