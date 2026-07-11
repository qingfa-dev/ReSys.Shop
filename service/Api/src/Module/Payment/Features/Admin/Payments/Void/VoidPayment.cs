using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Features.Admin.Payments.Void;

/// <summary>Voids a payment through the payment gateway, preventing settlement.</summary>
public static partial class VoidPayment
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, IPaymentGatewayActionProvider gateway)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Voids an authorized or pending payment via the configured gateway and persists the result.</summary>
        /// <param name="command">The command identifying the payment to void.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the voided payment details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=payment!=null && payment.CanVoid, post=payment.State==Void, throws=DbUpdateException
            // Load: Payment by ID.
            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

            // Check: Verify the payment exists.
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Construct: Gateway options from payment data.
            var options = new GatewayOptions(payment)
            {
                Email = string.Empty,
                StatementDescriptorSuffix = string.Empty,
                Customer = string.Empty,
                CustomerId = null,
                Ip = null,
                OrderId = payment.OrderId.ToString(),
                PaymentId = payment.Number,
                IdempotencyKey = $"spree-{payment.Number}",
            };

            // Void: Attempt to void the payment via gateway.
            var voidResult = await payment.VoidAsync(gateway, options, cancellationToken);
            if (voidResult.IsFailure)
                return voidResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return result.
            return new Response
            {
                Id = payment.Id,
                Number = payment.Number,
                Amount = payment.Amount,
                State = payment.State,
                Message = voidResult.Message ?? string.Empty
            };
        }
    }
}
