using DomainPaymentMethod = Module.Payment.Domain.PaymentMethods.PaymentMethod;

using Stripe;

namespace Module.Payment.Features.Storefront.Payment.SetupIntent;

/// <summary>Creates a Stripe SetupIntent for saving a payment method for future use.</summary>
public static partial class CreateSetupIntent
{
    public sealed record Command(Guid PaymentMethodId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Validates the payment method exists, then creates a Stripe SetupIntent for tokenization.</summary>
        /// <param name="command">The command containing the payment method ID to set up.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the Stripe client secret or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=paymentMethod!=null, post=clientSecret!=null, throws=DbUpdateException|StripeException
            // Load: Payment method
            var paymentMethod = await dbContext.Set<DomainPaymentMethod>()
                .FirstOrDefaultAsync(pm => pm.Id == command.PaymentMethodId && pm.Active && !pm.IsDeleted, cancellationToken);

            // Check: Verify the payment method exists.
            if (paymentMethod is null)
                return Domain.PaymentCaptures.PaymentCaptureResult.Failure.NotFound;

            // Call: Create Stripe SetupIntent
            try
            {
                var options = new SetupIntentCreateOptions
                {
                    Metadata = new Dictionary<string, string>
                    {
                        ["payment_method_id"] = paymentMethod.Id.ToString()
                    }
                };

                var setupIntent = await new SetupIntentService().CreateAsync(options, null, cancellationToken).ConfigureAwait(false);

                return new Response
                {
                    ClientSecret = setupIntent.ClientSecret
                };
            }
            // Boundary: Dynamic Stripe error — code and message are runtime values from StripeException, cannot be predefined.
            catch (StripeException ex)
            {
                var stripeError = ex.StripeError;
                return Error.BadRequest(
                    $"Stripe.{stripeError?.Code ?? "UnknownError"}",
                    stripeError?.Message ?? ex.Message);
            }
        }
    }
}
