using FluentValidation;
using FluentValidation.TestHelper;

using Module.Inventory.Features.Admin.StockItems.GetPaged;

namespace Module.UnitTests.Inventory.Features.Admin.StockItems.GetPaged;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockItemGetPaged")]
[Trait("Concern", "Validator")]
public class GetPagedStockItemsValidatorTests
{
    private readonly GetPagedStockItems.Validator _validator = new();

    [Fact(DisplayName = "Should fail when Parameters is null")]
    public void ShouldHaveError_WhenParametersNull()
    {
        var query = new GetPagedStockItems.Query(null!);
        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Parameters);
    }

    [Fact(DisplayName = "Should pass when Parameters is valid")]
    public void ShouldPass_WhenParametersValid()
    {
        var query = new GetPagedStockItems.Query(new GetPagedStockItems.Parameters
        {
            PageNumber = 1,
            PageSize = 10
        });
        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Parameters);
    }

}
