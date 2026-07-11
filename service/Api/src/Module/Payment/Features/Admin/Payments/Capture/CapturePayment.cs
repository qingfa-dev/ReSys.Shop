using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Features.Admin.Payments.Capture;

/// <summary>Captures an authorized payment through the payment gateway.</summary>
public static partial class CapturePayment
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, IPaymentGatewayActionProvider gateway)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Captures a previously authorized payment via the configured gateway and persists the result.</summary>
        /// <param name="command">The command containing the payment ID and optional capture amount.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the captured payment details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=payment!=null && payment.CanCapture, post=payment.State==Completed, throws=DbUpdateException
            // Load: Payment by ID.
            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

            // Check: Verify the payment exists.
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            var captureAmount = command.Request.Amount ?? payment.UncapturedAmount();

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

            // Capture: Attempt to capture the payment via gateway.
            var captureResult = await payment.CaptureAsync(gateway, options, captureAmount, cancellationToken);
            if (captureResult.IsFailure)
                return captureResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return result.
            return new Response
            {
                Id = payment.Id,
                Number = payment.Number,
                Amount = payment.Amount,
                CapturedAmount = captureAmount,
                State = payment.State,
                Message = captureResult.Message ?? string.Empty
            };
        }
    }
}
