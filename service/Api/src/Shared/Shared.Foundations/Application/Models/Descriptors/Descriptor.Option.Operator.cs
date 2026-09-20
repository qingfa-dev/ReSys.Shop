namespace Shared.Application.Models.Descriptors;

public partial record OptionDescriptor<TValue>
{
    // Convert: OptionDescriptor to its Value — enables implicit use as TValue
    /// <summary>Allows implicit conversion from OptionDescriptor&lt;TValue&gt; to TValue (returns Value).</summary>
    public static implicit operator TValue(OptionDescriptor<TValue> option) => option.Value;
}
