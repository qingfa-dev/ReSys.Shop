using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Features.Admin.Products.Variants.OptionValues.Get;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.OptionValues.Get;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantOptionValues")]
public class GetVariantOptionValuesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetVariantOptionValues.QueryHandler _handler;

    public GetVariantOptionValuesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetVariantOptionValues.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return all option values with IsAssigned flag")]
    public async Task Handle_ShouldReturnAllOptionValues_WithIsAssigned()
    {
        var optionType = OptionTypeMethod.Create("Color", "Color").Value;
        var assignedValue = OptionValueMethod.Create(optionType.Id, "Red", "Red").Value;
        var unassignedValue = OptionValueMethod.Create(optionType.Id, "Blue", "Blue").Value;
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        var junction = OptionValueVariantMethod.Create(variant.Id, assignedValue.Id).Value;

        _dbContext.Set<OptionType>().Add(optionType);
        _dbContext.Set<OptionValue>().AddRange(assignedValue, unassignedValue);
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<OptionValueVariant>().Add(junction);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetVariantOptionValues.Query(variant.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(2);

        var assigned = result.Value.Items.Single(x => x.OptionValueId == assignedValue.Id);
        assigned.IsAssigned.Should().BeTrue();
        assigned.Name.Should().Be("Red");
        assigned.OptionTypeName.Should().Be("Color");

        var unassigned = result.Value.Items.Single(x => x.OptionValueId == unassignedValue.Id);
        unassigned.IsAssigned.Should().BeFalse();
        unassigned.Name.Should().Be("Blue");
    }

    [Fact(DisplayName = "Handler: Should return empty list when no option values exist")]
    public async Task Handle_ShouldReturnEmpty_WhenNoOptionValues()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetVariantOptionValues.Query(variant.Id), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return failure when variant not found")]
    public async Task Handle_ShouldReturnFailure_WhenVariantNotFound()
    {
        var result = await _handler.Handle(new GetVariantOptionValues.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotFound(Guid.Empty).Code);
    }
}
