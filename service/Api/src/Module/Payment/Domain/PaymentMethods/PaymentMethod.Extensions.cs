namespace Module.Payment.Domain.PaymentMethods;

public static class PaymentMethodExtensions
{
    #region Factory Methods
    public static Result<PaymentMethod> Create(
        string name,
        string? code,
        string providerKey,
        bool autoCapture = PaymentMethodConstant.Defaults.AutoCapture,
        DisplayOn displayOn = DisplayOn.Both,
        Dictionary<string, string>? settings = null,
        string? description = null)
    {
        var method = new PaymentMethod
        {
            Id = Guid.NewGuid(),
            Name = name,
            Code = code,
            Description = description,
            ProviderKey = providerKey,
            Active = PaymentMethodConstant.Defaults.Active,
            AutoCapture = autoCapture,
            DisplayOn = displayOn,
            Position = PaymentMethodConstant.Defaults.Position,
            Preferences = [],
            Settings = settings ?? [],
            CreatedAtUtc = DateTimeOffset.UtcNow,
            CreatedBy = "System"
        };
        return method;
    }
    #endregion Factory Methods

    #region Methods
    public static Result Update(this PaymentMethod method,
        string? name = null,
        string? code = null,
        string? description = null,
        string? providerKey = null,
        bool? autoCapture = null,
        DisplayOn? displayOn = null,
        string? presentation = null,
        Dictionary<string, string>? settings = null,
        Dictionary<string, string>? preferences = null,
        bool? webhookEnabled = null)
    {
        method.Name = name ?? method.Name;
        method.Code = code ?? method.Code;
        method.Description = description ?? method.Description;
        method.ProviderKey = providerKey ?? method.ProviderKey;
        method.AutoCapture = autoCapture ?? method.AutoCapture;
        method.DisplayOn = displayOn ?? method.DisplayOn;
        method.Presentation = presentation ?? method.Presentation;
        if (settings is not null) method.Settings = settings;
        if (preferences is not null) method.Preferences = preferences;
        if (webhookEnabled.HasValue) method.WebhookEnabled = webhookEnabled.Value;
        method.ModifiedAtUtc = DateTimeOffset.UtcNow;
        method.ModifiedBy = "System";
        return Result.Ok(PaymentMethodResult.Success.Updated(method.Name));
    }

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

    public static Result UpdatePreferences(this PaymentMethod method, Dictionary<string, string> preferences)
    {
        method.Preferences = preferences;
        method.ModifiedAtUtc = DateTimeOffset.UtcNow;

        return Result.Ok(PaymentMethodResult.Success.Updated(method.Name));
    }

    public static string? BuildTrackingUrl(this PaymentMethod method, string tracking)
    {
        if (!method.Preferences.TryGetValue("tracking_url", out var template) || string.IsNullOrWhiteSpace(template))
            return null;

        return template.Replace(":tracking", tracking, StringComparison.OrdinalIgnoreCase);
    }
    #endregion Methods
}
