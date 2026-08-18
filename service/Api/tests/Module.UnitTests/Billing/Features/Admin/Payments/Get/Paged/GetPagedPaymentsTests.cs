using Module.Billing.Domain.PaymentCaptures;
using Module.Billing.Features.Admin.Payments.Get.Paged;

namespace Module.UnitTests.Billing.Features.Admin.Payments.Get.Paged;

[Trait("Category", "Unit")]
[Trait("Module", "Billing")]
[Trait("Feature", "GetPagedPayments")]
public class GetPagedPaymentsTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;
    private readonly GetPagedPayments.PagedQueryHandler _handler;

    public GetPagedPaymentsTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(PaymentCapture).Assembly];
        _dbContext = new ApplicationDbContext(options);
        _handler = new GetPagedPayments.PagedQueryHandler(_dbContext);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "Handler: Should page payment captures with mapped state and payment status")]
    public async Task Handle_ShouldReturnPagedItems_WithMappedStateAndStatus()
    {
        var payments = SeedPayments();

        var result = await _handler.Handle(
            new GetPagedPayments.Query(new QueryingParameters { PageSize = 2 }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(3);

        var byId = result.Items.ToDictionary(i => i.Id);
        byId[payments[0].Id].State.Should().Be(PaymentRecordState.Checkout);
        byId[payments[0].Id].PaymentStatus.Should().Be("requires_payment_method");
        byId[payments[1].Id].State.Should().Be(PaymentRecordState.Processing);
        byId[payments[1].Id].PaymentStatus.Should().Be("processing");
    }

    [Fact(DisplayName = "Handler: Should return all payment captures when page size covers them")]
    public async Task Handle_ShouldReturnAll_WhenPageSizeCoversAll()
    {
        SeedPayments();

        var result = await _handler.Handle(
            new GetPagedPayments.Query(new QueryingParameters { PageSize = 10 }),
            TestContext.Current.CancellationToken);

        result.IsSuccess.Should().BeTrue();
        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Items.Should().OnlyContain(i => i.PaymentStatus != null);
        result.Items.Should().Contain(i => i.State == PaymentRecordState.Completed && i.PaymentStatus == "succeeded");
    }

    private List<PaymentCapture> SeedPayments()
    {
        var payments = new[]
        {
            PaymentCaptureMethod.Create(10m, Guid.NewGuid(), Guid.NewGuid()).Value,
            PaymentCaptureMethod.Create(25m, Guid.NewGuid(), Guid.NewGuid()).Value,
            PaymentCaptureMethod.Create(40m, Guid.NewGuid(), Guid.NewGuid()).Value
        };
        payments[0].PaymentStatus = "requires_payment_method";
        payments[1].State = PaymentRecordState.Processing;
        payments[1].PaymentStatus = "processing";
        payments[2].State = PaymentRecordState.Completed;
        payments[2].PaymentStatus = "succeeded";

        _dbContext.Set<PaymentCapture>().AddRange(payments);
        _dbContext.SaveChanges();

        return payments.ToList();
    }
}