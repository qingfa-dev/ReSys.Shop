using System.Reflection;

using Shared.Operational.Persistence.Specifications.Helpers;

namespace Shared.UnitTests.Operational.Persistence.Specifications.Helpers;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Persistence")]
public sealed class QueryHelperMethodTests
{
    #region GetDefault

    [Theory]
    [InlineData(typeof(int), 0)]
    [InlineData(typeof(bool), false)]
    [InlineData(typeof(string), null)]
    [InlineData(typeof(Guid), "00000000-0000-0000-0000-000000000000")]
    [InlineData(typeof(decimal), 0.0)]
    [InlineData(typeof(double), 0.0)]
    [InlineData(typeof(byte), (byte)0)]
    [InlineData(typeof(char), '\0')]
    [InlineData(typeof(DateTime), "0001-01-01T00:00:00")]
    public void GetDefault_ShouldReturnExpectedValue(Type type, object? expectedValue)
    {
        // Act
        object? result = QueryHelper.GetDefault(type);

        // Assert
        if (type == typeof(Guid))
        {
            result.Should().Be(Guid.Empty);
        }
        else if (type == typeof(DateTime))
        {
            result.Should().Be(DateTime.MinValue);
        }
        else if (type == typeof(decimal))
        {
            result.Should().Be(0m);
        }
        else if (type == typeof(double))
        {
            result.Should().Be(0.0);
        }
        else if (type == typeof(char))
        {
            result.Should().Be('\0');
        }
        else
        {
            result.Should().Be(expectedValue);
        }
    }

    [Theory]
    [InlineData(typeof(int?))]
    [InlineData(typeof(bool?))]
    [InlineData(typeof(Guid?))]
    [InlineData(typeof(DateTime?))]
    [InlineData(typeof(decimal?))]
    [InlineData(typeof(long?))]
    public void GetDefault_ShouldReturnNull_ForNullableValueTypes(Type type)
    {
        // Act
        object? result = QueryHelper.GetDefault(type);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void GetDefault_ShouldReturnZeroedStruct_ForCustomStruct()
    {
        // Act
        object? result = QueryHelper.GetDefault(typeof(TestStruct));

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestStruct>();
        TestStruct casted = (TestStruct)result!;
        casted.Value.Should().Be(0);
        casted.Name.Should().BeNull();
    }

    [Fact]
    public void GetDefault_ShouldThrow_WhenTypeIsNull()
    {
        // Act
        Action act = () => QueryHelper.GetDefault(null!);

        // Assert: type.IsValueType throws when type is null
        act.Should().ThrowExactly<NullReferenceException>();
    }

    [Fact]
    public void GetDefault_ShouldReturnZero_ForEnumType()
    {
        // Act
        object? result = QueryHelper.GetDefault(typeof(TestEnum));

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<TestEnum>();
        ((TestEnum)result!).Should().Be(TestEnum.Default);
    }

    [Fact]
    public void GetDefault_ShouldReturnNull_ForClassType()
    {
        // Act
        object? result = QueryHelper.GetDefault(typeof(string));

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region ClearCache

    [Fact]
    public void ClearCache_ShouldResetExpressionCache()
    {
        // Arrange: Populate expression cache
        string cacheKey = "test-key";
        string opType = "Test";
        int factoryCalls = 0;

        QueryHelper.GetCachedExpression<ClearTestModel>(
            cacheKey, opType,
            () =>
            {
                factoryCalls++;
                return (System.Linq.Expressions.Expression<Func<ClearTestModel, bool>>)(x => true);
            });

        // Act
        QueryHelper.ClearCache();

        // Assert: Factory must be called again after cache clear
        QueryHelper.GetCachedExpression<ClearTestModel>(
            cacheKey, opType,
            () =>
            {
                factoryCalls++;
                return (System.Linq.Expressions.Expression<Func<ClearTestModel, bool>>)(x => true);
            });
        factoryCalls.Should().Be(2);
    }

    [Fact]
    public void ClearCache_ShouldResetPropertyCaches()
    {
        // Arrange: Populate ExactMatchCache and ResolvedCache
        QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "Name");
        QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "email_address");

        // Act
        QueryHelper.ClearCache();

        // Assert: Caches cleared, resolving names should still work
        PropertyInfo? nameProperty = QueryHelper.GetPropertyCaseInsensitive(typeof(TestModel), "Name");
        nameProperty.Should().NotBeNull();
        nameProperty.Name.Should().Be("Name");
    }

    [Fact]
    public void ClearCache_ShouldNotThrow_WhenCachesAreEmpty()
    {
        // Act
        Action act = () =>
        {
            QueryHelper.ClearCache();
            QueryHelper.ClearCache();
        };

        // Assert
        act.Should().NotThrow();
    }

    #endregion

    private enum TestEnum
    {
        Default,
        OptionA,
        OptionB,
    }

    private sealed class TestModel
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public string? EmailAddress { get; set; }
        public bool IsActive { get; set; }
    }

    private sealed class ClearTestModel
    {
        public int Id { get; set; }
    }

    private struct TestStruct
    {
        public int Value { get; set; }
        public string? Name { get; set; }
    }
}
