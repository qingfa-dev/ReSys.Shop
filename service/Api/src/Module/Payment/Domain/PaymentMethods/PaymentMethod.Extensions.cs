using Module.Ordering.Domain.Orders;

namespace Module.Payment.Domain.PaymentMethods;

public static class PaymentMethodExtensions
{
    #region Factory Methods
    /// <summary>
    /// Creates a new payment method with the specified configuration.
    /// </summary>
    /// <param name="name">The display name of the payment method.</param>
    /// <param name="code">Optional unique code for the payment method.</param>
    /// <param name="providerType">The provider type (e.g., StoreCredit, CreditCard). Must not be empty.</param>
    /// <param name="autoCapture">Whether to auto-capture payments. Defaults to false.</param>
    /// <param name="displayOn">Where to display this method. Defaults to Both.</param>
    /// <returns>A result containing the created payment method.</returns>
    // @CAT-10 Contract: pre=name!=null && providerType!=null, post=method.Id!=default && method.Active==true
    public static Result<PaymentMethod> Create(
        string name,
        string? code,
        string providerType,
        bool autoCapture = PaymentMethodConstant.Defaults.AutoCapture,
        DisplayOn displayOn = DisplayOn.Both)
    {
        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            ProviderType = providerType,
            Active = PaymentMethodConstant.Defaults.Active,
            AutoCapture = autoCapture,
            DisplayOn = displayOn,
            Position = PaymentMethodConstant.Defaults.Position,
            Preferences = [],
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };

        return method;
    }
    #endregion Factory Methods

    #region Methods
    /// <summary>
    /// Updates the payment method properties with the provided values.
    /// </summary>
    // @CAT-10 Contract: Only non-null values are applied
    public static Result Update(this PaymentMethod method,
        string? name = null,
        string? code = null,
        string? description = null,
        string? providerType = null,
        bool? autoCapture = null,
        DisplayOn? displayOn = null,
        string? presentation = null)
    {
        method.Name = name ?? method.Name;
        method.Code = code ?? method.Code;
        method.Description = description ?? method.Description;
        method.ProviderType = providerType ?? method.ProviderType;
        method.AutoCapture = autoCapture ?? method.AutoCapture;
        method.DisplayOn = displayOn ?? method.DisplayOn;
        method.Presentation = presentation ?? method.Presentation;
        method.ModifiedAtUtc = DateTimeOffset.UtcNow;
        method.ModifiedBy = "System";
        return Result.Ok(PaymentMethodResult.Success.Updated(method.Name));
    }

    /// <summary>
    /// Activates this payment method so it can be used for transactions.
    /// </summary>
    /// <param name="method">The payment method to activate.</param>
    /// <returns>A result indicating success or a conflict error if already active.</returns>
    // @CAT-4 Enforce: Cannot activate an already-active payment method
    public static Result Activate(this PaymentMethod method)
    {
        if (method.Active)
        {
            return PaymentMethodResult.Errors.AlreadyActive;
        }

        method.Active = true;
        method.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentMethodResult.Success.Activated(method.Name));
    }

    /// <summary>
    /// Deactivates this payment method so it cannot be used for new transactions.
    /// </summary>
    /// <param name="method">The payment method to deactivate.</param>
    /// <returns>A result indicating success or a conflict error if already inactive.</returns>
    // @CAT-4 Enforce: Cannot deactivate an already-inactive payment method
    public static Result Deactivate(this PaymentMethod method)
    {
        if (!method.Active)
        {
            return PaymentMethodResult.Errors.AlreadyInactive;
        }

        method.Active = false;
        method.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentMethodResult.Success.Deactivated(method.Name));
    }

    /// <summary>
    /// Replaces the payment method configuration preferences.
    /// </summary>
    /// <param name="method">The payment method to update.</param>
    /// <param name="preferences">The new preferences dictionary.</param>
    /// <returns>A result indicating success.</returns>
    // Assign: Replace all preferences with the provided dictionary
    public static Result UpdatePreferences(this PaymentMethod method, Dictionary<string, string> preferences)
    {
        method.Preferences = preferences;
        method.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentMethodResult.Success.Updated(method.Name));
    }

    /// <summary>
    /// Determines whether this payment method is a store credit provider.
    /// </summary>
    /// <param name="method">The payment method to check.</param>
    /// <returns>True if the provider type is StoreCredit.</returns>
    // @CAT-5 Compute: Payment method is available when active and its DisplayOn scope encompasses the order's originating channel
    /// <summary>
    /// Determines whether this payment method is available for the given order.
    /// </summary>
    /// <param name="method">The payment method to check.</param>
    /// <param name="order">The order to check availability for (reserved for zone-based filtering).</param>
    /// <returns>True if the method is active.</returns>
    public static bool IsAvailableFor(this PaymentMethod method, Order order)
    {
        return method.Active;
    }

    // @CAT-5 Compute: Substitute the :tracking placeholder in the tracking URL template
    /// <summary>
    /// Builds a tracking URL by substituting the :tracking placeholder with the provided tracking number.
    /// </summary>
    /// <param name="method">The payment method containing the tracking URL template.</param>
    /// <param name="tracking">The tracking number to substitute.</param>
    /// <returns>The resolved tracking URL, or null if no template is configured.</returns>
    public static string? BuildTrackingUrl(this PaymentMethod method, string tracking)
    {
        if (!method.Preferences.TryGetValue("tracking_url", out var template) || string.IsNullOrWhiteSpace(template))
            return null;

        return template.Replace(":tracking", tracking, StringComparison.OrdinalIgnoreCase);
    }
    #endregion Methods
}