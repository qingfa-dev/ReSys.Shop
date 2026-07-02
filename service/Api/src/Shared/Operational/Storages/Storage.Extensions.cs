using FluentValidation;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using nClam;

using Shared.Application.Extensions.Validations;
using Shared.Operational.Storages.Options;
using Shared.Operational.Storages.Processing;
using Shared.Operational.Storages.Providers;
using Shared.Operational.Storages.Providers.Options;
using Shared.Operational.Storages.Security;
using Shared.Operational.Storages.Security.Guard;
using Shared.Operational.Storages.Security.Guard.Options;

using Shared.Operational.Storages.Security.Options;
using Shared.Operational.Storages.Security.Scanners;
using Shared.Operational.Storages.Security.Scanners.Options;

using StorageAntiForgeryGuard = Shared.Operational.Storages.Security.Guard.StorageAntiForgeryGuard;

namespace Shared.Operational.Storages;

public static class StorageExtensions
{
    public static WebApplicationBuilder AddStorage(this WebApplicationBuilder builder)
    {
        builder.Services.AddStorageOptions();
        builder.Services.AddStorageServices();
        return builder;
    }

    private static void AddStorageOptions(this IServiceCollection services)
    {
        services.AddSingleton<IValidator<StorageSetting>, StorageSettingValidator>();
        services.AddOptions<StorageSetting>()
            .BindConfiguration(StorageSetting.SectionName)
            .ValidateFluentValidation();

        services.AddSingleton<IValidator<StorageSecuritySetting>, StorageSecuritySettingValidator>();
        services.AddOptions<StorageSecuritySetting>()
            .BindConfiguration(StorageSecuritySetting.SectionName)
            .ValidateFluentValidation();
        
        services.AddSingleton<IValidator<LocalStorageProviderSetting>, LocalStorageProviderSettingValidator>();
        services.AddOptions<LocalStorageProviderSetting>()
            .BindConfiguration($"{BaseStorageProviderSetting.BaseSection}:{LocalStorageProviderSetting.ProviderKey}")
            .ValidateFluentValidation();

        services.AddSingleton<IValidator<AzureStorageProviderSetting>, AzureStorageProviderSettingValidator>();
        services.AddOptions<AzureStorageProviderSetting>()
            .BindConfiguration($"{BaseStorageProviderSetting.BaseSection}:{AzureStorageProviderSetting.ProviderKey}")
            .ValidateFluentValidation();

        services.AddSingleton<IValidator<S3StorageProviderSetting>, S3StorageProviderSettingValidator>();
        services.AddOptions<S3StorageProviderSetting>()
            .BindConfiguration($"{BaseStorageProviderSetting.BaseSection}:{S3StorageProviderSetting.ProviderKey}")
            .ValidateFluentValidation();

        services.AddSingleton<IStorageSecurityEnforcer, StorageSecurityEnforcer>();
        services.AddSingleton<IValidator<AntiForgeryOptions>, AntiForgeryOptionsValidator>();
        services.AddOptions<AntiForgeryOptions>()
            .BindConfiguration(AntiForgeryOptions.SectionName)
            .ValidateFluentValidation();

        services.AddSingleton<IStorageAntiForgeryGuard, StorageAntiForgeryGuard>();

        services.AddSingleton<IValidator<StorageMalwareScannerOptions>, StorageMalwareScannerOptionsValidator>();
        services.AddOptions<StorageMalwareScannerOptions>()
            .BindConfiguration(StorageMalwareScannerOptions.SectionName)
            .ValidateFluentValidation();
    }

    private static void AddStorageServices(this IServiceCollection services)
    {
        services.AddSingleton<ContentMalwareScanner>();
        services.AddSingleton<LocalStorageProvider>();
        services.AddSingleton<S3StorageProvider>();
        services.AddSingleton<IStorageMalwareScanner>(sp =>
            new StorageMalwareScanner(
                sp.GetRequiredService<ILogger<StorageMalwareScanner>>(),
                sp.GetService<IOptions<StorageMalwareScannerOptions>>(),
                sp.GetService<IClamClient>(),
                sp.GetRequiredService<ContentMalwareScanner>()));

        services.AddSingleton<IImageProcessor, ImageProcessor>();
    }

    public static WebApplication UseStorage(this WebApplication app)
    {
        return app;
    }
}
