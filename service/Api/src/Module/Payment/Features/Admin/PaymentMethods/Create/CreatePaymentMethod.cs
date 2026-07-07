using Module.Payment.Domain.PaymentMethods;
using Module.Payment.Features.Admin.PaymentMethods.Shared.Mappings;

namespace Module.Payment.Features.Admin.PaymentMethods.Create;

    /// <summary>Handles CreatePaymentMethod feature.</summary>
    public static partial class CreatePaymentMethod
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Handles the command.</summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The result of handling the command.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {

        // Contract: pre=command!=null, post=result!=null
            var request = command.Request;

            // Create: Map the request to a new PaymentMethod entity.
            var createResult = request.MapToDomain();
            if (createResult.IsFailure)
                return createResult.Failures;

            var method = createResult.Value;

            // Update: Set additional properties after creation.
            method.Description = request.Description;

            // Persist: Save the new entity to the database.
            dbContext.Set<PaymentMethod>().Add(method);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Map: Return the created entity as response.
            return method.MapToDetail<Response>();
        }
    }
}
