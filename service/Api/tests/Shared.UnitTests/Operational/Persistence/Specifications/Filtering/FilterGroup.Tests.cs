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
        List<FilterCondition> conditions = [new FilterCondition { Field = "Name", Operator = FilterOperator.Equal, Value = "Apple" }];
        FilterGroup group = FilterGroup.FlatAnd(conditions.AsReadOnly());

        group.IsEmpty.Should().BeFalse();
    }

    [Fact(DisplayName = "FilterGroup: IsEmpty false when sub-groups exist")]
    public void IsEmpty_ShouldBeFalse_WhenSubGroupsExist()
    {
        FilterGroup inner = FilterGroup.FlatAnd(new[] { new FilterCondition { Field = "Name", Operator = FilterOperator.Equal, Value = "A" } });
        FilterGroup group = new() { Logic = FilterLogic.And, Conditions = new List<FilterCondition>().AsReadOnly(), Groups = new FilterGroup[] { inner } };

        group.IsEmpty.Should().BeFalse();
    }

    [Fact(DisplayName = "FilterGroup: TotalConditionCount counts all leaf conditions")]
    public void TotalConditionCount_ShouldCountRecursively()
    {
        FilterGroup inner = FilterGroup.FlatAnd(new[] { new FilterCondition { Field = "A", Operator = FilterOperator.Equal, Value = "1" } });
        FilterGroup group = new()
        {
            Logic = FilterLogic.And,
            Conditions = new[] { new FilterCondition { Field = "B", Operator = FilterOperator.Equal, Value = "2" }, new FilterCondition { Field = "C", Operator = FilterOperator.Equal, Value = "3" } },
            Groups = new[] { inner }
        };

        group.TotalConditionCount.Should().Be(3);
    }

    [Fact(DisplayName = "FilterGroup: FlattenConditions enumerates depth-first")]
    public void FlattenConditions_ShouldEnumerateDepthFirst()
    {
        FilterCondition leaf1 = new() { Field = "A", Operator = FilterOperator.Equal, Value = "1" };
        FilterCondition leaf2 = new() { Field = "B", Operator = FilterOperator.Equal, Value = "2" };
        FilterCondition leaf3 = new() { Field = "C", Operator = FilterOperator.Equal, Value = "3" };

        FilterGroup inner = FilterGroup.FlatAnd(new[] { leaf1 });
        FilterGroup root = new() { Logic = FilterLogic.And, Conditions = new[] { leaf2 }, Groups = new[] { inner } };
        FilterGroup wrapper = new() { Logic = FilterLogic.And, Conditions = new[] { leaf3 }, Groups = new[] { root } };

        List<FilterCondition> flat = wrapper.FlattenConditions().ToList();

        flat.Should().HaveCount(3);
        flat[0].Field.Should().Be("C");
        flat[1].Field.Should().Be("B");
        flat[2].Field.Should().Be("A");
    }

    [Fact(DisplayName = "FilterGroup: FlatAnd creates AND group with conditions")]
    public void FlatAnd_ShouldCreateAndGroup()
    {
        List<FilterCondition> conditions = [new FilterCondition { Field = "Name", Operator = FilterOperator.Equal, Value = "Apple" }];
        FilterGroup group = FilterGroup.FlatAnd(conditions.AsReadOnly());

        group.Logic.Should().Be(FilterLogic.And);
        group.Conditions.Should().HaveCount(1);
        group.Groups.Should().BeEmpty();
    }

    [Fact(DisplayName = "FilterGroup: FlatOr creates OR group with conditions")]
    public void FlatOr_ShouldCreateOrGroup()
    {
        List<FilterCondition> conditions = [new FilterCondition { Field = "Name", Operator = FilterOperator.Equal, Value = "Apple" }];
        FilterGroup group = FilterGroup.FlatOr(conditions.AsReadOnly());

        group.Logic.Should().Be(FilterLogic.Or);
        group.Conditions.Should().HaveCount(1);
    }

    [Fact(DisplayName = "FilterGroup: ToString displays logic and counts")]
    public void ToString_ShouldDisplayDiagnosticInfo()
    {
        List<FilterCondition> conditions = [new FilterCondition { Field = "A", Operator = FilterOperator.Equal, Value = "1" }];
        FilterGroup inner = FilterGroup.FlatAnd(new[] { new FilterCondition { Field = "B", Operator = FilterOperator.Equal, Value = "2" } });
        FilterGroup group = new() { Logic = FilterLogic.Or, Conditions = conditions.AsReadOnly(), Groups = new[] { inner } };

        group.ToString().Should().Be("Or[conditions=1, groups=1]");
    }
}
