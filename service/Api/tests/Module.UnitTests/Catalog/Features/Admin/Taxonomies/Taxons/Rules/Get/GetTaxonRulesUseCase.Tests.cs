using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Domain.Taxonomies.Taxons.Rules;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Get;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Taxons.Rules.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonRuleGet")]
public class GetTaxonRulesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetTaxonRules.QueryHandler _handler;

    public GetTaxonRulesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxon).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetTaxonRules.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return all rules for taxon ordered by Type")]
    public async Task Handle_ShouldReturnRules_WhenTaxonExists()
    {
        var taxonomy = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;

        var rule1 = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductPrice, TaxonRuleMatchPolicy.GreaterThan, "10.00");
        var rule2 = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductName, TaxonRuleMatchPolicy.IsEqualTo, "Shirt");

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<TaxonRule>().AddRange(rule1, rule2);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetTaxonRules.Query(taxonomy.Id, taxon.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(2);
        result.Value[0].Type.Should().Be("product_name");
        result.Value[1].Type.Should().Be("product_price");
    }

    [Fact(DisplayName = "Handler: Should return empty list when no rules exist")]
    public async Task Handle_ShouldReturnEmptyList_WhenNoRules()
    {
        var taxonomy = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetTaxonRules.Query(taxonomy.Id, taxon.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return failure when taxon does not exist")]
    public async Task Handle_ShouldReturnFailure_WhenTaxonNotFound()
    {
        var result = await _handler.Handle(new GetTaxonRules.Query(Guid.NewGuid(), Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should map rule properties correctly in response")]
    public async Task Handle_ShouldMapRuleProperties_WhenRulesExist()
    {
        var taxonomy = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var taxon = TaxonMethod.Create(taxonomy.Id, null, "Shirts", "Shirts", null, 0, "shirts", null, null, null, false, null, null, false, null, null).Value;
        var rule = TaxonRuleExtensions.Create(taxon.Id, TaxonRuleType.ProductSku, TaxonRuleMatchPolicy.Contains, "ABC");

        _dbContext.Set<Taxonomy>().Add(taxonomy);
        _dbContext.Set<Taxon>().Add(taxon);
        _dbContext.Set<TaxonRule>().Add(rule);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetTaxonRules.Query(taxonomy.Id, taxon.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        var response = result.Value.Single();
        response.Id.Should().Be(rule.Id);
        response.TaxonId.Should().Be(taxon.Id);
        response.Type.Should().Be("product_sku");
        response.MatchPolicy.Should().Be("contains");
        response.Value.Should().Be("ABC");
    }
}
