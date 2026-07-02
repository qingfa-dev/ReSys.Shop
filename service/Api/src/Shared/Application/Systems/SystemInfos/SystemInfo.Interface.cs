namespace Shared.Application.Systems.SystemInfos;

/// <summary>
/// Provides information about the running system and application.
/// </summary>
public interface ISystemInfo
{
    /// <summary>
    /// Gets the name of the application.
    /// </summary>
    string ApplicationName { get; }

    /// <summary>
    /// Gets the version of the application.
    /// </summary>
    string Version { get; }

    /// <summary>
    /// Gets the current execution environment (e.g., Development, Staging, Production).
    /// </summary>
    string Environment { get; }

    /// <summary>
    /// Gets the machine name where the application is running.
    /// </summary>
    string MachineName { get; }

    /// <summary>
    /// Gets the process identifier.
    /// </summary>
    int ProcessId { get; }
}
