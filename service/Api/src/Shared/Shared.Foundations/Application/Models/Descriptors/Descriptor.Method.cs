namespace Shared.Application.Models.Descriptors;

public partial record struct Descriptor
{
    // Factory: Create a Descriptor with required name and optional description/example
    /// <summary>Creates a Descriptor with the specified name, optional description, and optional example.</summary>
    public static Descriptor Named(string name, string? description = null, object? example = null) =>
        new()
        {
            Name = name,
            Description = description,
            Example = example
        };
}
