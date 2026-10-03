using Shared.Application.Models.Descriptors;

namespace Shared.UnitTests.Application.Models.Descriptors;

[Trait("Category", "Unit")]
[Trait("Module", "Shared")]
[Trait("Feature", "OptionDescriptor")]
public sealed class OptionDescriptorTests
{
    [Fact(DisplayName = "OptionDescriptor sets Value, Name, Description, Example")]
    public void OptionDescriptor_ShouldSetAllProperties()
    {
        var example = new { template = "welcome" };

        OptionDescriptor<int> option = new()
        {
            Value = 42,
            Name = "The Answer",
            Description = "The ultimate answer.",
            Example = example
        };

        option.Value.Should().Be(42);
        option.Name.Should().Be("The Answer");
        option.Description.Should().Be("The ultimate answer.");
        option.Example.Should().BeEquivalentTo(example);
    }

    [Fact(DisplayName = "OptionDescriptor with only required fields")]
    public void OptionDescriptor_WithRequiredFields()
    {
        OptionDescriptor<string> option = new()
        {
            Value = "email",
            Name = "Email"
        };

        option.Value.Should().Be("email");
        option.Name.Should().Be("Email");
        option.Description.Should().BeNull();
        option.Example.Should().BeNull();
    }

    [Fact(DisplayName = "OptionDescriptor implements IDescriptor")]
    public void OptionDescriptor_ShouldImplementIDescriptor()
    {
        OptionDescriptor<int> option = new()
        {
            Value = 1,
            Name = "SMS",
            Description = "Send via SMS",
            Example = "sms.example.com"
        };

#pragma warning disable CA1859 // Use concrete type — intentionally testing interface
        IDescriptor descriptor = option;
#pragma warning restore CA1859

        descriptor.Name.Should().Be("SMS");
        descriptor.Description.Should().Be("Send via SMS");
        descriptor.Example.Should().Be("sms.example.com");
    }

    [Fact(DisplayName = "OptionDescriptor value equality: same values are equal")]
    public void OptionDescriptor_SameValues_ShouldBeEqual()
    {
        OptionDescriptor<int> a = new() { Value = 1, Name = "One" };
        OptionDescriptor<int> b = new() { Value = 1, Name = "One" };

        a.Should().Be(b);
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact(DisplayName = "OptionDescriptor value equality: different Value not equal")]
    public void OptionDescriptor_DifferentValue_ShouldNotBeEqual()
    {
        OptionDescriptor<int> a = new() { Value = 1, Name = "One" };
        OptionDescriptor<int> b = new() { Value = 2, Name = "One" };

        a.Should().NotBe(b);
    }

    [Fact(DisplayName = "OptionDescriptor generic: works with enum type as TValue")]
    public void OptionDescriptor_WithEnumType()
    {
        OptionDescriptor<SampleEnum> option = new()
        {
            Value = SampleEnum.Beta,
            Name = "Beta"
        };

        option.Value.Should().Be(SampleEnum.Beta);
        option.Name.Should().Be("Beta");
    }

    [Fact(DisplayName = "OptionDescriptor generic: works with record type as TValue")]
    public void OptionDescriptor_WithRecordType()
    {
        var value = new SampleRecord { Key = "key1", Count = 100 };

        OptionDescriptor<SampleRecord> option = new()
        {
            Value = value,
            Name = "Sample"
        };

        option.Value.Should().Be(value);
        option.Name.Should().Be("Sample");
    }

    private enum SampleEnum
    {
        Alpha,
        Beta,
        Gamma
    }

    private sealed record SampleRecord
    {
        public string Key { get; init; } = default!;
        public int Count { get; init; }
    }
}
