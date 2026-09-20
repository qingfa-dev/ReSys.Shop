namespace Shared.Application.Models.Descriptors;

public partial record OptionDescriptor<TValue> : IDescriptor
{
    public required TValue Value { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public object? Example { get; init; }
}