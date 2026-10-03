using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Storefront.ShippingMethods.Get;

namespace Module.UnitTests.Shipping.Features.Storefront.Shipping.Methods;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "GetShippingMethods")]
public class GetShippingMethodsHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetShippingMethods.PagedQueryHandler _handler;

    public GetShippingMethodsHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetShippingMethods.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return available methods when exist")]
    public async Task Handle_ShouldReturnAvailableMethods_WhenExist()
    {
        var method1 = ShippingMethodMethod.Create("Standard", "flat_rate").Value;
        method1.AvailableToUsers = true;
        method1.IsDeleted = false;
        var method2 = ShippingMethodMethod.Create("Express", "flat_rate").Value;
        method2.AvailableToUsers = true;
        method2.IsDeleted = false;
        var method3 = ShippingMethodMethod.Create("Hidden", "flat_rate").Value;
        method3.AvailableToUsers = false;
        method3.IsDeleted = false;

        _dbContext.Set<ShippingMethod>().AddRange(method1, method2, method3);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetShippingMethods.Query(new GetShippingMethods.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(m => m.Name.Should().BeOneOf("Standard", "Express"));
    }

    [Fact(DisplayName = "Handler: Should return empty when no available methods")]
    public async Task Handle_ShouldReturnEmpty_WhenNoAvailableMethods()
    {
        var method = ShippingMethodMethod.Create("Hidden", "flat_rate").Value;
        method.AvailableToUsers = false;
        method.IsDeleted = false;
        _dbContext.Set<ShippingMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetShippingMethods.Query(new GetShippingMethods.Parameters()),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should filter methods by delivery zone when country code supplied")]
    public async Task Handle_ShouldFilterByZone_WhenCountryCodeSupplied()
    {
        var usOnly = ShippingMethodMethod.Create("US Only", "flat_rate").Value;
        usOnly.AvailableToUsers = true;
        usOnly.IsDeleted = false;

        var worldwide = ShippingMethodMethod.Create("Worldwide", "flat_rate").Value;
        worldwide.AvailableToUsers = true;
        worldwide.IsDeleted = false;

        var vietnamOnly = ShippingMethodMethod.Create("VN Only", "flat_rate").Value;
        vietnamOnly.AvailableToUsers = true;
        vietnamOnly.IsDeleted = false;

        _dbContext.Set<ShippingMethod>().AddRange(usOnly, worldwide, vietnamOnly);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        _dbContext.Set<ShippingMethodZone>().AddRange(
            new ShippingMethodZone { ShippingMethodId = usOnly.Id, CountryCode = "US" },
            new ShippingMethodZone { ShippingMethodId = worldwide.Id, CountryCode = "*" },
            new ShippingMethodZone { ShippingMethodId = vietnamOnly.Id, CountryCode = "VN" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetShippingMethods.Query(new GetShippingMethods.Parameters { CountryCode = "us" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.Should().AllSatisfy(m => m.Name.Should().BeOneOf("US Only", "Worldwide"));
    }

    [Fact(DisplayName = "Handler: Should return empty when available methods no zones")]
    public async Task Handle_ShouldReturnEmpty_WhenAvailableMethodsNoZones()
    {
        var method = ShippingMethodMethod.Create("Standard", "flat_rate").Value;
        method.AvailableToUsers = true;
        method.IsDeleted = false;
        _dbContext.Set<ShippingMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new GetShippingMethods.Query(new GetShippingMethods.Parameters { CountryCode = "US" }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }
}