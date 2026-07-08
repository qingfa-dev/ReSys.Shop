using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Module.Payment.Features.Storefront.Payment.Webhooks;
using Module.Payment.Infrastructure.Gateways.Bogus;
using Module.Payment.Infrastructure.Gateways.Stripe;
using Module.Payment.Persistence.Seeders;

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

        // Register: Stripe options from configuration
        services.Configure<StripeOptions>(configuration.GetSection("Stripe"));

        // Register: Bogus options from configuration
        services.Configure<BogusOptions>(configuration.GetSection(BogusOptions.SectionName));

        // Register: Payment gateway provider — Bogus (offline) takes precedence when UseBogusGateway=true
        var useBogus = configuration.GetValue<bool>("Payment:UseBogusGateway");
        if (useBogus)
        {
            services.AddScoped<Domain.Gateways.IPaymentGatewayActionProvider, BogusGateway>();
        }
        else
        {
            services.AddScoped<Domain.Gateways.IPaymentGatewayActionProvider, StripeGateway>();
        }

        // Register: Stripe webhook service
        services.AddSingleton<IStripeWebhookService, StripeWebhookService>();

        // Register: Seeders
        builder.AddSeeder<PaymentMethodSeeder>();

        return builder;
    }
}
