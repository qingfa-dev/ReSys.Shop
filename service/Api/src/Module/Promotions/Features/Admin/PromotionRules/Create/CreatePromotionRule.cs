using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using Module.Promotions.Domain.PromotionRules;
using Module.Promotions.Domain.Promotions;
using Module.Promotions.Features.Admin.PromotionRules.Shared.Mappings;

namespace Module.Promotions.Features.Admin.PromotionRules.Create;

public static partial class CreatePromotionRule
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

            // Create: Build the rule entity using the domain factory.
            var result = PromotionRuleExtensions.Create(
                command.Request.Type,
                command.Request.Preferences,
                command.Request.PromotionId);

            if (result.IsFailure)
                return result.Failures;

            var rule = result.Value;

            // Persist: Add the rule to the database.
            dbContext.Set<PromotionRule>().Add(rule);
            await dbContext.SaveChangesAsync(cancellationToken);

            // Log: Record successful rule creation.
            PromotionRuleLoggers.Created(logger, rule.Type, rule.Id);

            // Map: Return the created rule as response.
            return Result<Response>.Created(
                rule.MapToDetail<Response>(),
                PromotionRuleResult.Success.Created(rule.Id));
        }
    }
}
