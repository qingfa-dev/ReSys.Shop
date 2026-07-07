namespace Module.Payment.Infrastructure.Gateways.Bogus;

public class BogusOptions
{
    public const string SectionName = "Payment:Bogus";
    public bool Enabled { get; set; } = false;
}
