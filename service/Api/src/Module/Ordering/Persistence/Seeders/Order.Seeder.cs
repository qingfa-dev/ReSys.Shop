using Module.Catalog.Domain.Products.Variants;
using Module.Ordering.Domain.LineItems;
using Module.Ordering.Domain.Orders;
using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Domain.Payments;

using Module.Profile.Domain;
using Module.Profile.Domain.Addresses;
using Module.Shipping.Domain.ShippingMethods;
using Shared.Security.Identity.Domain.Users;

namespace Module.Ordering.Persistence.Seeders;

public sealed class OrderSeeder(IApplicationDbContext context) : AbstractDataSeeder(context)
{
    public override int Order => 190;

    public override async Task<Result> SeedAsync(CancellationToken cancellationToken)
    {
        var hasData = await HasDataAsync<Order>(cancellationToken);
        if (hasData)
            return Result.Ok();

        var users = await Context.Set<User>().ToListAsync(cancellationToken);
        var variants = await Context.Set<Variant>().Where(v => !v.IsDeleted).ToListAsync(cancellationToken);
        var addresses = await Context.Set<Address>().ToListAsync(cancellationToken);
        var shippingMethod = await Context.Set<ShippingMethod>().FirstOrDefaultAsync(sm => sm.Code == "standard", cancellationToken);
        var creditCard = await Context.Set<PaymentMethod>().FirstOrDefaultAsync(pm => pm.Code == "credit_card", cancellationToken);

        if (users.Count == 0 || variants.Count == 0 || addresses.Count == 0 || shippingMethod is null || creditCard is null)
            return Result.Ok();

        var storeId = Guid.Empty;

        var admin = users.FirstOrDefault(u => u.Email == "admin@resys.shop");
        var user1 = users.FirstOrDefault(u => u.Email == "user1@resys.shop");
        var user2 = users.FirstOrDefault(u => u.Email == "user2@resys.shop");
        if (admin is null || user1 is null || user2 is null)
            return Result.Ok();

        await CreateOrder(admin, "DICKY", "DY", shippingMethod, creditCard, storeId, addresses, variants, cancellationToken);
        await CreateOrder(user1, "USER1", "U1", shippingMethod, creditCard, storeId, addresses, variants, cancellationToken);
        await CreateOrder(user2, "USER2", "U2", shippingMethod, creditCard, storeId, addresses, variants, cancellationToken);

        await Context.SaveChangesAsync(cancellationToken);

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

        var orderResult = OrderExtensions.Create("USD", user.Id, storeId);
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

        var paymentResult = PaymentFactory.Create(order.Total, creditCard.Id, order.Id);
        if (paymentResult.IsFailure)
            return;
        var payment = paymentResult.Value;
        if (payment.Process().IsFailure)
            return;
        if (payment.Complete().IsFailure)
            return;
        order.Payments.Add(payment);
        order.PaymentTotal = payment.Amount;
        order.OutstandingBalance = order.Total - order.PaymentTotal;
        order.UpdatePaymentState();

        Context.Set<Order>().Add(order);
    }
}
