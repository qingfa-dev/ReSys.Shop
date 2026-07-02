using Shared.Operational.Persistence.Specifications.Filtering;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterGroupTests
{
    [Fact(DisplayName = "FilterGroup: Empty group is empty")]
    public void EmptyGroup_ShouldBeEmpty()
    {
        FilterGroup group = FilterGroup.Empty;

        group.IsEmpty.Should().BeTrue();
        group.Logic.Should().Be(FilterLogic.And);
        group.Conditions.Should().BeEmpty();
        group.Groups.Should().BeEmpty();
    }

    [Fact(DisplayName = "FilterGroup: IsEmpty false when conditions exist")]
    public void IsEmpty_ShouldBeFalse_WhenConditionsExist()
    {
        List<FilterCondition> conditions = [new("Name", FilterOperator.Equal, "Apple")];
        FilterGroup group = FilterGroup.FlatAnd(conditions.AsReadOnly());

        group.IsEmpty.Should().BeFalse();
    }

    [Fact(DisplayName = "FilterGroup: IsEmpty false when sub-groups exist")]
    public void IsEmpty_ShouldBeFalse_WhenSubGroupsExist()
    {
        FilterGroup inner = FilterGroup.FlatAnd(new[] { new FilterCondition("Name", FilterOperator.Equal, "A") });
        FilterGroup group = new(FilterLogic.And, new List<FilterCondition>().AsReadOnly(), new FilterGroup[] { inner });

        group.IsEmpty.Should().BeFalse();
    }

    [Fact(DisplayName = "FilterGroup: TotalConditionCount counts all leaf conditions")]
    public void TotalConditionCount_ShouldCountRecursively()
    {
        FilterGroup inner = FilterGroup.FlatAnd(new[] { new FilterCondition("A", FilterOperator.Equal, "1") });
        FilterGroup group = new(
            FilterLogic.And,
            new[] { new FilterCondition("B", FilterOperator.Equal, "2"), new FilterCondition("C", FilterOperator.Equal, "3") },
            new[] { inner });

        group.TotalConditionCount.Should().Be(3);
    }

    [Fact(DisplayName = "FilterGroup: FlattenConditions enumerates depth-first")]
    public void FlattenConditions_ShouldEnumerateDepthFirst()
    {
        FilterCondition leaf1 = new("A", FilterOperator.Equal, "1");
        FilterCondition leaf2 = new("B", FilterOperator.Equal, "2");
        FilterCondition leaf3 = new("C", FilterOperator.Equal, "3");

        FilterGroup inner = FilterGroup.FlatAnd(new[] { leaf1 });
        FilterGroup root = new(FilterLogic.And, new[] { leaf2 }, new[] { inner });
        FilterGroup wrapper = new(FilterLogic.And, new[] { leaf3 }, new[] { root });

        List<FilterCondition> flat = wrapper.FlattenConditions().ToList();

        flat.Should().HaveCount(3);
        flat[0].Field.Should().Be("C");
        flat[1].Field.Should().Be("B");
        flat[2].Field.Should().Be("A");
    }

    [Fact(DisplayName = "FilterGroup: FlatAnd creates AND group with conditions")]
    public void FlatAnd_ShouldCreateAndGroup()
    {
        List<FilterCondition> conditions = [new("Name", FilterOperator.Equal, "Apple")];
        FilterGroup group = FilterGroup.FlatAnd(conditions.AsReadOnly());

        group.Logic.Should().Be(FilterLogic.And);
        group.Conditions.Should().HaveCount(1);
        group.Groups.Should().BeEmpty();
    }

    [Fact(DisplayName = "FilterGroup: FlatOr creates OR group with conditions")]
    public void FlatOr_ShouldCreateOrGroup()
    {
        List<FilterCondition> conditions = [new("Name", FilterOperator.Equal, "Apple")];
        FilterGroup group = FilterGroup.FlatOr(conditions.AsReadOnly());

        group.Logic.Should().Be(FilterLogic.Or);
        group.Conditions.Should().HaveCount(1);
    }

    [Fact(DisplayName = "FilterGroup: ToString displays logic and counts")]
    public void ToString_ShouldDisplayDiagnosticInfo()
    {
        List<FilterCondition> conditions = [new("A", FilterOperator.Equal, "1")];
        FilterGroup inner = FilterGroup.FlatAnd(new[] { new FilterCondition("B", FilterOperator.Equal, "2") });
        FilterGroup group = new(FilterLogic.Or, conditions.AsReadOnly(), new[] { inner });

        group.ToString().Should().Be("Or[conditions=1, groups=1]");
    }
}
