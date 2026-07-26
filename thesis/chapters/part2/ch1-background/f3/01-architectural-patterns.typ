=== E-commerce Platform Architectures

Modern web applications are shaped by three architectural patterns, each trading off simplicity, scalability, and operational cost.

==== Monolith

A monolith packages the user interface, business logic, and data access into a single deployable unit. Development is straightforward at small scale, but coupling accumulates with growth: changing the checkout flow requires understanding the catalog module, and any deployment redeploys the entire application. The monolith does not scale with team size or codebase age.

==== Microservices

Microservices decompose an application into independently deployable services, each owning a discrete business capability. Teams work in parallel using different technology stacks per service. The operational cost is high: service discovery, inter-service authentication, network latency, partial failure modes, and distributed transaction management. For a system whose primary contribution lies in machine learning integration, this overhead is disproportionate.

==== Modular Monolith

The modular monolith occupies the middle ground. Code is organised into *nine logically isolated business modules* within a *single process*. Compile-time boundaries prevent direct cross-module references, preserving bounded-context independence. The nine modules (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, Dashboard) communicate through an *in-process message bus* with no namespace-level dependencies between them. A shared PostgreSQL instance allows relational product data and vector embeddings to coexist within the same transactional boundary.

The machine learning capability is the one exception to the single-process rule. It runs as a dedicated *Python sidecar service*, isolated because PyTorch and the broader Python scientific stack have incompatible runtime requirements with .NET. The sidecar communicates with the main application over HTTP, exposing a narrow embedding-generation interface while keeping GPU resource contention isolated from the e-commerce API.

==== Architectural Decision

ReSys.Shop adopts the modular monolith with a machine learning sidecar. A single deployment process for the core application avoids service discovery, inter-service authentication, and distributed transaction orchestration. The Python sidecar runs as a separate process due to incompatible runtimes but exposes a narrow HTTP interface restricted to embedding generation; a GPU failure in the sidecar does not affect e-commerce API availability. A single PostgreSQL instance hosts both relational data and pgvector embeddings, ensuring catalog updates and index changes share the same transactional boundary and eliminating stale-index drift.

// Diagram placeholder: Three architecture patterns side-by-side (Mermaid)
// #figure(image("images/diagrams/arch-patterns.png", width: 90%), caption: [Monolith, modular monolith, and microservices compared.])
