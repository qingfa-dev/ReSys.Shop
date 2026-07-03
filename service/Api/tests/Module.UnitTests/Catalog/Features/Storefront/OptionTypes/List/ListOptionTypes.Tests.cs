using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Storefront.OptionTypes.Get.All;

namespace Module.UnitTests.Catalog.Features.Storefront.OptionTypes.Get.All;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "StorefrontOptionTypesList")]
public class ListOptionTypesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetAllOptionTypes.PagedQueryHandler _handler;

    public ListOptionTypesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(OptionType).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _handler = new GetAllOptionTypes.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return filterable option types with values")]
    public async Task Handle_ShouldReturnFilterableOptionTypes()
    {
        var color = new OptionType { Name = "Color", Presentation = "Color", Position = 1, Filterable = true };
        color.OptionValues.Add(new OptionValue { Name = "Red", Presentation = "Red", Position = 1, OptionType = color });
        color.OptionValues.Add(new OptionValue { Name = "Blue", Presentation = "Blue", Position = 2, OptionType = color });
        _dbContext.Set<OptionType>().Add(color);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetAllOptionTypes.Query(new GetAllOptionTypes.Parameters()), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(1);
        result.Items.First().Values.Should().HaveCount(2);
    }

    [Fact(DisplayName = "Handler: Should exclude non-filterable option types")]
    public async Task Handle_ShouldExcludeNonFilterable()
    {
        var material = new OptionType { Name = "Material", Presentation = "Material", Position = 1, Filterable = false };
        _dbContext.Set<OptionType>().Add(material);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(new GetAllOptionTypes.Query(new GetAllOptionTypes.Parameters()), TestContext.Current.CancellationToken);

        result.Items.Should().BeEmpty();
    }
}
