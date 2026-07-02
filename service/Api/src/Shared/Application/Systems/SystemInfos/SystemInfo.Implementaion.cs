using System.Diagnostics;
using System.Reflection;

using Microsoft.Extensions.Hosting;

namespace Shared.Application.Systems.SystemInfos;

/// <summary>
/// Standard implementation of <see cref="ISystemInfo"/>.
/// </summary>
public sealed class SystemInfo(IHostEnvironment environment) : ISystemInfo
{
    private static readonly FileVersionInfo AssemblyVersion =
        FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

    /// <inheritdoc />
    public string ApplicationName => environment.ApplicationName; // Receive: Application name from host

    /// <inheritdoc />
    public string Version => AssemblyVersion.ProductVersion ?? "1.0.0"; // Receive: Version from assembly metadata

    /// <inheritdoc />
    public string Environment => environment.EnvironmentName; // Receive: Current environment name

    /// <inheritdoc />
    public string MachineName => System.Environment.MachineName; // Receive: Local machine name

    /// <inheritdoc />
    public int ProcessId => System.Environment.ProcessId; // Receive: OS process identifier
}
