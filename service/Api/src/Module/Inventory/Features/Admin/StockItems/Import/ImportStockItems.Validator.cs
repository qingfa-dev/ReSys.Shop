namespace Module.Inventory.Features.Admin.StockItems.Import;

public static partial class ImportStockItems
{
    public sealed class Validator : AbstractValidator<Command>
    {
        public Validator()
        {
            RuleFor(x => x.File)
                .NotNull()
                .WithErrorCode("Import.File.Required")
                .WithMessage("CSV file is required.");
        }
    }
}
