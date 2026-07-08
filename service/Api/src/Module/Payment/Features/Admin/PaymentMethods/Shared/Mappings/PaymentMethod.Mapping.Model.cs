using PaymentRecord = Module.Payment.Domain.PaymentMethods.PaymentMethod;

namespace Module.Payment.Features.Admin.PaymentMethods.Shared.Mappings;

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
            ProviderType = entity.ProviderType ?? string.Empty,
            AutoCapture = entity.AutoCapture,
            DisplayOn = entity.DisplayOn,
            Position = entity.Position,
            Presentation = entity.Presentation,
            Active = entity.Active,
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
            ProviderType = entity.ProviderType ?? string.Empty,
            AutoCapture = entity.AutoCapture,
            DisplayOn = entity.DisplayOn,
            Position = entity.Position,
            Presentation = entity.Presentation,
            Active = entity.Active,
            CreatedAtUtc = entity.CreatedAtUtc,
            ModifiedAtUtc = entity.ModifiedAtUtc,
        };
    }
}
