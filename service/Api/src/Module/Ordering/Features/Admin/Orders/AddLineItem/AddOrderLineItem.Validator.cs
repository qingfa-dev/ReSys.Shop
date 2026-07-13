using Module.Ordering.Domain.LineItems;

namespace Module.Ordering.Features.Admin.Orders.AddLineItem;

public static partial class AddOrderLineItem
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.OrderId)
                .NotEmpty()
                .WithErrorCode("Order.Id.Required")
                .WithMessage("Order ID is required.");

            RuleFor(x => x.Request)
                .NotNull()
                .WithErrorCode("Order.Request.Required")
                .WithMessage("Request body is required.");

            When(x => x.Request is not null, () =>
            {
                RuleFor(x => x.Request!.VariantId)
                    .NotEmpty()
                    .WithErrorCode("Order.VariantId.Required")
                    .WithMessage("Variant ID is required.");

                RuleFor(x => x.Request!.Quantity)
                    .InclusiveBetween(1, LineItemConstant.MaxQuantity)
                    .WithErrorCode("Order.Quantity.Invalid")
                    .WithMessage($"Quantity must be between 1 and {LineItemConstant.MaxQuantity}.");

                RuleFor(x => x.Request!.Price)
                    .GreaterThanOrEqualTo(0)
                    .WithErrorCode("Order.Price.Invalid")
                    .WithMessage("Price must be non-negative.");
            });
        }
    }
}
