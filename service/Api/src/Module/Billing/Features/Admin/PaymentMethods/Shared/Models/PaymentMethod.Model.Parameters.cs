using Module.Billing.Domain.PaymentMethods;

namespace Module.Billing.Features.Admin.PaymentMethods.Shared.Models;

/// <summary>
/// Abstract base class for payment method-related parameters, providing common properties.
/// </summary>
public abstract record PaymentMethodParameters : INamedParameters, IActivatableParameters, ISortableParameters
{
    /// <summary>Gets or sets the display name of the payment method.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>Gets or sets the optional unique code for the payment method.</summary>
    public string? Code { get; init; }

    /// <summary>Gets or sets the optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the gateway provider key (e.g., stripe, bogus).</summary>
    public string ProviderKey { get; init; } = string.Empty;

    /// <summary>Gets or sets the encrypted provider settings.</summary>
    public Dictionary<string, string>? Settings { get; init; }

    /// <summary>Gets or sets the non-sensitive behavioral preferences.</summary>
    public Dictionary<string, string>? Preferences { get; init; }

    /// <summary>Gets or sets whether webhooks are enabled for this payment method.</summary>
    public bool WebhookEnabled { get; init; }

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

    bool IActivatableParameters.IsActive { get => Active; init => Active = value; }
}