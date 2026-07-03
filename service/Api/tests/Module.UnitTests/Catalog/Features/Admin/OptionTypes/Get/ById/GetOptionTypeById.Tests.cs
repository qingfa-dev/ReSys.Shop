using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Get.ById;

namespace Module.UnitTests.Catalog.Features.Admin.OptionTypes.Get.ById;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypeGet")]
public class GetOptionTypeByIdTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetOptionTypeById.QueryHandler _handler;

    public GetOptionTypeByIdTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(OptionType).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetOptionTypeById.QueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should return option type when it exists")]
    public async Task Handle_ShouldReturnOptionType_WhenItExists()
    {
        // Arrange
        var entity = new OptionType { Name = "Size", Presentation = "P" };
        _dbContext.Set<OptionType>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new GetOptionTypeById.Query(entity.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(entity.Id);
        result.Value.Name.Should().Be("Size");
    }

    [Fact(DisplayName = "Handler: Should return NotFound when option type does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenItDoesNotExist()
    {
        // Act
        var result = await _handler.Handle(new GetOptionTypeById.Query(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Contain("NotFound");
    }
}
