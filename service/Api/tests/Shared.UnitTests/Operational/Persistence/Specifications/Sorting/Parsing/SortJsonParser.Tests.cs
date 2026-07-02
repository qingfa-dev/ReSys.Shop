using System.Text.Json;

using Shared.Operational.Persistence.Specifications.Sorting;
using Shared.Operational.Persistence.Specifications.Sorting.Parsing;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting.Parsing;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortJsonParserTests
{
    [Fact]
    public void Parse_ValidFieldOnly_ShouldReturnAscending()
    {
        using JsonDocument doc = JsonDocument.Parse("""{"field": "Name"}""");

        Result<SortClause> result = SortJsonParser.Parse(doc.RootElement);

        result.IsSuccess.Should().BeTrue();
        result.Value.Field.Should().Be("Name");
        result.Value.Direction.Should().Be(SortDirection.Ascending);
    }

    [Fact]
    public void Parse_WithDirectionAndNulls_ShouldResolveAll()
    {
        using JsonDocument doc = JsonDocument.Parse("""{"field": "Name", "direction": "desc", "nulls": "last"}""");

        Result<SortClause> result = SortJsonParser.Parse(doc.RootElement);

        result.IsSuccess.Should().BeTrue();
        result.Value.Direction.Should().Be(SortDirection.Descending);
        result.Value.Nulls.Should().Be(SortNulls.Last);
    }

    [Fact]
    public void Parse_MissingField_ShouldReturnFailure()
    {
        using JsonDocument doc = JsonDocument.Parse("""{"direction": "asc"}""");

        Result<SortClause> result = SortJsonParser.Parse(doc.RootElement);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Parse_EmptyField_ShouldReturnFailure()
    {
        using JsonDocument doc = JsonDocument.Parse("""{"field": "  "}""");

        Result<SortClause> result = SortJsonParser.Parse(doc.RootElement);

        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Parse_InvalidDirection_ShouldReturnFailure()
    {
        using JsonDocument doc = JsonDocument.Parse("""{"field": "Name", "direction": "sideways"}""");

        Result<SortClause> result = SortJsonParser.Parse(doc.RootElement);

        result.IsFailure.Should().BeTrue();
    }
}
