using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Module.Payment.Services.Configuration;
using Module.Payment.Services.Processing;
using Module.Payment.Services.Webhook;
using Module.Payment.Features.Storefront.Payment.Webhooks;
using Module.Payment.Persistence.Seeders;

using BogusGateway = Module.Payment.Services.Provider.Bogus.BogusGateway;
using BogusSetting = Module.Payment.Services.Provider.Bogus.BogusSetting;
using GatewayConstants = Module.Payment.Services.Provider.GatewayConstants;
using GatewayRegistry = Module.Payment.Services.Provider.GatewayRegistry;
using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;
using StripeGateway = Module.Payment.Services.Provider.Stripe.StripeGateway;
using StripeSetting = Module.Payment.Services.Provider.Stripe.StripeSetting;

using Shared.Operational.Persistence.Configurations.Dictionaries;
using Shared.Operational.Security.Encryption;

// @CAT-10 Boundary: Domain -> Infrastructure — do not import persistence concerns above this line
namespace Module.Payment;

public static class PaymentExtension
{
    /// <summary>
    /// Registers the Payment module services with the dependency injection container.
    /// </summary>
    /// <param name="builder">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    // @CAT-10 Boundary: Domain -> Infrastructure — do not import EF Core or repository types here
    public static WebApplicationBuilder AddPaymentModule(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        services.Configure<GatewayProvidersOptions>(
            configuration.GetSection(GatewayConstants.Configuration.SectionName));

        services.Configure<StripeSetting>(
            configuration.GetSection(StripeSetting.SectionName));
        services.Configure<BogusSetting>(
            configuration.GetSection(BogusSetting.SectionName));

        services.AddSingleton<IEncryptionService>(sp =>
        {
            var gwOpts = sp.GetRequiredService<IOptions<GatewayProvidersOptions>>();
            return new AesEncryptionService(gwOpts.Value.SettingsEncryptionKey!);
        });

        EncryptedDictionaryConverter.Configure(sp => sp.GetRequiredService<IEncryptionService>());

        services.AddTransient<StripeGateway>();
        services.AddTransient<BogusGateway>();

        services.AddSingleton<IGatewayRegistry>(sp =>
        {
            var registry = new GatewayRegistry();
            var stripeOpts = sp.GetRequiredService<IOptions<StripeSetting>>();
            if (stripeOpts.Value.Enabled)
                registry.Register(GatewayConstants.Providers.Stripe, sp.GetRequiredService<StripeGateway>);

            var bogusOpts = sp.GetRequiredService<IOptions<BogusSetting>>();
            if (bogusOpts.Value.Enabled)
                registry.Register(GatewayConstants.Providers.Bogus, sp.GetRequiredService<BogusGateway>);

            return registry;
        });

        services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();

        services.AddSingleton<IStripeWebhookService, StripeWebhookDispatcher>();
        // IWebhookHandler is the legacy gateway dispatcher interface; keep the
        // old handler bound for now — see plan TODO to remove in a follow-up.
        services.AddSingleton<IWebhookHandler, StripeWebhookHandler>();

        services.AddHostedService<EncryptedConverterServiceProviderInitializer>();

        builder.AddSeeder<PaymentMethodSeeder>();
        return builder;
    }

    /// <summary>
    /// Captures the host's <see cref="IServiceProvider"/> after the host is built and
    /// wires it into <see cref="EncryptedDictionaryConverter"/> so the value-converter
    /// can resolve <see cref="IEncryptionService"/> lazily without a second root container.
    /// </summary>
    private sealed class EncryptedConverterServiceProviderInitializer : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public EncryptedConverterServiceProviderInitializer(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            EncryptedDictionaryConverter.ConfigureServiceProvider(_serviceProvider);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
