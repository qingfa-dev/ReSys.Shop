namespace Module.Payment.Domain.PaymentMethods;

/// <summary>Concrete payment method factory for check-based payments — mirrors Spree::PaymentMethod::Check.</summary>
// Define: Concrete payment method for check-based payments — mirrors Spree::PaymentMethod::Check
// Invariant: ProviderType is always "Check"; no gateway integration required; manual processing only
public static class CheckPaymentMethod
{
    public const string ProviderType = "Check";

    // Contract: pre=name!=null, post=method.ProviderType=="Check" && method.Active==true
    // Create: A new check payment method with pre-configured check provider type
    public static Result<PaymentMethod> Create(
        string name,
        string? code = null,
        string? description = null,
        DisplayOn displayOn = PaymentMethodConstant.Defaults.DisplayOn,
        int position = PaymentMethodConstant.Defaults.Position)
    {
        var result = PaymentMethodExtensions.Create(
            name: name,
            code: code,
            providerType: ProviderType,
            autoCapture: true,
            displayOn: displayOn);

        if (result.IsSuccess)
        {
            result.Value.Description = description;
            result.Value.Position = position;
        }

        return result;
    }
}