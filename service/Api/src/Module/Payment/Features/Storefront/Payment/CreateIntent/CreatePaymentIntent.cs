using Module.Ordering.Domain.Orders;
using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;
using Module.Payment.Domain.PaymentMethods;

namespace Module.Payment.Features.Storefront.Payment.CreateIntent;

/// <summary>Creates a payment intent for an order and processes it through the payment gateway.</summary>
public static partial class CreatePaymentIntent
{
    public sealed record Command(Guid OrderId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IPaymentGatewayActionProvider gateway)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates the order and payment method, creates a payment record, and authorizes via gateway.</summary>
        /// <param name="command">The command containing the order ID for which to create the payment.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the payment intent details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=order!=null && paymentMethod!=null, post=payment.ClientSecret!=null, throws=DbUpdateException
            // Check: Verify the order exists and belongs to current user.
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return OrderResult.Errors.NotFound(command.OrderId);

            // Load: Retrieve data from database.
            var order = await dbContext.Set<Order>()
                .FirstOrDefaultAsync(x => x.Id == command.OrderId && x.UserId == userId, cancellationToken);

            if (order is null)
                return OrderResult.Errors.NotFound(command.OrderId);

            // Check: Find a default payment method.
            var paymentMethod = await dbContext.Set<PaymentMethod>()
                .FirstOrDefaultAsync(c => c.Active && !c.IsDeleted, cancellationToken);

            if (paymentMethod is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Create: Build payment record.
            var createResult = Domain.PaymentCaptures.PaymentCaptureMethod.Create(
                amount: order.Total,
                paymentMethodId: (Guid)paymentMethod.Id,
                orderId: order.Id);

            if (createResult.IsFailure)
                return createResult.Errors;

            var payment = createResult.Value;
            // Create: Persist new entity.
            dbContext.Set<PaymentCapture>().Add(payment);
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
            var processResult = await PaymentCaptureMethod.ProcessAsync(payment, gateway, options, cancellationToken);
            if (processResult.IsFailure)
                return processResult.Errors;

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
