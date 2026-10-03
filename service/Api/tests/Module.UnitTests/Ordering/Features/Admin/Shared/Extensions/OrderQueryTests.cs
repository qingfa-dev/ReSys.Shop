using System.Linq.Expressions;

using Module.Ordering.Domain.Orders;
using Module.Ordering.Features.Admin.Shared.Extensions;

namespace Module.UnitTests.Ordering.Features.Admin.Shared.Extensions;

[Trait("Category", "Unit")]
[Trait("Module", "Ordering")]
[Trait("Feature", "OrderQuery")]
public class OrderQueryTests : IDisposable
{
    private readonly ApplicationDbContext _dbContext;

    public OrderQueryTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        ApplicationDbContext.AdditionalConfigurationsAssemblies = [typeof(Order).Assembly];
        _dbContext = new ApplicationDbContext(options);
    }

    public void Dispose()
    {
        _dbContext.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact(DisplayName = "IncludeOrderDetail: Should reference all four detail collections")]
    public void IncludeOrderDetail_ShouldReferenceAllFourCollections()
    {
        var query = _dbContext.Set<Order>().IncludeOrderDetail();

        var includes = CollectIncludes(query.Expression);

        includes.Should().Contain(nameof(Order.LineItems));
        includes.Should().Contain(nameof(Order.Adjustments));
        includes.Should().Contain(nameof(Order.PaymentCaptures));
        includes.Should().Contain(nameof(Order.Shipments));
    }

    private static HashSet<string> CollectIncludes(Expression expression)
    {
        var includes = new HashSet<string>();
        Collect(expression, includes);
        return includes;
    }

    private static void Collect(Expression node, HashSet<string> includes)
    {
        switch (node)
        {
            case MethodCallExpression mce:
                if (mce.Method.Name is "Include" or "ThenInclude" && mce.Arguments.Count >= 2)
                {
                    if (TryGetNavigation(mce.Arguments[1], out var name))
                        includes.Add(name);
                }

                if (mce.Object is not null)
                    Collect(mce.Object, includes);
                foreach (var arg in mce.Arguments)
                    Collect(arg, includes);
                break;
            case UnaryExpression ue:
                Collect(ue.Operand, includes);
                break;
            case LambdaExpression le:
                Collect(le.Body, includes);
                break;
            case MemberExpression me when me.Expression is not null:
                Collect(me.Expression, includes);
                break;
        }
    }

    private static bool TryGetNavigation(Expression arg, out string name)
    {
        if (arg is UnaryExpression { NodeType: ExpressionType.Quote, Operand: LambdaExpression lambda }
            && lambda.Body is MemberExpression me)
        {
            name = me.Member.Name;
            return true;
        }

        name = null!;
        return false;
    }
}
