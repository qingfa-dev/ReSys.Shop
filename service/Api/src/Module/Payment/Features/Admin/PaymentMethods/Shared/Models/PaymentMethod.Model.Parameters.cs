using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Features.Admin.PaymentMethods.Shared.Models;

/// <summary>
/// Abstract base class for payment method-related parameters, providing common properties.
/// </summary>
public abstract class PaymentMethodParameters
{
    /// <summary>Gets or sets the display name of the payment method.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the optional unique code for the payment method.</summary>
    public string? Code { get; init; }

    /// <summary>Gets or sets the optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the provider type (e.g., StoreCredit, CreditCard).</summary>
    public string ProviderType { get; init; } = string.Empty;

    /// <summary>Gets or sets whether auto-capture is enabled.</summary>
    public bool AutoCapture { get; init; }

    /// <summary>Gets or sets where to display this method.</summary>
    public DisplayOn DisplayOn { get; init; }

    /// <summary>Gets or sets the display order position.</summary>
    public int Position { get; init; }

    /// <summary>Gets or sets the display presentation text.</summary>
    public string? Presentation { get; init; }

    /// <summary>Gets or sets whether the payment method is active.</summary>
    public bool Active { get; init; }
}
