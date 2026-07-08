using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.Payments;

using PaymentRecord = Module.Payment.Domain.Payments.PaymentRecord;

namespace Module.Payment.Features.Admin.Payments.Refund;

    /// <summary>
    /// [WIP-MVP] Refunds the full captured amount. The optional `Amount` parameter is accepted
    /// for API compatibility but ignored. Partial refund is deferred to v1.x.
    /// </summary>
    public static partial class RefundPayment
{
    public sealed record Command(Guid Id, Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext, IPaymentGatewayActionProvider gateway)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            var payment = await dbContext.Set<PaymentRecord>()
                .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

            // Check: Verify the payment exists.
            if (payment is null)
                return PaymentResult.Failure.NotFound;

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
            var refundAmount = payment.Amount;

            // Refund: Attempt to refund via gateway.
            var refundResult = await payment.RefundAsync(gateway, options, refundAmount, cancellationToken);
            if (refundResult.IsFailure)
                return refundResult.Failures;

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken);

            return new Response
            {
                Id = payment.Id,
                Number = payment.Number,
                Amount = payment.Amount,
                RefundedAmount = command.Request.Amount,
                State = payment.State,
                Message = refundResult.Message ?? string.Empty
            };
        }
    }
}
