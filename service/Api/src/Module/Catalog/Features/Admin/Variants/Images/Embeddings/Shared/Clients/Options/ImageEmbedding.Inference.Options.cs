namespace Module.Catalog.Features.Admin.Variants.Images.Embeddings.Shared.Clients.Options;

public sealed class InferenceClientSetting
{
    public const string SectionName = "Http:Clients:Inference";

    public string BaseAddress { get; set; } = "http://embedding:8000";
    public int TimeoutSeconds { get; set; }
    public string ApiKey { get; set; } = string.Empty;
    public Dictionary<string, string> DefaultHeaders { get; set; } = [];
}