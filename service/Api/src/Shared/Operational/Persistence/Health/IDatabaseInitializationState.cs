namespace Shared.Operational.Persistence.Health;

public interface IDatabaseInitializationState
{
    bool IsComplete { get; }
    Exception? Failure { get; }
    void MarkComplete();
    void MarkFailed(Exception ex);
}
