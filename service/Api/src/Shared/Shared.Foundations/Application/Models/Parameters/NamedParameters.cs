namespace Shared.Application.Models.Parameters;

public interface INamedParameters
{
    string Name { get; init; }
    string? Presentation { get; init; }
}
