namespace Shared.Application.Models.Descriptors;

#pragma warning disable CA1000
public partial record OptionDescriptor<TValue>
{
    // Factory: Create an OptionDescriptor with Value and Name
    /// <summary>Creates an OptionDescriptor with the required value, name, description, and example.</summary>
    public static OptionDescriptor<TValue> Option(TValue value, string name, string? description = null, object? example = null) =>
        new()
        {
            Value = value,
            Name = name,
            Description = description,
            Example = example
        };
}
#pragma warning restore CA1000
