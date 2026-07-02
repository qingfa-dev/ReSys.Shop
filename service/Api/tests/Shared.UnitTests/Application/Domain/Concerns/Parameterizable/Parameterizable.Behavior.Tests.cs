using Shared.Application.Domain.Concerns.Parameterizable;

namespace Shared.UnitTests.Application.Domain.Concerns.Parameterizable;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class ParameterizableBehaviorTests
{
    private sealed class TestParameterizable : IParameterizable
    {
        public string Name { get; set; } = string.Empty;
        public string? Presentation { get; set; }
    }

    [Fact(DisplayName = "Normalize should trim, lowercase, and convert hyphens to underscores")]
    public void Normalize_ShouldTrimLowercaseAndSnakeCase()
    {
        var result = ParameterizableBehavior.Normalize("  Hello-World  ");
        result.Should().Be("hello_world");
    }

    [Fact(DisplayName = "Normalize should handle already normalized input")]
    public void Normalize_AlreadyNormalized_ShouldReturnSame()
    {
        var result = ParameterizableBehavior.Normalize("hello_world");
        result.Should().Be("hello_world");
    }

    [Fact(DisplayName = "Normalize should replace hyphens with underscores")]
    public void Normalize_Hyphens_ShouldReplaceWithUnderscores()
    {
        var result = ParameterizableBehavior.Normalize("hello-world");
        result.Should().Be("hello_world");
    }

    [Fact(DisplayName = "ToNormalize should return null for null input")]
    public void ToNormalize_Null_ShouldReturnNull()
    {
        var result = ParameterizableBehavior.ToNormalize(null);
        result.Should().BeNull();
    }

    [Fact(DisplayName = "ToNormalize should return null for whitespace input")]
    public void ToNormalize_Whitespace_ShouldReturnNull()
    {
        var result = ParameterizableBehavior.ToNormalize("   ");
        result.Should().BeNull();
    }

    [Fact(DisplayName = "ToNormalize should return null for empty string")]
    public void ToNormalize_Empty_ShouldReturnNull()
    {
        var result = ParameterizableBehavior.ToNormalize(string.Empty);
        result.Should().BeNull();
    }

    [Fact(DisplayName = "ToNormalize should return normalized value for valid input")]
    public void ToNormalize_ValidInput_ShouldReturnNormalized()
    {
        var result = ParameterizableBehavior.ToNormalize("Hello-World");
        result.Should().Be("hello_world");
    }

    [Fact(DisplayName = "ApplyNormalization should set Name from Presentation when Name is empty")]
    public void ApplyNormalization_EmptyNameWithPresentation_ShouldUsePresentation()
    {
        var entity = new TestParameterizable
        {
            Name = string.Empty,
            Presentation = "Hello-World"
        };

        ParameterizableBehavior.ApplyNormalization(entity);

        entity.Name.Should().Be("hello_world");
    }

    [Fact(DisplayName = "ApplyNormalization should normalize Name when non-empty")]
    public void ApplyNormalization_NonEmptyName_ShouldNormalizeName()
    {
        var entity = new TestParameterizable
        {
            Name = "  Hello-World  ",
            Presentation = null
        };

        ParameterizableBehavior.ApplyNormalization(entity);

        entity.Name.Should().Be("hello_world");
    }

    [Fact(DisplayName = "GetNormalizedValues should normalize both name and presentation")]
    public void GetNormalizedValues_ShouldNormalizeBoth()
    {
        (string Name, string? Presentation) result = ParameterizableBehavior.GetNormalizedValues("Hello-World", "Foo-Bar");

        result.Name.Should().Be("hello_world");
        result.Presentation.Should().Be("foo_bar");
    }

    [Fact(DisplayName = "GetNormalizedValues should return null presentation when whitespace")]
    public void GetNormalizedValues_WhitespacePresentation_ShouldReturnNull()
    {
        (string Name, string? Presentation) result = ParameterizableBehavior.GetNormalizedValues("Hello-World", "   ");

        result.Name.Should().Be("hello_world");
        result.Presentation.Should().BeNull();
    }

    [Fact(DisplayName = "GetNormalizedValues should return null presentation when null")]
    public void GetNormalizedValues_NullPresentation_ShouldReturnNull()
    {
        (string Name, string? Presentation) result = ParameterizableBehavior.GetNormalizedValues("Hello-World", null);

        result.Name.Should().Be("hello_world");
        result.Presentation.Should().BeNull();
    }
}
