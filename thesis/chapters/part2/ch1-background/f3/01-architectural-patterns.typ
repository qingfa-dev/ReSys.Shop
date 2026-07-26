=== Architectural Patterns

An application's *architecture* determines how code is partitioned and how components communicate. Three patterns govern system-level structure; a fourth, *vertical slice architecture*, organises code within each module.

==== Monolith

All code runs in a single deployable unit.

- Single codebase, build, and deployment; no network overhead between components.
- Components entangle over time: modifying one area requires understanding others, and any deployment restarts the entire application.

Suitable for small applications with one team and few business domains.

==== Microservices

Independent services each own one business capability, communicating over the network.

- Teams operate autonomously with different technology stacks; services scale and deploy independently.
- Introduces service discovery, inter-service authentication, partial failure handling, and distributed transaction coordination. Each network hop adds latency.

Suitable for large organisations where teams and domains evolve independently.

==== Modular Monolith

Code partitions into independent modules within a single process. Modules communicate through an *in-process message bus*; compile-time rules prevent direct cross-module references.

- Preserves module independence without networking cost. One deployment and one database serve all modules.
- Established boundaries simplify future migration to microservices.
- The single database may limit extreme-scale growth; all modules restart on any deployment.

Suitable for single-team projects requiring logical separation without distributed infrastructure.

==== Vertical Slice Architecture

Each feature is a self-contained folder containing its handler, validation, data access, and endpoint definition. This pattern is orthogonal to those above: it organises code *within* a module, not *across* the system.

- All code for one feature is co-located; features are added, modified, or removed in isolation.
- Cross-cutting concerns (authentication, logging) require explicit extraction to avoid duplication across slices.

Suitable when features are numerous, independent, and evolve at different cadences.

==== Architectural Decision

ReSys.Shop combines three patterns. At the system level, a *modular monolith* partitions nine business modules (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, Dashboard) into a single process with an *in-process message bus*. Within each module, *vertical slice architecture* organises features as self-contained folders. A dedicated *Python sidecar*, the only cross-process component, handles embedding generation to isolate PyTorch dependencies from the .NET runtime; a GPU failure in the sidecar does not affect e-commerce API availability. This combination provides the deployment simplicity of a monolith, the code isolation of microservices, and machine learning capability without distributed infrastructure overhead.

// Diagram placeholder: Architecture patterns comparison
// #figure(image("figures/chapters/arch-patterns.png", width: 90%), caption: [Monolith, modular monolith, microservices, and vertical slice compared.])
