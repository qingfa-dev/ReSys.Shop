namespace Shared.Application.Models.Descriptors;

public readonly partial record struct Descriptor(string Name, string? Description, object? Example) : IDescriptor
{
    public required string Name { get; init; } = Name;
    public string? Description { get; init; } = Description;
    public object? Example { get; init; } = Example;
}