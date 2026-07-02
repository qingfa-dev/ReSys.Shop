using System.Runtime.Serialization;

using Shared.Governance.Conventions;

namespace Shared.UnitTests.Governance.Conventions;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "Extensions")]
public class EnumExtensionsTests
{
    public enum SampleValues
    {
        [EnumMember(Value = "value_one")]
        ValueOne,
        [EnumMember(Value = "value_two")]
        ValueTwo,
        PlainValue
    }

    public enum PlainValues
    {
        First,
        Second
    }

    public class GetValues
    {
        [Fact]
        public void WithEnumMemberAttributes_ReturnsMemberValues()
        {
            IReadOnlyList<string> result = EnumExtensions.GetValues<SampleValues>();

            result.Should().BeEquivalentTo(["value_one", "value_two", "PlainValue"]);
        }

        [Fact]
        public void WithoutEnumMemberAttributes_ReturnsFieldNames()
        {
            IReadOnlyList<string> result = EnumExtensions.GetValues<PlainValues>();

            result.Should().BeEquivalentTo(["First", "Second"]);
        }
    }

    public class FromEnumMemberValue
    {
        [Fact]
        public void WithEnumMemberValue_ReturnsCorrespondingEnum()
        {
            SampleValues result = EnumExtensions.FromEnumMemberValue<SampleValues>("value_one");

            result.Should().Be(SampleValues.ValueOne);
        }

        [Fact]
        public void WithFieldName_ReturnsCorrespondingEnum()
        {
            SampleValues result = EnumExtensions.FromEnumMemberValue<SampleValues>("PlainValue");

            result.Should().Be(SampleValues.PlainValue);
        }

        [Fact]
        public void WithUnknownValue_ThrowsArgumentException()
        {
            Func<SampleValues> act = () => EnumExtensions.FromEnumMemberValue<SampleValues>("nonexistent");

            act.Should().Throw<ArgumentException>()
                .WithMessage("Requested value 'nonexistent' was not found in enum SampleValues.*");
        }
    }

    public class ToEnumMemberValue
    {
        [Fact]
        public void WithEnumMemberAttribute_ReturnsMemberValue()
        {
            var result = SampleValues.ValueOne.ToEnumMemberValue();

            result.Should().Be("value_one");
        }

        [Fact]
        public void WithoutEnumMemberAttribute_ReturnsFieldName()
        {
            var result = SampleValues.PlainValue.ToEnumMemberValue();

            result.Should().Be("PlainValue");
        }

        [Fact]
        public void WithNoAttributesOnEnum_ReturnsFieldName()
        {
            var result = PlainValues.First.ToEnumMemberValue();

            result.Should().Be("First");
        }
    }
}
