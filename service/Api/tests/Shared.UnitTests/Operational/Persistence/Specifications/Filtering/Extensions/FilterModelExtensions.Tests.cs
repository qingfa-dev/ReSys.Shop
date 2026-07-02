using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Filtering.Extensions;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterModelExtensionsTests
{
    [Fact(DisplayName = "FromString: Valid DSL returns success with parsed model")]
    public void FromString_ValidDsl_ShouldReturnSuccess()
    {
        Result<FilterModel> result = FilterModelExtensions.FromString("Name=Apple");

        result.IsSuccess.Should().BeTrue();
        result.Value.Conditions.Should().HaveCount(1);
        result.Value.Conditions[0].Field.Should().Be("Name");
    }

    [Fact(DisplayName = "FromString: Null input returns Empty model")]
    public void FromString_Null_ShouldReturnEmpty()
    {
        Result<FilterModel> result = FilterModelExtensions.FromString(null);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(FilterModel.Empty);
    }

    [Fact(DisplayName = "FromString: Empty string returns Empty model")]
    public void FromString_Empty_ShouldReturnEmpty()
    {
        Result<FilterModel> result = FilterModelExtensions.FromString("");

        result.Value.Should().BeSameAs(FilterModel.Empty);
    }

    [Fact(DisplayName = "FromString: Whitespace returns Empty model")]
    public void FromString_Whitespace_ShouldReturnEmpty()
    {
        Result<FilterModel> result = FilterModelExtensions.FromString("   \t ");

        result.Value.Should().BeSameAs(FilterModel.Empty);
    }

    [Fact(DisplayName = "FromString: Malformed filter produces empty model (fail-safe)")]
    public void FromString_InvalidSyntax_ShouldReturnEmpty()
    {
        // The DSL parser is fail-safe — malformed input produces empty group, which yields Empty model
        Result<FilterModel> result = FilterModelExtensions.FromString("===invalid===");

        result.IsSuccess.Should().BeTrue();
        result.Value.IsEmpty.Should().BeTrue();
    }

    [Fact(DisplayName = "FromString: AllowedFields whitelist honors valid fields")]
    public void FromString_AllowedFields_ShouldAllowListedFields()
    {
        HashSet<string> allowedFields = new(["Name"], StringComparer.OrdinalIgnoreCase);
        Result<FilterModel> result = FilterModelExtensions.FromString("Name=Apple", allowedFields);

        result.Value.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "FromString: Violation when field not in whitelist")]
    public void FromString_Violation_WhenFieldNotAllowed()
    {
        HashSet<string> allowedFields = new(["Name"], StringComparer.OrdinalIgnoreCase);
        Result<FilterModel> result = FilterModelExtensions.FromString("Age=25", allowedFields);

        result.Value.IsValid.Should().BeFalse();
        result.Value.Violations.Should().Contain("Age");
    }

    [Fact(DisplayName = "FromString: String array overload delegates correctly")]
    public void FromString_StringArrayOverload_ShouldWork()
    {
        Result<FilterModel> result = FilterModelExtensions.FromString("Name=Apple", ["Name"]);

        result.Value.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "FromJson: Valid JSON array returns success")]
    public void FromJson_ValidJson_ShouldReturnSuccess()
    {
        string json = """[{"field":"Name","op":"eq","value":"Apple"}]""";

        Result<FilterModel> result = FilterModelExtensions.FromJson(json);

        result.IsSuccess.Should().BeTrue();
        result.Value.Conditions.Should().HaveCount(1);
    }

    [Fact(DisplayName = "FromJson: Null returns Empty model")]
    public void FromJson_Null_ShouldReturnEmpty()
    {
        Result<FilterModel> result = FilterModelExtensions.FromJson(null);

        result.Value.Should().BeSameAs(FilterModel.Empty);
    }

    [Fact(DisplayName = "FromJson: Empty string returns Empty model")]
    public void FromJson_Empty_ShouldReturnEmpty()
    {
        Result<FilterModel> result = FilterModelExtensions.FromJson("");

        result.Value.Should().BeSameAs(FilterModel.Empty);
    }

    [Fact(DisplayName = "FromJson: Invalid JSON returns failure")]
    public void FromJson_InvalidJson_ShouldReturnFailure()
    {
        Result<FilterModel> result = FilterModelExtensions.FromJson("{bad json}");

        result.IsFailure.Should().BeTrue();
    }

    [Fact(DisplayName = "FromJson: String array overload delegates correctly")]
    public void FromJson_StringArrayOverload_ShouldWork()
    {
        string json = """[{"field":"Name","op":"eq","value":"Apple"}]""";

        Result<FilterModel> result = FilterModelExtensions.FromJson(json, ["Name"]);

        result.Value.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "FromQueryString: Valid triplets return success")]
    public void FromQueryString_ValidTriplets_ShouldReturnSuccess()
    {
        Result<FilterModel> result = FilterModelExtensions.FromQueryString(["Name:eq:Apple", "Age:gt:18"]);

        result.IsSuccess.Should().BeTrue();
        result.Value.Conditions.Should().HaveCount(2);
    }

    [Fact(DisplayName = "FromQueryString: Null returns Empty model")]
    public void FromQueryString_Null_ShouldReturnEmpty()
    {
        Result<FilterModel> result = FilterModelExtensions.FromQueryString(null);

        result.Value.Should().BeSameAs(FilterModel.Empty);
    }

    [Fact(DisplayName = "FromQueryString: Empty sequence returns Empty model")]
    public void FromQueryString_Empty_ShouldReturnEmpty()
    {
        Result<FilterModel> result = FilterModelExtensions.FromQueryString([]);

        result.Value.Should().BeSameAs(FilterModel.Empty);
    }

    [Fact(DisplayName = "FromQueryString: Malformed triplet returns failure")]
    public void FromQueryString_MalformedTriplet_ShouldReturnFailure()
    {
        Result<FilterModel> result = FilterModelExtensions.FromQueryString(["invalid"]);

        result.IsFailure.Should().BeTrue();
    }
}
