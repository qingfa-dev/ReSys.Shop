using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Module.Payment.Services.Configuration;
using Module.Payment.Services.Processing;
using Module.Payment.Services.Webhook;
using Module.Payment.Persistence.Seeders;

using BogusGateway = Module.Payment.Services.Provider.Bogus.BogusGateway;
using BogusSetting = Module.Payment.Services.Provider.Bogus.BogusSetting;
using GatewayConstants = Module.Payment.Services.Provider.GatewayConstants;
using GatewayRegistry = Module.Payment.Services.Provider.GatewayRegistry;
using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;
using StripeGateway = Module.Payment.Services.Provider.Stripe.StripeGateway;
using StripeSetting = Module.Payment.Services.Provider.Stripe.StripeSetting;

using Shared.Operational.Security.Encryption;
using Shared.Operational.Persistence.Configurations.Dictionaries;

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

        EncryptedDictionaryConverter.Configure(() =>
        {
            var sp = builder.Services.BuildServiceProvider();
            return sp.GetRequiredService<IEncryptionService>();
        });

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

        services.AddSingleton<IWebhookHandler, StripeWebhookHandler>();
        services.AddSingleton<IStripeWebhookService, StripeWebhookHandler>();

        builder.AddSeeder<PaymentMethodSeeder>();
        return builder;
    }
}
