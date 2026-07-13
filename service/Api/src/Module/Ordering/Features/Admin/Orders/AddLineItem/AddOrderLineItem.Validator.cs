using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;

namespace Module.Ordering.Features.Admin.Orders.AddLineItem;

public static partial class AddOrderLineItem
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode(OrderResult.Errors.IdRequired.Code)
                .WithMessage(OrderResult.Errors.IdRequired.Message);

            RuleFor(x => x.Request)
                .NotNull()
                .WithErrorCode(OrderResult.Errors.RequestRequired.Code)
                .WithMessage(OrderResult.Errors.RequestRequired.Message);

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request!.VariantId)
                    .NotEmpty()
                    .WithErrorCode(OrderResult.Errors.VariantIdRequired.Code)
                    .WithMessage(OrderResult.Errors.VariantIdRequired.Message);

                RuleFor(x => x.Request!.Quantity)
                    .ApplyQuantityRules();

                RuleFor(x => x.Request!.Price)
                    .ApplyPriceRules();
            });
        }
    }
}