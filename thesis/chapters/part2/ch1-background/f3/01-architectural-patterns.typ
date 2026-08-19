=== Architectural Patterns

An application's architecture determines code partitioning and component communication @shaw2012software. @tbl-arch-patterns compares four patterns governing system-level structure.

#figure(
  table(
    columns: (auto, 1.2fr, 1.5fr, 1.5fr, 1fr),
    stroke: 0.5pt,
    align: (left, left, left, left, left),
    table.header([*Pattern*], [*Structure*], [*Strengths*], [*Limitations*], [*Suited For*]),
    [Monolith @newman2019monolith], [Single deployable unit; one codebase, build, and deployment], [No network overhead; simple development and debugging], [Components entangle over time; full restart on any deployment], [Small applications; one team; few business domains],
    [Microservices @lewis2014microservices], [Independent services each owning one business capability; network communication], [Autonomous teams with different stacks; independent scaling and deployment], [Network latency; service discovery; partial failure handling; distributed transaction coordination], [Large organisations; independently evolving teams and domains],
    [Modular Monolith @newman2019monolith], [Independent modules in a single process; in-process message bus; compile-time cross-module isolation], [Module independence without networking cost; one deployment and one database; simple migration path to microservices], [Single database may limit extreme-scale growth; all modules restart on any deployment], [Single-team projects requiring logical separation without distributed infrastructure],
    [VSA @bogard2018vertical], [Self-contained feature folders with handler, validation, data access, and endpoint definition], [All code for one feature co-located; features added, modified, or removed in isolation], [Cross-cutting concerns (authentication, logging) require explicit extraction across slices], [Numerous independent features evolving at different cadences],
  ),
    kind: table,
  caption: [Comparison of architectural patterns],
) <tbl-arch-patterns>

==== Architectural Decision

ReSys.Shop combines three patterns:

- *Modular monolith.* Nine business modules in a single .NET process communicate through MediatR CQRS @young2010cqrs. Compile-time rules prevent direct cross-module references.
- *Vertical slice architecture* within each module @bogard2018vertical. Each feature is a self-contained folder with handler, endpoint, and validator.
- *Python sidecar.* Embedding generation runs in a dedicated FastAPI service over HTTP, isolated from the .NET runtime.

This combines monolith deployment simplicity, microservice-level code isolation, and ML capability without distributed infrastructure overhead @newman2019monolith.

// Diagram placeholder: Architecture patterns comparison
// #figure(image("figures/chapters/arch-patterns.png", width: 90%), caption: [Monolith, modular monolith, microservices, and vertical slice compared.])
