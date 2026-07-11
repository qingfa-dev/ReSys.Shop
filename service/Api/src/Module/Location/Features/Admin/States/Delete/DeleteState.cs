using Module.Location.Domain.States;
using Module.Location.Features.Admin.States.Shared.Mappings;

namespace Module.Location.Features.Admin.States.Delete;

/// <summary>Hard-deletes a state by its identifier.</summary>
public static partial class DeleteState
{
    public sealed record Command(Guid Id) : ICommand<Response>;

    public sealed class CommandHandler(
        IApplicationDbContext dbContext)
        : ICommandHandler<Command, Response>
    {
        /// <summary>Finds the state by ID and removes it from the database.</summary>
        /// <param name="command">The command containing the state identifier.</param>
        /// <param name="cancellationToken">Propagates cancellation signal.</param>
        /// <returns>A result containing the deleted state details or an error.</returns>
        /// <exception cref="DbUpdateException">Thrown when database persistence fails.</exception>
        public async Task<Result<Response>> Handle(Command command, CancellationToken cancellationToken)
        {
            // Contract: pre=state!=null, post=state removed, throws=DbUpdateException
            // Load: Find the state by identifier.
            var state = await dbContext.Set<State>()
                .FirstOrDefaultAsync(predicate: s => s.Id == command.Id, cancellationToken: cancellationToken);

            if (state is null)
                return StateResult.Failure.NotFound;

            // Remove: Delete the state from the database.
            dbContext.Set<State>().Remove(entity: state);

            await dbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            // Map: Return the deleted state as response.
            return state.MapToListItem<Response>();
        }
    }
}