using Module.Catalog.Domain.Products.Variants;
using Module.Catalog.Domain.Products.Variants.Options;
using Module.Catalog.Features.Admin.Products.Variants.OptionValues.Assign;

namespace Module.UnitTests.Catalog.Features.Admin.Products.Variants.OptionValues.Assign;

[Trait("Category", "Unit")]
[Trait("Module", "Catalog")]
[Trait("Feature", "VariantOptionValues")]
public class AssignVariantOptionValuesTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly Mock<ILogger<AssignVariantOptionValues.CommandHandler>> _loggerMock;
    private readonly AssignVariantOptionValues.CommandHandler _handler;

    public AssignVariantOptionValuesTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Variant).Assembly];
        _dbContext = new ApplicationDbContext(options);

        _loggerMock = new Mock<ILogger<AssignVariantOptionValues.CommandHandler>>();

        _handler = new AssignVariantOptionValues.CommandHandler(_dbContext, _loggerMock.Object);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should assign option values to variant")]
    public async Task Handle_ShouldAssignOptionValues()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        _dbContext.Set<Variant>().Add(variant);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var optionValueId = Guid.NewGuid();
        var request = new AssignVariantOptionValues.Request { OptionValueIds = [optionValueId] };

        var result = await _handler.Handle(new AssignVariantOptionValues.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();

        var junctions = await _dbContext.Set<OptionValueVariant>()
            .Where(x => x.VariantId == variant.Id)
            .ToListAsync(TestContext.Current.CancellationToken);
        junctions.Should().HaveCount(1);
        junctions[0].OptionValueId.Should().Be(optionValueId);
    }

    [Fact(DisplayName = "Handler: Should skip already-assigned option values")]
    public async Task Handle_ShouldSkipAlreadyAssigned()
    {
        var variant = VariantExtensions.Create(Guid.NewGuid(), "SKU-001", isMaster: true).Value;
        var optionValueId = Guid.NewGuid();
        var existing = OptionValueVariantExtensions.Create(variant.Id, optionValueId).Value;
        _dbContext.Set<Variant>().Add(variant);
        _dbContext.Set<OptionValueVariant>().Add(existing);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var request = new AssignVariantOptionValues.Request { OptionValueIds = [optionValueId] };

        var result = await _handler.Handle(new AssignVariantOptionValues.Command(variant.Id, request), TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return failure when variant not found")]
    public async Task Handle_ShouldReturnFailure_WhenVariantNotFound()
    {
        var request = new AssignVariantOptionValues.Request { OptionValueIds = [Guid.NewGuid()] };

        var result = await _handler.Handle(new AssignVariantOptionValues.Command(Guid.NewGuid(), request), TestContext.Current.CancellationToken);

        result.IsFailure.Should().BeTrue();
        result.Errors[0].Code.Should().Be(VariantResult.Errors.NotFound(Guid.Empty).Code);
    }
}
