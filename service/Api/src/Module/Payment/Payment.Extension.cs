using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Module.Payment.Domain.Gateways;
using Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Bogus;
using Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Stripe;
using Module.Payment.Features.Admin.PaymentMethods.Services.Gateways.Webhooks;
using Module.Payment.Features.Admin.PaymentMethods.Services.Registry;
using Module.Payment.Features.Admin.Payments.Services.GatewayProcessing;
using Module.Payment.Persistence.Seeders;

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

        services.Configure<StripeOptions>(
            configuration.GetSection(StripeOptions.SectionName));
        services.Configure<BogusOptions>(
            configuration.GetSection(BogusOptions.SectionName));

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
            var stripeOpts = sp.GetRequiredService<IOptions<StripeOptions>>();
            if (stripeOpts.Value.Enabled)
                registry.Register(GatewayConstants.Providers.Stripe, sp.GetRequiredService<StripeGateway>);

            var bogusOpts = sp.GetRequiredService<IOptions<BogusOptions>>();
            if (bogusOpts.Value.Enabled)
                registry.Register(GatewayConstants.Providers.Bogus, sp.GetRequiredService<BogusGateway>);

            return registry;
        });

        services.AddScoped<IPaymentProcessingService, PaymentProcessingService>();

        services.AddSingleton<IWebhookHandler, StripeWebhookHandler>();

        builder.AddSeeder<PaymentMethodSeeder>();
        return builder;
    }
}
