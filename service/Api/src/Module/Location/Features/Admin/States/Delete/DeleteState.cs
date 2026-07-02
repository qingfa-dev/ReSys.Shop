using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;

namespace Module.Location.Features.Admin.States.Delete;

/// <summary>Handles deletion of a state.</summary>
public static partial class DeleteState
{
    /// <summary>Command to delete a state.</summary>
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Executes the delete state command.</summary>
        /// <param name="command">The command containing the state identifier.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>A result containing the deleted state details.</returns>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=command!=null, post=result!=null
            // Check: Find the state by identifier.
            var state = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s => s.Id == command.Id, cancellationToken: cancellationToken);

            if (state is null)
                return StateResult.Failure.NotFound;

            // Remove: Delete the state from the database.
            dbContext.Set<State>().Remove(entity: state);

            // Persist: Save changes to the database.
            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            // Map: Return the deleted state as response.
            return state.MapToListItem<Response>();
        }
    }
}