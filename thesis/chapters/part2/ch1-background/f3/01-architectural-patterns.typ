=== Architectural Patterns

An application's *architecture* determines how code is partitioned and how components communicate @shaw2012software. Three patterns govern system-level structure; a fourth, *vertical slice architecture*, organises code within each module.

==== Monolith

All code runs in a single deployable unit.

- Single codebase, build, and deployment; no network overhead between components.
- Components entangle over time: modifying one area requires understanding others, and any deployment restarts the entire application.

Suitable for small applications with one team and few business domains @newman2019monolith.

==== Microservices

Independent services each own one business capability, communicating over the network @lewis2014microservices.

- Teams operate autonomously with different technology stacks; services scale and deploy independently.
- Introduces service discovery, inter-service authentication, partial failure handling, and distributed transaction coordination. Each network hop adds latency.

Suitable for large organisations where teams and domains evolve independently.

==== Modular Monolith

Code partitions into independent modules within a single process. Modules communicate through an *in-process message bus*; compile-time rules prevent direct cross-module references @newman2019monolith.

- Preserves module independence without networking cost. One deployment and one database serve all modules.
- Established boundaries simplify future migration to microservices.
- The single database may limit extreme-scale growth; all modules restart on any deployment.

Suitable for single-team projects requiring logical separation without distributed infrastructure.

==== Vertical Slice Architecture

Each feature is a self-contained folder containing its handler, validation, data access, and endpoint definition @bogard2018vertical. This pattern is orthogonal to those above: it organises code *within* a module, not *across* the system.

- All code for one feature is co-located; features are added, modified, or removed in isolation.
- Cross-cutting concerns (authentication, logging) require explicit extraction to avoid duplication across slices.

Suitable when features are numerous, independent, and evolve at different cadences.

==== Architectural Decision

ReSys.Shop combines three patterns:

- *Modular monolith* at the system level. Nine business modules (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, Dashboard) run in a single .NET process. Modules communicate through MediatR, an in-process message bus implementing the CQRS pattern @young2010cqrs. Compile-time rules prevent direct cross-module references; all inter-module communication passes through commands and queries.

- *Vertical slice architecture* within each module @bogard2018vertical. Each feature is a self-contained folder containing its handler, request, response, endpoint, and validator. Features are added, modified, or removed in isolation without touching code in other slices.

- *Python sidecar* as the only cross-process component. Embedding generation runs in a dedicated FastAPI service, isolated from the .NET runtime. The sidecar communicates over HTTP; a GPU failure in the sidecar does not affect e-commerce API availability.

This combination provides the deployment simplicity of a monolith, the code isolation of microservices, and machine learning capability without distributed infrastructure overhead @newman2019monolith.

// Diagram placeholder: Architecture patterns comparison
// #figure(image("figures/chapters/arch-patterns.png", width: 90%), caption: [Monolith, modular monolith, microservices, and vertical slice compared.])
