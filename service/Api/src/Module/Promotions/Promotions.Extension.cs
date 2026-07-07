using Microsoft.Extensions.DependencyInjection;

namespace Module.Promotions;

// @CAT-10 Boundary: Domain -> Infrastructure — do not import persistence concerns; this is the module composition root
public static class PromotionsExtension
{
    public static IServiceCollection AddPromotionsModule(this IServiceCollection services)
    {
        // Domain services are instantiated directly by handlers with runtime data.
        return services;
    }
}
