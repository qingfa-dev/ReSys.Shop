using System.Collections.Concurrent;

using Microsoft.Extensions.Logging;

namespace Shared.UnitTests.Application.Mediators.Fixtures;

public class TestLogger<T> : ILogger<T>
{
    public ConcurrentQueue<LogEntry> Entries { get; } = new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    public bool IsEnabled(LogLevel logLevel) => _enabledLevels.TryGetValue(logLevel, out var enabled) ? enabled : true;

    private readonly ConcurrentDictionary<LogLevel, bool> _enabledLevels = new();

    public void SetEnabled(LogLevel logLevel, bool enabled)
    {
        _enabledLevels[logLevel] = enabled;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        Entries.Enqueue(new LogEntry(
            logLevel,
            eventId,
            formatter(state, exception),
            exception));
    }

    public record LogEntry(
        LogLevel Level,
        EventId EventId,
        string Message,
        Exception? Exception);
}
