using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Module.Promotions.Domain.PromotionRules;

namespace Module.Promotions.Features.Admin.PromotionRules.Delete;

public static partial class DeletePromotionRule
{
    public sealed record Command(Guid PromotionId, Guid RuleId) : ICommand;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext,
        ILogger<CommandHandler> logger)
        : ICommandHandler<Command>
    {
        public async Task<Result> Handle(Command command, CancellationToken cancellationToken)
        {
            var rule = await dbContext.Set<PromotionRule>()
                .FirstOrDefaultAsync(r => r.Id == command.RuleId && r.PromotionId == command.PromotionId, cancellationToken);

            if (rule is null)
                return PromotionRuleResult.Errors.NotFound(command.RuleId);

            dbContext.Set<PromotionRule>().Remove(rule);
            await dbContext.SaveChangesAsync(cancellationToken);

            PromotionRuleLoggers.Deleted(logger, rule.Type, rule.Id);

            return Result.Ok();
        }
    }
}
