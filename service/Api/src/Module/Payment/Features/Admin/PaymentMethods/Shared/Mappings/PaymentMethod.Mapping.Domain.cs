using Module.Payment.Domain.PaymentMethods;

using PaymentRecord = Module.Payment.Domain.PaymentMethods.PaymentMethod;

namespace Module.Payment.Features.Admin.PaymentMethods.Shared.Mappings;

/// <summary>Provides mapping methods from request models to PaymentMethod domain entities.</summary>
public static partial class PaymentMethodMapping
{
    /// <summary>Maps a request to a new PaymentMethod domain entity (create).</summary>
    public static Result<PaymentRecord> MapToDomain<T>(this T request) where T : Models.PaymentMethodRequest
    {
        return PaymentMethodMethod.Create(
            name: request.Name,
            code: request.Code,
            providerKey: request.ProviderKey,
            autoCapture: request.AutoCapture,
            displayOn: request.DisplayOn,
            settings: request.Settings);
    }

    /// <summary>Maps a request to an existing PaymentMethod domain entity (update).</summary>
    public static Result MapToDomain<T>(this T request, PaymentRecord method) where T : Models.PaymentMethodRequest
    {
        return method.Update(
            name: request.Name,
            code: request.Code,
            description: request.Description,
            providerKey: request.ProviderKey,
            autoCapture: request.AutoCapture,
            displayOn: request.DisplayOn,
            presentation: request.Presentation,
            settings: request.Settings,
            preferences: request.Preferences,
            webhookEnabled: request.WebhookEnabled);
    }

    /// <summary>Maps a partial-update request (PATCH) to an existing PaymentMethod domain entity.</summary>
    public static Result MapUpdateToDomain<T>(this T request, PaymentRecord method) where T : Models.PaymentMethodUpdateRequest
    {
        return method.Update(
            name: request.Name,
            code: request.Code,
            description: request.Description,
            providerKey: request.ProviderKey,
            autoCapture: request.AutoCapture,
            displayOn: request.DisplayOn,
            presentation: request.Presentation,
            settings: request.Settings,
            preferences: request.Preferences,
            webhookEnabled: request.WebhookEnabled);
    }
}