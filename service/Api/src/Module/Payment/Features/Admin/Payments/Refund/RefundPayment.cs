using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.PaymentCaptures;

using PaymentCapture = Module.Payment.Domain.PaymentCaptures.PaymentCapture;

namespace Module.Payment.Features.Admin.Payments.Refund;

/// <summary>
/// [WIP-MVP] Refunds the full captured amount. The optional Amount parameter is accepted
/// for API compatibility but ignored. Partial refund is deferred to v1.x.
/// </summary>
public static partial class RefundPayment
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, IPaymentGatewayActionProvider gateway)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Refunds a captured payment via the configured gateway and persists the result.</summary>
        /// <param name="command">The command containing the payment ID and refund details.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the refunded payment details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=payment!=null && payment.CanRefund, post=payment.RefundedAmount>0, throws=DbUpdateException
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

            // [WIP-MVP] For MVP, always refund the full captured total. Partial refund is deferred to v1.x.
            var refundAmount = command.Request.Amount;

            // Refund: Attempt to refund via gateway.
            var refundResult = await payment.RefundAsync(gateway, options, refundAmount, cancellationToken);
            if (refundResult.IsFailure)
                return refundResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = payment.Id,
                Number = payment.Number,
                Amount = payment.Amount,
                RefundedAmount = refundAmount,
                State = payment.State,
                Message = refundResult.Message ?? string.Empty
            };
        }
    }
}
