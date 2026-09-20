namespace Shared.Application.Models.Descriptors;

public static class DescriptorExtensions
{
    // Format: Create formatted display string from descriptor properties
    /// <summary>Formats the descriptor as a display string.</summary>
    public static string Format(this Descriptor descriptor)
    {
        string result = descriptor.Name;

        if (descriptor.Description is not null)
        {
            result = $"{result}: {descriptor.Description}";
        }

        if (descriptor.Example is not null)
        {
            result = $"{result} (e.g. {descriptor.Example})";
        }

        return result;
    }

    // Convert: Return new Descriptor with updated name
    /// <summary>Returns a new Descriptor with the specified name.</summary>
    public static Descriptor WithName(this Descriptor descriptor, string name) =>
        descriptor with { Name = name };

    // Convert: Return new Descriptor with updated description
    /// <summary>Returns a new Descriptor with the specified description.</summary>
    public static Descriptor WithDescription(this Descriptor descriptor, string? description) =>
        descriptor with { Description = description };

    // Convert: Return new Descriptor with updated example
    /// <summary>Returns a new Descriptor with the specified example.</summary>
    public static Descriptor WithExample(this Descriptor descriptor, object? example) =>
        descriptor with { Example = example };
}
