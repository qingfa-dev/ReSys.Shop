using DomainPaymentMethod = Module.Payment.Domain.PaymentMethods.PaymentMethod;

using Stripe;

namespace Module.Payment.Features.Storefront.Payment.SetupIntent;

    /// <summary>Handles CreateSetupIntent feature.</summary>
    public static partial class CreateSetupIntent
{
    public sealed record Command(Guid PaymentMethodId) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            // Query: Load payment method
            var paymentMethod = await dbContext.Set<DomainPaymentMethod>()
                .FirstOrDefaultAsync(pm => pm.Id == command.PaymentMethodId && pm.Active && !pm.IsDeleted, cancellationToken);

            // Check: Verify the payment method exists.
            if (paymentMethod is null)
                return Domain.Payments.PaymentResult.Failure.NotFound;

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
