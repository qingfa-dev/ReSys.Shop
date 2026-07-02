using System.Linq.Expressions;

using Shared.Operational.Persistence.Specifications.Helpers;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Helpers;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class QueryHelperBehaviorTests
{
    private int _factoryCalls;
    #region GetCachedExpression

    [Fact]
    public void GetCachedExpression_ShouldCacheAndReturnSameInstance()
    {
        // Arrange
        QueryHelper.ClearCache();
        string cacheKey = "test-filter";
        string opType = "Filter";
        int factoryCalls = 0;

        Expression<Func<BehaviorTestModel, bool>> Factory()
        {
            factoryCalls++;
            return x => x.Name == "test";
        }

        // Act
        LambdaExpression? expr1 = QueryHelper.GetCachedExpression<BehaviorTestModel>(cacheKey, opType, Factory);
        LambdaExpression? expr2 = QueryHelper.GetCachedExpression<BehaviorTestModel>(cacheKey, opType, Factory);

        // Assert
        expr1.Should().NotBeNull();
        expr2.Should().BeSameAs(expr1);
        factoryCalls.Should().Be(1);
    }

    [Fact]
    public void GetCachedExpression_ShouldReturnNull_WhenFactoryReturnsNull()
    {
        // Act
        LambdaExpression? result = QueryHelper.GetCachedExpression<BehaviorTestModel>(
            "null-key", "Test", () => null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetCachedExpression_ShouldSeparateByOperationType()
    {
        // Arrange
        QueryHelper.ClearCache();
        _factoryCalls = 0;
        string cacheKey = "shared-key";

        Expression<Func<BehaviorTestModel, bool>> Factory()
        {
            _factoryCalls++;
            return x => x.Name == "test";
        }

        // Act
        LambdaExpression? filterResult = QueryHelper.GetCachedExpression<BehaviorTestModel>(
            cacheKey, "Filter", Factory);
        LambdaExpression? searchResult = QueryHelper.GetCachedExpression<BehaviorTestModel>(
            cacheKey, "Search", Factory);

        // Assert
        filterResult.Should().NotBeNull();
        searchResult.Should().NotBeNull();
        filterResult.Should().NotBeSameAs(searchResult);
        _factoryCalls.Should().Be(2);
    }

    [Fact]
    public void GetCachedExpression_ShouldSeparateByCacheKey()
    {
        // Arrange
        QueryHelper.ClearCache();
        _factoryCalls = 0;
        string opType = "Filter";

        Expression<Func<BehaviorTestModel, bool>> Factory()
        {
            _factoryCalls++;
            return x => x.Name == "test";
        }

        // Act
        LambdaExpression? key1Result = QueryHelper.GetCachedExpression<BehaviorTestModel>(
            "key-1", opType, Factory);
        LambdaExpression? key2Result = QueryHelper.GetCachedExpression<BehaviorTestModel>(
            "key-2", opType, Factory);

        // Assert
        key1Result.Should().NotBeNull();
        key2Result.Should().NotBeNull();
        key1Result.Should().NotBeSameAs(key2Result);
        _factoryCalls.Should().Be(2);
    }

    [Fact]
    public void GetCachedExpression_ShouldSeparateByEntityType()
    {
        // Arrange
        QueryHelper.ClearCache();
        string cacheKey = "same-key";
        string opType = "Filter";
        int factoryCalls = 0;

        static Expression<Func<BehaviorTestModel, bool>> Factory1() =>
            x => x.Name == "test";
        static Expression<Func<AltTestModel, bool>> Factory2() =>
            x => x.Id == 1;

        // Act
        LambdaExpression? result1 = QueryHelper.GetCachedExpression<BehaviorTestModel>(
            cacheKey, opType, Factory1);
        LambdaExpression? result2 = QueryHelper.GetCachedExpression<AltTestModel>(
            cacheKey, opType, Factory2);

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().NotBeSameAs(result2);
        factoryCalls.Should().Be(0);
    }

    [Fact]
    public void GetCachedExpression_ShouldThrow_WhenFactoryThrows()
    {
        // Arrange
        Action act = () => QueryHelper.GetCachedExpression<BehaviorTestModel>(
            "throws", "Test",
            () => throw new InvalidOperationException("factory failed"));

        // Assert
        act.Should().ThrowExactly<InvalidOperationException>().WithMessage("factory failed");
    }

    [Fact]
    public void GetCachedExpression_ShouldCompileAndExecute()
    {
        // Validate: The cached expression tree can be compiled and executed
        // as a working predicate against entity instances.
        // Arrange
        QueryHelper.ClearCache();
        LambdaExpression? expr = QueryHelper.GetCachedExpression<BehaviorTestModel>(
            "compile-test", "Test",
            () => (Expression<Func<BehaviorTestModel, bool>>)(x => x.Name == "test"));

        // Act
        Expression<Func<BehaviorTestModel, bool>> typed =
            (Expression<Func<BehaviorTestModel, bool>>)expr!;
        Func<BehaviorTestModel, bool> compiled = typed.Compile();
        BehaviorTestModel matching = new BehaviorTestModel { Name = "test" };
        BehaviorTestModel nonMatching = new BehaviorTestModel { Name = "other" };

        // Assert
        compiled(matching).Should().BeTrue();
        compiled(nonMatching).Should().BeFalse();
    }

    [Fact]
    public void GetCachedExpression_ShouldUseCachedInstance_WhenFactoryChanges()
    {
        // Validate: Once cached, a different factory (returning a different expression)
        // is ignored — the first result is retained.
        // Arrange
        QueryHelper.ClearCache();
        string cacheKey = "immutable-key";
        string opType = "Test";

        LambdaExpression? first = QueryHelper.GetCachedExpression<BehaviorTestModel>(
            cacheKey, opType,
            () => (Expression<Func<BehaviorTestModel, bool>>)(x => x.Name == "first"));

        // Act: Second call with different factory
        LambdaExpression? second = QueryHelper.GetCachedExpression<BehaviorTestModel>(
            cacheKey, opType,
            () => (Expression<Func<BehaviorTestModel, bool>>)(x => x.Name == "second"));

        // Assert
        second.Should().BeSameAs(first);

        // Also verify the cached expression still returns the first factory's behavior
        Expression<Func<BehaviorTestModel, bool>> typed =
            (Expression<Func<BehaviorTestModel, bool>>)second!;
        Func<BehaviorTestModel, bool> compiled = typed.Compile();
        compiled(new BehaviorTestModel { Name = "first" }).Should().BeTrue();
        compiled(new BehaviorTestModel { Name = "second" }).Should().BeFalse();
    }

    [Fact]
    public void GetCachedExpression_ShouldCacheByTypeParameter()
    {
        // Validate: Same cacheKey and opType but different generic type T
        // produce separate cache entries.
        // Arrange
        QueryHelper.ClearCache();
        string cacheKey = "shared-key";
        string opType = "Filter";

        LambdaExpression? result1 = QueryHelper.GetCachedExpression<BehaviorTestModel>(
            cacheKey, opType,
            () => (Expression<Func<BehaviorTestModel, bool>>)(x => x.Name == "test"));
        LambdaExpression? result2 = QueryHelper.GetCachedExpression<AltTestModel>(
            cacheKey, opType,
            () => (Expression<Func<AltTestModel, bool>>)(x => x.Id == 1));

        // Assert
        result1.Should().NotBeNull();
        result2.Should().NotBeNull();
        result1.Should().NotBeSameAs(result2);
    }

    [Fact]
    public void GetCachedExpression_ThreadSafety_ShouldNotThrow()
    {
        // Contract: ConcurrentDictionary.GetOrAdd must handle parallel access.
        // Arrange
        QueryHelper.ClearCache();
        string[] cacheKeys = { "a", "b", "c", "d", "e" };
        string[] opTypes = { "Filter", "Search", "Sort" };
        int iterations = 100;
        ParallelOptions parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };

        // Act
        Action act = () =>
        {
            Parallel.ForEach(cacheKeys, parallelOptions, key =>
            {
                foreach (string op in opTypes)
                {
                    for (int i = 0; i < iterations; i++)
                    {
                        LambdaExpression? expr = QueryHelper.GetCachedExpression<BehaviorTestModel>(
                            key, op, () => (Expression<Func<BehaviorTestModel, bool>>)(x => x.Name == key));
                        if (expr == null)
                        {
                            throw new InvalidOperationException(
                                $"Expression is null for key '{key}', op '{op}'");
                        }
                    }
                }
            });
        };

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    private sealed class BehaviorTestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
    }

    private sealed class AltTestModel
    {
        public int Id { get; set; }
    }
}
