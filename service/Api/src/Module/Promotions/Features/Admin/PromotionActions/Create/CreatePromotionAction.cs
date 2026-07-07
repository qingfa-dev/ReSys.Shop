using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Module.Promotions.Domain.PromotionActions;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.PromotionActions.Shared.Mappings;

namespace Module.Promotions.Features.Admin.PromotionActions.Create;

public static partial class CreatePromotionAction
{
    public sealed record Command(Request Request) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command, Response>
    {
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Check: Verify the promotion exists.
            var promotion = await dbContext.Set<Promotion>()
                .FirstOrDefaultAsync(p => p.Id == command.Request.PromotionId, cancellationToken);

            if (promotion is null)
                return PromotionResult.Errors.NotFound(command.Request.PromotionId);

            // Create: Build the action entity using the domain factory.
            var result = PromotionActionExtensions.Create(
                command.Request.Type,
                command.Request.Preferences,
                command.Request.CalculatorType,
                command.Request.PromotionId);

            if (result.IsFailure)
                return result.Failures;

            var action = result.Value;

            // Persist: Add the action to the database.
            dbContext.Set<PromotionAction>().Add(action);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record successful action creation.
            PromotionActionLoggers.Created(logger, action.Type, action.Id);

            // Map: Return the created action as response.
            return Result<Response>.Created(
                action.MapToDetail<Response>(),
                PromotionActionResult.Success.Created(action.Id));
        }
    }
}
