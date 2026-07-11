namespace Module.Ordering.Infrastructure.Options;

public sealed class OutboundWebhookOptions
{
    public const string SectionName = "Webhooks:Outbound";

    public bool Enabled { get; set; }
    public List<string> Urls { get; set; } = [];
}
