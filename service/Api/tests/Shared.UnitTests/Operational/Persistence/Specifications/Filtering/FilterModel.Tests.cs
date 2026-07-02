using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterModelTests
{
    [Fact(DisplayName = "FilterModel: Empty sentinel has no conditions")]
    public void Empty_ShouldHaveNoConditions()
    {
        FilterModel.Empty.IsEmpty.Should().BeTrue();
        FilterModel.Empty.Conditions.Should().BeEmpty();
        FilterModel.Empty.IsValid.Should().BeTrue();
        FilterModel.Empty.Violations.Should().BeEmpty();
    }

    [Fact(DisplayName = "FilterModel: Constructor populates Root and Conditions")]
    public void Constructor_ShouldPopulateRootAndConditions()
    {
        IReadOnlyList<FilterCondition> conditions = new[] { new FilterCondition("Name", FilterOperator.Equal, "Apple") };
        FilterGroup root = FilterGroup.FlatAnd(conditions);
        FilterModel model = new(root);

        model.Root.Should().Be(root);
        model.Conditions.Should().HaveCount(1);
        model.Conditions[0].Field.Should().Be("Name");
    }

    [Fact(DisplayName = "FilterModel: Conditions flatten nested groups")]
    public void Conditions_ShouldFlattenNestedGroups()
    {
        FilterGroup inner = FilterGroup.FlatAnd(
            new[] { new FilterCondition("B", FilterOperator.Equal, "2") });
        FilterGroup root = new(FilterLogic.And,
            new[] { new FilterCondition("A", FilterOperator.Equal, "1") },
            new[] { inner });
        FilterModel model = new(root);

        model.Conditions.Should().HaveCount(2);
        model.Conditions.Select(c => c.Field).Should().Contain(["A", "B"]);
    }

    [Fact(DisplayName = "FilterModel: RawInput stored for diagnostics")]
    public void RawInput_ShouldBeStored()
    {
        FilterGroup root = FilterGroup.Empty;
        FilterModel model = new(root, rawInput: "Name=Apple");

        model.RawInput.Should().Be("Name=Apple");
    }

    [Fact(DisplayName = "FilterModel: RawInput null when not provided")]
    public void RawInput_ShouldBeNull_WhenNotProvided()
    {
        FilterGroup root = FilterGroup.Empty;
        FilterModel model = new(root);

        model.RawInput.Should().BeNull();
    }

    [Fact(DisplayName = "FilterModel: IsValid true when no allowedFields")]
    public void IsValid_ShouldBeTrue_WhenNoAllowedFields()
    {
        FilterGroup root = FilterGroup.FlatAnd(
            new[] { new FilterCondition("AnyField", FilterOperator.Equal, "value") });
        FilterModel model = new(root);

        model.IsValid.Should().BeTrue();
    }

    [Fact(DisplayName = "FilterModel: IsValid false when field not in whitelist")]
    public void IsValid_ShouldBeFalse_WhenFieldNotInWhitelist()
    {
        FilterGroup root = FilterGroup.FlatAnd(
            new[] { new FilterCondition("Forbidden", FilterOperator.Equal, "value") });
        HashSet<string> allowedFields = new(["Name", "Age"], StringComparer.OrdinalIgnoreCase);
        FilterModel model = new(root, allowedFields);

        model.IsValid.Should().BeFalse();
        model.Violations.Should().Contain("Forbidden");
    }

    [Fact(DisplayName = "FilterModel: Violations case-insensitive")]
    public void Violations_ShouldBeCaseInsensitive()
    {
        FilterGroup root = FilterGroup.FlatAnd(
            new[] { new FilterCondition("NAME", FilterOperator.Equal, "value") });
        HashSet<string> allowedFields = new(["Name"], StringComparer.OrdinalIgnoreCase);
        FilterModel model = new(root, allowedFields);

        model.IsValid.Should().BeTrue();
        model.Violations.Should().BeEmpty();
    }

    [Fact(DisplayName = "FilterModel: ConditionsFor filters by field name")]
    public void ConditionsFor_ShouldFilterByFieldName()
    {
        FilterGroup root = FilterGroup.FlatAnd(new FilterCondition[]
        {
            new("Name", FilterOperator.Equal, "Apple"),
            new("Age", FilterOperator.Equal, "25"),
            new("Name", FilterOperator.NotEqual, "Banana"),
        });
        FilterModel model = new(root);

        model.ConditionsFor("Name").Should().HaveCount(2);
        model.ConditionsFor("Age").Should().HaveCount(1);
        model.ConditionsFor("Missing").Should().BeEmpty();
    }

    [Fact(DisplayName = "FilterModel: HasField returns true when field exists")]
    public void HasField_ShouldReturnTrue_WhenFieldExists()
    {
        FilterGroup root = FilterGroup.FlatAnd(
            new[] { new FilterCondition("Name", FilterOperator.Equal, "X") });
        FilterModel model = new(root);

        model.HasField("Name").Should().BeTrue();
        model.HasField("name").Should().BeTrue();
        model.HasField("Age").Should().BeFalse();
    }

    [Fact(DisplayName = "FilterModel: ToDslString joins conditions with comma-space")]
    public void ToDslString_ShouldJoinConditions()
    {
        FilterGroup root = FilterGroup.FlatAnd(new FilterCondition[]
        {
            new("Name", FilterOperator.Equal, "Apple"),
            new("Age", FilterOperator.GreaterThan, "18"),
        });
        FilterModel model = new(root);

        model.ToDslString().Should().Be("Name = Apple, Age > 18");
    }
}
