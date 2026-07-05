using Module.Inventory.Features.Admin.StockTransfers.Shared.Models;

namespace Module.Inventory.Features.Admin.StockTransfers.Shared.Validators;

public class StockTransferRequestValidator : AbstractValidator<StockTransferRequest>
{
    public StockTransferRequestValidator()
    {
        RuleFor(x => x.SourceLocationId)
            .NotEmpty()
            .WithMessage("Source location is required.");

        RuleFor(x => x.DestinationLocationId)
            .NotEmpty()
            .WithMessage("Destination location is required.");

        RuleFor(x => x.SourceLocationId)
            .NotEqual(x => x.DestinationLocationId)
            .WithMessage("Source and destination locations must be different.");

        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("At least one transfer item is required.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.VariantId)
                .NotEmpty()
                .WithMessage("Variant is required.");

            item.RuleFor(i => i.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero.");
        });
    }
}
