using Module.Catalog.Domain.Products.Variants;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Billing.Domain.PaymentMethods;
using Module.Billing.Domain.PaymentCaptures;

using Module.Customer.Domain;
using Module.Customer.Domain.Addresses;
using Module.Shipping.Domain.ShippingMethods;

using Shared.Security.Identity.Domain.Users;

namespace Module.Ordering.Persistence.Seeders;

// Initialize: Seed sample orders with line items, payments, and shipping for development and testing
public sealed class OrderSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 190;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        // Check: Skip seeding if order data already exists
        var hasData = await HasDataAsync<Order>(cancellationToken);
        if (hasData)
            return Result.Ok();

        // Acquire: Fetch reference data needed for order creation (users, variants, addresses, shipping, payment)
        var users = await Context.Set<User>().ToListAsync(cancellationToken);
        var variants = await Context.Set<Variant>().Where(v => !v.IsDeleted).ToListAsync(cancellationToken);
        var addresses = await Context.Set<Address>().ToListAsync(cancellationToken);
        var shippingMethod = await Context.Set<ShippingMethod>().FirstOrDefaultAsync(sm => sm.Code == "standard", cancellationToken);
        var creditCard = await Context.Set<PaymentMethod>().FirstOrDefaultAsync(pm => pm.Code == "credit_card", cancellationToken);

        // Validate: Skip seeding if required reference data is missing
        if (users.Count == 0 || variants.Count == 0 || addresses.Count == 0 || shippingMethod is null || creditCard is null)
            return Result.Ok();

        var storeId = Guid.Empty;

        var admin = users.FirstOrDefault(u => u.Email == "admin@resys.shop");
        var user1 = users.FirstOrDefault(u => u.Email == "user1@resys.shop");
        var user2 = users.FirstOrDefault(u => u.Email == "user2@resys.shop");
        if (admin is null || user1 is null || user2 is null)
            return Result.Ok();

        // Create: Seed orders for admin and test users with randomized line items and full payment lifecycle
        await CreateOrder(admin, "DICKY", "DY", shippingMethod, creditCard, storeId, addresses, variants, cancellationToken);
        await CreateOrder(user1, "USER1", "U1", shippingMethod, creditCard, storeId, addresses, variants, cancellationToken);
        await CreateOrder(user2, "USER2", "U2", shippingMethod, creditCard, storeId, addresses, variants, cancellationToken);

        await SaveChangesWithIdempotencyAsync(cancellationToken);

        return Result.Ok();
    }

    private async Task CreateOrder(
        User user,
        string firstName,
        string initials,
        ShippingMethod shippingMethod,
        PaymentMethod creditCard,
        Guid storeId,
        List<Address> addresses,
        List<Variant> variants,
        CancellationToken ct)
    {
        var profile = await Context.Set<UserProfile>().FirstOrDefaultAsync(up => up.UserId == user.Id, ct);
        if (profile is null)
            return;

        var address = addresses.FirstOrDefault(a => a.UserProfileId == profile.Id);
        if (address is null)
            return;

        var orderResult = OrderMethod.Create(OrderConstant.Defaults.Currency, user.Id, storeId);
        var order = orderResult.Value;
        order.BillAddressId = address.Id;
        order.ShipAddressId = address.Id;
        order.ShippingMethodId = shippingMethod.Id;

        var eligibleVariants = variants.Where(v => !v.IsMaster).ToList();
        if (eligibleVariants.Count == 0)
            return;

        var rng = new Random(initials.GetHashCode());
        int itemCount = rng.Next(2, 4);

        for (int i = 0; i < itemCount && i < eligibleVariants.Count; i++)
        {
            var variant = eligibleVariants[i % eligibleVariants.Count];
            int qty = rng.Next(1, 3);
            var lineResult = LineItemMethod.Create(order.Id, variant.Id, qty, variant.Price.GetValueOrDefault());
            if (lineResult.IsSuccess)
            {
                order.LineItems.Add(lineResult.Value);
            }
        }

        if (order.LineItems.Count == 0)
            return;

        order.ItemTotal = order.LineItems.Sum(li => li.Total);
        order.Total = order.ItemTotal;
        order.OutstandingBalance = order.Total;

        var finalizeResult = order.Finalize();
        if (finalizeResult.IsFailure)
            return;

        var paymentResult = PaymentCaptureMethod.Create(order.Total, creditCard.Id, order.Id);
        if (paymentResult.IsFailure)
            return;
        var payment = paymentResult.Value;
        if (payment.Process().IsFailure)
            return;
        if (payment.Complete().IsFailure)
            return;
        order.PaymentTotal = payment.Amount;
        order.OutstandingBalance = order.Total - order.PaymentTotal;
        order.UpdatePaymentState(); // Result unused — seeder writes domain state directly

        Context.Set<Order>().Add(order);
    }
}