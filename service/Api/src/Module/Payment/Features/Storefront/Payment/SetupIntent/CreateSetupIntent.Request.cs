namespace Module.Payment.Features.Storefront.Payment.SetupIntent;

public static partial class CreateSetupIntent
{
    public class Request
    {
        public Guid PaymentMethodId { get; set; }
    }
}
