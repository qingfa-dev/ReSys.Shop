using System.Linq.Expressions;

using Shared.Operational.Persistence.Specifications.Filtering;
using Shared.Operational.Persistence.Specifications.Filtering.Expression;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering.Expression;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterGroupVisitorTests
{
    private static readonly ParameterExpression Param = System.Linq.Expressions.Expression.Parameter(typeof(TestEntity), "x");

    private sealed class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public string? Email { get; set; }
    }

    #region Basic

    [Fact(DisplayName = "Build: Empty group returns null")]
    public void Build_EmptyGroup_ShouldReturnNull()
    {
        FilterGroupVisitor<TestEntity>.Build(FilterGroup.Empty, Param).Should().BeNull();
    }

    [Fact(DisplayName = "Build: Single condition produces expression")]
    public void Build_SingleCondition_ShouldProduceExpression()
    {
        FilterGroup group = FilterGroup.FlatAnd(
            new FilterCondition[] { new() { Field = "Name", Operator = FilterOperator.Equal, Value = "Apple" } });
        FilterGroupVisitor<TestEntity>.Build(group, Param).Should().NotBeNull();
    }

    #endregion

    #region Logical Connective (parameterized)

    [Theory(DisplayName = "Build: AND group → AndAlso, OR group → OrElse")]
    [InlineData(FilterLogic.And, ExpressionType.AndAlso)]
    [InlineData(FilterLogic.Or, ExpressionType.OrElse)]
    public void Build_LogicalConnective_ShouldMatchExpressionType(FilterLogic logic, ExpressionType expectedType)
    {
        FilterGroup group = new()
        { Logic = logic,
            Conditions = new FilterCondition[] { new() { Field = "Name", Operator = FilterOperator.Equal, Value = "A" }, new() { Field = "Name", Operator = FilterOperator.Equal, Value = "B" } },
            Groups = Array.Empty<FilterGroup>() };

        System.Linq.Expressions.Expression? result = FilterGroupVisitor<TestEntity>.Build(group, Param);
        result!.NodeType.Should().Be(expectedType);
    }

    #endregion

    #region Nested Groups

    [Fact(DisplayName = "Build: Nested group combines correctly")]
    public void Build_NestedGroup_ShouldWork()
    {
        FilterGroup inner = FilterGroup.FlatOr(
            new FilterCondition[] { new() { Field = "Name", Operator = FilterOperator.Equal, Value = "A" }, new() { Field = "Name", Operator = FilterOperator.Equal, Value = "B" } });
        FilterGroup root = new()
        { Logic = FilterLogic.And,
            Conditions = new FilterCondition[] { new() { Field = "IsActive", Operator = FilterOperator.Equal, Value = "true" } },
            Groups = new FilterGroup[] { inner } };

        System.Linq.Expressions.Expression? result = FilterGroupVisitor<TestEntity>.Build(root, Param);
        result.Should().NotBeNull();
        result!.NodeType.Should().Be(ExpressionType.AndAlso);
    }

    #endregion

    #region Fail-Safe

    [Fact(DisplayName = "Build: Non-existent field skipped safely")]
    public void Build_InvalidField_ShouldBeSkipped()
    {
        FilterGroup group = FilterGroup.FlatAnd(new FilterCondition[]
        {
            new() { Field = "NonExistent", Operator = FilterOperator.Equal, Value = "value" },
            new() { Field = "Name", Operator = FilterOperator.Equal, Value = "Apple" },
        });

        System.Linq.Expressions.Expression? result = FilterGroupVisitor<TestEntity>.Build(group, Param);
        result.Should().NotBeNull();
    }

    [Fact(DisplayName = "Build: All conditions invalid returns null")]
    public void Build_AllInvalid_ShouldReturnNull()
    {
        FilterGroup group = FilterGroup.FlatAnd(
            new FilterCondition[] { new() { Field = "NonExistent", Operator = FilterOperator.Equal, Value = "value" } });
        FilterGroupVisitor<TestEntity>.Build(group, Param).Should().BeNull();
    }

    #endregion

    #region Integration: Compiled Expression against IQueryable (parameterized)

    [Theory(DisplayName = "Build Integration: Filters IQueryable correctly")]
    [InlineData(FilterLogic.And, "Age", FilterOperator.GreaterThan, "20", "IsActive", FilterOperator.Equal, "true", 2)]
    [InlineData(FilterLogic.Or, "Name", FilterOperator.Equal, "Apple", "Name", FilterOperator.Equal, "Banana", 2)]
    [InlineData(FilterLogic.And, "Age", FilterOperator.GreaterThan, "20", "Age", FilterOperator.LessThan, "30", 1)]
    public void Build_Integration_FiltersQueryable(
        FilterLogic logic, string f1, FilterOperator op1, string v1, string f2, FilterOperator op2, string v2, int expectedCount)
    {
        List<TestEntity> data =
        [
            new() { Name = "Apple", Age = 25, IsActive = true },
            new() { Name = "Banana", Age = 30, IsActive = false },
            new() { Name = "Orange", Age = 35, IsActive = true },
        ];

        FilterGroup group = new()
        { Logic = logic,
            Conditions = new FilterCondition[] { new() { Field = f1, Operator = op1, Value = v1 }, new() { Field = f2, Operator = op2, Value = v2 } },
            Groups = Array.Empty<FilterGroup>() };

        System.Linq.Expressions.Expression? body = FilterGroupVisitor<TestEntity>.Build(group, Param);
        body.Should().NotBeNull();
        Expression<Func<TestEntity, bool>> lambda = System.Linq.Expressions.Expression.Lambda<Func<TestEntity, bool>>(body!, Param);
        data.AsQueryable().Where(lambda).Should().HaveCount(expectedCount);
    }

    [Fact(DisplayName = "Build Integration: Null check filters correctly")]
    public void Build_Integration_NullCheck()
    {
        List<TestEntity> data =
        [
            new() { Name = "A", Email = "a@test.com" },
            new() { Name = "B", Email = null },
            new() { Name = "C", Email = "c@test.com" },
        ];

        FilterGroup group = FilterGroup.FlatAnd(
            new FilterCondition[] { new() { Field = "Email", Operator = FilterOperator.Equal, Value = "null" } });
        System.Linq.Expressions.Expression? body = FilterGroupVisitor<TestEntity>.Build(group, Param);
        Expression<Func<TestEntity, bool>> lambda = System.Linq.Expressions.Expression.Lambda<Func<TestEntity, bool>>(body!, Param);
        data.AsQueryable().Where(lambda).Should().HaveCount(1).And.Contain(x => x.Name == "B");
    }

    [Fact(DisplayName = "Build Integration: Not-null check")]
    public void Build_Integration_NotNullCheck()
    {
        List<TestEntity> data =
        [
            new() { Name = "A", Email = "a@test.com" },
            new() { Name = "B", Email = null },
            new() { Name = "C", Email = "c@test.com" },
        ];

        FilterGroup group = FilterGroup.FlatAnd(
            new FilterCondition[] { new() { Field = "Email", Operator = FilterOperator.NotEqual, Value = "null" } });
        System.Linq.Expressions.Expression? body = FilterGroupVisitor<TestEntity>.Build(group, Param);
        Expression<Func<TestEntity, bool>> lambda = System.Linq.Expressions.Expression.Lambda<Func<TestEntity, bool>>(body!, Param);
        data.AsQueryable().Where(lambda).Should().HaveCount(2);
    }

    #endregion

    #region Deep Nesting

    [Fact(DisplayName = "Build: Deeply nested tree (3 levels) produces correct expression")]
    public void Build_DeepNesting_ShouldWork()
    {
        List<TestEntity> data =
        [
            new() { Name = "A", Age = 10 },
            new() { Name = "B", Age = 20 },
            new() { Name = "C", Age = 30 },
        ];

        FilterGroup leaf1 = FilterGroup.FlatAnd(new FilterCondition[] { new() { Field = "Age", Operator = FilterOperator.GreaterThan, Value = "5" } });
        FilterGroup leaf2 = FilterGroup.FlatAnd(new FilterCondition[] { new() { Field = "Age", Operator = FilterOperator.LessThan, Value = "25" } });
        FilterGroup mid = new() { Logic = FilterLogic.And, Conditions = Array.Empty<FilterCondition>(), Groups = new FilterGroup[] { leaf1, leaf2 } };
        FilterGroup root = new() { Logic = FilterLogic.And, Conditions = new FilterCondition[] { new() { Field = "Name", Operator = FilterOperator.Equal, Value = "B" } }, Groups = new FilterGroup[] { mid } };

        System.Linq.Expressions.Expression? body = FilterGroupVisitor<TestEntity>.Build(root, Param);
        Expression<Func<TestEntity, bool>> lambda = System.Linq.Expressions.Expression.Lambda<Func<TestEntity, bool>>(body!, Param);
        data.AsQueryable().Where(lambda).Should().HaveCount(1).And.Contain(x => x.Name == "B");
    }

    #endregion
}
