using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Update;

namespace Module.UnitTests.Catalog.Features.Admin.OptionTypes.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypeUpdate")]
public class UpdateOptionTypeTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<UpdateOptionType.CommandHandler>> _loggerMock;
    private readonly UpdateOptionType.CommandHandler _handler;

    public UpdateOptionTypeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(OptionType).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _loggerMock = new Mock<ILogger<UpdateOptionType.CommandHandler>>();

        _handler = new UpdateOptionType.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update option type successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var entity = new OptionType { Name = "Old", Presentation = "Old Presentation" };
        _dbContext.Set<OptionType>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateOptionType.Request
        {
            Name = "New",
            Presentation = "New Presentation",
            Position = 10,
            Filterable = true
        };

        // Act
        var result = await _handler.Handle(new UpdateOptionType.Command(entity.Id, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("New");

        var persisted = await _dbContext.Set<OptionType>().FindAsync(new object?[] { entity.Id }, TestContext.Current.CancellationToken);
        persisted!.Name.Should().Be("New");
    }

    [Fact(DisplayName = "Handler: Should return NotFound when entity does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenEntityDoesNotExist()
    {
        // Arrange
        var request = new UpdateOptionType.Request { Name = "New" };

        // Act
        var result = await _handler.Handle(new UpdateOptionType.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Contain("NotFound");
    }

    [Fact(DisplayName = "Handler: Should return failure when name is duplicate")]
    public async Task Handle_ShouldReturnFailure_WhenNameIsDuplicate()
    {
        // Arrange
        _dbContext.Set<OptionType>().Add(new OptionType { Name = "Existing", Presentation = "P1" });
        var target = new OptionType { Name = "Target", Presentation = "P2" };
        _dbContext.Set<OptionType>().Add(target);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateOptionType.Request { Name = "Existing" };

        // Act
        var result = await _handler.Handle(new UpdateOptionType.Command(target.Id, request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OptionTypeResult.Failure.DuplicateName.Code);
    }
}
