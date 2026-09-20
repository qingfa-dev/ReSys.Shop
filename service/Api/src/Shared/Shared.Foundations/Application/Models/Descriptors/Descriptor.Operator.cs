namespace Shared.Application.Models.Descriptors;

public partial record struct Descriptor
{
    // Convert: Descriptor to its Name string — enables implicit use as string
    /// <summary>Allows implicit conversion from Descriptor to string (returns Name).</summary>
    public static implicit operator string(Descriptor descriptor) => descriptor.Name;
}
