using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Domain.OptionTypes.Values;
using Module.Catalog.Features.Admin.OptionTypes.Delete;

namespace Module.UnitTests.Catalog.Features.Admin.OptionTypes.Delete;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypeDelete")]
public class DeleteOptionTypeTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<DeleteOptionType.CommandHandler>> _loggerMock;
    private readonly DeleteOptionType.CommandHandler _handler;

    public DeleteOptionTypeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(OptionType).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _loggerMock = new Mock<ILogger<DeleteOptionType.CommandHandler>>();

        _handler = new DeleteOptionType.CommandHandler(_dbContext, _loggerMock.Object, _currentUserMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should delete option type successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var entity = new OptionType { Name = "DeleteMe", Presentation = "P" };
        _dbContext.Set<OptionType>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(new DeleteOptionType.Command(entity.Id), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();

        var persisted = await _dbContext.Set<OptionType>().FindAsync(new object?[] { entity.Id }, TestContext.Current.CancellationToken);
        persisted.Should().BeNull();
    }

    [Fact(DisplayName = "Handler: Should return NotFound when entity does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        // Act
        var result = await _handler.Handle(new DeleteOptionType.Command(Guid.NewGuid()), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Contain("NotFound");
    }

    [Fact(DisplayName = "Handler: Should return failure when option type has associated values")]
    public async Task Handle_ShouldReturnFailure_WhenHasValues()
    {
        // Arrange
        var result = OptionTypeMethod.Create("HasValues", "P");
        var entity = result.Value;
        entity.OptionValues.Add(OptionValueMethod.Create(entity.Id, "V1", "V1").Value);
        _dbContext.Set<OptionType>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var handleResult = await _handler.Handle(new DeleteOptionType.Command(entity.Id), TestContext.Current.CancellationToken);

        // Assert
        handleResult.IsFailure.Should().BeTrue();
        handleResult.Errors[0].Code.Should().Be(OptionTypeResult.Failure.CannotDeleteWithValues.Code);
    }
}
