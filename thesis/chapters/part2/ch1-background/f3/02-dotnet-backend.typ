=== .NET Backend

The backend is built on .NET 10, a high-performance runtime with ahead-of-time compilation and native asynchronous I/O @microsoft-aspnet-core. Its architecture is organised around five core libraries:

- *Carter* extends ASP.NET minimal APIs with module-based endpoint registration. Each business module declares its own routes independently, keeping endpoint definitions co-located with their handlers rather than centralised in a startup file.

- *MediatR* implements CQRS, routing commands (writes) and queries (reads) to handlers through an in-process message bus @young2010cqrs. Handlers are discovered at startup and dispatched by request type, with no direct coupling between modules.

- *Entity Framework Core* maps C\# domain objects to PostgreSQL tables, including pgvector column types for embedding storage @microsoft-ef-core. Migrations are version-controlled and applied at startup.

- *FluentValidation* enforces input rules at the application boundary. Each request type has an associated validator that runs before the handler executes, rejecting invalid data before it reaches business logic.

- *Vertical slice architecture* @bogard2018vertical (described in Section 1.5.1) organises each feature as a self-contained folder containing the handler, request, response, endpoint, and validator. This colocation keeps feature concerns together rather than spread across technical layers.
