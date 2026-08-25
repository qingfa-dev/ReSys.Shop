# Chapter 1 — Problem Analysis

## 1.1 Background

This thesis is submitted in partial fulfillment of the requirements for the degree of **Master of Science in Software Engineering** (or equivalent). It presents the complete analysis, design, implementation, and evaluation of ReSys.Shop — a fashion e-commerce platform with Content-Based Image Retrieval (CBIR) capabilities, featuring a comparative evaluation of multiple pretrained visual feature extraction models to identify the most suitable approach for deployment.

Fashion e-commerce represents one of the most competitive and technically demanding domains in online retail. Consumers expect rich visual experiences, personalized recommendations, and seamless checkout flows across multiple devices. Traditional text-based search often fails in fashion because shoppers struggle to articulate visual preferences (e.g., "a dress like this but in blue"). Meanwhile, architectural complexity in e-commerce systems tends to grow uncontrollably as features are added, leading to tightly coupled modules, unpredictable error handling, and deployment fragility.

ReSys.Shop was conceived as a research-oriented e-commerce platform that addresses **three distinct problems simultaneously**:
1. **The user-facing problem**: How can a fashion e-commerce platform provide intuitive visual search using modern machine learning techniques?
2. **The engineering problem**: How can a complex e-commerce system be architected to maintain modularity, testability, and operational clarity as it scales across 8 business domains?
3. **The ML evaluation problem**: Which pretrained visual feature extraction model (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic) offers the optimal balance of retrieval effectiveness (Precision@K, Recall@K, mAP) and operational performance (embedding time, query latency, storage) for fashion CBIR?

## 1.2 Problem Statement

Existing fashion e-commerce platforms typically fall into one of two categories:
- **Monolithic platforms** (e.g., early Shopify, Magento) that become unmaintainable as business logic interleaves across features
- **Microservice platforms** that introduce excessive operational overhead for small-to-medium teams, with distributed-transaction complexity for e-commerce workflows (checkout, payment, inventory)

Neither approach optimally serves a research context where:
- Rapid iteration on ML-powered features (visual search, recommendations) must coexist with stable transactional domains (orders, payments, inventory)
- The system must be demonstrable as a single deployable unit for thesis evaluation
- Code quality and architectural clarity must be examinable and justifiable

**Specific technical gaps identified:**

| Gap | Evidence from prior art | Consequence |
|-----|------------------------|-------------|
| Exception-driven error handling | Typical ASP.NET controllers throw exceptions for validation failures | Unpredictable control flow, implicit error contracts |
| Anemic domain models | EF entities are data bags with no behavior | Business rules scattered across services, duplication |
| Horizontal layering | Controllers → Services → Repositories → Entities | Changes touch 4+ files; cross-cutting concerns bleed |
| Tight module coupling | Services directly reference other modules' repositories | Cannot reason about or test modules in isolation |
| Missing vector search integration | Standard SQL databases cannot perform similarity search on image embeddings | Fashion image search requires separate infrastructure |
| No model comparison for CBIR | Prior art selects embedding models arbitrarily (e.g., Fashion-CLIP by popularity) without empirical comparison on operational metrics | Suboptimal model may be deployed; higher latency or lower accuracy than alternatives |

## 1.3 Objectives

### Primary Objectives

1. **Design and implement a modular monolith** with 8 self-contained business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping) that communicate exclusively via in-process message dispatch, enforcing zero direct cross-module references.

2. **Apply a vertical-slice architecture** where each feature action (e.g., "Create Product", "Checkout Cart") is cohesively implemented in a single folder containing its handler, endpoint, request, response, and validator.

3. **Integrate Content-Based Image Retrieval (CBIR)** via a dedicated Python sidecar supporting multiple pretrained embedding models (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic), storing embeddings in PostgreSQL pgvector with a pluggable model interface.

4. **Enforce explicit error handling** through a `Result<T>` / `Error` type system that eliminates exceptions for control flow, making all failure paths explicit and traceable.

5. **Conduct comparative ML evaluation** to measure retrieval effectiveness (Precision@K, Recall@K, mAP) and operational performance (embedding generation time, query latency, storage footprint) across 4 embedding models on a fashion ground-truth dataset.

### Secondary Objectives

6. Provide dual-channel frontends (Admin SPA and Storefront SPA) with distinct UI libraries optimized for their respective user roles.

7. Implement multi-provider abstractions for storage (Local/S3), notifications (SendGrid/SMTP/Sinch), payment gateways (Stripe/Bogus), and **embedding models** to demonstrate the Strategy pattern across both infrastructure and ML domains.

8. Achieve >70% unit-test coverage for domain logic and integration tests for all critical paths (checkout, payment webhooks, auth, CBIR search).

## 1.4 Scope and Delimitations

### 1.4.1 Problem Boundary

This thesis addresses the **dual contribution** of (a) architectural design and implementation of a fashion e-commerce platform, and (b) comparative evaluation of pretrained visual embedding models for CBIR. The boundary is drawn around **software engineering process evidence** and **ML model evaluation methodology** — analysis, design, implementation, and comparative evaluation — rather than operational deployment or business operations.

### 1.4.2 In-Scope Deliverables

| Deliverable | Description | Thesis Chapter |
|-------------|-------------|----------------|
| **Backend system** | 8 business modules (Catalog, Identity, Inventory, Location, Ordering, Payment, Profile, Shipping) implemented as vertical slices with CQRS, MediatR, and explicit `Result<T>` error handling | Chapters 3–7 |
| **Database design** | PostgreSQL 17 + pgvector schema with per-module namespaces, vector embeddings for CBIR, EF Core migrations | Chapter 5 |
| **ML sidecar** | Python FastAPI service with **pluggable embedding model interface** supporting Fashion-CLIP, ResNet-50, EfficientNet-B0, and CLIP-generic; HTTP API consumed by Catalog module | Chapters 3, 5, 7 |
| **ML model comparison** | Comparative evaluation of 4 embedding models on retrieval effectiveness (Precision@K, Recall@K, mAP) and operational performance (latency, storage, memory) | Chapter 11 |
| **Dual-channel frontends** | Vue 3 Admin SPA (PrimeVue) for administrators; Vue 3 Storefront SPA (Nuxt UI) for customers | Chapters 3, 6 |
| **Security stack** | JWT bearer auth with rotation/reuse detection, permission-based authorization, rate limiting, anti-forgery, file upload guards | Chapter 8 |
| **Testing strategy** | Unit tests (InMemory EF + Moq), integration tests (Testcontainers + WebApplicationFactory), frontend unit tests (Vitest), Python tests (pytest) | Chapter 10 |
| **Observability** | OpenTelemetry traces/metrics/logs, correlation IDs, health checks, Hangfire background jobs | Chapters 3, 9 |
| **Design documentation** | This thesis document set: 11 chapters of analysis, design rationale, and evaluation | All chapters |

### 1.4.3 Out-of-Scope (Explicitly Deferred)

| Feature | Rationale | Impact on Thesis | Evidence |
|---------|-----------|------------------|----------|
| **YARP API Gateway** | SPA→API direct calls are sufficient for thesis demonstration; gateway adds ops complexity without research value | No architectural gap; API handles auth/rate limiting directly | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:5-7` |
| **Azure Blob Storage Provider** | S3 + Local providers cover all thesis file-storage needs | Strategy pattern is demonstrated with 2 providers; adding a 3rd is mechanical | `appsettings.json:163-168` vs `Storage.Extensions.cs:79-82` |
| **Facebook / Microsoft OAuth** | Google OAuth demonstrates external-login architecture; adding more providers is identical pattern | No design gap; config blocks are disabled (`Enabled=false`) | `appsettings.json:48-57` |
| **CI/CD Pipeline** | Thesis evaluation is manual build/test; no production environment exists | Build/test commands are documented; CI is a deployment concern outside scope | `README.md:177-179` |
| **Dockerfiles / Container Images** | Aspire manages containers for local development only | Production containerization is a deployment extension, not a design contribution | `README.md:177` |
| **Recommendation Engine (Collaborative Filtering)** | CBIR with model comparison is the primary ML contribution; collaborative filtering is orthogonal | Listed as Future Work in Chapter 11 | `Chapter 11 — Evaluation` |
| **Payment Provider Beyond Stripe + Bogus** | Two providers (real + dev stand-in) demonstrate the Strategy pattern adequately | No architectural gap | `Module/Payment/Services/Provider/` |
| **Multi-tenancy / Multi-store**  | Database is forward-compatible; business logic extension is Future Work | `Order.cs:55` |
| **Mobile Native Apps** | Responsive web SPAs are the only client surfaces | Mobile is a separate client implementation using the same API | `app/Admin/`, `app/Store/` |
| **Custom model training / fine-tuning** | Using pretrained models only; training requires GPU cluster and dataset curation beyond thesis scope | Evaluation focuses on model *selection*, not model *creation* | `service/Embedding/pyproject.toml` |

### 1.4.4 Scope Justification

The out-of-scope items share three characteristics:

1. **They do not affect the dual thesis contribution** — the architectural patterns (modular monolith, vertical slices, Result<T>) and the ML model comparison (Precision@K, Recall@K, mAP across 4 models) are fully demonstrable without them.
2. **They are additive, not structural** — each can be added later without redesigning existing modules (Strategy pattern, config blocks, provider patterns, additional embedding models).
3. **They shift focus from design/evaluation to operations** — CI/CD, Docker, gateway configuration, and model training are operational concerns rather than software architecture or ML evaluation contributions.

This scope aligns with the **principle of sufficient completeness for evaluation** (Shaw & Garlan, *Software Architecture*): the system must be complete enough to demonstrate its architectural properties and the ML model comparison, but need not be production-ready in every operational dimension.

## 1.5 Stakeholders

| Stakeholder | Interest | How addressed |
|-------------|----------|---------------|
| **Examiner / Thesis Committee** | Evidence of structured SE process, design rationale, testability | This documentation set; clean architecture; comprehensive test suite |
| **End Customer** | Visual search, recommendations, smooth checkout | Storefront SPA; Fashion-CLIP CBIR; MediatR pipeline for reliable checkout |
| **Administrator / Merchant** | Product management, order fulfillment, user administration | Admin SPA; permission-based authorization; full CRUD on all modules |
| **Future Developer / Researcher** | Understandable, extensible codebase | Vertical slices; module isolation; `Result<T>` makes failure paths explicit; extensive inline docs |

## 1.6 Evidence

- `README.md:1-184` — project intent and WIP notes
- `service/Api/src/Api/Program.cs:26-66` — composition root showing 8 modules
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs:36-78` — vertical slice anatomy
- `service/Api/src/Shared/Application/Models/Results/Result.cs:1-43` — `Result<T>` type
- `Directory.Build.targets:42-53` — `ValidateVerticalSliceIsolation` (currently disabled, but intent is clear)
- `service/Embedding/src/main.py:1-29` — Python sidecar entry

---

## [ASK USER] Items

1. Is the thesis target an MSc, PhD, or BSc? This affects the depth of literature review and evaluation expected.
2. Should the "out of scope" items be formally documented as a project boundary section, or are they acceptable as a simple table?
