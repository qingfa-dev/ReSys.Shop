using Module.Shipping.Domain.ShippingMethods;
using Module.Shipping.Features.Admin.ShippingMethods.Update;
using Module.Shipping.Features.Admin.ShippingMethods.Shared.Models;

namespace Module.UnitTests.Shipping.Features.Admin.ShippingMethods.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Shipping")]
[Trait("Feature", "UpdateShippingMethod")]
public class UpdateShippingMethodHandlerTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<UpdateShippingMethod.CommandHandler>> _loggerMock;
    private readonly UpdateShippingMethod.CommandHandler _handler;

    public UpdateShippingMethodHandlerTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(ShippingMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _loggerMock = new Mock<ILogger<UpdateShippingMethod.CommandHandler>>();
        _handler = new UpdateShippingMethod.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update method when found")]
    public async Task Handle_ShouldUpdateMethod_WhenFound()
    {
        var method = ShippingMethodExtensions.Create("Original", "flat_rate").Value;
        _dbContext.Set<ShippingMethod>().Add(method);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateShippingMethod.Request
        {
            Name = "Updated Name",
            AdminName = "Updated Admin"
        };

        var result = await _handler.Handle(
            new UpdateShippingMethod.Command(method.Id, request),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Updated Name");

        var saved = await _dbContext.Set<ShippingMethod>()
            .FirstAsync(m => m.Id == method.Id, TestContext.Current.CancellationToken);
        saved.Name.Should().Be("Updated Name");
        saved.AdminName.Should().Be("Updated Admin");
    }

    [Fact(DisplayName = "Handler: Should return not found when missing")]
    public async Task Handle_ShouldReturnNotFound_WhenMissing()
    {
        var request = new UpdateShippingMethod.Request { Name = "Updated" };
        var result = await _handler.Handle(
            new UpdateShippingMethod.Command(Guid.NewGuid(), request),
            TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.FirstFailure!.Code.Should().Be("ShippingMethod.NotFound");
    }
}