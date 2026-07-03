using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Delete;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonRuleDelete")]
public class DeleteTaxonRuleTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IAutoClassificationService> _autoClassificationMock;
    private readonly Mock<ILogger<DeleteTaxonRule.CommandHandler>> _loggerMock;
    private readonly DeleteTaxonRule.CommandHandler _handler;

    public DeleteTaxonRuleTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _autoClassificationMock = new Mock<IAutoClassificationService>();
        _autoClassificationMock.Setup(x => x.RegenerateForTaxonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _loggerMock = new Mock<ILogger<DeleteTaxonRule.CommandHandler>>();

        _handler = new DeleteTaxonRule.CommandHandler(_dbContext, _autoClassificationMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete rule successfully when taxon is non-automatic")]
    public async Task Handle_ShouldReturnSuccess_WhenTaxonNotAutomatic()
    {
        var taxonomy = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonExtensions.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var rule = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.IsEqualTo, "T-Shirt");

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<TaxonRule>().Add(rule);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteTaxonRule.Command(taxonomy.Id, taxon.Id, rule.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(rule.Id);

        var deleted = await _dbContext.Set<TaxonRule>().FindAsync([rule.Id], TestContext.Current.CancellationToken);
        deleted.Should().BeNull();

        _autoClassificationMock.Verify(x => x.RegenerateForTaxonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should delete rule and trigger auto-classification when taxon is automatic")]
    public async Task Handle_ShouldTriggerAutoClassification_WhenTaxonIsAutomatic()
    {
        var taxonomy = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonExtensions.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, true, null, null, false, null, null).Value;
        var rule = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.IsEqualTo, "T-Shirt");

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<TaxonRule>().Add(rule);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new DeleteTaxonRule.Command(taxonomy.Id, taxon.Id, rule.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        _autoClassificationMock.Verify(x => x.RegenerateForTaxonAsync(taxon.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxon not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonNotFound()
    {
        var result = await _handler.Handle(
            new DeleteTaxonRule.Command(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when rule not found")]
    public async Task Handle_ShouldReturnFailure_WhenRuleNotFound()
    {
        var taxonomy = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonExtensions.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteTaxonRule.Command(taxonomy.Id, taxon.Id, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonRuleResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should not propagate exception when auto-classification throws")]
    public async Task Handle_ShouldNotPropagate_WhenAutoClassificationThrows()
    {
        var taxonomy = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonExtensions.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, true, null, null, false, null, null).Value;
        var rule = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.IsEqualTo, "T-Shirt");

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<TaxonRule>().Add(rule);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _autoClassificationMock.Setup(x => x.RegenerateForTaxonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Service error"));

        var result = await _handler.Handle(new DeleteTaxonRule.Command(taxonomy.Id, taxon.Id, rule.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return taxon-not-found when taxon belongs to different taxonomy")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonIdMismatch()
    {
        var taxonomy = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var otherTaxonomy = TaxonomyExtensions.Create("Brands", "Brands", 0).Value;
        var taxon = TaxonExtensions.Create(otherTaxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().AddRange(taxonomy, otherTaxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new DeleteTaxonRule.Command(taxonomy.Id, taxon.Id, Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }
}
