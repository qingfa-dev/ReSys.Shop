using Shared.Application.Domain.Models;

namespace Module.Promotions.Domain.Actions;
/// <summary>Represents a Promotion Action Line Item.</summary>

public sealed class PromotionActionLineItem : Entity
{
    public Guid PromotionActionId { get; set; }
    public Guid VariantId { get; set; }
}