using Module.Inventory.Features.Admin.StockLocations.GetPaged;

namespace Module.UnitTests.Inventory.Features.Admin.StockLocations.GetPaged;

[Trait("Category", "Unit")]
[Trait("Module", "Inventory")]
[Trait("Feature", "StockLocationList")]
[Trait("Validator", "GetPagedStockLocations")]
public class GetPagedStockLocationsValidatorTests
{
    private readonly GetPagedStockLocations.Validator _validator = new();

    [Fact(DisplayName = "Validator: Should fail when parameters is null")]
    public void Validate_ShouldFail_WhenParametersIsNull()
    {
        var query = new GetPagedStockLocations.Query(null!);
        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.Parameters);
    }

    [Fact(DisplayName = "Validator: Should pass when parameters is valid")]
    public void Validate_ShouldPass_WhenParametersIsValid()
    {
        var query = new GetPagedStockLocations.Query(new QueryingParameters { PageSize = 10 });
        var result = _validator.TestValidate(query);

        result.ShouldNotHaveValidationErrorFor(x => x.Parameters);
    }

    [Fact(DisplayName = "Validator: Should pass with default parameters")]
    public void Validate_ShouldPass_WithDefaultParameters()
    {
        var query = new GetPagedStockLocations.Query(new QueryingParameters());
        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
