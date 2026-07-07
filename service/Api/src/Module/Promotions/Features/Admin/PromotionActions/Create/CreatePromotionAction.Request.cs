using Module.Promotions.Features.Admin.PromotionActions.Shared.Models;

namespace Module.Promotions.Features.Admin.PromotionActions.Create;

public static partial class CreatePromotionAction
{
    public class Request : PromotionActionRequest
    {
        public required Guid PromotionId { get; init; }
    }
}
