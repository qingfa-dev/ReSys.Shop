using Module.Catalog.Domain.OptionTypes;

namespace Module.UnitTests.Catalog.Domain.OptionTypes;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "OptionType")]
public class OptionTypeMethodTests
{
    [Theory(DisplayName = "Create: Should return OptionType with correct properties")]
    [InlineData("Color", "Color", 1)]
    [InlineData("Size", "Size", 0)]
    public void Create_WithValidParameters_ShouldReturnOptionType(string name, string presentation, int position)
    {
        var id = Guid.NewGuid();

        var result = OptionTypeMethod.Create(name, presentation, position, id: id);
        var optionType = result.Value;

        result.IsSuccess.Should().BeTrue();
        optionType.Should().NotBeNull();
        optionType.Id.Should().Be(id);
        optionType.Name.Should().Be(name);
        optionType.Presentation.Should().Be(presentation);
        optionType.Position.Should().Be(position);
    }

    [Fact(DisplayName = "Create: Should generate new ID when none is provided")]
    public void Create_WithoutId_ShouldGenerateNewId()
    {
        var result = OptionTypeMethod.Create("Size", "Size");

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "Create: With null presentation should set null")]
    public void Create_WithNullPresentation_ShouldSetNull()
    {
        var result = OptionTypeMethod.Create("Name", null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Presentation.Should().BeNull();
    }

    [Theory(DisplayName = "Update: Should update properties correctly")]
    [InlineData("New Name", "New Presentation", 5, true)]
    [InlineData("Another Name", "Another Presentation", -1, false)]
    public void Update_WithValidParameters_ShouldUpdateProperties(string newName, string newPresentation, int newPosition, bool filterable)
    {
        var optionType = OptionTypeMethod.Create("Old Name", "Old Presentation").Value;

        var result = optionType.Update(newName, newPresentation, newPosition, filterable);

        result.IsSuccess.Should().BeTrue();
        optionType.Name.Should().Be(newName);
        optionType.Presentation.Should().Be(newPresentation);
        optionType.Position.Should().Be(newPosition);
        optionType.Filterable.Should().Be(filterable);
    }

    [Fact(DisplayName = "Delete: Should mark as deleted")]
    public void Delete_ShouldSetIsDeletedTrue()
    {
        var optionType = OptionTypeMethod.Create("Name", "Presentation").Value;

        var result = optionType.Delete();

        result.IsSuccess.Should().BeTrue();
        optionType.IsDeleted.Should().BeTrue();
    }
}
