namespace Shared.Application.Models.Descriptors;

public interface IDescriptor
{
    string Name { get; }
    string? Description { get; }
    object? Example { get; }
}