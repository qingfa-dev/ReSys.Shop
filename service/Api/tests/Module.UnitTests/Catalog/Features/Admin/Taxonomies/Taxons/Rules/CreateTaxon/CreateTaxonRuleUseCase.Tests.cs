using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxons;
using Module.Catalog.Domain.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxons.Rules.Create;
using Module.Catalog.Features.Admin.Taxons.Services.AutoClassification.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Rules.CreateTaxon;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonRuleCreate")]
public class CreateTaxonRuleTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IAutoClassificationService> _autoClassificationMock;
    private readonly Mock<ILogger<CreateTaxonRule.CommandHandler>> _loggerMock;
    private readonly CreateTaxonRule.CommandHandler _handler;

    public CreateTaxonRuleTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _autoClassificationMock = new Mock<IAutoClassificationService>();
        _autoClassificationMock.Setup(x => x.RegenerateForTaxonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _loggerMock = new Mock<ILogger<CreateTaxonRule.CommandHandler>>();

        _handler = new CreateTaxonRule.CommandHandler(_dbContext, _autoClassificationMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create rule successfully when taxon is non-automatic")]
    public async Task Handle_ShouldReturnSuccess_WhenTaxonNotAutomatic()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateTaxonRule.Request
        {
            Type = "product_name",
            MatchPolicy = "is_equal_to",
            Value = "T-Shirt"
        };

        var result = await _handler.Handle(new CreateTaxonRule.Command(taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.TaxonId.Should().Be(taxon.Id);
        result.Value.Type.Should().Be("product_name");
        result.Value.MatchPolicy.Should().Be("is_equal_to");
        result.Value.Value.Should().Be("T-Shirt");

        var persisted = await _dbContext.Set<TaxonRule>().FirstOrDefaultAsync(x => x.TaxonId == taxon.Id, cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Type.Should().Be(TaxonRuleType.ProductName);
        persisted.MatchPolicy.Should().Be(TaxonRuleMatchPolicy.IsEqualTo);
        persisted.Value.Should().Be("T-Shirt");

        _autoClassificationMock.Verify(x => x.RegenerateForTaxonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should create rule and trigger auto-classification when taxon is automatic")]
    public async Task Handle_ShouldTriggerAutoClassification_WhenTaxonIsAutomatic()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, true, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateTaxonRule.Request
        {
            Type = "product_price",
            MatchPolicy = "greater_than",
            Value = "10.00"
        };

        var result = await _handler.Handle(new CreateTaxonRule.Command(taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        _autoClassificationMock.Verify(x => x.RegenerateForTaxonAsync(taxon.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxon not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonNotFound()
    {
        var result = await _handler.Handle(
            new CreateTaxonRule.Command(Guid.NewGuid(), new CreateTaxonRule.Request()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should not propagate exception when auto-classification throws")]
    public async Task Handle_ShouldNotPropagate_WhenAutoClassificationThrows()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, true, null, null, false, null, null).Value;
        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _autoClassificationMock.Setup(x => x.RegenerateForTaxonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var request = new CreateTaxonRule.Request
        {
            Type = "product_name",
            MatchPolicy = "is_equal_to",
            Value = "T-Shirt"
        };

        var result = await _handler.Handle(new CreateTaxonRule.Command(taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }


}
