using Module.Catalog.Domain.OptionTypes;
using Module.Catalog.Features.Admin.OptionTypes.Create;

namespace Module.UnitTests.Catalog.Features.Admin.OptionTypes.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "OptionTypeCreate")]
public class CreateOptionTypeTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ICurrentUser> _currentUserMock;
    private readonly Mock<ILogger<CreateOptionType.CommandHandler>> _loggerMock;
    private readonly CreateOptionType.CommandHandler _handler;

    public CreateOptionTypeTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(OptionType).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _currentUserMock = new Mock<ICurrentUser>();
        _currentUserMock.Setup(x => x.UserName).Returns("admin");

        _loggerMock = new Mock<ILogger<CreateOptionType.CommandHandler>>();

        _handler = new CreateOptionType.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create option type successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        // Arrange
        var request = new CreateOptionType.Request
        {
            Name = "Color",
            Presentation = "Select Color",
            Position = 1,
            Filterable = true
        };

        // Act
        var result = await _handler.Handle(new CreateOptionType.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("Color");

        var persisted = await _dbContext.Set<OptionType>().FirstOrDefaultAsync(x => x.Name == "Color", cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Presentation.Should().Be("Select Color");
    }

    [Fact(DisplayName = "Handler: Should return failure when name is duplicate")]
    public async Task Handle_ShouldReturnFailure_WhenNameIsDuplicate()
    {
        // Arrange
        _dbContext.Set<OptionType>().Add(new OptionType { Name = "Color", Presentation = "Existing" });
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateOptionType.Request
        {
            Name = "Color",
            Presentation = "New"
        };

        // Act
        var result = await _handler.Handle(new CreateOptionType.Command(request), TestContext.Current.CancellationToken);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(OptionTypeResult.Failure.DuplicateName.Code);
    }
}
