=== Architectural Patterns

An application's *architecture* defines how code is divided into parts and how those parts communicate. Three patterns govern system-level partitioning; a fourth pattern, *vertical slice architecture*, governs organisation within each part. This section explains each pattern, when it applies, and its trade-offs.

==== Monolith

In a monolith, all code (user interface, business rules, data access) lives in one program deployed as a single unit.

- *When to use.* Small applications with a single team. The project has fewer than three distinct business domains. Rapid prototyping where deployment simplicity outweighs future scaling concerns.
- *Pros.* One codebase, one build pipeline, one deployment. No network calls between components: fast internal execution. Simple debugging with a single stack trace.
- *Cons.* Parts become entangled over time. A change in checkout logic requires understanding the catalog module. Any deployment restarts the entire application. Does not scale with team size or codebase age.

==== Microservices

Microservices split the application into independent services, each owning one business capability (a payment service, an inventory service, and so on). Services communicate over the network.

- *When to use.* Large organisations with multiple independent teams. The system has clearly separated business domains that evolve at different speeds. Scaling individual components independently is a requirement.
- *Pros.* Teams work in parallel with different technology stacks per service. Each service scales and deploys independently. A failure in the payment service does not take down the inventory service.
- *Cons.* High operational cost: service discovery, network authentication, distributed tracing, and partial failure handling. Transactions spanning services require complex coordination. Latency increases with every network hop. Infrastructure overhead is disproportionate for small-to-medium projects.

==== Modular Monolith

A modular monolith partitions code into independent business modules within a single process. Compile-time rules prevent modules from referencing each other's internal classes. Communication occurs through an in-process message bus rather than direct calls.

- *When to use.* A single team maintains multiple business domains. Deploying as one unit is preferred but logical separation is needed. The system requires transactional consistency across domains without distributed coordination.
- *Pros.* Logical isolation without networking cost: modules stay independent but function calls remain in-process. One deployment, one database, one transaction boundary. Refactoring into microservices later is possible because module boundaries already exist. Lower operational burden than microservices.
- *Cons.* The single database may become a bottleneck at extreme scale. A deployment restarts all modules together, even if only one changed. The entire application must be written in one technology stack (except for the ML sidecar, addressed below).

==== Vertical Slice Architecture

While the patterns above organise code *across* business domains, vertical slice architecture organises code *within* each domain. Instead of grouping code by technical layer (all controllers in one folder, all services in another), each feature is a self-contained folder containing everything it needs: the request handler, input validation, database access, and API endpoint definition.

- *When to use.* Applications with many small, independent features. Teams that add features frequently and want to minimise the risk of breaking existing ones. The domain has clear feature boundaries (e.g., "Create Product," "Search by Image," "Checkout").
- *Pros.* All code for one feature lives together: reading, modifying, or deleting a feature touches one folder. New features are added without altering existing code. Features can be implemented, tested, and shipped independently. The pattern scales with team size: each developer works within a single slice.
- *Cons.* Shared logic (e.g., authentication, logging) must be extracted into cross-cutting utilities, or it risks duplication across slices. Not every feature is cleanly separable: features that depend on shared workflows require explicit coordination between slices.

==== Architectural Decision

ReSys.Shop combines three complementary patterns:

- *System level.* Modular monolith. *Nine independent business modules* (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, Dashboard) run in a *single process*, communicate through an *in-process message bus*, and share one PostgreSQL database. This eliminates service discovery, distributed transactions, and network overhead while preserving module independence.

- *Feature level.* Vertical slice architecture. Each module organises its features as self-contained folders. For example, the Catalog module contains slices for CreateProduct, UpdateProduct, SearchByImage, and ViewProductDetails, each with its own handler, validation, and endpoint definition.

- *Machine learning.* Python sidecar. PyTorch requires Python; the backend requires .NET. A dedicated *sidecar service* runs as a separate process and handles only embedding generation. It communicates over HTTP and exposes a narrow interface. A GPU failure in the sidecar does not affect e-commerce API availability.

This combination provides the deployment simplicity of a monolith, the code isolation of microservices, and the machine learning capability that neither pattern alone would deliver on the platform's chosen technology stack.

// Diagram placeholder: Architecture patterns comparison and ReSys.Shop decision
// #figure(image("images/diagrams/arch-patterns.png", width: 90%), caption: [Monolith, modular monolith, microservices, and vertical slice compared.])
