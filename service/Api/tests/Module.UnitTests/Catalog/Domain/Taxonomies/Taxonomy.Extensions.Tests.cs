using Module.Catalog.Domain.Taxonomies;

namespace Module.UnitTests.Catalog.Domain.Taxonomies;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "Taxonomy")]
public class TaxonomyExtensionsTests
{
    [Theory(DisplayName = "Create: Should return Taxonomy with correct properties")]
    [InlineData("Categories", "Product Categories", 0)]
    [InlineData("Brands", null, 1)]
    [InlineData("tags", "Product Tags", -1)]
    public void Create_WithValidParameters_ShouldReturnTaxonomy(string name, string? presentation, int position)
    {
        var id = Guid.NewGuid();

        var result = TaxonomyMethod.Create(name, presentation, position, id);
        var taxonomy = result.Value;

        result.IsSuccess.Should().BeTrue();
        taxonomy.Should().NotBeNull();
        taxonomy.Id.Should().Be(id);
        taxonomy.Name.Should().Be(name.ToLowerInvariant());
        taxonomy.Presentation.Should().Be(presentation);
        taxonomy.Position.Should().Be(position);
    }

    [Fact(DisplayName = "Create: Should generate new ID when not provided")]
    public void Create_WithoutId_ShouldGenerateNewId()
    {
        var result = TaxonomyMethod.Create("Name", "Presentation", 0);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
    }

    [Theory(DisplayName = "Update: Should update properties")]
    [InlineData("New Name", "New Presentation", 5)]
    [InlineData(null, "Just Presentation", 0)]
    [InlineData("Just Name", null, 10)]
    public void Update_WithParameters_ShouldUpdateCorrectly(string? name, string? presentation, int position)
    {
        var taxonomy = TaxonomyMethod.Create("Old", "Old", 1).Value;

        var result = taxonomy.Update(name, presentation, position);

        result.IsSuccess.Should().BeTrue();
        if (name != null)
            taxonomy.Name.Should().Be(name.ToLowerInvariant());
        if (presentation != null) taxonomy.Presentation.Should().Be(presentation);
        taxonomy.Position.Should().Be(position);
    }

    [Fact(DisplayName = "Delete: Should mark as deleted")]
    public void Delete_ShouldSetIsDeletedTrue()
    {
        var taxonomy = TaxonomyMethod.Create("Name", "Presentation", 0).Value;

        var result = taxonomy.Delete();

        result.IsSuccess.Should().BeTrue();
        taxonomy.IsDeleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Restore: Should mark as not deleted")]
    public void Restore_ShouldSetIsDeletedFalse()
    {
        var taxonomy = TaxonomyMethod.Create("Name", "Presentation", 0).Value;
        taxonomy.Delete();

        var result = taxonomy.Restore();

        result.IsSuccess.Should().BeTrue();
        taxonomy.IsDeleted.Should().BeFalse();
    }

    [Fact(DisplayName = "Delete: When already deleted should still work")]
    public void Delete_WhenAlreadyDeleted_ShouldStillSetDeleted()
    {
        var taxonomy = TaxonomyMethod.Create("Name", "Presentation", 0).Value;
        taxonomy.Delete();

        var result = taxonomy.Delete();

        result.IsSuccess.Should().BeTrue();
        taxonomy.IsDeleted.Should().BeTrue();
    }

    [Fact(DisplayName = "Restore: When not deleted should still work")]
    public void Restore_WhenNotDeleted_ShouldStillRestore()
    {
        var taxonomy = TaxonomyMethod.Create("Name", "Presentation", 0).Value;

        var result = taxonomy.Restore();

        result.IsSuccess.Should().BeTrue();
        taxonomy.IsDeleted.Should().BeFalse();
    }
}
