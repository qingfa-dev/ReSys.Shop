using System.Linq.Expressions;

using Shared.Operational.Persistence.Specifications.Sorting;
using Shared.Operational.Persistence.Specifications.Sorting.Expression;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Sorting.Expression;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SortExpressionBuilderTests
{
    private sealed class TestEntity
    {
        public string Name { get; set; } = string.Empty;

        public int Age { get; set; }
    }

    [Theory]
    [InlineData("Name", "Alice")]
    [InlineData("Age", 25)]
    public void BuildKeySelector_ShouldCompileAndReturnValue(string prop, object expected)
    {
        Expression<Func<TestEntity, object>> expression = SortExpressionBuilder.BuildKeySelector<TestEntity>(prop)!;
        Func<TestEntity, object> compiled = expression.Compile();

        compiled(new TestEntity { Name = "Alice", Age = 25 }).Should().Be(expected);
    }

    [Fact]
    public void BuildKeySelector_FromSortClause_ShouldBuildExpression()
    {
        SortClause clause = new("Name", SortDirection.Descending);

        Expression<Func<TestEntity, object>>? expression = SortExpressionBuilder.BuildKeySelector<TestEntity>(clause);

        expression.Should().NotBeNull();
    }

    [Fact]
    public void BuildKeySelector_UnknownProperty_ShouldReturnNull()
    {
        Expression<Func<TestEntity, object>>? expression = SortExpressionBuilder.BuildKeySelector<TestEntity>("NonExistent");

        expression.Should().BeNull();
    }
}
