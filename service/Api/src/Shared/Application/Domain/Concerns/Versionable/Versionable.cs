namespace Shared.Application.Domain.Concerns.Versionable;

public interface IVersionable
{
    uint Version { get; set; }
}
