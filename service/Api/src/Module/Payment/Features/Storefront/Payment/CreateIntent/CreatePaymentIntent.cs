using Module.Ordering.Domain.Orders;
using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.Payments;
using Module.Payment.Domain.PaymentMethods;

using PaymentDomain = Module.Payment.Domain.Payments.Payment;

namespace Module.Payment.Features.Storefront.Payment.CreateIntent;

    /// <summary>Handles CreatePaymentIntent feature.</summary>
    public static partial class CreatePaymentIntent
{
    public sealed record Command(Guid OrderId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IPaymentGatewayActionProvider gateway)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            // Check: Verify the order exists and belongs to current user.
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.NotFound(command.OrderId);

            // Query: Retrieve data from database.
            var order = await dbContext.Set<Order>()
                .FirstOrDefaultAsync(x => x.Id == command.OrderId && x.UserId == userId, cancellationToken);

            if (order is null)
                return OrderResult.Errors.NotFound(command.OrderId);

            // Check: Find a default payment method.
            var paymentMethod = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(cancellationToken);

            if (paymentMethod is null)
                return PaymentResult.Failure.NotFound;

            // Create: Build payment record.
            var createResult = Domain.Payments.PaymentFactory.Create(
                amount: order.Total,
                paymentMethodId: (Guid)paymentMethod.Id,
                orderId: order.Id);

            if (createResult.IsFailure)
                return createResult.Errors;

            var payment = createResult.Value;
            // Create: Persist new entity.
            dbContext.Set<PaymentDomain>().Add(payment);
            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            // Construct: Gateway options from order and payment data.
            var options = new GatewayOptions(payment)
            {
                Email = order.Email ?? string.Empty,
                StatementDescriptorSuffix = string.Empty,
                Customer = order.Email ?? string.Empty,
                CustomerId = currentUser.UserId,
                Ip = null,
                OrderId = $"{order.Number}-{payment.Number}",
                PaymentId = payment.Number,
                IdempotencyKey = $"spree-{payment.Number}",
            };

            // Process: Execute payment via gateway (authorize or purchase based on auto_capture).
            var processResult = await PaymentFactory.ProcessAsync(payment, gateway, options, cancellationToken);
            if (processResult.IsFailure)
                return processResult.Errors;

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = payment.Id,
                Amount = payment.Amount,
                Currency = order.Currency,
                OrderId = payment.OrderId,
                PaymentMethodId = payment.PaymentMethodId,
                State = payment.State.ToString(),
                ClientSecret = payment.IntentClientSecret,
                CreatedAtUtc = payment.CreatedAtUtc,
                ModifiedAtUtc = payment.ModifiedAtUtc,
                CreatedBy = payment.CreatedBy,
                ModifiedBy = payment.ModifiedBy
            };
        }
    }
}
