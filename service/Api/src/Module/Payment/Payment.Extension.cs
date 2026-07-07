using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Module.Payment.Features.Storefront.Payment.Webhooks;
using Module.Payment.Infrastructure.Gateways.Bogus;
using Module.Payment.Infrastructure.Gateways.Stripe;

// @CAT-10 Boundary: Domain -> Infrastructure — do not import persistence concerns above this line
namespace Module.Payment;

public static class PaymentExtension
{
    /// <summary>
    /// Registers the Payment module services with the dependency injection container.
    /// </summary>
    /// <param name="services">The service collection to register into.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    // @CAT-10 Boundary: Domain -> Infrastructure — do not import EF Core or repository types here
    public static IServiceCollection AddPaymentModule(this IServiceCollection services, IConfiguration configuration)
    {
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

        return services;
    }
}
