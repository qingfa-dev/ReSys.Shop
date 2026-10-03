using Module.Catalog.Domain.Variants;
using Module.Catalog.Domain.Variants.Options;
using Module.Catalog.Features.Admin.Variants.Values.Sync;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.OptionValues.Sync;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantOptionValues")]
public class SyncVariantOptionValuesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<SyncVariantOptionValues.CommandHandler>> _loggerMock;
    private readonly SyncVariantOptionValues.CommandHandler _handler;

    public SyncVariantOptionValuesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<SyncVariantOptionValues.CommandHandler>>();

        _handler = new SyncVariantOptionValues.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should add new and remove stale option values")]
    public async Task Handle_ShouldAddAndRemove_WhenDiff()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        var staleId = Guid.NewGuid();
        var staleJunction = OptionValueVariantMethod.Create(variant.Id, staleId).Value;
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<OptionValueVariant>().Add(staleJunction);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var newId = Guid.NewGuid();
        var request = new SyncVariantOptionValues.Request { OptionValueIds = [newId] };

        var result = await _handler.Handle(new SyncVariantOptionValues.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var remaining = await _dbContext.Set<OptionValueVariant>()
            .Where(x => x.VariantId == variant.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        remaining.Should().HaveCount(1);
        remaining[0].OptionValueId.Should().Be(newId);
    }

    [Fact(DisplayName = "Handler: Should no-op when sets match")]
    public async Task Handle_ShouldNoOp_WhenIdentical()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        var optionValueId = Guid.NewGuid();
        var junction = OptionValueVariantMethod.Create(variant.Id, optionValueId).Value;
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<OptionValueVariant>().Add(junction);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncVariantOptionValues.Request { OptionValueIds = [optionValueId] };

        var result = await _handler.Handle(new SyncVariantOptionValues.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var remaining = await _dbContext.Set<OptionValueVariant>()
            .Where(x => x.VariantId == variant.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        remaining.Should().HaveCount(1);
    }

    [Fact(DisplayName = "Handler: Should remove all when empty request")]
    public async Task Handle_ShouldRemoveAll_WhenEmptyRequest()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        var junction = OptionValueVariantMethod.Create(variant.Id, Guid.NewGuid()).Value;
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<OptionValueVariant>().Add(junction);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new SyncVariantOptionValues.Request { OptionValueIds = [] };

        var result = await _handler.Handle(new SyncVariantOptionValues.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var remaining = await _dbContext.Set<OptionValueVariant>()
            .Where(x => x.VariantId == variant.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        remaining.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should return failure when variant not found")]
    public async Task Handle_ShouldReturnFailure_WhenVariantNotFound()
    {
        var request = new SyncVariantOptionValues.Request { OptionValueIds = [] };

        var result = await _handler.Handle(new SyncVariantOptionValues.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotFound(Guid.Empty).Code);
    }
}
