using Module.Payment.Features.Storefront.Payment.Shared.Mappings;

using Module.Ordering.Domain.Orders;
using IGatewayRegistry = Module.Payment.Services.Provider.IGatewayRegistry;

using Module.Payment.Services.Models;
using Module.Payment.Domain.PaymentCaptures;

namespace Module.Payment.Features.Storefront.Payment.Confirm;

public static partial class ConfirmPayment
{
    public sealed record Command(Guid PaymentId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ICurrentUser currentUser,
        IGatewayRegistry gatewayRegistry)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Current user must own the order
            if (!Guid.TryParse(currentUser.UserId, out var userId))
                return PaymentCaptureResult.Failure.NotFound;

            // Load: Payment capture by ID
            var payment = await dbContext.Set<PaymentCapture>()
                .FirstOrDefaultAsync(p => p.Id == command.PaymentId, cancellationToken);
            if (payment is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Load: Order — verify ownership
            var order = await dbContext.Set<Order>()
                .FirstOrDefaultAsync(o => o.Id == payment.OrderId && o.UserId == userId, cancellationToken);
            if (order is null)
                return PaymentCaptureResult.Failure.NotFound;

            // Check: Payment must be in Processing or Pending state to confirm
            if (payment.State is not (PaymentRecordState.Processing or PaymentRecordState.Pending))
            {
                if (payment.State is PaymentRecordState.Completed)
                    return PaymentCaptureResult.Failure.AlreadyCompleted;
                return PaymentCaptureResult.Failure.InvalidStateTransition(payment.State, PaymentRecordState.Completed);
            }

            // Check: Gateway must be registered
            var gatewayResult = gatewayRegistry.GetGateway(payment.ProviderKey);
            if (gatewayResult.IsFailure)
                return PaymentCaptureResult.Failure.ProviderNotRegistered(payment.ProviderKey);
            var gateway = gatewayResult.Value;

            // Check: Response code (PaymentIntent ID) required for status check
            if (string.IsNullOrEmpty(payment.ResponseCode))
                return PaymentCaptureResult.Failure.NotSucceeded;

            // Call: Gateway status API — verify Stripe PaymentIntent succeeded
            var status = await gateway.GetPaymentStatusAsync(payment.ResponseCode, cancellationToken);
            if (status != GatewayConstants.Stripe.IntentStatus.Succeeded)
                return PaymentCaptureResult.Failure.NotSucceeded;

            // Update: Transition payment to Completed
            var completeResult = payment.Complete();
            if (completeResult.IsFailure) return completeResult.Errors;

            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Payment → storefront response DTO
            var response = payment.MapToStoreDetail<Response>();
            response.Message = completeResult.Message ?? "Payment confirmed.";
            return response;
        }
    }
}
