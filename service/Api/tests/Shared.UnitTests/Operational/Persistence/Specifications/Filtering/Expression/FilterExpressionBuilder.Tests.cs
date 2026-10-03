using System.Linq.Expressions;

using Shared.Operational.Persistence.Specifications.Filtering.Expression;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Filtering.Expression;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class FilterExpressionBuilderTests
{
    private static readonly ParameterExpression Param = System.Linq.Expressions.Expression.Parameter(typeof(TestEntity), "x");

    public sealed class TestEntity
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public bool IsActive { get; set; }
        public string? Email { get; set; }
        public TestCategory? Category { get; set; }
        public TestStatus Status { get; set; }
        public decimal Balance { get; set; }
    }

    public sealed class TestCategory
    {
        public string Name { get; set; } = string.Empty;
        public TestProject? Project { get; set; }
    }

    public sealed class TestProject
    {
        public string Title { get; set; } = string.Empty;
    }

    public enum TestStatus
    {
        Active,
        Inactive,
        Pending
    }

    #region Build<T> — Basic Operators (parameterized)

    [Theory(DisplayName = "Build: Produces expression for all base operators")]
    [InlineData("Name", "=", "Apple", false)]
    [InlineData("Name", "!=", "Banana", false)]
    [InlineData("Name", "*", "ap", false)]
    [InlineData("Name", "^", "Ap", false)]
    [InlineData("Name", "$", "le", false)]
    [InlineData("Name", "!*", "x", false)]
    [InlineData("Name", "!^", "X", false)]
    [InlineData("Name", "!$", "y", false)]
    [InlineData("Age", "=", "25", false)]
    [InlineData("Age", "!=", "30", false)]
    [InlineData("Age", ">", "20", false)]
    [InlineData("Age", "<", "40", false)]
    [InlineData("Age", ">=", "25", false)]
    [InlineData("Age", "<=", "35", false)]
    public void Build_AllBaseOperators_ShouldProduceExpression(string field, string op, string value, bool cs)
    {
        System.Linq.Expressions.Expression? expr = FilterExpressionBuilder.Build<TestEntity>(Param, field, op, value, cs);
        expr.Should().NotBeNull();
    }

    #endregion

    #region Build<T> — Null Operators

    [Theory(DisplayName = "Build: Null operators produce expression")]
    [InlineData("Email", "=", "null")]
    [InlineData("Email", "!=", "null")]
    [InlineData("Category", "=", "null")]
    [InlineData("Category", "!=", "null")]
    public void Build_NullOperators_ShouldProduceExpression(string field, string op, string value)
    {
        System.Linq.Expressions.Expression? expr = FilterExpressionBuilder.Build<TestEntity>(Param, field, op, value, false);
        expr.Should().NotBeNull();
    }

    [Fact(DisplayName = "Build: Null comparison with unsupported operator returns null")]
    public void Build_NullWithUnsupportedOperator_ShouldReturnNull()
    {
        System.Linq.Expressions.Expression? expr = FilterExpressionBuilder.Build<TestEntity>(Param, "Email", ">", "null", false);
        expr.Should().BeNull();
    }

    #endregion

    #region Build<T> — Navigation

    [Theory(DisplayName = "Build: Navigation properties produce expression")]
    [InlineData("Category.Name", "=", "Electronics")]
    [InlineData("Category.Name", "!=", "Other")]
    [InlineData("Category.Project.Title", "=", "X")]
    [InlineData("Category.Project.Title", "*", "Internal")]
    public void Build_NavigationProperties_ShouldProduceExpression(string field, string op, string value)
    {
        System.Linq.Expressions.Expression? expr = FilterExpressionBuilder.Build<TestEntity>(Param, field, op, value, false);
        expr.Should().NotBeNull();
    }

    #endregion

    #region Build<T> — Fail-Safe

    [Theory(DisplayName = "Build: Non-existent field returns null")]
    [InlineData("NonExistent", "=", "value", false)]
    [InlineData("Missing", ">", "10", false)]
    [InlineData("Invalid.Nested", "=", "x", false)]
    public void Build_NonExistentField_ShouldReturnNull(string field, string op, string value, bool cs)
    {
        System.Linq.Expressions.Expression? expr = FilterExpressionBuilder.Build<TestEntity>(Param, field, op, value, cs);
        expr.Should().BeNull();
    }

    [Theory(DisplayName = "Build: Invalid value type returns null")]
    [InlineData("Age", "=", "not-a-number")]
    [InlineData("Balance", "=", "invalid")]
    [InlineData("IsActive", "=", "not-a-bool")]
    public void Build_InvalidValueType_ShouldReturnNull(string field, string op, string value)
    {
        System.Linq.Expressions.Expression? expr = FilterExpressionBuilder.Build<TestEntity>(Param, field, op, value, false);
        expr.Should().BeNull();
    }

    #endregion

    #region Build<T> — Case-Sensitivity

    [Fact(DisplayName = "Build: Case-sensitive flag forwards without throwing")]
    public void Build_CaseSensitive_ShouldNotThrow()
    {
        System.Linq.Expressions.Expression? expr = FilterExpressionBuilder.Build<TestEntity>(Param, "Name", "=", "Apple", true);
        expr.Should().NotBeNull();
    }

    #endregion

    #region Build<T> — Integration (compiled expression against IQueryable)

    [Fact(DisplayName = "Build: Compiled expression actually filters IQueryable")]
    public void Build_Integration_FiltersQueryable()
    {
        List<TestEntity> data =
        [
            new() { Name = "Apple", Age = 25 },
            new() { Name = "Banana", Age = 30 },
        ];

        System.Linq.Expressions.Expression? body = FilterExpressionBuilder.Build<TestEntity>(Param, "Name", "=", "Apple", false);
        Expression<Func<TestEntity, bool>> lambda = System.Linq.Expressions.Expression.Lambda<Func<TestEntity, bool>>(body!, Param);
        List<TestEntity> result = data.AsQueryable().Where(lambda).ToList();

        result.Should().HaveCount(1);
        result[0].Age.Should().Be(25);
    }

    [Fact(DisplayName = "Build: Compiled expression for numeric comparison")]
    public void Build_Integration_NumericComparison()
    {
        List<TestEntity> data =
        [
            new() { Name = "A", Age = 25 },
            new() { Name = "B", Age = 30 },
            new() { Name = "C", Age = 35 },
        ];

        System.Linq.Expressions.Expression? body = FilterExpressionBuilder.Build<TestEntity>(Param, "Age", ">", "25", false);
        Expression<Func<TestEntity, bool>> lambda = System.Linq.Expressions.Expression.Lambda<Func<TestEntity, bool>>(body!, Param);
        List<TestEntity> result = data.AsQueryable().Where(lambda).ToList();

        result.Should().HaveCount(2);
        result.Select(x => x.Name).Should().BeEquivalentTo(["B", "C"]);
    }

    [Fact(DisplayName = "Build: Compiled expression for null navigation")]
    public void Build_Integration_NullNavigation()
    {
        List<TestEntity> data =
        [
            new() { Name = "A", Category = new TestCategory { Name = "Electronics" } },
            new() { Name = "B", Category = null },
        ];

        System.Linq.Expressions.Expression? body = FilterExpressionBuilder.Build<TestEntity>(Param, "Category.Name", "=", "Electronics", false);
        Expression<Func<TestEntity, bool>> lambda = System.Linq.Expressions.Expression.Lambda<Func<TestEntity, bool>>(body!, Param);
        List<TestEntity> result = data.AsQueryable().Where(lambda).ToList();

        result.Should().HaveCount(1);
        result[0].Name.Should().Be("A");
    }

    #endregion

    #region ParseConstant — Type Coverage (parameterized)

    [Theory(DisplayName = "ParseConstant: All boolean aliases parse correctly")]
    [InlineData("true", true)]
    [InlineData("True", true)]
    [InlineData("TRUE", true)]
    [InlineData("false", false)]
    [InlineData("False", false)]
    [InlineData("FALSE", false)]
    [InlineData("1", true)]
    [InlineData("0", false)]
    [InlineData("yes", true)]
    [InlineData("YES", true)]
    [InlineData("no", false)]
    [InlineData("NO", false)]
    [InlineData("y", true)]
    [InlineData("Y", true)]
    [InlineData("n", false)]
    [InlineData("N", false)]
    public void ParseConstant_BoolAliases_ShouldParse(string input, bool expected)
    {
        ConstantExpression? result = FilterExpressionBuilder.ParseConstant(input, typeof(bool));
        result!.Value.Should().Be(expected);
    }

    [Fact(DisplayName = "ParseConstant: Invalid bool returns null")]
    public void ParseConstant_InvalidBool_ShouldReturnNull()
    {
        FilterExpressionBuilder.ParseConstant("maybe", typeof(bool)).Should().BeNull();
        FilterExpressionBuilder.ParseConstant("2", typeof(bool)).Should().BeNull();
    }

    [Fact(DisplayName = "ParseConstant: Null sentinel returns null constant")]
    public void ParseConstant_NullSentinel_ShouldReturnNullConstant()
    {
        ConstantExpression? result = FilterExpressionBuilder.ParseConstant("null", typeof(string));
        result.Should().NotBeNull();
        result!.Value.Should().BeNull();

        ConstantExpression? resultCs = FilterExpressionBuilder.ParseConstant("NULL", typeof(string));
        resultCs!.Value.Should().BeNull();
    }

    [Theory(DisplayName = "ParseConstant: Enum values parsed case-insensitively")]
    [InlineData("Active", TestStatus.Active)]
    [InlineData("active", TestStatus.Active)]
    [InlineData("Inactive", TestStatus.Inactive)]
    [InlineData("Pending", TestStatus.Pending)]
    public void ParseConstant_EnumValues_ShouldParse(string input, TestStatus expected)
    {
        ConstantExpression? result = FilterExpressionBuilder.ParseConstant(input, typeof(TestStatus));
        result!.Value.Should().Be(expected);
    }

    [Fact(DisplayName = "ParseConstant: Invalid enum returns null")]
    public void ParseConstant_InvalidEnum_ShouldReturnNull()
    {
        FilterExpressionBuilder.ParseConstant("NotAStatus", typeof(TestStatus)).Should().BeNull();
    }

    [Fact(DisplayName = "ParseConstant: DateTimeOffset parses")]
    public void ParseConstant_DateTimeOffset_ShouldParse()
    {
        ConstantExpression? result = FilterExpressionBuilder.ParseConstant("2023-01-01T00:00:00.0000000+00:00", typeof(DateTimeOffset));
        result!.Value.Should().BeOfType<DateTimeOffset>();
    }

    [Fact(DisplayName = "ParseConstant: DateTimeOffset parses")]
    public void ParseConstant_DateTime_ShouldParse()
    {
        ConstantExpression? result = FilterExpressionBuilder.ParseConstant("2023-01-01T00:00:00.0000000", typeof(DateTime));
        result!.Value.Should().BeOfType<DateTime>();
    }

    [Theory(DisplayName = "ParseConstant: Guid formats parse")]
    [InlineData("00000000-0000-0000-0000-000000000001")]
    [InlineData("00000000000000000000000000000001")]
    public void ParseConstant_GuidFormats_ShouldParse(string input)
    {
        ConstantExpression? result = FilterExpressionBuilder.ParseConstant(input, typeof(Guid));
        result!.Value.Should().BeOfType<Guid>();
        ((Guid)result.Value).Should().NotBe(Guid.Empty);
    }

    [Fact(DisplayName = "ParseConstant: Invalid Guid returns null")]
    public void ParseConstant_InvalidGuid_ShouldReturnNull()
    {
        FilterExpressionBuilder.ParseConstant("not-a-guid", typeof(Guid)).Should().BeNull();
    }

    [Theory(DisplayName = "ParseConstant: Empty string — string type returns string.Empty, other returns null")]
    [InlineData("", typeof(string), true)]
    [InlineData("", typeof(int), false)]
    public void ParseConstant_EmptyString_ShouldReturnCorrectly(string input, Type type, bool expectNonNull)
    {
        ConstantExpression? result = FilterExpressionBuilder.ParseConstant(input, type);
        if (expectNonNull)
            result!.Value.Should().Be(string.Empty);
        else
            result.Should().BeNull();
    }

    [Theory(DisplayName = "ParseConstant: Numeric types parse")]
    [InlineData("100", typeof(int), 100)]
    [InlineData("-50", typeof(int), -50)]
    [InlineData("0", typeof(int), 0)]
    [InlineData("9223372036854775807", typeof(long), 9223372036854775807L)]
    public void ParseConstant_NumericTypes_ShouldParse(string input, Type type, object expected)
    {
        ConstantExpression? result = FilterExpressionBuilder.ParseConstant(input, type);
        result!.Value.Should().Be(expected);
    }

    #endregion

    #region Build<T> — Edge Cases

    [Fact(DisplayName = "Build: Null comparison on non-nullable value type throws")]
    public void Build_NullOnNonNullable_ShouldThrow()
    {
        // = null on non-nullable int: Expression.Equal(int, object) → type mismatch
        Action act = () => FilterExpressionBuilder.Build<TestEntity>(Param, "Age", "=", "null", false);
        act.Should().Throw<Exception>();
    }

    [Fact(DisplayName = "Build: Comparison operators (> < >= <=) on string return null")]
    public void Build_ComparisonOnString_ShouldReturnNull()
    {
        FilterExpressionBuilder.Build<TestEntity>(Param, "Name", ">", "A", false).Should().BeNull();
        FilterExpressionBuilder.Build<TestEntity>(Param, "Name", "<", "Z", false).Should().BeNull();
        FilterExpressionBuilder.Build<TestEntity>(Param, "Name", ">=", "A", false).Should().BeNull();
        FilterExpressionBuilder.Build<TestEntity>(Param, "Name", "<=", "Z", false).Should().BeNull();
    }

    [Fact(DisplayName = "Build: String operators (* ^ $) on non-string type return null")]
    public void Build_StringOpsOnNumeric_ShouldReturnNull()
    {
        FilterExpressionBuilder.Build<TestEntity>(Param, "Age", "*", "25", false).Should().BeNull();
        FilterExpressionBuilder.Build<TestEntity>(Param, "Age", "^", "2", false).Should().BeNull();
        FilterExpressionBuilder.Build<TestEntity>(Param, "Age", "$", "5", false).Should().BeNull();
    }

    #endregion
}
