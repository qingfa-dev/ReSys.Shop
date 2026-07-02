using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterModelResultTests
{
    [Fact(DisplayName = "Success.Parsed returns expected message")]
    public void Success_Parsed_ShouldReturnExpectedMessage()
    {
        FilterModelResult.Success.Parsed.Should().Be("Filter parsed successfully.");
    }

    [Fact(DisplayName = "Success.Empty returns expected message")]
    public void Success_Empty_ShouldReturnExpectedMessage()
    {
        FilterModelResult.Success.Empty.Should().Be("No filter input provided; empty model returned.");
    }

    [Fact(DisplayName = "Failure.InvalidSyntax includes raw input in message")]
    public void InvalidSyntax_ShouldIncludeRawInput()
    {
        Error error = FilterModelResult.Failure.InvalidSyntax("bad(filter");

        error.Code.Should().Be("Filter.String.InvalidSyntax");
        error.Message.Should().Contain("bad(filter");
    }

    [Fact(DisplayName = "Failure.InvalidJson includes detail in message")]
    public void InvalidJson_ShouldIncludeDetail()
    {
        Error error = FilterModelResult.Failure.InvalidJson("missing bracket");

        error.Code.Should().Be("Filter.Json.InvalidStructure");
        error.Message.Should().Contain("missing bracket");
    }

    [Fact(DisplayName = "Failure.UnknownOperator includes token in message")]
    public void UnknownOperator_ShouldIncludeToken()
    {
        Error error = FilterModelResult.Failure.UnknownOperator("??");

        error.Code.Should().Be("Filter.Operator.Unknown");
        error.Message.Should().Contain("??");
    }

    [Fact(DisplayName = "Failure.MissingField returns expected error code")]
    public void MissingField_ShouldReturnExpectedCode()
    {
        Error error = FilterModelResult.Failure.MissingField;

        error.Code.Should().Be("Filter.Field.Missing");
    }

    [Fact(DisplayName = "Failure.MissingOperator returns expected error code")]
    public void MissingOperator_ShouldReturnExpectedCode()
    {
        Error error = FilterModelResult.Failure.MissingOperator;

        error.Code.Should().Be("Filter.Operator.Missing");
    }

    [Fact(DisplayName = "Failure.DisallowedField includes field name in message")]
    public void DisallowedField_ShouldIncludeFieldName()
    {
        Error error = FilterModelResult.Failure.DisallowedField("SecretField");

        error.Code.Should().Be("Filter.Field.Disallowed");
        error.Message.Should().Contain("SecretField");
    }

    [Fact(DisplayName = "Failure.DisallowedFields aggregates multiple field names")]
    public void DisallowedFields_ShouldAggregateFieldNames()
    {
        Error error = FilterModelResult.Failure.DisallowedFields(["A", "B"]);

        error.Code.Should().Be("Filter.Field.Disallowed");
        error.Message.Should().Contain("A").And.Contain("B");
    }

    [Fact(DisplayName = "Failure.InvalidTriplet includes entry in message")]
    public void InvalidTriplet_ShouldIncludeEntry()
    {
        Error error = FilterModelResult.Failure.InvalidTriplet("bad:entry");

        error.Code.Should().Be("Filter.QueryString.InvalidTriplet");
        error.Message.Should().Contain("bad:entry");
    }
}
