== Scope and Delimitations

=== Problem Boundary

This thesis addresses the *dual contribution* of (a) architectural design and implementation of a fashion e-commerce platform, and (b) comparative evaluation of pretrained visual embedding models for CBIR. The boundary is drawn around *software engineering process evidence* and *ML model evaluation methodology* — analysis, design, implementation, and comparative evaluation — rather than operational deployment or business operations.

=== In-Scope Deliverables

#figure(
  caption: [In-scope deliverables and corresponding thesis chapters],
  table(
    columns: 3,
    align: (left, left, left),
    table.header(
      [*Deliverable*], [*Description*], [*Thesis Chapter*],
    ),
    [*Backend system*], [8 business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping) implemented as vertical slices with CQRS, MediatR, and explicit `Result<T>` error handling], [Chapters 3–7],
    [*Database design*], [PostgreSQL 17 + pgvector schema with per-module namespaces, vector embeddings for CBIR, EF Core migrations], [Chapter 5],
    [*ML sidecar*], [Python FastAPI service with *pluggable embedding model interface* supporting Fashion-CLIP, ResNet-50, EfficientNet-B0, and CLIP-generic; HTTP API consumed by Catalog module], [Chapters 3, 5, 7],
    [*ML model comparison*], [Comparative evaluation of 4 embedding models on retrieval effectiveness (Precision\@K, Recall\@K, mAP) and operational performance (latency, storage, memory)], [Chapter 11],
    [*Dual-channel frontends*], [Vue 3 Admin SPA (PrimeVue) for administrators; Vue 3 Storefront SPA (Nuxt UI) for customers], [Chapters 3, 6],
    [*Security stack*], [JWT bearer auth with rotation/reuse detection, permission-based authorization, rate limiting, anti-forgery, file upload guards], [Chapter 8],
    [*Testing strategy*], [Unit tests (InMemory EF + Moq), integration tests (Testcontainers + WebApplicationFactory), frontend unit tests (Vitest), Python tests (pytest)], [Chapter 10],
    [*Observability*], [OpenTelemetry traces/metrics/logs, correlation IDs, health checks, Hangfire background jobs], [Chapters 3, 9],
    [*Design documentation*], [This thesis document set: 11 chapters of analysis, design rationale, and evaluation], [All chapters],
  )
) <tab:in-scope>

=== Out-of-Scope (Explicitly Deferred)

#figure(
  caption: [Features explicitly deferred from thesis scope],
  table(
    columns: 4,
    align: (left, left, left, left),
    table.header(
      [*Feature*], [*Rationale*], [*Impact on Thesis*], [*Evidence*],
    ),
    [*YARP API Gateway*], [SPA→API direct calls are sufficient for thesis demonstration; gateway adds ops complexity without research value], [No architectural gap; API handles auth/rate limiting directly], [`infra/Aspire/src/ReSys.AppHost/AppHost.cs:5-7`],
    [*Azure Blob Storage Provider*], [S3 + Local providers cover all thesis file-storage needs], [Strategy pattern is demonstrated with 2 providers; adding a 3rd is mechanical], [`appsettings.json:163-168` vs `Storage.Extensions.cs:79-82`],
    [*Facebook / Microsoft OAuth*], [Google OAuth demonstrates external-login architecture; adding more providers is identical pattern], [No design gap; config blocks are disabled (`Enabled=false`)], [`appsettings.json:48-57`],
    [*CI/CD Pipeline*], [Thesis evaluation is manual build/test; no production environment exists], [Build/test commands are documented; CI is a deployment concern outside scope], [`README.md:177-179`],
    [*Dockerfiles / Container Images*], [Aspire manages containers for local development only], [Production containerization is a deployment extension, not a design contribution], [`README.md:177`],
    [*Recommendation Engine (Collaborative Filtering)*], [CBIR with model comparison is the primary ML contribution; collaborative filtering is orthogonal], [Listed as Future Work in Chapter 11], [`Chapter 11 — Evaluation`],
    [*Payment Provider Beyond Stripe + Bogus*], [Two providers (real + dev stand-in) demonstrate the Strategy pattern adequately], [No architectural gap], [`Module/Payment/Services/Provider/`],
    [*Multi-tenancy / Multi-store*], [Current schema has `StoreId` columns but single-store logic], [Database is forward-compatible; business logic extension is Future Work], [`Order.cs:55`],
    [*Mobile Native Apps*], [Responsive web SPAs are the only client surfaces], [Mobile is a separate client implementation using the same API], [`app/Admin/`, `app/Store/`],
    [*Custom model training / fine-tuning*], [Using pretrained models only; training requires GPU cluster and dataset curation beyond thesis scope], [Evaluation focuses on model *selection*, not model *creation*], [`service/Embedding/pyproject.toml`],
  )
) <tab:out-of-scope>

=== Scope Justification

The out-of-scope items share three characteristics:

+ *They do not affect the dual thesis contribution* — the architectural patterns (modular monolith, vertical slices, Result\<T\>) and the ML model comparison (Precision\@K, Recall\@K, mAP across 4 models) are fully demonstrable without them.
+ *They are additive, not structural* — each can be added later without redesigning existing modules (Strategy pattern, config blocks, provider patterns, additional embedding models).
+ *They shift focus from design/evaluation to operations* — CI/CD, Docker, gateway configuration, and model training are operational concerns rather than software architecture or ML evaluation contributions.

This scope aligns with the *principle of sufficient completeness for evaluation* (Shaw & Garlan, _Software Architecture_): the system must be complete enough to demonstrate its architectural properties and the ML model comparison, but need not be production-ready in every operational dimension.

== Stakeholders

#figure(
  caption: [Stakeholders and how their interests are addressed],
  table(
    columns: 3,
    align: (left, left, left),
    table.header(
      [*Stakeholder*], [*Interest*], [*How addressed*],
    ),
    [*Examiner / Thesis Committee*], [Evidence of structured SE process, design rationale, testability], [This documentation set; clean architecture; comprehensive test suite],
    [*End Customer*], [Visual search, recommendations, smooth checkout], [Storefront SPA; Fashion-CLIP CBIR; MediatR pipeline for reliable checkout],
    [*Administrator / Merchant*], [Product management, order fulfillment, user administration], [Admin SPA; permission-based authorization; full CRUD on all modules],
    [*Future Developer / Researcher*], [Understandable, extensible codebase], [Vertical slices; module isolation; `Result<T>` makes failure paths explicit; extensive inline docs],
  )
) <tab:stakeholders>

== Evidence

- `README.md:1-184` — project intent and WIP notes
- `AGENTS.md:1-80` — non-negotiable architectural rules
- `service/Api/src/Api/Program.cs:26-66` — composition root showing 8 modules
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs:36-78` — vertical slice anatomy
- `service/Api/src/Shared/Application/Models/Results/Result.cs:1-43` — `Result<T>` type
- `Directory.Build.targets:42-53` — `ValidateVerticalSliceIsolation` (currently disabled, but intent is clear)
- `service/Embedding/src/main.py:1-29` — Python sidecar entry
