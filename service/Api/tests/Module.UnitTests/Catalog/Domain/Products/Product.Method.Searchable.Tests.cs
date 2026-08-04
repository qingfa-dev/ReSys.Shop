using Module.Catalog.Domain.Products;

namespace Module.UnitTests.Catalog.Domain.Products;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Product")]
[Trait("Concern", "Searchable")]
public class ProductMethodSearchableTests
{
    [Fact(DisplayName = "SearchIndexText: Should combine name, description, slug")]
    public void SearchIndexText_ShouldCombineFields()
    {
        var product = ProductMethod.Create(name: "Product Name", slug: "product-slug", description: "Product description").Value;

        var result = product.SearchIndexText();

        result.Should().Be("Product Name Product description product-slug");
    }

    [Fact(DisplayName = "SearchIndexText: Should include meta keywords and description when present")]
    public void SearchIndexText_WithMetaFields_ShouldIncludeThem()
    {
        var product = ProductMethod.Create(name: "Product", slug: "product-slug", description: "desc", metaKeywords: "keyword1 keyword2", metaDescription: "meta desc").Value;

        var result = product.SearchIndexText();

        result.Should().Contain("keyword1");
        result.Should().Contain("meta desc");
    }

    [Fact(DisplayName = "SearchTokens: Should produce distinct lowercase tokens")]
    public void SearchTokens_ShouldProduceDistinctLowerTokens()
    {
        var product = ProductMethod.Create(name: "Hello World", slug: "hello-world", description: "Hello World").Value;

        var result = product.SearchTokens();

        result.Should().Contain("hello");
        result.Should().Contain("world");
        result.Should().HaveCount(2);
    }

    [Fact(DisplayName = "MatchesSearchQuery: Should return true when query matches name")]
    public void MatchesSearchQuery_WhenMatchesName_ShouldReturnTrue()
    {
        var product = ProductMethod.Create(name: "Blue T-Shirt", slug: "blue-t-shirt", description: "A blue shirt").Value;

        var result = product.MatchesSearchQuery("blue");

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "MatchesSearchQuery: Should return true when query is empty")]
    public void MatchesSearchQuery_WhenEmpty_ShouldReturnTrue()
    {
        var product = ProductMethod.Create(name: "Product", slug: "product").Value;

        var result = product.MatchesSearchQuery("");

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "MatchesSearchQuery: Should return false when query does not match")]
    public void MatchesSearchQuery_WhenNoMatch_ShouldReturnFalse()
    {
        var product = ProductMethod.Create("Blue T-Shirt", "blue-t-shirt").Value;

        var result = product.MatchesSearchQuery("red");

        result.Should().BeFalse();
    }
}
