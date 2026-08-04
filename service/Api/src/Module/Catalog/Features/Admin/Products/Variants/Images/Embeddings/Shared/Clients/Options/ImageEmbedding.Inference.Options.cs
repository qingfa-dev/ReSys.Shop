namespace Module.Catalog.Features.Admin.Products.Variants.Images.Embeddings.Shared.Clients.Options;

public sealed class InferenceClientSetting
{
    public const string SectionName = "Http:Clients:Inference";

    public string BaseAddress { get; set; } = "http://embedding";
    public int TimeoutSeconds { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public Dictionary<string, string> DefaultHeaders { get; set; } = [];
}