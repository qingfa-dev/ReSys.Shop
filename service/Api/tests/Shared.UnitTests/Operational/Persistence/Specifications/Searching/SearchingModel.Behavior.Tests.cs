using Shared.Operational.Persistence.Specifications.Searching;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchingModelBehaviorTests
{
    [Fact(DisplayName = "ComputeViolations: null AllowedFields returns no violations")]
    public void NullAllowedFields_ShouldReturnNoViolations()
    {
        SearchModel model = new(
            new SearchTerm { Value = "hello" },
            ["Name", "Description"],
            SearchMode.Any,
            null);

        model.IsValid.Should().BeTrue();
        model.Violations.Should().BeEmpty();
    }

    [Fact(DisplayName = "ComputeViolations: empty Fields returns no violations even with AllowedFields")]
    public void EmptyFields_ShouldReturnNoViolations()
    {
        HashSet<String> allowedFields = new(["Name"], StringComparer.OrdinalIgnoreCase);
        SearchModel model = new(
            new SearchTerm { Value = "hello" },
            [],
            SearchMode.Any,
            allowedFields);

        model.IsValid.Should().BeTrue();
        model.Violations.Should().BeEmpty();
    }

    [Theory(DisplayName = "ComputeViolations: Matching field produces no violation")]
    [InlineData("Name", "Name")]
    [InlineData("NAME", "Name")]
    public void MatchingField_ShouldProduceNoViolation(String field, String allowedField)
    {
        HashSet<String> allowedFields = new([allowedField], StringComparer.OrdinalIgnoreCase);
        SearchModel model = new(
            new SearchTerm { Value = "hello" },
            [field],
            SearchMode.Any,
            allowedFields);

        model.IsValid.Should().BeTrue();
        model.Violations.Should().BeEmpty();
    }

    [Theory(DisplayName = "ComputeViolations: Non-matching field produces violation")]
    [InlineData("Forbidden", "Name")]
    [InlineData("forbidden", "Name")]
    public void NonMatchingField_ShouldProduceViolation(String field, String allowedField)
    {
        HashSet<String> allowedFields = new([allowedField], StringComparer.OrdinalIgnoreCase);
        SearchModel model = new(
            new SearchTerm { Value = "hello" },
            [field],
            SearchMode.Any,
            allowedFields);

        model.IsValid.Should().BeFalse();
        model.Violations.Should().Contain(field);
    }

    [Fact(DisplayName = "ComputeViolations: duplicate violations appear only once")]
    public void DuplicateViolations_ShouldAppearOnce()
    {
        HashSet<String> allowedFields = new(["Name"], StringComparer.OrdinalIgnoreCase);
        SearchModel model = new(
            new SearchTerm { Value = "hello" },
            ["Forbidden", "FORBIDDEN", "forbidden"],
            SearchMode.Any,
            allowedFields);

        model.Violations.Should().HaveCount(1);
        model.Violations[0].Should().Be("Forbidden");
    }

    [Fact(DisplayName = "ComputeViolations: multiple distinct violations")]
    public void MultipleDistinctViolations_ShouldAllAppear()
    {
        HashSet<String> allowedFields = new(["Name"], StringComparer.OrdinalIgnoreCase);
        SearchModel model = new(
            new SearchTerm { Value = "hello" },
            ["ForbiddenA", "ForbiddenB"],
            SearchMode.Any,
            allowedFields);

        model.Violations.Should().HaveCount(2);
        model.Violations.Should().Contain(["ForbiddenA", "ForbiddenB"]);
    }
}