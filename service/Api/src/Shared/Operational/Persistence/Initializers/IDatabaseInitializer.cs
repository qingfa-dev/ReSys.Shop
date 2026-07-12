namespace Shared.Operational.Persistence.Initializers;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken cancellationToken = default);
}
