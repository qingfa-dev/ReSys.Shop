using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.Promotions.Duplicate;

namespace Module.UnitTests.Promotions.Features.Admin.Promotions.Duplicate;

[Trait("Category", "Unit")]
[Trait("Module", "Promotions")]
[Trait("Feature", "DuplicatePromotion")]
public class DuplicatePromotionTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly DuplicatePromotion.CommandHandler _handler;

    public DuplicatePromotionTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Promotion).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new DuplicatePromotion.CommandHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should duplicate promotion with rules and actions")]
    public async Task Handle_ShouldDuplicatePromotion()
    {
        // Arrange
        var promotionId = Guid.NewGuid();
        var promotion = new Promotion
        {
            Id = promotionId,
            Name = "Original Sale",
            Code = "ORIGINAL",
            Description = "Original description",
            Kind = PromotionKind.Automatic,
            MatchPolicy = MatchPolicy.All,
            Active = true,
            Advertise = true,
            Position = 1,
            UsageLimit = 100,
            PerCustomerUsageLimit = 1,
            StartsAtUtc = DateTimeOffset.UtcNow,
            ExpiresAtUtc = DateTimeOffset.UtcNow.AddDays(30),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        promotion.PromotionRules.Add(new PromotionRule
        {
            Id = Guid.NewGuid(),
            Type = "Modules.Promotions.Domain.Rules.ItemTotalRule",
            Preferences = new Dictionary<string, string> { ["amount_min"] = "50" },
            PromotionId = promotionId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        promotion.PromotionActions.Add(new PromotionAction
        {
            Id = Guid.NewGuid(),
            Type = "Modules.Promotions.Domain.Actions.CreateAdjustmentAction",
            Preferences = new Dictionary<string, string> { ["percent"] = "10" },
            CalculatorType = "FlatPercentItemTotalCalculator",
            PromotionId = promotionId,
            CreatedAtUtc = DateTimeOffset.UtcNow
        });

        _dbContext.Set<Promotion>().Add(promotion);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        // Act
        var result = await _handler.Handle(
            new DuplicatePromotion.Command(promotionId),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().NotBe(promotionId);
        result.Value.Name.Should().Contain("Original Sale");
        result.Value.Active.Should().BeFalse(); // Duplicates start inactive

        // Verify: The duplicate was persisted.
        var duplicates = await _dbContext.Set<Promotion>()
            .Include(p => p.PromotionRules)
            .Include(p => p.PromotionActions)
            .Where(p => p.Id == result.Value.Id)
            .ToListAsync(TestContext.Current.CancellationToken);

        duplicates.Should().HaveCount(1);
        var duplicate = duplicates[0];
        duplicate.PromotionRules.Should().HaveCount(1);
        duplicate.PromotionActions.Should().HaveCount(1);
        duplicate.PromotionRules.First().Type.Should().Be("Modules.Promotions.Domain.Rules.ItemTotalRule");
        duplicate.PromotionActions.First().Type.Should().Be("Modules.Promotions.Domain.Actions.CreateAdjustmentAction");
    }

    [Fact(DisplayName = "Handler: Should return not found when source promotion does not exist")]
    public async Task Handle_ShouldReturnNotFound_WhenNotFound()
    {
        // Act
        var result = await _handler.Handle(
            new DuplicatePromotion.Command(Guid.NewGuid()),
            TestContext.Current.CancellationToken);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Failures.Should().Contain(f => f.Code == "Promotion.NotFound");
    }
}
