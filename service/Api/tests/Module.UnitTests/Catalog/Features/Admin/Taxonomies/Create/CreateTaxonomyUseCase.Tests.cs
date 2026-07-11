using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Features.Admin.Taxonomies.Create;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Create;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Create;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonomyCreate")]
public class CreateTaxonomyTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly CreateTaxonomy.CommandHandler _handler;

    public CreateTaxonomyTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxonomy).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _senderMock = new Mock<ISender>();
        _senderMock.Setup(x => x.Send(It.IsAny<CreateTaxon.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CreateTaxon.Response>.Ok(new CreateTaxon.Response { Id = Guid.NewGuid() }));

        _handler = new CreateTaxonomy.CommandHandler(_dbContext, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should create taxonomy successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var request = new CreateTaxonomy.Request
        {
            Name = "Categories",
            Presentation = "Product Categories",
            Position = 1
        };

        var result = await _handler.Handle(new CreateTaxonomy.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Name.Should().Be("categories");

        var persisted = await _dbContext.Set<Taxonomy>().FirstOrDefaultAsync(x => x.Name == "categories", cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Presentation.Should().Be("Product Categories");

        _senderMock.Verify(x => x.Send(
            It.Is<CreateTaxon.Command>(c =>
                c.TaxonomyId == persisted.Id &&
                c.Request.Name == "categories" &&
                c.Request.Presentation == "Product Categories" &&
                c.Request.Slug == "categories" &&
                c.Request.Position == 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should return failure when name is duplicate")]
    public async Task Handle_ShouldReturnFailure_WhenNameIsDuplicate()
    {
        var existing = TaxonomyExtensions.Create("Categories", "Existing", 0).Value;
        _dbContext.Set<Taxonomy>().Add(existing);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new CreateTaxonomy.Request
        {
            Name = "Categories",
            Presentation = "New"
        };

        var result = await _handler.Handle(new CreateTaxonomy.Command(request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.DuplicateName.Code);

        _senderMock.Verify(x => x.Send(It.IsAny<CreateTaxon.Command>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should create taxonomy when presentation is empty")]
    public async Task Handle_ShouldReturnSuccess_WhenPresentationIsEmpty()
    {
        var request = new CreateTaxonomy.Request
        {
            Name = "EmptyPresentation",
            Presentation = "",
            Position = 0
        };

        var result = await _handler.Handle(new CreateTaxonomy.Command(request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("emptypresentation");

        var persisted = await _dbContext.Set<Taxonomy>().FirstOrDefaultAsync(x => x.Name == "emptypresentation", cancellationToken: TestContext.Current.CancellationToken);
        persisted.Should().NotBeNull();
        persisted!.Presentation.Should().Be("");

        _senderMock.Verify(x => x.Send(
            It.Is<CreateTaxon.Command>(c => c.TaxonomyId == persisted.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
