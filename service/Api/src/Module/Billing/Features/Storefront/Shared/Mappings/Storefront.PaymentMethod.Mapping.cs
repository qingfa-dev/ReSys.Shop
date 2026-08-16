using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Storefront.Shared.Models;

namespace Module.Billing.Features.Storefront.Shared.Mappings;

// Map: PaymentMethod → storefront list item DTO
public static class PaymentMethodStoreMapping
{
    public static T MapToStoreListItem<T>(this PaymentMethod method) where T : StorePaymentMethodListItemResponse, new()
    {
        return new T
        {
            Id = method.Id,
            Name = method.Name,
            Code = method.Code,
            Description = method.Description,
            ProviderKey = method.ProviderKey,
            Preferences = method.Preferences,
            Active = method.Active,
            AutoCapture = method.AutoCapture,
            DisplayOn = method.DisplayOn,
            Position = method.Position,
            Presentation = method.Presentation,
            WebhookEnabled = false,
        };
    }
}
