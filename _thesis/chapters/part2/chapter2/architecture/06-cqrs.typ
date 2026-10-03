==== Command Query Responsibility Segregation (CQRS)

The system distinguishes between operations that modify state and those that simply read it. This is implemented logically via MediatR:

- *Commands (Writes):* (e.g., `PlaceOrder`, `UpdateInventory`)
  - Intent: Change the system state.
  - Implementation: Loaded Aggregates including all child entities. Enforced by strong consistency and Transaction scopes.
  - Return: `ErrorOr<Result>` to handle domain failures explicitly.

- *Queries (Reads):* (e.g., `GetProductDetails`, `SearchProducts`)
  - Intent: Retrieve data for display.
  - Implementation: Optimized "Projection" queries. Can bypass the Domain Model (Entities) and project directly to DTOs using `Select()`.
  - Optimization: Uses `AsNoTracking()` in EF Core to avoid the overhead of change tracking.
