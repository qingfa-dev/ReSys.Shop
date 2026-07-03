using Module.Catalog.Domain.Products;

namespace Module.UnitTests.Catalog.Domain.Products;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Product")]
[Trait("Concern", "Slugs")]
public class ProductMethodSlugsTests
{
    [Fact(DisplayName = "GenerateSlug: Should return existing slug")]
    public void GenerateSlug_WhenSlugExists_ShouldReturnSlug()
    {
        var product = ProductMethod.Create("Product", "existing-slug").Value;

        var result = product.GenerateSlug();

        result.Should().Be("existing-slug");
    }

    [Fact(DisplayName = "GenerateSlug: Should generate from name when no slug")]
    public void GenerateSlug_WhenNoSlug_ShouldGenerateFromName()
    {
        var product = ProductMethod.Create("Product", "product").Value;
        product.Slug = null!;

        var result = product.GenerateSlug();

        result.Should().Be("product");
    }

    [Fact(DisplayName = "GenerateSlugFromName: Should produce URL-safe slug")]
    public void GenerateSlugFromName_WithSpaces_ShouldProduceHyphenated()
    {
        var result = ProductMethod.GenerateSlugFromName("My Great Product");

        result.Should().Be("my-great-product");
    }

    [Fact(DisplayName = "GenerateSlugFromName: Should remove special characters")]
    public void GenerateSlugFromName_WithSpecialChars_ShouldRemoveThem()
    {
        var result = ProductMethod.GenerateSlugFromName("Hello! @World #2024");

        result.Should().Be("hello-world-2024");
    }

    [Fact(DisplayName = "GenerateSlugFromName: Should return short id when name is empty")]
    public void GenerateSlugFromName_WhenEmpty_ShouldReturnId()
    {
        var result = ProductMethod.GenerateSlugFromName("");

        result.Length.Should().Be(8);
    }

    [Fact(DisplayName = "IsSlugAvailable: Should return true when different from current")]
    public void IsSlugAvailable_WhenDifferent_ShouldReturnTrue()
    {
        var product = ProductMethod.Create("Product", "current-slug").Value;

        var result = product.IsSlugAvailable("new-slug");

        result.Should().BeTrue();
    }

    [Fact(DisplayName = "IsSlugAvailable: Should return false when same as current")]
    public void IsSlugAvailable_WhenSame_ShouldReturnFalse()
    {
        var product = ProductMethod.Create("Product", "current-slug").Value;

        var result = product.IsSlugAvailable("current-slug");

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "IsSlugAvailable: Should return false when candidate is empty")]
    public void IsSlugAvailable_WhenEmpty_ShouldReturnFalse()
    {
        var product = ProductMethod.Create("Product", "current-slug").Value;

        var result = product.IsSlugAvailable("");

        result.Should().BeFalse();
    }

    [Fact(DisplayName = "NormalizeSlug: Should downcase the slug")]
    public void NormalizeSlug_WhenHasUpper_ShouldDowncase()
    {
        var product = ProductMethod.Create("Product", "UPPER-SLUG").Value;

        product.NormalizeSlug();

        product.Slug.Should().Be("upper-slug");
    }

    [Fact(DisplayName = "EnsureSlugIsUnique: Should return candidate when available")]
    public void EnsureSlugIsUnique_WhenAvailable_ShouldReturnCandidate()
    {
        var product = ProductMethod.Create("Product", "current-slug").Value;

        var result = product.EnsureSlugIsUnique("new-slug");

        result.Should().Be("new-slug");
    }

    [Fact(DisplayName = "EnsureSlugIsUnique: Should append suffix when not available")]
    public void EnsureSlugIsUnique_WhenCollision_ShouldAppendSuffix()
    {
        var product = ProductMethod.Create("Product", "same-slug").Value;

        var result = product.EnsureSlugIsUnique("same-slug");

        result.Should().StartWith("same-slug-");
        result.Length.Should().BeLessThanOrEqualTo(255);
    }
}
