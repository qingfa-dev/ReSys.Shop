namespace Module.Payment.Features.Storefront.Payment.SetupIntent;

public static partial class CreateSetupIntent
{
    public class Response
    {
        public string ClientSecret { get; init; } = null!;
    }
}
