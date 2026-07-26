=== Architectural Patterns

An application's *architecture* defines how its code is divided into parts and how those parts communicate. Three patterns dominate modern web development, each trading off simplicity, scalability, and operational overhead.

==== Monolith

A monolith places all code -- user interface, business rules, and data access -- into a single program deployed as one unit. This is the simplest structure: one codebase, one build, one server to maintain. The drawback emerges with growth. Parts become intertwined: modifying checkout logic requires understanding the catalog module; deploying a payment fix restarts the entire application. Monoliths work well for small teams but do not scale with codebase size or developer count.

==== Microservices

Microservices divide the application into independent services, each owning one business capability (a payment service, an inventory service, and so on). Teams work in parallel and may use different programming languages per service. The trade-off is operational complexity: services must discover each other on the network, authenticate every request, and coordinate work spanning multiple services. When one service fails, others must degrade gracefully. For a project whose research contribution lies in machine learning integration rather than distributed systems, this overhead is unwarranted.

==== Modular Monolith

The modular monolith occupies the middle ground. Code is partitioned into *nine independent business modules* (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location, Dashboard), each handling one area of the e-commerce domain. All modules run within a *single process* and share one deployment, but the code enforces that no module references another's internal classes directly. Instead, modules communicate through an *in-process message bus*: a dispatcher that routes requests between modules without creating compile-time dependencies. This preserves independence without the networking cost of microservices.

A single PostgreSQL database hosts both relational business data and vector embeddings within the same transactional boundary. The one exception to single-process design is the machine learning capability. PyTorch requires Python; the backend requires .NET's CLR. The two cannot share a process. The solution is a dedicated *Python sidecar service*: a separate program running alongside the main application that handles only embedding generation. The sidecar communicates over HTTP. A GPU failure in the sidecar does not affect e-commerce API availability.

==== Architectural Decision

The platform adopts the modular monolith with a machine learning sidecar. A single process eliminates service discovery and distributed transactions. The Python sidecar confines AI workloads to a narrow HTTP interface, isolating them from transactional logic. A shared PostgreSQL database ensures catalog updates and vector index changes remain consistent within the same ACID boundary.

// Diagram placeholder: Three architecture patterns side-by-side (Mermaid)
// #figure(image("images/diagrams/arch-patterns.png", width: 90%), caption: [Monolith, modular monolith, and microservices compared.])
