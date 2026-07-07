namespace Module.Promotions.Features.Admin.PromotionActions.Shared.Models;

public class PromotionActionDetailResponse : PromotionActionParameters
{
    public Guid Id { get; init; }
    public Guid PromotionId { get; init; }
    public DateTimeOffset CreatedAtUtc { get; init; }
}

public class PromotionActionListResponse : PromotionActionParameters
{
    public Guid Id { get; init; }
}
