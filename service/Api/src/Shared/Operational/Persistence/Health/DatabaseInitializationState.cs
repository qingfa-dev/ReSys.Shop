namespace Shared.Operational.Persistence.Health;

public sealed class DatabaseInitializationState : IDatabaseInitializationState
{
    private int _complete;
    public bool IsComplete => Volatile.Read(ref _complete) == 1;
    public Exception? Failure { get; private set; }

    public void MarkComplete() => Volatile.Write(ref _complete, 1);

    public void MarkFailed(Exception ex) => Failure = ex;
}
