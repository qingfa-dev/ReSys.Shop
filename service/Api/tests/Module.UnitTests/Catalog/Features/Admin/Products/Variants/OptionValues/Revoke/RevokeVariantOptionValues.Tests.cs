using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Features.Admin.Products.Variants.OptionValues.Revoke;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.OptionValues.Revoke;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantOptionValues")]
public class RevokeVariantOptionValuesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<RevokeVariantOptionValues.CommandHandler>> _loggerMock;
    private readonly RevokeVariantOptionValues.CommandHandler _handler;

    public RevokeVariantOptionValuesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<RevokeVariantOptionValues.CommandHandler>>();

        _handler = new RevokeVariantOptionValues.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should revoke assigned option values")]
    public async Task Handle_ShouldRevokeAssignedOptionValues()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        var optionValueId = Guid.NewGuid();
        var junction = OptionValueVariantMethod.Create(variant.Id, optionValueId).Value;
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<OptionValueVariant>().Add(junction);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeVariantOptionValues.Request { OptionValueIds = [optionValueId] };

        var result = await _handler.Handle(new RevokeVariantOptionValues.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var remaining = await _dbContext.Set<OptionValueVariant>()
            .Where(x => x.VariantId == variant.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        remaining.Should().BeEmpty();
    }

    [Fact(DisplayName = "Handler: Should no-op when option value not assigned")]
    public async Task Handle_ShouldNoOp_WhenNotAssigned()
    {
        var variant = VariantMethod.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new RevokeVariantOptionValues.Request { OptionValueIds = [Guid.NewGuid()] };

        var result = await _handler.Handle(new RevokeVariantOptionValues.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when variant not found")]
    public async Task Handle_ShouldReturnFailure_WhenVariantNotFound()
    {
        var request = new RevokeVariantOptionValues.Request { OptionValueIds = [Guid.NewGuid()] };

        var result = await _handler.Handle(new RevokeVariantOptionValues.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotFound(Guid.Empty).Code);
    }
}
