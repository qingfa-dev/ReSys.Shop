using Module.Billing.Domain.PaymentMethods;
using PaymentRecord = Module.Billing.Domain.PaymentMethods.PaymentMethod;

namespace Module.Billing.Features.Admin.Shared.Mappings;

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

/// <summary>Provides mapping methods between PaymentMethod domain entities and response models.</summary>
public static partial class PaymentMethodMapping
{
    /// <summary>Maps a domain PaymentMethod to a detail response.</summary>
    /// <typeparam name="T">The response type, must inherit from PaymentMethodDetailResponse.</typeparam>
    /// <param name="entity">The domain entity.</param>
    /// <returns>The mapped response.</returns>
    public static T MapToDetail<T>(this PaymentRecord entity) where T : Models.PaymentMethodDetailResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Code = entity.Code,
            Description = entity.Description,
            ProviderKey = entity.ProviderKey ?? string.Empty,
            AutoCapture = entity.AutoCapture,
            DisplayOn = entity.DisplayOn,
            Position = entity.Position,
            Presentation = entity.Presentation,
            Active = entity.Active,
            Settings = entity.Settings,
            Preferences = entity.Preferences,
            WebhookEnabled = entity.WebhookEnabled,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
            CreatedBy = entity.CreatedBy,
            ModifiedBy = entity.ModifiedBy,
        };
    }

    /// <summary>Maps a domain PaymentMethod to a list item response.</summary>
    public static T MapToListItem<T>(this PaymentRecord entity) where T : Models.PaymentMethodListItemResponse, new()
    {
        return new T
        {
            Id = entity.Id,
            Name = entity.Name ?? string.Empty,
            Code = entity.Code,
            Description = entity.Description,
            ProviderKey = entity.ProviderKey ?? string.Empty,
            AutoCapture = entity.AutoCapture,
            DisplayOn = entity.DisplayOn,
            Position = entity.Position,
            Presentation = entity.Presentation,
            Active = entity.Active,
            Settings = entity.Settings,
            Preferences = entity.Preferences,
            WebhookEnabled = entity.WebhookEnabled,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }
}
