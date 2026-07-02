namespace Shared.Operational.Persistence.Seeders;

/// <summary>
/// Defines a contract for database seeding operations.
/// </summary>
public interface IDataSeeder
{
    /// <summary>
    /// Executes the seeding logic.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A result indicating success or failure of the seeding operation.</returns>
    Task<Result> SeedAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets the execution order (lower numbers run first).
    /// </summary>
    int Order { get; }
}
