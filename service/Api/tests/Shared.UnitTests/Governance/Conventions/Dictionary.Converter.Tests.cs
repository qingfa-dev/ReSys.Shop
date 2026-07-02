using Shared.Governance.Conventions;

namespace Shared.UnitTests.Governance.Conventions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Extensions")]
public class DictionaryExtensionsTests
{
    public class ToDictionaryMethod
    {
        [Fact]
        public void WithEmptyArray_ReturnsNull()
        {
            (string Key, object? Value)[] input = [];

            IReadOnlyDictionary<string, object?>? result = input.ToDictionary();

            result.Should().BeNull();
        }

        [Fact]
        public void WithSingleElement_ReturnsDictionaryWithOneEntry()
        {
            (string Key, object? Value)[] input = [("key1", "value1")];

            IReadOnlyDictionary<string, object?>? result = input.ToDictionary();

            result.Should().NotBeNull()
                .And.HaveCount(1)
                .And.ContainKey("key1").WhoseValue.Should().Be("value1");
        }

        [Fact]
        public void WithMultipleElements_ReturnsFullDictionary()
        {
            (string Key, object? Value)[] input = [("a", 1), ("b", "two"), ("c", 3.0)];

            IReadOnlyDictionary<string, object?>? result = input.ToDictionary();

            result.Should().NotBeNull()
                .And.HaveCount(3)
                .And.Contain("a", 1)
                .And.Contain("b", "two")
                .And.Contain("c", 3.0);
        }

        [Fact]
        public void WithNullValue_PreservesNullInDictionary()
        {
            (string Key, object? Value)[] input = [("key", null)];

            IReadOnlyDictionary<string, object?>? result = input.ToDictionary();

            result.Should().NotBeNull()
                .And.HaveCount(1)
                .And.ContainKey("key").WhoseValue.Should().BeNull();
        }
    }
}
