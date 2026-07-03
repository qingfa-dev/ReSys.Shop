using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;

public static class InferenceClientDependencyInjection
{
    public static IServiceCollection AddInferenceClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<InferenceAuthOptions>(configuration.GetSection(InferenceAuthOptions.SectionName));
        services.AddTransient<InferenceAuthHandler>();
        services.AddHttpClient<IInferenceClient, InferenceClient>(client =>
        {
            client.BaseAddress = new Uri("http://inference");
        })
        .AddHttpMessageHandler<InferenceAuthHandler>();

        return services;
    }
}
