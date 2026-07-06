using MediatR;

using Module.Catalog.Domain.Taxonomies;
using Module.Catalog.Domain.Taxonomies.Taxons;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Create;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Restore;
using Module.Catalog.Features.Admin.Taxonomies.Taxons.Update;
using Module.Catalog.Features.Admin.Taxonomies.Update;

namespace Module.UnitTests.Catalog.Features.Admin.Taxonomies.Update;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "TaxonomyUpdate")]
public class UpdateTaxonomyTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ISender> _senderMock;
    private readonly UpdateTaxonomy.CommandHandler _handler;

    public UpdateTaxonomyTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Taxonomy).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _senderMock = new Mock<ISender>();
        _senderMock.Setup(x => x.Send(It.IsAny<CreateTaxon.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<CreateTaxon.Response>.Ok(new CreateTaxon.Response { Id = Guid.NewGuid() }));
        _senderMock.Setup(x => x.Send(It.IsAny<UpdateTaxon.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<UpdateTaxon.Response>.Ok(new UpdateTaxon.Response { Id = Guid.NewGuid() }));
        _senderMock.Setup(x => x.Send(It.IsAny<RestoreTaxon.Command>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Ok());

        _handler = new UpdateTaxonomy.CommandHandler(_dbContext, _senderMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should update taxonomy successfully")]
    public async Task Handle_ShouldReturnSuccess_WhenValid()
    {
        var entity = TaxonomyExtensions.Create("Old Name", "Old Presentation", 0).Value;
        _dbContext.Set<Taxonomy>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateTaxonomy.Request
        {
            Name = "New Name",
            Presentation = "New Presentation",
            Position = 5
        };

        var result = await _handler.Handle(new UpdateTaxonomy.Command(entity.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("new name");

        var persisted = await _dbContext.Set<Taxonomy>().FindAsync([entity.Id], TestContext.Current.CancellationToken);
        persisted!.Name.Should().Be("new name");
        persisted.Presentation.Should().Be("New Presentation");
        persisted.Position.Should().Be(5);
    }

    [Fact(DisplayName = "Handler: Should return failure when taxonomy not found")]
    public async Task Handle_ShouldReturnFailure_WhenNotFound()
    {
        var request = new UpdateTaxonomy.Request { Name = "New" };

        var result = await _handler.Handle(new UpdateTaxonomy.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.NotFound.Code);
    }

    [Fact(DisplayName = "Handler: Should return failure when new name is duplicate")]
    public async Task Handle_ShouldReturnFailure_WhenNameIsDuplicate()
    {
        var entity = TaxonomyExtensions.Create("Target", "Presentation", 0).Value;
        var other = TaxonomyExtensions.Create("Other", "Presentation", 0).Value;
        _dbContext.Set<Taxonomy>().AddRange(entity, other);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateTaxonomy.Request { Name = "Other" };

        var result = await _handler.Handle(new UpdateTaxonomy.Command(entity.Id, request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(TaxonomyResult.Errors.DuplicateName.Code);
    }

    [Fact(DisplayName = "Handler: Should create root taxon via sender when root not found")]
    public async Task Handle_ShouldCreateRootTaxon_WhenRootNotFound()
    {
        var entity = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        _dbContext.Set<Taxonomy>().Add(entity);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new UpdateTaxonomy.Request
        {
            Name = "Updated Name",
            Presentation = "Updated",
            Position = 2
        };

        var result = await _handler.Handle(new UpdateTaxonomy.Command(entity.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        _senderMock.Verify(x => x.Send(
            It.Is<CreateTaxon.Command>(c =>
                c.Request.Name == "updated name" &&
                c.Request.Presentation == "Updated" &&
                c.Request.Slug == "updated name" &&
                c.Request.Position == 0),
            It.IsAny<CancellationToken>()), Times.Once);

        _senderMock.Verify(x => x.Send(
            It.IsAny<RestoreTaxon.Command>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _senderMock.Verify(x => x.Send(
            It.IsAny<UpdateTaxon.Command>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact(DisplayName = "Handler: Should restore then update root taxon via sender when root is soft-deleted")]
    public async Task Handle_ShouldRestoreThenUpdateRoot_WhenRootIsDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var entity = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        _dbContext.Set<Taxonomy>().Add(entity);
        await _dbContext.SaveChangesAsync(ct);

        var root = TaxonMethod.Create(entity.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        root.Delete();
        _dbContext.Set<Taxon>().Add(root);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        var request = new UpdateTaxonomy.Request
        {
            Name = "Updated",
            Presentation = "Updated Display",
            Position = 2
        };

        var result = await _handler.Handle(new UpdateTaxonomy.Command(entity.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        _senderMock.Verify(x => x.Send(
            It.IsAny<CreateTaxon.Command>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _senderMock.Verify(x => x.Send(
            It.Is<RestoreTaxon.Command>(c => c.TaxonomyId == entity.Id && c.Id == root.Id),
            It.IsAny<CancellationToken>()), Times.Once);

        _senderMock.Verify(x => x.Send(
            It.Is<UpdateTaxon.Command>(c =>
                c.TaxonomyId == entity.Id &&
                c.Request.Name == "updated" &&
                c.Request.Presentation == "Updated Display" &&
                c.Request.Slug == "updated"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact(DisplayName = "Handler: Should update root taxon via sender when root is active")]
    public async Task Handle_ShouldUpdateRootTaxon_WhenRootIsActive()
    {
        var ct = TestContext.Current.CancellationToken;
        var entity = TaxonomyExtensions.Create("Categories", "Categories", 0).Value;
        var root = TaxonMethod.Create(entity.Id, null, "Root", "Root", null, 0, "root", null, null, null, false, null, null, false, null, null).Value;
        root.Lft = 1; root.Rgt = 2; root.Depth = 0;

        _dbContext.Set<Taxonomy>().Add(entity);
        _dbContext.Set<Taxon>().Add(root);
        await _dbContext.SaveChangesAsync(ct);
        _dbContext.ChangeTracker.Clear();

        var request = new UpdateTaxonomy.Request
        {
            Name = "Updated",
            Presentation = "Updated",
            Position = 3
        };

        var result = await _handler.Handle(new UpdateTaxonomy.Command(entity.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        _senderMock.Verify(x => x.Send(
            It.IsAny<CreateTaxon.Command>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _senderMock.Verify(x => x.Send(
            It.IsAny<RestoreTaxon.Command>(),
            It.IsAny<CancellationToken>()), Times.Never);

        _senderMock.Verify(x => x.Send(
            It.Is<UpdateTaxon.Command>(c =>
                c.TaxonomyId == entity.Id &&
                c.Request.Name == "updated" &&
                c.Request.Presentation == "Updated" &&
                c.Request.Slug == "updated"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
