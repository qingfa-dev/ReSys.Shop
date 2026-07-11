using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Admin.PaymentMethods.Shared.Mappings;

namespace Module.Payment.Features.Admin.PaymentMethods.Create;

/// <summary>Creates a new payment method and persists it to the database.</summary>
public static partial class CreatePaymentMethod
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Creates a payment method from the request and persists it.</summary>
        /// <param name="command">The command containing payment method data.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the created payment method details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null && command.Request!=null, post=paymentMethod!=null, throws=DbUpdateException
            var request = command.Request;

            // Create: Map the request to a new PaymentMethod entity.
            var createResult = request.MapToDomain();
            if (createResult.IsFailure)
                return createResult.Errors;

            var method = createResult.Value;

            // Update: Set additional properties after creation.
            method.Description = request.Description;

            dbContext.Set<PaymentMethod>().Add(method);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the created entity as response.
            return method.MapToDetail<Response>();
        }
    }
}
