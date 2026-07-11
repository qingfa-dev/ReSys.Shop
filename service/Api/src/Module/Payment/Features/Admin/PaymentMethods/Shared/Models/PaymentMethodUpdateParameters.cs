using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Features.Admin.PaymentMethods.Shared.Models;

/// <summary>
/// Abstract base class for payment method update parameters, with all-nullable properties for PATCH semantics.
/// </summary>
public abstract class PaymentMethodUpdateParameters
{
    /// <summary>Gets or sets the display name of the payment method.</summary>
    public string? Name { get; init; }

    /// <summary>Gets or sets the optional unique code.</summary>
    public string? Code { get; init; }

    /// <summary>Gets or sets the optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets or sets the gateway provider key.</summary>
    public string? ProviderKey { get; init; }

    /// <summary>Gets or sets whether auto-capture is enabled.</summary>
    public bool? AutoCapture { get; init; }

    /// <summary>Gets or sets where to display this method.</summary>
    public DisplayOn? DisplayOn { get; init; }

    /// <summary>Gets or sets the display order position.</summary>
    public int? Position { get; init; }

    /// <summary>Gets or sets the display presentation text.</summary>
    public string? Presentation { get; init; }

    /// <summary>Gets or sets whether the payment method is active.</summary>
    public bool? Active { get; init; }
}
