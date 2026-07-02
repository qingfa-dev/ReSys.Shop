using Shared.Application.Domain.Models;

namespace Shared.UnitTests.Application.Domain.Models;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Concerns")]
public class ValueObjectTests
{
    private sealed class TestValueObject(string first, string second) : ValueObject
    {
        public string First { get; } = first;
        public string Second { get; } = second;

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return First;
            yield return Second;
        }
    }

    private sealed class AnotherValueObject : ValueObject
    {
        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return "value";
        }
    }

    [Fact(DisplayName = "Equals should return true when all components match")]
    public void Equals_SameComponents_ShouldReturnTrue()
    {
        var a = new TestValueObject("Hello", "World");
        var b = new TestValueObject("Hello", "World");

        a.Equals(b).Should().BeTrue();
    }

    [Fact(DisplayName = "Equals should return false when components differ")]
    public void Equals_DifferentComponents_ShouldReturnFalse()
    {
        var a = new TestValueObject("Hello", "World");
        var b = new TestValueObject("Hello", "There");

        a.Equals(b).Should().BeFalse();
    }

    [Fact(DisplayName = "Equals should return false when compared to null")]
    public void Equals_Null_ShouldReturnFalse()
    {
        var a = new TestValueObject("Hello", "World");

        a.Equals(null).Should().BeFalse();
    }

    [Fact(DisplayName = "Equals should return false when compared to different type")]
    public void Equals_DifferentType_ShouldReturnFalse()
    {
        var a = new TestValueObject("Hello", "World");
        var b = new AnotherValueObject();

        a.Equals(b).Should().BeFalse();
    }

    [Fact(DisplayName = "GetHashCode should be equal when components match")]
    public void GetHashCode_SameComponents_ShouldBeEqual()
    {
        var a = new TestValueObject("Hello", "World");
        var b = new TestValueObject("Hello", "World");

        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact(DisplayName = "GetHashCode should differ when components differ")]
    public void GetHashCode_DifferentComponents_ShouldNotBeEqual()
    {
        var a = new TestValueObject("Hello", "World");
        var b = new TestValueObject("Hello", "There");

        a.GetHashCode().Should().NotBe(b.GetHashCode());
    }

    [Fact(DisplayName = "operator == should return true when both value objects have same components")]
    public void OperatorEquals_SameComponents_ShouldReturnTrue()
    {
        var a = new TestValueObject("Hello", "World");
        var b = new TestValueObject("Hello", "World");

        (a == b).Should().BeTrue();
    }

    [Fact(DisplayName = "operator == should return false when value objects have different components")]
    public void OperatorEquals_DifferentComponents_ShouldReturnFalse()
    {
        var a = new TestValueObject("Hello", "World");
        var b = new TestValueObject("Hello", "There");

        (a == b).Should().BeFalse();
    }

    [Fact(DisplayName = "operator == should return true when both are null")]
    public void OperatorEquals_BothNull_ShouldReturnTrue()
    {
        TestValueObject? a = null;
        TestValueObject? b = null;

        (a == b).Should().BeTrue();
    }

    [Fact(DisplayName = "operator == should return false when one is null")]
    public void OperatorEquals_OneNull_ShouldReturnFalse()
    {
        var a = new TestValueObject("Hello", "World");
        TestValueObject? b = null;

        (a == b).Should().BeFalse();
        (b == a).Should().BeFalse();
    }

    [Fact(DisplayName = "operator != should return false when components match")]
    public void OperatorNotEquals_SameComponents_ShouldReturnFalse()
    {
        var a = new TestValueObject("Hello", "World");
        var b = new TestValueObject("Hello", "World");

        (a != b).Should().BeFalse();
    }

    [Fact(DisplayName = "operator != should return true when components differ")]
    public void OperatorNotEquals_DifferentComponents_ShouldReturnTrue()
    {
        var a = new TestValueObject("Hello", "World");
        var b = new TestValueObject("Hello", "There");

        (a != b).Should().BeTrue();
    }
}
