namespace Shared.Application.Domain.Concerns.Parameterizable;

public interface IParameterizable
{
    string Name { get; set; }
    string? Presentation { get; set; }
}