using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients.Options;

using Shared.Operational.Http;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;

public static class InferenceClientDependencyInjection
{
    public static IServiceCollection AddInferenceClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(InferenceClientSetting.SectionName);

        services.Configure<InferenceClientSetting>(section);

        var options = section.Get<InferenceClientSetting>() ?? new InferenceClientSetting();

        services.AddTypedHttpClient<IInferenceClient, InferenceClient>(options.BaseAddress, client =>
        {
            if (options.TimeoutSeconds > 0)
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);

            foreach (var header in options.DefaultHeaders)
            {
                if (!string.IsNullOrEmpty(header.Value))
                    client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
            }
        });

        return services;
    }
}