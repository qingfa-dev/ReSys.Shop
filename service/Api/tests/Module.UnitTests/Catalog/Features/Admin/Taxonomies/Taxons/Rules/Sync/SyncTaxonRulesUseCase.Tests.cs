using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Sync;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Services.AutoClassification.Abstractions;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonRuleSync")]
public class SyncTaxonRulesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<IAutoClassificationService> _autoClassificationMock;
    private readonly Mock<ILogger<SyncTaxonRules.PagedQueryHandler>> _loggerMock;
    private readonly SyncTaxonRules.PagedQueryHandler _handler;

    public SyncTaxonRulesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _autoClassificationMock = new Mock<IAutoClassificationService>();
        _autoClassificationMock.Setup(x => x.RegenerateForTaxonAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _loggerMock = new Mock<ILogger<SyncTaxonRules.PagedQueryHandler>>();

        _handler = new SyncTaxonRules.PagedQueryHandler(_dbContext, _autoClassificationMock.Object, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should add new rules when they have no Id")]
    public async Task Handle_ShouldAddNewRules_WhenNoId()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncTaxonRules.Request
        {
            Rules =
            [
                new SyncTaxonRules.SyncItem
                {
                    Type = "product_name",
                    MatchPolicy = "is_equal_to",
                    Value = "T-Shirt"
                },
                new SyncTaxonRules.SyncItem
                {
                    Type = "product_price",
                    MatchPolicy = "greater_than",
                    Value = "10.00"
                }
            ]
        };

        var result = await _handler.Handle(new SyncTaxonRules.Command(taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);

        var persisted = await _dbContext.Set<TaxonRule>().Where(x => x.TaxonId == taxon.Id).ToListAsync(TestContext.Current.CancellationToken);
        persisted.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should update existing and remove omitted rules")]
    public async Task Handle_ShouldUpdateAndRemove_WhenIdsProvided()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var existingRule = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.IsEqualTo, "Old");
        var toRemove = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductSku, TaxonRuleMatchPolicy.Contains, "XYZ");

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<TaxonRule>().AddRange(existingRule, toRemove);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncTaxonRules.Request
        {
            Rules =
            [
                new SyncTaxonRules.SyncItem
                {
                    Id = existingRule.Id,
                    Type = "product_price",
                    MatchPolicy = "greater_than",
                    Value = "50.00"
                }
            ]
        };

        var result = await _handler.Handle(new SyncTaxonRules.Command(taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);

        var updated = await _dbContext.Set<TaxonRule>().FindAsync([existingRule.Id], TestContext.Current.CancellationToken);
        updated.Should().NotBeNull();
        updated!.Type.Should().Be(TaxonRuleType.ProductPrice);
        updated.Value.Should().Be("50.00");

        var removed = await _dbContext.Set<TaxonRule>().FindAsync([toRemove.Id], TestContext.Current.CancellationToken);
        removed.Should().BeNull();
    }

    [Fact(DisplayName = "Handler: Should handle mixed add-update-remove correctly")]
    public async Task Handle_ShouldHandleMixedScenario()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var keep = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.IsEqualTo, "Keep");
        var remove = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductSku, TaxonRuleMatchPolicy.Contains, "Remove");

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<TaxonRule>().AddRange(keep, remove);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncTaxonRules.Request
        {
            Rules =
            [
                new SyncTaxonRules.SyncItem
                {
                    Id = keep.Id,
                    Type = "product_price",
                    MatchPolicy = "greater_than",
                    Value = "25.00"
                },
                new SyncTaxonRules.SyncItem
                {
                    Type = "product_sku",
                    MatchPolicy = "is_equal_to",
                    Value = "NEW-SKU"
                }
            ]
        };

        var result = await _handler.Handle(new SyncTaxonRules.Command(taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);

        var allPersisted = await _dbContext.Set<TaxonRule>().Where(x => x.TaxonId == taxon.Id).ToListAsync(TestContext.Current.CancellationToken);
        allPersisted.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should remove all existing rules when incoming list is empty")]
    public async Task Handle_ShouldRemoveAll_WhenEmptyIncomingList()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var rule1 = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.IsEqualTo, "A");
        var rule2 = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductPrice, TaxonRuleMatchPolicy.GreaterThan, "10");

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<TaxonRule>().AddRange(rule1, rule2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncTaxonRules.Request { Rules = [] };

        var result = await _handler.Handle(new SyncTaxonRules.Command(taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();

        var persisted = await _dbContext.Set<TaxonRule>().Where(x => x.TaxonId == taxon.Id).ToListAsync(TestContext.Current.CancellationToken);
        persisted.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return failure when taxon not found")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonNotFound()
    {
        var result = await _handler.Handle(
            new SyncTaxonRules.Command(Guid.NewGuid(), new SyncTaxonRules.Request()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeFalse();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should trigger auto-classification when taxon is automatic")]
    public async Task Handle_ShouldTriggerAutoClassification_WhenTaxonIsAutomatic()
    {
        var taxonomy = TaxonomyMethod.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, true, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncTaxonRules.Request
        {
            Rules =
            [
                new SyncTaxonRules.SyncItem
                {
                    Type = "product_name",
                    MatchPolicy = "is_equal_to",
                    Value = "Shirt"
                }
            ]
        };

        var result = await _handler.Handle(new SyncTaxonRules.Command(taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        _autoClassificationMock.Verify(x => x.RegenerateForTaxonAsync(taxon.Id, It.IsAny<CancellationToken>()), Times.Once);
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

        var request = new SyncTaxonRules.Request
        {
            Rules =
            [
                new SyncTaxonRules.SyncItem
                {
                    Type = "product_name",
                    MatchPolicy = "is_equal_to",
                    Value = "Shirt"
                }
            ]
        };

        var result = await _handler.Handle(new SyncTaxonRules.Command(taxon.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }


}
