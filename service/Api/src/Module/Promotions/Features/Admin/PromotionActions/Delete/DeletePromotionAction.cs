using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Module.Promotions.Domain.PromotionActions;

namespace Module.Promotions.Features.Admin.PromotionActions.Delete;

public static partial class DeletePromotionAction
{
    public sealed record Command(Guid PromotionId, Guid ActionId) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var action = await dbContext.Set<PromotionAction>()
                .FirstOrDefaultAsync(a => a.Id == command.ActionId && a.PromotionId == command.PromotionId, cancellationToken);

            if (action is null)
                return PromotionActionResult.Errors.NotFound(command.ActionId);

            dbContext.Set<PromotionAction>().Remove(action);
            await dbContext.SaveChangesAsync(cancellationToken);

            PromotionActionLoggers.Deleted(logger, action.Type, action.Id);

            return Result.Ok();
        }
    }
}
