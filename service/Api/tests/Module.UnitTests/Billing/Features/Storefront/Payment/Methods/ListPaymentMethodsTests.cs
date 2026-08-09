using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Features.Storefront.Payment.Methods;
using PaymentMethod = Module.Billing.Domain.PaymentMethods.PaymentMethod;

namespace Module.UnitTests.Payment.Features.Storefront.Payment.Methods;

[Trait("Category", "Unit")]
[Trait("Module", "Payment")]
[Trait("Feature", "ListPaymentMethods")]
public class ListPaymentMethodsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ListPaymentMethods.PagedQueryHandler _handler;

    public ListPaymentMethodsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentMethod).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new ListPaymentMethods.PagedQueryHandler(_dbContext);
    }

    public void Dispose() { _dbContext.Dispose(); GC.SuppressFinalize(this); }

    [Fact(DisplayName = "Handler: Should return only active, non-deleted payment methods")]
    public async Task Handle_ShouldReturnOnlyActiveMethods()
    {
        SeedPaymentMethods();

        var result = await _handler.Handle(
            new ListPaymentMethods.Query(new QueryingParameters { PageSize = 50 }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.Items.All(m => m.Name is "Active Card" or "Active Wallet").Should().BeTrue();
    }

    [Fact(DisplayName = "Handler: Should return empty when no active methods exist")]
    public async Task Handle_WhenNoActiveMethods_ShouldReturnEmpty()
    {
        _dbContext.Set<PaymentMethod>().Add(PaymentMethodMethod.Create(
            "Inactive Card", null, "stripe", true, DisplayOn.Both, null, null).Value);
        _dbContext.Set<PaymentMethod>().Add(PaymentMethodMethod.Create(
            "Deleted Card", null, "stripe", true, DisplayOn.Both, null, null).Value);
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var inactive = await _dbContext.Set<PaymentMethod>().FirstAsync(TestContext.Current.CancellationToken);
        inactive.Active = false;
        var deleted = await _dbContext.Set<PaymentMethod>().LastAsync(TestContext.Current.CancellationToken);
        deleted.IsDeleted = true;
        await _dbContext.SaveChangesAsync(TestContext.Current.CancellationToken);

        var result = await _handler.Handle(
            new ListPaymentMethods.Query(new QueryingParameters { PageSize = 50 }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().BeEmpty();
    }

    private void SeedPaymentMethods()
    {
        var active1 = PaymentMethodMethod.Create(
            "Active Card", "AC", "stripe", true, DisplayOn.Both, null, "Active credit card").Value;
        var active2 = PaymentMethodMethod.Create(
            "Active Wallet", "AW", "bogus", true, DisplayOn.Frontend, null, "Digital wallet").Value;
        var inactive = PaymentMethodMethod.Create(
            "Inactive Bank", "IB", "stripe", true, DisplayOn.Backend, null, null).Value;
        inactive.Active = false;
        var deleted = PaymentMethodMethod.Create(
            "Deleted Method", "DM", "stripe", true, DisplayOn.Both, null, null).Value;
        deleted.IsDeleted = true;

        _dbContext.Set<PaymentMethod>().AddRange(active1, active2, inactive, deleted);
        _dbContext.SaveChanges();
    }
}
