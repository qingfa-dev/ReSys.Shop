using Module.Catalog.Domain.OptionTypes.Values;

namespace Module.UnitTests.Catalog.Domain.OptionTypes.Values;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Entity", "OptionValue")]
public class OptionValueExtensionsTests
{
    [Theory(DisplayName = "Create: Should return OptionValue with correct properties")]
    [InlineData("Red", "Red", 1)]
    [InlineData("XL", "Extra Large", 0)]
    public void Create_WithValidParameters_ShouldReturnOptionValue(string name, string presentation, int position)
    {
        var optionTypeId = Guid.NewGuid();
        var id = Guid.NewGuid();

        var result = OptionValueMethod.Create(optionTypeId, name, presentation, position, id);
        var optionValue = result.Value;

        result.IsSuccess.Should().BeTrue();
        optionValue.Should().NotBeNull();
        optionValue.Id.Should().Be(id);
        optionValue.OptionTypeId.Should().Be(optionTypeId);
        optionValue.Name.Should().Be(name);
        optionValue.Presentation.Should().Be(presentation);
        optionValue.Position.Should().Be(position);
    }

    [Fact(DisplayName = "Create: Should generate new ID when none is provided")]
    public void Create_WithoutId_ShouldGenerateNewId()
    {
        var result = OptionValueMethod.Create(Guid.NewGuid(), "Green", "Green");

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Theory(DisplayName = "Update: Should update properties correctly")]
    [InlineData("New Name", "New Presentation", 10)]
    [InlineData("Another Name", "Another Presentation", -1)]
    public void Update_WithValidParameters_ShouldUpdateProperties(string newName, string newPresentation, int newPosition)
    {
        var optionValue = OptionValueMethod.Create(Guid.NewGuid(), "Old Name", "Old Presentation").Value;

        var result = optionValue.Update(newName, newPresentation, newPosition);

        result.IsSuccess.Should().BeTrue();
        optionValue.Name.Should().Be(newName);
        optionValue.Presentation.Should().Be(newPresentation);
        optionValue.Position.Should().Be(newPosition);
    }
}
