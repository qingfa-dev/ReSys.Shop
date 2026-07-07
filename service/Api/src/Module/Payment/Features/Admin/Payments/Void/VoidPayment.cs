using Microsoft.EntityFrameworkCore;
using Module.Payment.Domain.Gateways;
using Module.Payment.Domain.Payments;
using PaymentDomain = Module.Payment.Domain.Payments.Payment;

namespace Module.Payment.Features.Admin.Payments.Void;

    /// <summary>Handles VoidPayment feature.</summary>
    public static partial class VoidPayment
{
    public sealed record Command(Guid Id) : ICommand<Response>;

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
            // Query: Get payment by ID.
            var payment = await dbContext.Set<PaymentDomain>()
                .FirstOrDefaultAsync(p => p.Id == command.Id, cancellationToken);

            // Check: Verify the payment exists.
            if (payment is null)
                return PaymentResult.Errors.NotFound;

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
                return voidResult.Failures;

            // Persist: Save changes.
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
