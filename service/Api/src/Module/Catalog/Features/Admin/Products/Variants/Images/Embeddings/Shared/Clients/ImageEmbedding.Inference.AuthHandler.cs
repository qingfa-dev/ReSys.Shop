using Microsoft.Extensions.Options;

namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients;

public class InferenceAuthOptions
{
    public const string SectionName = "Services:Inference";
    public string ApiKey { get; set; } = string.Empty;
}

public class InferenceAuthHandler : DelegatingHandler
{
    private readonly IOptions<InferenceAuthOptions> _options;

    public InferenceAuthHandler(IOptions<InferenceAuthOptions> options)
    {
        _options = options;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(_options.Value.ApiKey))
        {
            request.Headers.Add("X-API-Key", _options.Value.ApiKey);
        }
        return await base.SendAsync(request, cancellationToken);
    }
}
