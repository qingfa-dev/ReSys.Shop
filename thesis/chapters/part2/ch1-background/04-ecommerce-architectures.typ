== E-commerce Platform Architectures

Having covered how embeddings are generated and stored, this section examines the architectural patterns that organise the ReSys.Shop platform around these capabilities. Modern web applications are shaped by three architectural patterns, each trading off simplicity, scalability, and operational cost.

=== Monolith

A monolith packages the user interface, business logic, and data access into a single deployable unit. Development is straightforward: one codebase, one build pipeline, one deployment. At small scale this works well. As the system grows, subsystems accumulate coupling. Changing the checkout flow requires understanding the catalog module; deploying a payment fix means redeploying the entire application. The monolith does not scale with team size or codebase age.

=== Microservices

Microservices decompose an application into independently deployable services, each owning a discrete business capability. Teams can work in parallel using different technology stacks per service. The trade-off is operational complexity: service discovery, inter-service authentication, network latency, partial failure modes, and distributed transaction management. For a system where the primary research contribution lies in machine learning integration rather than infrastructure engineering, this overhead is disproportionate.

=== Modular Monolith

The modular monolith occupies the middle ground. Code is organised into logically isolated business modules within a single process. Compile-time boundaries prevent direct cross-module references, preserving the logical independence of bounded contexts. There is one build, one deployment, and one shared database. The nine business modules in ReSys.Shop (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, Dashboard) communicate through an in-process message bus with no namespace-level dependencies between them. A shared PostgreSQL instance allows relational product data and vector embeddings to coexist within the same transactional boundary, maintaining consistency between catalog updates and search index changes.

The machine learning capability is the one exception to the single-process rule. It runs as a dedicated Python sidecar service, isolated because PyTorch and the broader Python scientific stack have incompatible runtime requirements with .NET. The sidecar communicates with the main application over HTTP, exposing a narrow embedding-generation interface while keeping GPU resource contention isolated from the e-commerce API.

=== Architectural Decision

ReSys.Shop adopts the modular monolith with a machine learning sidecar. The decision is guided by three trade-offs:

- *Deployment.* One process for the core application avoids service discovery, inter-service authentication, and distributed transaction orchestration. The Python sidecar runs as a separate process because PyTorch and .NET have incompatible runtime environments, but the sidecar exposes a narrow HTTP interface restricted to embedding generation. A GPU failure in the sidecar does not affect e-commerce API availability.

- *Data consistency.* A single PostgreSQL instance hosts both relational product data and pgvector embeddings. Catalog updates and embedding index changes share the same transactional boundary, eliminating the class of stale-index bugs that arise when a vector store and relational database drift out of sync.

- *Module boundaries.* Nine business modules (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, Dashboard) are isolated by namespace convention within one assembly. Inter-module communication uses an in-process message bus. There are no direct cross-module references at compile time, preserving bounded-context independence without the operational cost of separate deployment units.

// Diagram placeholder: Three architecture patterns side-by-side (Mermaid)
// #figure(image("images/diagrams/arch-patterns.png", width: 90%), caption: [Monolith, modular monolith, and microservices compared.])
