using System.Linq.Expressions;

using Shared.Operational.Persistence.Specifications.Searching;
using Shared.Operational.Persistence.Specifications.Searching.Expression;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Searching.Expression;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class SearchExpressionBuilderTests
{
    private sealed class TestEntity
    {
        public Guid Id { get; set; }
        public String Name { get; set; } = String.Empty;
        public String Description { get; set; } = String.Empty;
        public String Category { get; set; } = String.Empty;
    }

    private static Boolean Evaluate(Expression<Func<TestEntity, Boolean>> predicate, TestEntity entity)
    {
        return predicate.Compile().Invoke(entity);
    }

    [Theory(DisplayName = "Build: Empty model or empty fields returns true")]
    [InlineData(true)]
    [InlineData(false)]
    public void EmptyInput_ReturnsTrue(Boolean isEmptyModel)
    {
        TestEntity entity = new() { Name = "Anything" };

        SearchModel model = isEmptyModel
            ? SearchModel.Empty
            : new(new SearchTerm { Value = "hello" }, []);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().BeTrue();
    }

    [Theory(DisplayName = "Build: Any mode, single field")]
    [InlineData("hello", "Hello World", true)]
    [InlineData("xyz", "Hello World", false)]
    public void AnyMode_SingleField_FiltersCorrectly(String term, String entityName, Boolean expected)
    {
        TestEntity entity = new() { Name = entityName, Description = "Nothing" };
        SearchModel model = new(new SearchTerm { Value = term }, ["Name"]);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().Be(expected);
    }

    [Theory(DisplayName = "Build: Any mode, two fields")]
    [InlineData("hello", true)]
    [InlineData("xyz", false)]
    public void AnyMode_TwoFields_MatchesAny(String term, Boolean expected)
    {
        TestEntity entity = new() { Name = "Hello", Description = "Nothing" };
        SearchModel model = new(new SearchTerm { Value = term }, ["Name", "Description"]);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().Be(expected);
    }

    [Theory(DisplayName = "Build: All mode, two fields")]
    [InlineData("hello", "hello world", "hello there", true)]
    [InlineData("hello", "hello world", "nothing", false)]
    public void AllMode_TwoFields_MatchesAll(String term, String entityName, String entityDescription, Boolean expected)
    {
        TestEntity entity = new() { Name = entityName, Description = entityDescription };
        SearchModel model = new(new SearchTerm { Value = term }, ["Name", "Description"], SearchMode.All);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().Be(expected);
    }

    [Theory(DisplayName = "Build: Case-sensitive matching")]
    [InlineData("Hello World", true)]
    [InlineData("hello world", false)]
    public void CaseSensitive_Matching(String entityName, Boolean expected)
    {
        TestEntity entity = new() { Name = entityName };
        SearchModel model = new(new SearchTerm { Value = "Hello", CaseSensitive = true }, ["Name"]);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().Be(expected);
    }

    [Fact(DisplayName = "Build: Case-insensitive — 'hello' matches 'Hello'")]
    public void Build_CaseInsensitive_MatchesDifferentCase()
    {
        TestEntity entity = new() { Name = "Hello World" };
        SearchModel model = new(new SearchTerm { Value = "hello", CaseSensitive = false }, ["Name"]);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().BeTrue();
    }

    [Fact(DisplayName = "Build: defaultFields fallback when model.Fields is empty")]
    public void Build_DefaultFieldsFallback_ShouldSearchDefaults()
    {
        TestEntity entity = new() { Name = "Nothing", Category = "hello world" };
        SearchModel model = new(new SearchTerm { Value = "hello" }, []);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, ["Category"]);

        Evaluate(lambda, entity).Should().BeTrue();
    }

    [Fact(DisplayName = "Build: Term with special characters works")]
    public void Build_SpecialCharacters_ShouldWork()
    {
        TestEntity entity = new() { Name = "hello (world) [test]" };
        SearchModel model = new(new SearchTerm { Value = "(" }, ["Name"]);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().BeTrue();
    }

    [Fact(DisplayName = "Build: Term matches Description field")]
    public void Build_MatchesDescriptionField_ShouldReturnTrue()
    {
        TestEntity entity = new() { Name = "Nothing", Description = "search target" };
        SearchModel model = new(new SearchTerm { Value = "target" }, ["Description"]);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().BeTrue();
    }

    [Fact(DisplayName = "Build: Any mode matches across fields")]
    public void Build_AnyMode_CrossFieldMatch_ShouldReturnTrue()
    {
        TestEntity entity = new() { Name = "Nothing", Description = "hello", Category = "world" };
        SearchModel model = new(new SearchTerm { Value = "hello" }, ["Name", "Description", "Category"]);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().BeTrue();
    }

    [Fact(DisplayName = "Build: Non-string fields are silently skipped")]
    public void Build_NonStringFields_ShouldSkipAndNotThrow()
    {
        TestEntity entity = new() { Id = Guid.NewGuid(), Name = "hello world" };
        SearchModel model = new(new SearchTerm { Value = "hello" }, ["Id", "Name"]);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().BeTrue();
    }

    [Fact(DisplayName = "Build: When all fields are non-string, returns true (no-op)")]
    public void Build_AllNonStringFields_ShouldReturnTrue()
    {
        TestEntity entity = new() { Id = Guid.NewGuid() };
        SearchModel model = new(new SearchTerm { Value = "hello" }, ["Id"]);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().BeTrue();
    }

    [Fact(DisplayName = "Build: Case-insensitive field name resolution works")]
    public void Build_CaseInsensitiveFieldName_ShouldResolveProperty()
    {
        TestEntity entity = new() { Name = "Hello World", Description = "Nothing" };
        SearchModel model = new(new SearchTerm { Value = "hello" }, ["name"]);

        Expression<Func<TestEntity, Boolean>> lambda = SearchExpressionBuilder.Build<TestEntity>(model, null);

        Evaluate(lambda, entity).Should().BeTrue();
    }
}