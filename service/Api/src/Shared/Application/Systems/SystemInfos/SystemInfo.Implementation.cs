using System.Diagnostics;
using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

using Shared.Application.Domain.Currencies;

namespace Shared.Application.Systems.SystemInfos;

public sealed class SystemInfo(IHostEnvironment environment, IConfiguration configuration) : ISystemInfo
{
    private static readonly FileVersionInfo AssemblyVersion =
        FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location);

    public string ApplicationName => environment.ApplicationName;

    public string Version => AssemblyVersion.ProductVersion ?? "1.0.0";

    public string Environment => environment.EnvironmentName;

    public string MachineName => System.Environment.MachineName;

    public int ProcessId => System.Environment.ProcessId;

    public string DefaultCurrency => configuration["System:DefaultCurrency"] ?? SystemCurrencyConstant.Defaults.Code;
}
