using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;

using Shared.Operational.Persistence.Configurations.Dictionaries;
using Shared.Operational.Security.Encryption;

namespace Module.UnitTests;

internal static class TestModuleInitializer
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        var serviceProvider = new ServiceCollection()
            .AddSingleton<IEncryptionService>(new AesEncryptionService("0123456789abcdef0123456789abcdef"))
            .BuildServiceProvider();

        EncryptedDictionaryConverter.Configure(sp => sp.GetRequiredService<IEncryptionService>());
        EncryptedDictionaryConverter.ConfigureServiceProvider(serviceProvider);
    }
}
