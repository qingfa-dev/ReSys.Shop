# Design: Port Thesis to CTU Typst Template (v2)

**Date:** 2026-07-25
**Branch:** `feature/port-thesis`
**Status:** Draft
**Supersedes:** `2026-07-25-port-thesis-design.md`

## 1. Goal

Rebuild the bachelor-level thesis at `thesis/` using CTU template (`ctu-thesis`). Synthesize content from two sources: `_thesis/` (reviewed, academic prose) and `docs/thesis/*.md` (newer data, but WIP/out-of-scope context). English language, academic prose, CTU formatting. No code/file references. Diagrams via Mermaid/PlantUML source files + Makefile pipeline.

## 2. Source Material

| Source | Location | Role | How to use |
|---|---|---|---|
| Old thesis | `_thesis/` | Primary — reviewed academic prose, CTU-compliant | Keep prose structure, adapt scope to current implementation |
| New docs | `docs/thesis/*.md` | Secondary — newer data, partially outdated | Selectively reference for updated scope; ignore code-audit sections |
| Benchmarks | `benchmarks/outputs/thesis/` | Authoritative data | Use real numbers from aggregate/efficiency tables; replace old 3-model data |

**Synthesis rule**: `_thesis` is the prose backbone. `docs/thesis/` supplements where it extends scope (e.g. security design, model configuration). Neither source is copied — all text is written fresh with these as reference. The thesis describes ReSys.Shop as a completed system, not a codebase.

---

## 3. Target Structure — 7 Chapters, ~50 Pages

| Part | Ch | Title | Pages |
|---|---|---|---|
| Part 1 | 1 | Introduction | 5-7 |
| Part 2 | 2 | Background & Related Work | 6-8 |
| | 3 | Requirements Analysis | 5-7 |
| | 4 | System Architecture & Design | 10-14 |
| | 5 | Implementation | 6-8 |
| | 6 | Testing & Evaluation | 6-8 |
| Part 3 | 7 | Conclusion & Future Work | 3-5 |

---

## 4. Chapter 1: Introduction (5-7 pages)

### Narrative Arc

Open with the e-commerce boom and the enduring problem of text search for visual products. Narrow to the semantic gap — users can recognize fashion visually but cannot describe it in keywords. Present ReSys.Shop as the solution: a fashion e-commerce platform with integrated CBIR powered by pre-trained deep learning models. State research questions, scope, and methodology. End with a thesis roadmap.

### 4.1 Section 1.1 — Context & Motivation (1.0 page)

**Content**: Why visual search matters in fashion e-commerce. The limitations of keyword search. The semantic gap problem — discrepancy between visual complexity and linguistic description. Statistical context: fashion e-commerce market size, bounce rates from failed searches.

**Source**: `_thesis/part1/01-context.typ` lines 1-10

**Keep**: Opening paragraph about digital shift toward fashion e-commerce. The semantic gap explanation. "A customer may easily recognize a specific pattern, silhouette, or texture but struggle to articulate it using standardized metadata terms." This is the strongest sentence — keep it verbatim.

**Adapt**: Add a statistic or industry context sentence at the opening. Update "multi-billion dollar" to a specific year reference.

**Drop**: The paragraph starting "ReSys.Shop addresses this..." — this belongs in 1.3 Objectives, not here. End 1.1 with the problem, not the solution.

### 4.2 Section 1.2 — Problem Statement (1.0 page)

**Content**: Decompose the search problem into four concrete challenges.

**Source**: `_thesis/part1/01-context.typ` lines 11-16

**Keep verbatim**: The four decomposed problems:
1. Linguistic inconsistency — descriptors vary ("floral" vs "botanical"), fragmented results
2. Visual inexpressibility — draping, texture, gradients cannot be queried in text
3. Cold start data scarcity — new items lack interaction data for collaborative filtering; visual features offer a path
4. Polyglot integration complexity — bridging Python ML (PyTorch) with .NET e-commerce backends

**Adapt**: Rephrase #4 to match the actual implementation — it's a modular monolith with a Python sidecar, not microservices.

### 4.3 Section 1.3 — Objectives (1.5 pages)

**Content**: What this thesis aims to achieve — objectives, research questions, specific tasks.

**Source**: `_thesis/part1/03-objectives.typ` lines 1-48

**Keep**:
- The four technical objectives (demonstrate integration, architect polyglot system, validate pgvector feasibility, benchmark performance)
- The three research questions (RQ1, RQ2, RQ3)
- The five specific tasks (build AI service, set up vector search, connect services, create UI, evaluate results)

**Adapt**:
- RQ1: Update from 3 models to "multiple embedding models spanning CNN and ViT architectures"
- RQ2: Keep the accuracy-vs-speed trade-off question — it's the thesis's core contribution
- RQ3: Change "microservices architecture" to "service-oriented architecture" or "sidecar pattern"
- Task list: Update "Comparison of three AI models" to "Comparison of embedding models" (remove count, it changes)

**Drop**: Nothing — this section is well-structured.

### 4.4 Section 1.4 — Scope & Limitations (1.0 page)

**Content**: What's included, what's excluded, and known constraints.

**Source**: `_thesis/part1/03-objectives.typ` lines 49-75

**Keep verbatim**: The "Included Scope" and "Excluded Scope" lists. The four known limitations (dataset size, hardware, no user testing, pre-trained models only).

**Adapt**: Update dataset size from "5,000 products" to actual benchmark dataset size. Update hardware description if the benchmark environment changed.

**Drop**: None.

### 4.5 Section 1.5 — Research Methodology (0.5 page)

**Content**: Design Science Research approach — build an artifact, evaluate it, learn from it.

**Source**: `_thesis/part1/04-research-methods.typ`

**Keep**: The DSR description. Keep it brief — half a page is sufficient for a bachelor thesis.

### 4.6 Section 1.6 — Thesis Outline (0.5 page)

**Content**: One paragraph per chapter summarizing what the reader will find.

**Adapt**: Rewrite entirely to match the new 7-chapter structure. Each chapter gets 2-3 sentences. End with "Chapter 7 concludes the thesis with a summary of contributions and directions for future work."

### Ch1 Checklist

- [ ] Opening sentence hooks the reader (market context, not "This thesis...")
- [ ] Semantic gap explained with the "floral vs botanical" example
- [ ] All three research questions stated explicitly as bullet points
- [ ] Scope section clearly separates Included from Excluded
- [ ] Limitations are honest and specific (not "time constraints were tight")
- [ ] Outline is telegraphic — 2 sentences per chapter, no fluff
- [ ] No code references, no file paths, no version numbers

---

## 5. Chapter 2: Background & Related Work (6-8 pages)

### Narrative Arc

Start with "what is a vector embedding" — the mathematical foundation that makes everything else possible. Progress through neural network architectures (CNN → ViT → CLIP → Fashion-CLIP). Then cover the infrastructure: vector databases and pgvector. Survey related systems (academic and commercial). End with the technology stack and the gap this thesis fills.

### 5.1 Section 2.1 — E-commerce Platform Architectures (0.5 page)

**Content**: Brief survey of e-commerce architecture patterns — monolith, microservices, modular monolith. Justification for the modular monolith choice in this thesis.

**Source**: New, informed by `docs/thesis/01` and `_thesis/ch2/architecture/01-style-and-rationale.typ`

**Write fresh**: 2 paragraphs. First: define the three patterns in 1-2 sentences each. Second: state why modular monolith was chosen for this project (simpler deployment, shared data context, but still logically separated by bounded context).

**Do not**: dive into implementation details. This is background, not design.

### 5.2 Section 2.2 — Visual Search & CBIR (1.5 pages)

**Content**: What CBIR is. What embeddings are. How similarity is measured.

**Source**: `_thesis/ch1/01-vector-embeddings.typ` (all 56 lines)

**Keep verbatim**:
- "At the heart of visual search is a simple idea: turning images into lists of numbers that a computer can compare." (perfect opening)
- The vector example: `[0.23, -0.15, 0.87, 0.42, ..., -0.31] (512 numbers total)`
- The latent space explanation (embedding space, "hidden" dimensions)
- Cosine similarity formula and explanation
- "For fashion images, a cosine similarity above 0.7 typically indicates visual similarity"

**Adapt**: Move the paragraph "Vector embeddings are the mathematical foundation..." to the section closing. Add a sentence bridging to next section: "How these embeddings are generated is the subject of the next section."

**Diagram 1**: CBIR Pipeline Overview (Mermaid flowchart, ~0.3 page)
```
flowchart LR
    A[User uploads image] --> B[Preprocessing\nresize, normalize]
    B --> C[CNN / ViT Model\nfeature extraction]
    C --> D[512-d Embedding Vector]
    D --> E[pgvector ANN Search\ncosine similarity]
    E --> F[Top-K Results\nranked by similarity]
```
Caption: "High-level CBIR pipeline: from image upload to ranked search results."

### 5.3 Section 2.3 — Deep Learning Models for Fashion (2.0 pages)

**Content**: A survey of neural architectures used for image embedding, progressing from CNNs to modern vision transformers and multimodal models.

**Source**: `_thesis/ch1/02-cnn-architectures.typ`, `03-vit-architectures.typ`, `04-clip-models.typ`, `05-model-selection.typ`

**Sub-section breakdown**:

**2.3.1 Convolutional Neural Networks (0.5 page)**
- What CNNs are: hierarchical feature extraction (edges → textures → patterns → objects)
- ResNet: residual connections solve vanishing gradients, 50/101/152 layer variants
- EfficientNet: compound scaling (depth, width, resolution), B0-B7 variants, B0 is the lightweight option
- Why CNNs are relevant: pre-trained on ImageNet (1M+ images, 1000 classes), serve as feature extractors

**Keep from `_thesis`**: The conceptual explanation of CNN layers. The EfficientNet compound scaling concept.

**Drop**: Architecture diagrams showing layer dimensions. Specific layer counts. Training procedure details. This is background, not a deep learning textbook.

**2.3.2 Vision Transformers (0.5 page)**
- What ViTs are: image as sequence of patches, self-attention, capture long-range dependencies
- DINOv2: self-supervised pre-training, no labeled data needed, strong feature extraction
- ViT vs CNN: ViTs capture global context, CNNs capture local patterns

**Keep**: The patch-based processing metaphor. The self-supervision concept of DINOv2.

**Drop**: Attention mechanism math. Positional encoding details. ViT architecture variants beyond what's used.

**2.3.3 CLIP and Multimodal Models (0.5 page)**
- CLIP concept: dual-tower architecture (image encoder + text encoder), trained on 400M image-text pairs
- Shared latent space: text and images map to the same embedding space
- Fashion-CLIP: CLIP fine-tuned on 700K+ fashion images with domain-specific vocabulary
- Key advantage: multimodal capability enables "red floral summer dress" → image search

**Keep**: The dual-tower metaphor. The fashion-specific fine-tuning explanation. The multimodal advantage.

**Drop**: Training procedure. Contrastive loss explanation. Dataset composition details.

**2.3.4 Model Comparison Table (0.3 page)**

| Model | Architecture | Embedding Dim | Training | Domain | Inference (ms) |
|---|---|---|---|---|---|
| ResNet-50 | CNN | 2048 | Supervised (ImageNet) | General | ~15 |
| EfficientNet-B0 | CNN | 1280 | Supervised (ImageNet) | General | ~21 |
| DINOv2 ViT-S/14 | ViT | 384 | Self-supervised | General | ~80 |
| CLIP ViT-B/16 | ViT | 512 | Contrastive (image-text) | General | ~60 |
| Fashion-CLIP ViT-B/16 | ViT | 512 | Contrastive (fashion) | Fashion | ~68 |

**Source**: Adapt from `_thesis/ch1/05-model-selection.typ` table at line 9-27. Expand to include all benchmark models.

### 5.4 Section 2.4 — Vector Search & Databases (1.0 page)

**Content**: How embeddings are stored, indexed, and queried efficiently.

**Source**: `_thesis/ch1/06-vector-databases.typ`

**Keep**:
- The ANN (Approximate Nearest Neighbor) concept — why exact search is too slow for large catalogs
- HNSW (Hierarchical Navigable Small World) index — graph-based, logarithmic search complexity
- pgvector: PostgreSQL extension, stores vectors alongside relational data, ACID compliance
- Cosine vs L2 distance — cosine used for normalized embeddings
- The "dual-database problem" — why storing vectors in the same database as business data matters

**Adapt**: Keep the engineering rationale — "eliminating the class of stale index bugs." This is a key contribution claim.

**Drop**: pgvector SQL syntax. HNSW parameter tuning (m, ef_construction). Index build time benchmarks.

### 5.5 Section 2.5 — Related Systems (1.0 page)

**Content**: What exists in the market and in research — and what gap this thesis fills.

**Source**: `_thesis/ch1/09-related-work.typ` (all 77 lines)

**Keep verbatim**:
- The academic research survey (DeepFashion dataset, Conversational Fashion Retrieval, Pre-trained Foundation Models)
- The commercial systems comparison table (Google Lens, Pinterest Lens, ASOS Style Match, ViSenze)
- The "Technical Positioning and Contribution" section — the four unique contributions (Polyglot VSA, Vector-Native Consistency, Commodity Hardware, Applied Evaluation)
- "This project demonstrates that similar functionality can be achieved with open-source tools, providing a reference implementation and cost-effective solution"

**Adapt**: The "Polyglot Vertical Slice Architecture" contribution point — change language from "microservices mesh" to "modular monolith with ML sidecar" to match current implementation. This section is otherwise perfect.

**Drop**: None. This is one of the strongest sections in `_thesis`.

### 5.6 Section 2.6 — Technology Stack (1.0 page)

**Content**: The frameworks and tools used to build the system.

**Source**: `_thesis/ch1/07-backend-stack.typ`, `08-frontend-stack.typ`

**Structure as a table with brief prose per row**:

| Layer | Technology | Purpose |
|---|---|---|
| Frontend | Vue 3 + TypeScript + Vite | Customer storefront and admin panel; reactive UI, Pinia state management |
| Backend API | .NET 10 + Carter + MediatR | REST endpoints, CQRS command/query separation, minimal API routing |
| Database | PostgreSQL 17 + pgvector | Relational data + vector embeddings in single ACID context |
| Caching | Redis 7 + HybridCache | Multi-tier cache (memory L1 + Redis L2), session storage |
| ML Sidecar | Python 3.12 + FastAPI + PyTorch | Image embedding generation, model inference, GPU acceleration |
| Orchestration | .NET Aspire | Service discovery, container orchestration, local dev environment |
| Background Jobs | Hangfire | Cart expiry, embedding generation queue, maintenance tasks |
| Auth | JWT + ASP.NET Identity | Access tokens (15min), refresh tokens with rotation, role-based access |

**Write fresh**: 1-2 sentences per row explaining the rationale. No version numbers. No package names beyond what's in the table.

### Ch2 Checklist

- [ ] 2.2 opens with the accessible "lists of numbers" metaphor, not a formal definition
- [ ] Cosine similarity formula is rendered correctly in Typst math mode
- [ ] 2.3.4 model comparison table has real numbers from benchmark data
- [ ] 2.4 mentions the "dual-database problem" and pgvector's ACID solution
- [ ] 2.5 commercial comparison table is concise (4 rows, 3 columns)
- [ ] 2.5 contribution points are updated to match current architecture (modular monolith, not microservices)
- [ ] Diagram 1 renders and has a meaningful caption
- [ ] No code snippets anywhere
- [ ] No PyTorch/TensorFlow API calls mentioned

---

## 6. Chapter 3: Requirements Analysis (5-7 pages)

### Narrative Arc

Define who uses the system, what they can do, and how features are classified. Move from actors → functional requirements → non-functional requirements → use cases → feature classification. The reader should understand the system's scope before seeing its design.

### 6.1 Section 3.1 — System Actors (0.5 page)

**Content**: Three actors with roles, permissions, and interaction surfaces.

**Source**: `_thesis/ch2/02-functional-requirements.typ` lines 1-33

**Keep**:
- Customer (Guest + Authenticated): browse, search, cart, checkout, profile, orders
- Administrator: product CRUD, taxonomy, inventory, order fulfillment, user management
- System (Background): embedding generation, index maintenance, stock reservation, cart expiry

**Format**: Table with columns [Actor, Role Description, Surface]. Brief but complete.

### 6.2 Section 3.2 — Functional Requirements (2.0 pages)

**Content**: Features grouped by business module. Each module gets a paragraph of prose describing its responsibility and key features. Follow with a summary table.

**Source**: `_thesis/ch2/02-functional-requirements.typ` lines 34-80

**Prose structure per module** (each ~3-5 sentences):

**Catalog Module**: The Catalog module manages the product lifecycle — creating products with fashion-specific metadata (style code, season, material, department), defining variants with SKUs and pricing, uploading images, and organizing products through hierarchical taxonomies. It also hosts the CBIR infrastructure: variant images are vectorized via the ML sidecar, and embeddings are stored in PostgreSQL pgvector for similarity search. The catalog supports configurable embedding models, allowing the system to switch between Fashion-CLIP, ResNet-50, and other architectures without application changes.

**Ordering Module**: The Ordering module handles the customer purchase workflow from cart to completed order. Guest and authenticated users add items to carts (auto-expiring after 7 days of inactivity). Checkout follows a forward-only state machine: Address → Delivery → Payment → Confirm → Complete. Orders track item totals, adjustments, shipment costs, and payment state independently, with cancellation available at any pre-confirmation stage.

**Payment Module**: The Payment module manages payment intents, captures, refunds, and voids. It supports two gateway providers: Stripe for production (with webhook signature validation) and a Bogus gateway for development and testing. Payment intents follow their own state machine (Pending → RequiresAction → Processing → Succeeded/Canceled/Failed).

**Inventory Module**: The Inventory module tracks physical stock across warehouse locations, manages reservations during checkout (preventing overselling), records stock movements for audit trails, and handles inter-warehouse transfers.

**Identity Module**: The Identity module provides JWT-based authentication with 15-minute access tokens and refresh token rotation with reuse detection. Guest sessions enable anonymous cart usage. Role-based and permission-based authorization segregates admin and storefront access.

**Supporting Modules**: The Profile module manages user addresses, wishlists, and notification preferences. The Shipping module configures delivery methods and calculates rates by zone. The Location module provides country and state data with ISO codes.

**Summary table** (after prose):

| Module | Key Responsibilities | Research Classification |
|---|---|---|
| Catalog | Products, variants, images, taxonomies, CBIR infrastructure | Core Research |
| Ordering | Cart, checkout state machine, order lifecycle | Supporting |
| Payment | Payment intents, Stripe/Bogus gateways | Supporting |
| Inventory | Stock tracking, reservations, transfers | Supporting |
| Identity | JWT auth, roles, permissions, guest sessions | Supporting |
| Profile | Addresses, wishlists, notifications | Supporting |
| Shipping | Methods, rates, zones | Supporting |
| Location | Countries, states | Supporting |

**Do not**: List 5-10 "FR-XX" IDs per module. This is not a software requirements specification — it's a chapter in a thesis. Use prose, not ticket IDs.

### 6.3 Section 3.3 — Non-Functional Requirements (1.0 page)

**Content**: System qualities — performance, security, modularity, observability.

**Source**: New from `docs/thesis/02` (NFR section — selective, not the code-audit parts)

**Cover these NFR categories** with 1-2 sentences each:

1. **Performance**: CBIR search latency target < 1 second total (image upload → embedding → pgvector → response). API response time < 200ms for non-search endpoints. Supports concurrent users through async I/O.

2. **Security**: JWT short-lived access tokens with refresh rotation. Role-based authorization per endpoint. Rate limiting on auth endpoints. Security headers on all responses. File upload validation (magic-byte check, extension allowlist, size limit).

3. **Modularity**: Business modules separated by namespace convention with no direct cross-references. Communication via MediatR message dispatch. Each module independently testable.

4. **Observability**: OpenTelemetry distributed tracing across .NET and Python services. Structured logging with correlation IDs. Health check endpoints for orchestration.

5. **Reliability**: Background jobs (cart expiry, embedding generation) survive process restarts via Redis-backed Hangfire. Payment webhooks include idempotency handling.

**Format**: Table with columns [Quality, Target, Rationale]. One row per NFR. Keep concrete numbers where available.

### 6.4 Section 3.4 — Use Cases (1.0 page)

**Content**: Three key use cases that represent the system's core functionality.

**Source**: `_thesis/ch2/03-use-cases.typ`, `use-cases/customer/uc-0004-visual-search.typ`, `use-cases/customer/uc-0002-checkout.typ`

**Use Case 1: Visual Search (CBIR)** — Primary research use case
- Actor: Customer
- Precondition: ML service online, catalog contains products with embeddings
- Flow: Customer uploads image → System sends to ML sidecar → Sidecar generates 512-d embedding → System queries pgvector (cosine similarity, Top-K) → Results displayed
- Postcondition: Visually similar products shown, ranked by similarity score

**Use Case 2: Checkout** — Primary e-commerce use case
- Actor: Customer (Guest or Authenticated)
- Precondition: Cart contains items, stock available
- Flow: Customer sets address → selects shipping method → selects payment → confirms → system finalizes order (creates order, reserves stock, clears cart)
- Postcondition: Order created with complete state, payment intent linked

**Use Case 3: Model Benchmark Evaluation** — Research-enabling use case
- Actor: System / Researcher
- Precondition: Benchmark dataset available, ML sidecar running
- Flow: Configure model → generate embeddings for all query and catalog images → execute Top-K similarity search → record Precision at K, Recall at K, latency → repeat for each model → compute aggregate statistics
- Postcondition: Comparison report identifying optimal model for deployment

**Format**: Each use case as a compact table (actor, precondition, main flow in numbered steps, postcondition). No UML use case templates with alternative flows — too detailed for a thesis chapter.

**Diagram 2**: Use Case Diagram (PlantUML, ~0.4 page)
```
left to right direction
actor Customer
actor Administrator
actor "System\n(Background)" as System

rectangle "ReSys.Shop" {
  Customer --> (Visual Search)
  Customer --> (Checkout)
  Customer --> (Browse Catalog)
  Customer --> (Manage Account)
  Administrator --> (Manage Products)
  Administrator --> (Process Orders)
  Administrator --> (Manage Users)
  System --> (Generate Embeddings)
  System --> (Expire Carts)
}
```
Convert from `docs/thesis/diagrams/use-case.mmd`. Caption: "System actors and their primary use cases."

### 6.5 Section 3.5 — Feature Classification (0.5 page)

**Content**: Distinguish what the thesis contributes vs what is infrastructure.

**Source**: `_thesis/ch2/02-functional-requirements.typ` lines 34-64 (table)

**Keep the Core Research vs Supporting Infrastructure distinction**:

| Feature Area | Classification | Rationale |
|---|---|---|
| Visual Search (CBIR) | Core Research | Primary contribution: multi-model CBIR with pluggable architecture |
| ML Embedding Pipeline | Core Research | Critical infrastructure: automated ingestion, vector generation, indexing |
| Model Benchmark System | Core Research | Secondary contribution: systematic comparison of 11 embedding models |
| Product Catalog | Supporting | Required context: provides the dataset for search evaluation |
| Order System | Supporting | Metric validation: provides conversion events to measure search success |
| Inventory | Supporting | Realism constraint: ensures search results reflect actual availability |
| Authentication | Supporting | Security baseline: protects admin functions and user data |

**Do not**: List ALL modules — only the ones that demonstrate the research-vs-supporting distinction.

### Ch3 Checklist

- [ ] Actors table is clear — any reader can identify the three user types
- [ ] Functional requirements are written as prose paragraphs, not bullet lists of IDs
- [ ] NFR table has concrete targets where possible ("< 1 second", not "fast")
- [ ] Three use cases only — visual search, checkout, model benchmark
- [ ] Use case flows are in plain numbered steps, not UML template syntax
- [ ] Feature classification table clearly separates research from infrastructure
- [ ] Diagram 2 renders and is readable at thesis page width

---

## 7. Chapter 4: System Architecture & Design (10-14 pages)

### Narrative Arc

This is the largest chapter. Start with the big picture — three services working together. Then zoom in: domain model (what the system models), C4 architecture views (how components connect), database (how data is stored), API (how clients interact), security (how it's protected). End at the detail level needed to understand the implementation in Ch5.

### 7.1 Section 4.1 — System Overview (1.0 page)

**Content**: High-level description of the three-service architecture.

**Source**: `_thesis/ch2/01-system-overview.typ` (all 283 lines)

**Keep the structure** from `_thesis`:
- Opening paragraph: "ReSys.Shop follows a microservices-inspired architecture with three distinct services"
- Table of three services with technology stack and responsibilities
- Vue Frontend: customer storefront + admin panel, PrimeVue components, Pinia state
- .NET Backend: REST API, MediatR CQRS, PostgreSQL persistence, pgvector search
- Python ML: FastAPI, PyTorch, embedding generation, multi-model support
- Bounded contexts overview table (8 contexts with aggregate roots and domain entities)

**Adapt**:
- Change "microservices-inspired" to "service-oriented" or "three-tier with sidecar"
- Update the bounded context table to match current domain model (e.g., add Dashboard if relevant)
- The table at lines 48-82 in `_thesis` is good — keep its format but verify entity names match current implementation

**Drop**: Detailed entity lists per context beyond aggregate roots. The deep entity enumeration belongs in 4.2 (DDD).

### 7.2 Section 4.2 — Domain-Driven Design (2.5 pages)

**Content**: How the business domain is modeled — bounded contexts, aggregates, entities, state machines, and the ubiquitous language glossary.

**Source**: `_thesis/ch2/architecture/07-ddd.typ`, `08-cross-context-patterns.typ`, `_thesis/ch2/04-architecture.typ` DDD sections

**4.2.1 Bounded Context Map (1.0 page)**

Describe the 8 bounded contexts, their responsibilities, and their integration pattern.

**Keep from `_thesis`**:
- The Conformist integration pattern explanation
- The in-process MediatR dispatch model
- The shared Result<T> technical kernel

**Diagram 6**: Bounded Context Map (PlantUML, ~0.4 page)
Convert from `docs/thesis/diagrams/bounded-context-map.mmd`.

Content: 8 context boxes with Published Language labels between them.
Key relationships to show:
- Catalog → Ordering (ProductId, VariantId, Price)
- Ordering → Payment (PaymentIntentId, Amount)
- Ordering → Shipping (ShipmentTotal)
- Ordering → Inventory (StockItemId, Quantity)
- Identity → Profile (UserId)
- Location → Inventory (CountryId, StateId)

Context responsibilities table (adapt from `_thesis/ch2/01-system-overview.typ` lines 48-88):

| Context | Aggregate Root | Key Entities | Published Language |
|---|---|---|---|
| Catalog | Product | Variant, VariantImage, OptionType, OptionValue, Taxonomy, Taxon | ProductId, VariantId, Sku, Price, Slug |
| Ordering | Order | LineItem, Adjustment | OrderId, OrderNumber, Total, Currency, CheckoutState |
| Payment | PaymentIntent | PaymentCapture | PaymentIntentId, PaymentState |
| Inventory | StockItem | StockLocation, StockMovement, StockTransfer, StockReservation | StockItemId, QuantityOnHand |
| Identity | User | Role, UserClaim | UserId, Email, PermissionClaim |
| Profile | UserProfile | Address, Wishlist | ProfileId, AddressId |
| Shipping | ShippingMethod | ShippingRate | ShippingMethodId, Rate |
| Location | Country | State | CountryId, StateId |

**4.2.2 Aggregates and Entities (1.0 page)**

Describe the aggregate design pattern and list key aggregates with their invariants.

**Keep from `_thesis`**: The aggregate definition. The invariant concept ("consistency boundary"). The design decision: no explicit ValueObject base classes, pragmatic DDD.

**Focus on these key aggregates** (1 paragraph each + invariants):
- **Product** (Catalog): Root with Variant (1:n), VariantImage (1:n), Price (1:n), OptionType/OptionValue. Invariant: slug unique, HasVariants → ≥1 OptionType, MasterVariantId must exist.
- **Order** (Ordering): Root with LineItem (1:n), Adjustment (0:n). Invariant: Total = ItemTotal + AdjustmentTotal + ShipmentTotal, checkout state forward-only, finalized orders immutable except Cancel.
- **PaymentIntent** (Payment): Root with PaymentCapture (0:n). Invariant: state machine Pending → RequiresAction → Processing → Succeeded/Canceled, captures sum ≤ intent amount.
- **StockItem** (Inventory): Root with StockMovement. Invariant: quantity cannot go negative.

**4.2.3 State Machines (0.5 page)**

Two state machine diagrams with brief prose explanation.

**Diagram 8**: Order Checkout State Machine (PlantUML, ~0.2 page)
Convert from `docs/thesis/diagrams/state-order.mmd`

Content: States (Address → Delivery → Payment → Confirm → Complete). Cancel transition from any non-terminal state. Forward-only progression.

Caption: "Order checkout state machine: five sequential states with cancellation available from any pre-confirmation state."

**Diagram 9**: Payment Intent State Machine (PlantUML, ~0.2 page)
Convert from `docs/thesis/diagrams/state-payment.mmd`

Content: States (Pending → RequiresAction → Processing → Succeeded / Canceled / Failed). 3D Secure/SCA branch. Capture → Refunded/Voided transitions.

Caption: "Payment intent lifecycle reflecting Stripe gateway states with system-managed parallel state for offline operations."

**Keep from `_thesis`**: The payment state design decision — "The system maintains its own PaymentIntent entity state in parallel with Stripe's state to support the Bogus gateway and enable offline operations."

### 7.3 Section 4.3 — C4 Architecture (2.0 pages)

**Content**: Three levels of the C4 model — context, container, component.

**Source**: `_thesis/ch2/04-architecture.typ`, `architecture/01-style-and-rationale.typ`, `architecture/02-core-components.typ`

**Diagram 3**: C4 Context (PlantUML, ~0.3 page)
Convert from `docs/thesis/diagrams/c4-context.mmd`

Content: One system box "ReSys.Shop" with external actors:
- Customer (Storefront User) — browses, searches, purchases
- Administrator (Back Office User) — manages catalog, orders, users
- Stripe (Payment Gateway) — processes payments, sends webhooks
- SendGrid (Email Service) — sends transactional emails
- Python ML Sidecar (Internal Service) — generates image embeddings

Caption: "System Context diagram showing ReSys.Shop and its external dependencies."

**Diagram 4**: C4 Container (PlantUML, ~0.3 page)
Convert from `docs/thesis/diagrams/c4-container.mmd`

Content:
- Vue 3 SPA (Storefront + Admin) — user interfaces
- .NET 10 API — business logic, REST endpoints, MediatR CQRS
- Python ML Sidecar — embedding generation
- PostgreSQL 17 + pgvector — relational data + vector storage
- Redis 7 — caching + session storage + Hangfire job storage
- Hangfire Server — background job processing

Arrows: Vue → API (HTTPS), API → ML Sidecar (HTTP), API → PostgreSQL (TCP), API → Redis (TCP), API → Stripe (HTTPS), API → SendGrid (SMTP).

Caption: "Container diagram showing the deployable units of ReSys.Shop and their communication paths."

**Diagram 5**: C4 Component (PlantUML, ~0.4 page)
Convert from `docs/thesis/diagrams/c4-component.mmd`

Content: Inside the .NET API container, show the 8 business modules (Catalog, Ordering, Payment, Inventory, Identity, Profile, Shipping, Location) connected via a MediatR bus. Show Shared kernel (Result<T>, ICommand, IQuery, Entity base class, cross-cutting concerns).

Caption: "Component diagram of the .NET API showing business modules communicating via MediatR in-process message dispatch."

**Prose**: Accompany each diagram with a paragraph explaining what the reader should notice — key architectural decisions, integration patterns, boundaries. Don't just caption-and-move-on.

**Diagram 10**: Deployment Diagram (PlantUML, ~0.4 page)
Convert from `docs/thesis/diagrams/deployment.mmd`

Content: Docker host with containers: API, ML Sidecar, PostgreSQL, Redis. Aspire orchestration boundary. Network zones (internal Docker network, external internet).

Caption: "Deployment diagram showing containerized services orchestrated by .NET Aspire."

### 7.4 Section 4.4 — Database Design (2.0 pages)

**Content**: ERD with key entities, pgvector integration, indexing strategy, schema organization.

**Source**: `_thesis/ch2/05-database-design.typ`, `_thesis/database/01-identity.typ` through `04-inventory.typ`

**Diagram 7**: ERD (Mermaid erDiagram, ~0.5 page)
Convert from `docs/thesis/diagrams/erd-core.mmd`

Content: Core entities and relationships:
- Product 1--* Variant
- Variant 1--* VariantImage
- Variant 1--* Price
- Variant *--* OptionValue (through OptionValueVariant)
- OptionType 1--* OptionValue
- Product *--* Taxon (through Classification)
- Taxonomy 1--* Taxon
- Taxon *--1 Taxon (self-referencing parent)
- Order 1--* LineItem
- Order 1--* Adjustment
- User 1--* UserRole
- Role 1--* UserRole
- StockLocation 1--* StockItem
- Variant 1--1 StockItem

Caption: "Core entity-relationship diagram showing the primary domain entities and their relationships across bounded contexts."

**Prose sections**:

**4.4.1 Schema Organization (0.3 page)**: Describe schema-per-bounded-context approach. Each module owns its tables. EF Core manages migrations. No cross-module foreign keys (IDs only). This enables logical isolation within a single database.

**4.4.2 pgvector Integration (0.5 page)**: How vectors are stored. The `variant_images` table has an `embedding vector(512)` column. HNSW index for ANN search. Cosine distance operator (`<=>`). Query pattern: `SELECT * FROM variant_images ORDER BY embedding <=> $1 LIMIT K`. Model metadata stored alongside embeddings (model_name, model_version) enabling filtered queries per model.

**4.4.3 Key Design Decisions (0.3 page)**:
- GUID primary keys — distributed generation, no central sequence
- Soft deletes — `IsDeleted` flag, global query filters
- Audit columns — `CreatedAtUtc`, `ModifiedAtUtc` on all entities
- Composite indexes — `(UserId, Status)`, `(SessionId, Status)` for order queries
- Vector dimensions vary by model (384 DINOv2-S, 512 Fashion-CLIP/CLIP, 768 DINOv2-B, 1280 EfficientNet, 2048 ResNet-50)

**4.4.4 Per-Context Schema Description (0.4 page)**: 1-2 sentences per bounded context describing key tables and their purpose. Use `_thesis/database/01-identity.typ` through `04-inventory.typ` as reference.

### 7.5 Section 4.5 — API Design (1.5 pages)

**Content**: REST API structure, endpoint organization, MediatR CQRS pattern, key endpoints.

**Source**: `_thesis/ch2/06-api-design.typ`

**Prose sections**:

**4.5.1 API Architecture (0.5 page)**: Carter minimal APIs. Request → MediatR IRequest → Handler → Response flow. Vertical slice organization — each feature (e.g., CreateProduct) is a self-contained folder with endpoint, handler, request, response, validator files. FluentValidation for input validation.

**4.5.2 Endpoint Organization (0.5 page)**: Endpoints grouped by module and surface (Admin vs Storefront). URL pattern: `/api/{module}/{surface}/{action}`. Examples: `/api/catalog/storefront/search-by-image`, `/api/ordering/storefront/cart/checkout`. Auth: JWT Bearer token for authenticated endpoints, anonymous for catalog browsing.

**4.5.3 Key API Endpoints (0.5 page)**: Brief table of significant endpoints:

| Endpoint | Module | Surface | Description |
|---|---|---|---|
| POST /api/catalog/storefront/search-by-image | Catalog | Storefront | Upload image, get visually similar products |
| GET /api/catalog/storefront/products | Catalog | Storefront | Browse products with filters |
| POST /api/ordering/storefront/cart/checkout | Ordering | Storefront | Complete checkout flow |
| GET /api/ordering/storefront/orders | Ordering | Storefront | View order history |
| POST /api/identity/store/auth/login | Identity | Storefront | JWT login with email/password |
| POST /api/payment/storefront/payment/create-intent | Payment | Storefront | Create Stripe payment intent |

**Do not**: List all endpoints. Do not show request/response bodies. Do not show HTTP method tables. The API section describes the design philosophy, not the API reference.

### 7.6 Section 4.6 — Security Design (1.5 pages)

**Content**: Authentication flow, authorization model, security measures.

**Source**: New, informed by `docs/thesis/08-security-design.md` (conceptual parts only, no code audit)

**4.6.1 Authentication (0.5 page)**: JWT access tokens (15-minute expiry). Refresh tokens with rotation and reuse detection (one-time use, blacklist on reuse). Guest sessions via cookie for anonymous cart. Login methods: email/password, Google OAuth. Password reset via email token.

**4.6.2 Authorization (0.3 page)**: Role-based access control with permission granularity. Permission format: `{domain}:{category}:{action}` (e.g., `catalog:products:create`). Admin vs Storefront surface segregation. Guest → Customer → Admin role hierarchy.

**4.6.3 Security Measures (0.4 page)**:
- Rate limiting: auth endpoints (5/min), registration (3/hour), payment (30/min)
- Security headers: CSP, HSTS, X-Frame-Options, X-Content-Type-Options on all responses
- File upload validation: magic-byte verification, extension allowlist, 10MB size limit
- Payment webhooks: Stripe signature validation before processing

**4.6.4 Token Flow Diagram (text description, 0.2 page)**: Describe the refresh flow in prose: Client sends refresh token → Server validates and marks as used → Issues new access token + new refresh token → If a used refresh token is presented again → Blacklist all tokens for that user. No sequence diagram needed — prose is clearer for auth flows.

### Ch4 Checklist

- [ ] 4.1 system overview is visual — reader can picture the three services
- [ ] 4.2 ubiquitous language glossary is present (one of the strongest academic elements)
- [ ] 4.2 state machines have clear start/end states and transitions
- [ ] C4 diagrams follow consistent visual style (same colors, same notation)
- [ ] ERD shows only core entities (not every join table)
- [ ] Deployment diagram shows Aspire orchestration boundary
- [ ] API section describes philosophy, not endpoint catalog
- [ ] Security section is conceptual — no config snippets, no token payload examples
- [ ] All 8 diagrams in this chapter render correctly
- [ ] Diagram captions are descriptive, not just labels

---

## 8. Chapter 5: Implementation (6-8 pages)

### Narrative Arc

Focus on the research-contributing parts: the ML pipeline and the CBIR search flow. Briefly mention how the e-commerce core was built. End with the model configuration mechanism that makes the system flexible.

### 8.1 Section 5.1 — Vertical Slice Architecture (0.5 page)

**Content**: How feature code is organized — the vertical slice pattern.

**Source**: `_thesis/ch2/architecture/04-vertical-slice.typ`, `05-unified-vertical.typ`

**Keep**: The vertical slice concept — each feature is co-located (handler, endpoint, request, response, validator in one folder). The MediatR pipeline: Request → Validator → Handler → Response → Endpoint. The separation of Admin and Storefront feature folders.

**Drop**: All code examples. All file counts. All namespace naming conventions. This is a brief conceptual description.

### 8.2 Section 5.2 — ML Embedding Pipeline (2.0 pages)

**Content**: The Python sidecar service — how it loads models, generates embeddings, and serves them to the .NET backend. This is the core research implementation.

**Source**: `_thesis/ch2/implementation/01-ml-service.typ` (all 64 lines + 5 sub-files)

**Sub-sections**:

**5.2.1 Service Architecture (0.5 page)**
- FastAPI framework with async endpoints
- Three-layer design: HTTP interface → Model Manager (singleton) → PyTorch runtime
- Endpoints: `/embeddings` (POST — generate embedding), `/health` (GET — liveness check)
- Containerized via Docker, orchestrated by .NET Aspire
- API key authentication between .NET backend and ML sidecar

**Keep the layered architecture diagram concept** from `_thesis` (lines 28-58) — but convert to Mermaid flowchart:

**Diagram 11 (part a)**: ML Service Internal Architecture (Mermaid, ~0.2 page)
```
flowchart TB
    A[FastAPI Interface\n/embeddings, /health] --> B[ModelManager Singleton\nCache & Lazy Loading]
    B --> C[PyTorch Runtime\nCUDA / CPU]
```

**5.2.2 Model Loading Strategy (0.5 page)**
- Lazy loading: models loaded on first request, cached in memory thereafter
- Singleton ModelManager: single instance, thread-safe, serves all requests
- Strategy pattern: each model implements a common interface, selected via environment variable
- Supported models: Fashion-CLIP, CLIP, DINOv2 (multiple sizes), ResNet-50, EfficientNet variants

**Keep from `_thesis`**: The lazy loading explanation. The singleton pattern rationale.

**Drop**: Python code showing model loading. The Model Zoo catalog listing. Framework internals.

**5.2.3 Embedding Generation Flow (0.5 page)**
1. .NET backend sends image bytes to `/embeddings`
2. FastAPI validates API key
3. ModelManager retrieves (or loads) the configured model
4. Image preprocessed: resized, normalized, converted to tensor
5. Model inference: forward pass through the network
6. Output: 512-dimensional (or model-specific) float vector
7. Vector returned as JSON array to .NET backend

**Diagram 11 (part b)**: ML Embedding Pipeline (Mermaid, ~0.3 page)
Convert from `docs/thesis/diagrams/ml-pipeline.mmd`

Content:
```
flowchart LR
    A[.NET Backend\nsends image bytes] --> B[FastAPI\nvalidate API key]
    B --> C[Preprocessing\nresize, normalize, to tensor]
    C --> D[Model Inference\nforward pass]
    D --> E[512-d Vector]
    E --> F[Return JSON\nfloat array]
```

Caption: "Embedding generation pipeline: from image bytes received by the ML sidecar to vector output."

**5.2.4 Health Monitoring (0.2 page)**
- `/health` endpoint returns model status (loaded/not loaded), last inference time, GPU memory usage
- Enables Aspire health checks and Docker restart policies

### 8.3 Section 5.3 — CBIR Search Flow (1.5 pages)

**Content**: The end-to-end visual search — from user upload to displayed results.

**Source**: `_thesis/ch2/implementation/02-backend/04-product-images-vectorization.typ`

**The full flow described in prose** (1.0 page):

1. **Image Upload**: Customer uploads an image via Vue storefront. Client-side validation: JPEG/PNG/WebP, max 10MB.

2. **Backend Receives**: .NET API endpoint receives the image. Performs server-side validation: magic-byte verification, extension check, size limit.

3. **Embedding Generation**: Backend sends image bytes to Python ML sidecar at `/embeddings`. ML sidecar generates 512-d embedding vector. Response time: typically 50-100ms depending on model.

4. **Vector Search**: Backend queries PostgreSQL pgvector:
   - `SELECT v.*, vi.file_path, vi.embedding <=> $embedding AS distance FROM variant_images vi JOIN variants v ON vi.variant_id = v.id WHERE vi.embedding IS NOT NULL AND vi.model_name = $model ORDER BY distance LIMIT 20`
   - HNSW index enables sub-10ms search on 10K+ vectors
   - Model name filter ensures embeddings from the currently configured model

5. **Result Assembly**: Backend joins variant data with product data, computes similarity scores (1 - cosine_distance), filters by minimum similarity threshold (default 0.7).

6. **Response**: JSON array of matching products with similarity scores, variant thumbnails, pricing, and product URLs. Total end-to-end latency target: < 1 second.

**Diagram 12**: CBIR Search Sequence Diagram (PlantUML, ~0.4 page)
New sequence diagram showing the full CBIR flow across all services.

```
@startuml
actor Customer
participant "Vue SPA" as Vue
participant ".NET API" as API
participant "Python ML\nSidecar" as ML
database "PostgreSQL\n+ pgvector" as DB

Customer -> Vue: Upload image
Vue -> API: POST /api/catalog/storefront/search-by-image\n(multipart form data)
API -> API: Validate image\n(magic byte, extension, size)

API -> ML: POST /embeddings\n(image bytes, X-API-Key)
activate ML
ML -> ML: Preprocess image\n(resize, normalize)
ML -> ML: Model inference\n(forward pass)
ML --> API: 512-d embedding vector
deactivate ML

API -> DB: SELECT * FROM variant_images\nORDER BY embedding <=> $embedding\nLIMIT 20
DB --> API: Top-20 results with distances

API -> API: Join variant + product data\nCompute similarity scores\nFilter by threshold (0.7)

API --> Vue: JSON response\n(matched products + similarity)
Vue -> Vue: Render results grid\n(thumbnails, prices, scores)
Vue --> Customer: Display similar products
@enduml
```

Caption: "Sequence diagram of the CBIR search flow showing the interaction between Vue frontend, .NET API, Python ML sidecar, and PostgreSQL pgvector."

**Prose** (0.5 page): After the diagram, describe what happens at each step and why — the design decisions embedded in the flow (why model_name filter, why minimum similarity threshold, why HNSW index).

### 8.4 Section 5.4 — Model Configuration (0.5 page)

**Content**: How the system supports switching between embedding models without code changes.

**Source**: New, informed by current implementation

Describe the `EMBEDDING_MODEL` environment variable mechanism:
- Environment variable controls which model the ML sidecar loads
- Supported values: `fashion-clip`, `resnet50`, `efficientnet_b0`, `clip`, `dinov2_s14`, etc.
- Each embedding stored with `model_name` and `model_version` metadata
- Search queries filter by `model_name` — only embeddings from the active model are considered
- Enables A/B testing: deploy two instances with different models, compare results

This is a key implementation detail that supports the benchmark evaluation in Ch6 — without it, comparing 11 models would require 11 redeployments.

### 8.5 Section 5.5 — E-commerce Core (1.5 pages)

**Content**: Brief description of the supporting e-commerce features. These are NOT research contributions — keep them concise.

**Source**: `_thesis/ch2/implementation/02-backend/03-key-features-intro.typ` through `07-system-automation.typ`

**One paragraph per module** (not one per feature):

**Catalog Management**: Products are created with fashion-specific fields (style code, season, material, department, gender target). Variants define sellable configurations (size + color) with SKUs, barcodes, and independent pricing. Product images support multiple variants with automatic thumbnail generation. Hierarchical taxonomies organize products into categories (e.g., Clothing → Dresses → Evening Dresses).

**Order Processing**: Carts auto-expire after 7 days via Hangfire background job. Checkout progresses through the 5-state machine described in Chapter 4. Order numbers are generated inside database transactions with RepeatableRead isolation to prevent duplicates. Orders track payment state and shipment state independently, enabling partial fulfillment.

**Payment Handling**: Payment intents support two gateway providers. The Stripe gateway handles real payments with webhook signature validation and idempotency. The Bogus gateway simulates the payment lifecycle for development, automatically transitioning through states without external calls.

**Inventory Tracking**: Stock quantities are maintained per variant per warehouse location. Reservations temporarily hold stock during active checkouts, preventing overselling. Stock movements are auditable, recording who adjusted what quantity and why.

**Background Automation**: Hangfire processes background jobs for cart expiry, embedding generation retries, and periodic index maintenance. Jobs persist in Redis, surviving application restarts.

**Do not**:
- Describe admin panel UI
- Describe frontend routing or component structure
- List all CRUD operations per module
- Mention individual C# classes or Python modules by name

### Ch5 Checklist

- [ ] 5.1 is under 0.5 page — brief conceptual description only
- [ ] 5.2 ML pipeline is the longest section (2 pages) — it's the core contribution
- [ ] 5.3 CBIR flow includes the sequence diagram AND explanatory prose
- [ ] 5.3 mentions pgvector query pattern conceptually, not as SQL
- [ ] 5.3 mentions HNSW index and model_name filter
- [ ] 5.4 model configuration is clearly explained as the enabler for Ch6 benchmarks
- [ ] 5.5 e-commerce core is brief — no module gets more than 5 sentences
- [ ] Diagram 11 (pipeline) and Diagram 12 (sequence) render correctly
- [ ] No code snippets, no file paths, no version numbers

---

## 9. Chapter 6: Testing & Evaluation (6-8 pages)

### Narrative Arc

Describe the testing approach (brief). Then present the benchmark protocol — the rigorous part. Then show results: retrieval accuracy and efficiency. End with interpretation — what the numbers mean, which model to use when. This chapter answers the research questions from Chapter 1.

### 9.1 Section 6.1 — Testing Strategy (1.0 page)

**Content**: How the system was tested at different levels.

**Source**: `_thesis/ch3/01-objectives.typ`, `03-test-cases.typ`

**Three testing levels** (1 paragraph each):

**Unit Testing**: xUnit v3 for .NET, pytest for Python. Tests validate individual handler logic, domain invariants (e.g., Order.Total calculation, checkout state transitions), and validation rules. Tests run without external dependencies (no Docker required for unit tests).

**Integration Testing**: Testcontainers for PostgreSQL, pgvector, and Redis. Tests verify database queries (including vector similarity search correctness), API endpoint integration, and cross-service communication (backend → ML sidecar). End-to-end CBIR test: upload image → verify embedding generated → verify search returns expected products.

**E2E Testing**: Manual verification of key user flows (visual search, checkout, admin product management) using documented HTTP test files. Automated E2E via Playwright for critical paths.

**Keep brief**: 1 page total. This is not a QA manual. The real contribution is the benchmark evaluation.

### 9.2 Section 6.2 — Benchmark Protocol (1.5 pages)

**Content**: How the CBIR models were systematically evaluated.

**Source**: `_thesis/ch3/02-methodology.typ`, `ch3/testing/01-dataset.typ`, `ch3/testing/goal.typ`, `ch3/testing/scenario.typ`, `ch3/testing/qualitative.typ`

**Structured explanation**:

**9.2.1 Dataset**: Describe the benchmark dataset — number of query images, number of catalog images, grouping methodology. Human-labeled similarity groups. Category distribution. Image preprocessing (all images resized to 224x224, normalized with ImageNet stats).

**9.2.2 Models Evaluated**: List all 11 models evaluated in the benchmark. Group by architecture type:

| Architecture | Models |
|---|---|
| CNN | ResNet-50, ResNet-101, EfficientNet-B0, EfficientNet-B4 |
| Vision Transformer | DINOv2 ViT-S/14, DINOv2 ViT-B/14 |
| CLIP-based | CLIP ViT-B/32, CLIP ViT-B/16, CLIP ViT-L/14 |
| Fashion-specific | Fashion-CLIP ViT-B/16 |
| Other | (add any remaining from benchmark) |

**9.2.3 Metrics**: Define each metric clearly:
- **Mean Average Precision (mAP)**: Mean of average precision scores across all queries. Measures overall retrieval quality — how many relevant results appear and how early.
- **Precision at K (P@K)**: Fraction of top-K results that are relevant. P@20 = 0.82 means 82% of the first 20 results were relevant.
- **Recall at K (R@K)**: Fraction of all relevant items that appear in the top-K results. R@20 = 0.35 means 35% of all relevant items were found in the top 20.
- **Inference Time**: Average time to generate one embedding (milliseconds).
- **Throughput**: Embeddings generated per second.
- **Storage**: Disk space consumed by the embedding index.
- **RAM**: Memory required to load and run the model.

**9.2.4 Methodology**:
1. Generate embeddings for all query images and catalog images using Model A
2. Execute Top-K (K=20) similarity search for each query image
3. Compute Precision at K and Recall at K, then aggregate to mAP
4. Record inference time per image and total throughput
5. Record storage and RAM usage
6. Repeat steps 1-5 for each of the 11 models
7. Compute mean and standard deviation across all queries per model
8. Produce comparison tables and analysis

**9.2.5 Hardware Environment**: Specify the hardware used for benchmarks (GPU model, VRAM, CPU, RAM). This is critical for reproducibility — different hardware produces different latency numbers.

### 9.3 Section 6.3 — Retrieval Performance (1.5 pages)

**Content**: The accuracy numbers — which models find relevant products best.

**Source**: `benchmarks/outputs/thesis/tables/thesis_aggregate.typ` (authoritative)

**Table 13**: Aggregate Retrieval Metrics (Typst table, ~0.5 page)

Columns: Model | Architecture | mAP | P@20 | R@20 | Inference (ms)

Sort by mAP descending. Include mean ± SD where available. Bold the best value in each column.

Use the real data from `thesis_aggregate.typ`. Do not fabricate numbers.

**Prose analysis** (1.0 page):
- Which model achieved highest mAP? By how much?
- CNN vs ViT vs CLIP-based: which architecture family performed best?
- Fashion-CLIP vs general CLIP: how much does domain-specific training help?
- DINOv2's performance — does self-supervised learning compete with contrastive training?
- ResNet-50 (oldest architecture) vs modern models — how far have we come?
- The precision-recall trade-off: models with high P@K may have low R@K, and vice versa
- Category-level variation if available: do all models struggle with the same categories?

**Do not**: Just read the table. Interpret it. Answer RQ1 from Chapter 1 explicitly: "How does a fashion-specific model compare to general-purpose models?"

### 9.4 Section 6.4 — Efficiency Metrics (1.0 page)

**Content**: The speed and resource numbers — how fast and at what cost.

**Source**: `benchmarks/outputs/thesis/tables/thesis_efficiency.typ` (authoritative)

**Table 14**: Efficiency Metrics (Typst table, ~0.4 page)

Columns: Model | Latency (ms) | Throughput (img/s) | Storage (MB) | RAM (GB)

Sort by latency ascending. Bold the best value in each column.

**Prose analysis** (0.6 page):
- Fastest model for inference? Slowest?
- Storage cost: larger models (ViT-L, ResNet-101, DINOv2-B) vs smaller (EfficientNet-B0, ResNet-50)
- RAM: which models require GPU-level memory vs can run on CPU?
- Throughput: can all models serve real-time search (>10 images/sec)?
- The accuracy-efficiency trade-off: is the best model also the fastest? (Answer: usually no.)

Answer RQ2: "What are the trade-offs between search accuracy and processing speed?"

### 9.5 Section 6.5 — Model Comparison & Discussion (1.5 pages)

**Content**: Synthesis — put accuracy and efficiency together and tell the story.

**Source**: `_thesis/ch3/05-discussion.typ`, `ch3/discussion/01-analysis.typ`, `ch3/discussion/02-lessons-learned.typ`

**Sub-sections**:

**9.5.1 Accuracy-Efficiency Trade-off (0.5 page)**:
- Create a scatter plot (conceptually): X-axis = inference time, Y-axis = mAP. Each point is a model.
- Which models are in the "sweet spot" — high accuracy, reasonable speed?
- Which models are "accuracy at any cost" (high mAP, slow)?
- Which are "fast but inaccurate" (low mAP, fast inference)?
- Fashion-CLIP typically appears as the balanced choice

**Diagram 15**: Model Comparison (Typst table or generated chart, ~0.3 page)
A ranked table combining both metrics:
| Model | mAP | P@20 | R@20 | Latency (ms) | Throughput | RAM (GB) |
|---|---|---|---|---|---|---|
(Sorted by a composite score or simply by mAP within speed tiers)

**9.5.2 Deployment Recommendations (0.5 page)**:
- For production e-commerce with GPU: Fashion-CLIP (best accuracy, acceptable speed)
- For CPU-only deployment: EfficientNet-B0 (fastest, reasonable accuracy)
- For maximum accuracy regardless of cost: DINOv2-B or CLIP ViT-L
- For mobile/edge: ResNet-50 (lightweight, good enough)
- The pluggable model architecture (Section 5.4) makes switching between these recommendations trivial

**9.5.3 Discussion of Limitations (0.3 page)**:
- P@20 and R@20 values may be zero for some model/dataset combinations — explain why (embedding quality, dataset mismatch, category granularity)
- Benchmark dataset size limits statistical significance
- Hardware-specific latency numbers (different GPU → different results)
- The "relevance" criterion (same category) is a proxy for true visual similarity

**9.5.4 Lessons Learned (0.2 page)**:
- Domain-specific models (Fashion-CLIP) outperform general models on fashion data
- Transformer architectures (ViT) generally outperform CNNs for retrieval
- Self-supervised models (DINOv2) can compete with supervised models without labeled data
- The dual-database problem is real — pgvector's ACID integration saved significant engineering effort
- Commodity hardware can serve production-grade visual search

### Ch6 Checklist

- [ ] 6.1 testing strategy is under 1 page — no test case tables
- [ ] 6.2 protocol describes dataset, models, metrics, methodology, and hardware
- [ ] 6.2 metrics are defined clearly before they appear in tables
- [ ] Table 13 uses real data from `thesis_aggregate.typ` — no fabrication
- [ ] Table 14 uses real data from `thesis_efficiency.typ` — no fabrication
- [ ] 6.3 prose interprets the numbers, not just reads the table
- [ ] 6.5 explicitly answers RQ1, RQ2, RQ3 from Chapter 1
- [ ] 6.5 includes deployment recommendations per use case
- [ ] Diagram 15 (comparison table/chart) exists
- [ ] Zero values in P@20/R@20 are acknowledged and explained, not hidden

---

## 10. Chapter 7: Conclusion & Future Work (3-5 pages)

### Narrative Arc

Close the thesis. Summarize what was built. Answer the research questions definitively. List concrete contributions. Be honest about limitations. Suggest realistic future work. Trace how the objectives were met.

### 10.1 Section 7.1 — Summary of Work (1.0 page)

**Content**: What was accomplished in this thesis.

**Write fresh**: 3-4 paragraphs covering:
1. What was built: a fashion e-commerce platform with integrated CBIR, supporting 11 embedding models via a pluggable architecture
2. What was evaluated: systematic benchmark comparing retrieval accuracy and efficiency across 11 models
3. What was found: the key result (state which model performed best overall, quantify the trade-off)
4. Whether objectives were met: revisit the four technical objectives and three research questions, state each answer explicitly

**Explicitly answer the research questions**:
- RQ1: Fashion-CLIP outperformed general-purpose models by X% in mAP, demonstrating that domain-specific training provides measurable advantages for fashion retrieval.
- RQ2: The fastest model (EfficientNet-B0, Y ms) is Z% less accurate than the most accurate model (Model X, W ms). The trade-off favors Model Y for deployment because...
- RQ3: The sidecar architecture successfully separated ML inference from web application logic. End-to-end search latency of Z ms confirms the approach is viable for real-time use.

(Use actual numbers from benchmark data.)

### 10.2 Section 7.2 — Contributions (0.5 page)

**Content**: What this thesis adds to the field.

**Source**: Adapt from `_thesis/part3/01-conclusion.typ` and the contribution claims in `_thesis/ch1/09-related-work.typ`

**List as bullet points**:
1. A reference implementation of CBIR integrated into a production-style e-commerce platform, demonstrating that open-source tools can match commercial visual search capabilities.
2. A systematic benchmark comparing 11 embedding models across retrieval accuracy, inference latency, throughput, and resource consumption — providing a practical guide for model selection.
3. A pluggable model architecture that enables switching embedding models via a single configuration parameter, lowering the barrier for A/B testing and iterative improvement.
4. Demonstration of pgvector's ACID-compliant vector storage as a solution to the dual-database problem, eliminating the class of stale-index bugs in visual search systems.
5. A polyglot architecture pattern (.NET + Python sidecar) that combines enterprise-grade backend reliability with access to the Python AI ecosystem.

(Adapt contribution claims to what was actually achieved. If the benchmark has 11 models, claim 11. If Fashion-CLIP was the winner, say so.)

### 10.3 Section 7.3 — Limitations (0.5 page)

**Content**: Honest assessment of what this thesis does NOT address.

**Source**: `_thesis/part1/03-objectives.typ` lines 65-75 + new limitations discovered

**List**:
- Dataset size: benchmark dataset of N images is smaller than production catalogs (millions); scalability not tested
- Hardware: benchmarks run on specific GPU/CPU; results are hardware-dependent
- No user testing: evaluation uses automated metrics; no formal user experience study was conducted
- Pre-trained models only: no fine-tuning or custom training; domain-specific fine-tuning might improve results further
- Single modality (images): no multi-modal search combining text + image queries in evaluation
- Category-level evaluation: limited per-category breakdown; some categories may have insufficient samples
- P@20/R@20 zero values: certain model/dataset combinations produce zero retrieval metrics due to embedding quality or dataset mismatch

### 10.4 Section 7.4 — Future Work (0.5 page)

**Content**: What could be done next — realistic, specific, prioritized.

**Source**: `_thesis/part3/02-future-work.typ`

**List**:
1. **Fine-tuning**: Fine-tune Fashion-CLIP or DINOv2 on the specific product catalog to improve retrieval accuracy beyond pre-trained baselines
2. **Multi-modal search**: Implement combined text + image queries ("find products like this but in blue")
3. **Personalization**: Add collaborative filtering and user behavior tracking for personalized recommendations
4. **Scalability testing**: Benchmark on larger datasets (100K-1M products) to validate pgvector HNSW scaling
5. **User experience study**: Conduct A/B testing with real users comparing text search vs visual search success rates
6. **Production deployment**: Containerize the full stack for cloud deployment with CI/CD pipeline and monitoring
7. **Additional modalities**: Explore style embeddings, outfit compatibility, and trend detection

### 10.5 Section 7.5 — Requirements Traceability (0.5 page)

**Content**: Prove the thesis is coherent — every objective was addressed somewhere.

**Source**: `docs/thesis/12-requirements-traceability-matrix.md` (selective, simplified)

| Objective / RQ | Addressed In | Key Finding |
|---|---|---|
| RQ1: Fashion-specific vs general models | Ch6, Section 6.3 | Fashion-CLIP achieves highest mAP (X.XX); domain training improves retrieval by X% |
| RQ2: Accuracy vs speed trade-off | Ch6, Section 6.4-6.5 | Trade-off quantified; model Y recommended for production |
| RQ3: Sidecar architecture viability | Ch5, Section 5.2-5.3 | Architecture achieves <1s end-to-end search latency |
| Build AI service | Ch5, Section 5.2 | Python FastAPI sidecar with lazy-loading model manager |
| Set up vector search | Ch4, Section 4.4 | PostgreSQL pgvector with HNSW index |
| Connect services | Ch5, Section 5.3 | CBIR search sequence diagram and flow description |
| Create UI | Ch5, Section 5.3 (brief mention) | Vue 3 storefront with image upload and results display |
| Evaluate results | Ch6, Sections 6.3-6.5 | 11-model benchmark with accuracy and efficiency metrics |

### Ch7 Checklist

- [ ] 7.1 answers all three research questions with numbers, not hand-waving
- [ ] 7.2 contributions are specific and verifiable ("11-model benchmark" not "comprehensive evaluation")
- [ ] 7.3 limitations are honest, not sugar-coated
- [ ] 7.4 future work is actionable and prioritized, not a wishlist
- [ ] 7.5 traceability table confirms every Ch1 objective has a Ch4-Ch6 counterpart
- [ ] Conclusion does not introduce new information not covered in earlier chapters

---

## 11. Front Matter & Back Matter

### Front Matter (CTU template, Roman numeral pages)

| # | File | Content | Source |
|---|---|---|---|
| 1 | Cover page | CTU logo, "CAN THO UNIVERSITY", thesis title, student name/ID/class, advisor, year | `_thesis/info.typ` metadata, CTU template |
| 2 | Inner cover | Same as cover with department info | CTU template |
| 3 | Evaluation | Advisor evaluation form fields | CTU template placeholder — student provides |
| 4 | Acknowledgements | ~100 words thanking advisor, family | CTU template placeholder — student provides |
| 5 | Table of Contents | Auto-generated | `#outline()` in CTU template |
| 6 | List of Figures | Auto-generated | CTU template |
| 7 | List of Tables | Auto-generated | CTU template |
| 8 | Abbreviations | API, ANN, CBIR, CNN, DSR, HNSW, mAP, pgvector, SPA, ViT, VSA | `_thesis/info.typ` abbreviations section |
| 9 | Abstract | 200-350 words: problem, approach, key results. Keywords: e-commerce, visual search, deep learning, modular monolith, computer vision | New — synthesize from Ch1 problem + Ch6 results |

### Back Matter

| # | File | Content | Source |
|---|---|---|---|
| 1 | References | IEEE style, minimum 20 entries | `bibliography.bib` (16 existing, expand with entries from `benchmarks/docs/07-references.md`) |
| 2 | Appendices | A. Full benchmark results table (all metrics, all models). B. Dataset composition and category distribution. C. Hardware specifications for benchmark environment. | Generated from benchmark outputs; new |

**Abstract template**:
> E-commerce platforms rely primarily on text-based search, yet fashion products are inherently visual — patterns, silhouettes, and textures resist keyword description. This thesis presents ReSys.Shop, a fashion e-commerce platform with integrated Content-Based Image Retrieval (CBIR) that enables customers to search for products by uploading images rather than typing keywords. The system implements a modular architecture with a .NET backend, Vue.js frontend, and a Python machine learning sidecar for embedding generation. A systematic benchmark evaluates 11 pre-trained deep learning models spanning convolutional neural networks and vision transformers across retrieval accuracy and operational efficiency. Results show that [best model] achieves a mean Average Precision of [X.XX] with [Y]ms inference latency, demonstrating that domain-specific models provide measurable advantages for fashion retrieval while remaining viable for real-time deployment on commodity hardware.

---

## 12. Diagram Plan — Complete Specifications

### 12.1 Diagram Inventory

| # | Diagram | Ch | Tool | Source | Status |
|---|---|---|---|---|---|
| 1 | CBIR Pipeline Overview | 2 | Mermaid | New | Create |
| 2 | Use Case Diagram | 3 | PlantUML | Adapt `use-case.mmd` | Convert |
| 3 | C4 Context | 4 | PlantUML | Adapt `c4-context.mmd` | Convert |
| 4 | C4 Container | 4 | PlantUML | Adapt `c4-container.mmd` | Convert |
| 5 | C4 Component | 4 | PlantUML | Adapt `c4-component.mmd` | Convert |
| 6 | Bounded Context Map | 4 | PlantUML | Adapt `bounded-context-map.mmd` | Convert |
| 7 | ERD (Core) | 4 | Mermaid | Adapt `erd-core.mmd` | Convert |
| 8 | Order State Machine | 4 | PlantUML | Adapt `state-order.mmd` | Convert |
| 9 | Payment State Machine | 4 | PlantUML | Adapt `state-payment.mmd` | Convert |
| 10 | Deployment Diagram | 4 | PlantUML | Adapt `deployment.mmd` | Convert |
| 11 | ML Embedding Pipeline | 5 | Mermaid | Adapt `ml-pipeline.mmd` | Convert |
| 12 | CBIR Search Sequence | 5 | PlantUML | New | Create |
| 13 | Aggregate Metrics Table | 6 | Typst | `thesis_aggregate.typ` | Keep |
| 14 | Efficiency Metrics Table | 6 | Typst | `thesis_efficiency.typ` | Keep |
| 15 | Model Comparison | 6 | Typst | New from benchmark data | Create |

### 12.2 Diagram 1 — CBIR Pipeline Overview (Mermaid)

```mermaid
flowchart LR
    A["User uploads\nimage"] --> B["Preprocessing\n(resize 224x224,\nnormalize)"]
    B --> C["CNN / ViT Model\n(feature extraction)"]
    C --> D["512-d Embedding\nVector"]
    D --> E["pgvector ANN Search\n(cosine similarity,\nHNSW index)"]
    E --> F["Top-K Results\n(ranked by\nsimilarity score)"]
```

### 12.3 Diagram 12 — CBIR Search Sequence (PlantUML)

Full specification provided in Section 8.3 above. Key requirement: show all four participants (Customer, Vue SPA, .NET API, Python ML, PostgreSQL) with activation/deactivation on the ML sidecar.

### 12.4 Diagram Format Requirements

- All diagrams: 1200px width on generation, scaled to fit thesis text width
- PNG format (lossless) for PlantUML, PNG for Mermaid
- Consistent color palette across all diagrams:
  - External actors: gray/blue
  - System components: blue/teal
  - Databases: green
  - ML components: purple
- 10pt minimum font size in diagrams (legible in print)
- Output: `thesis/images/diagrams/` directory
- All diagrams have descriptive captions explaining what the reader should observe

### 12.5 Makefile

```makefile
# diagrams/Makefile
# Requires: mmdc (npm i -g @mermaid-js/mermaid-cli)
#           plantuml.jar (download from plantuml.com)
#           java (for PlantUML)

PLANTUML = java -jar plantuml.jar
MMDC = mmdc
OUTDIR = ../thesis/images/diagrams
PLANTUML_SRC = $(wildcard *.puml)
MMD_SRC = $(wildcard *.mmd)
PLANTUML_OUT = $(patsubst %.puml,$(OUTDIR)/%.png,$(PLANTUML_SRC))
MMD_OUT = $(patsubst %.mmd,$(OUTDIR)/%.png,$(MMD_SRC))

.PHONY: all clean plantuml mermaid

all: $(OUTDIR) plantuml mermaid

$(OUTDIR):
	mkdir -p $(OUTDIR)

plantuml: $(PLANTUML_OUT)

$(OUTDIR)/%.png: %.puml
	$(PLANTUML) -tpng -output $(OUTDIR) $<

mermaid: $(MMD_OUT)

$(OUTDIR)/%.png: %.mmd
	$(MMDC) -i $< -o $@ -w 1200 -b transparent

clean:
	rm -rf $(OUTDIR)
```

---

## 13. Writing Rules — Complete

### 13.1 Prohibited (automatic rejection if found)

| Category | Example of violation | Why prohibited |
|---|---|---|
| Code snippets | ````csharp public class Product { ... }```` | Thesis describes the system, not the code |
| File paths | `Module/Catalog/Domain/Products/Product.cs:17-43` | Codebase details, not academic content |
| Evidence columns | Tables with a column titled "Evidence" | This is a system audit pattern, not thesis writing |
| CLI commands | `dotnet test`, `git log`, `docker run` | Operational details, not thesis content |
| Git references | `commit abc123`, `git log --oneline` | Development history, not thesis content |
| Version numbers | `MediatR 12.4.1`, `PostgreSQL 17.2` | Implementation details that date quickly |
| Config excerpts | `appsettings.json`, `.env` contents | Configuration, not research |
| MD artifacts | `## headings`, ` ``` ` fences in thesis text | Markdown is not Typst |
| Directory trees | `src/Module/Catalog/Domain/...` | Codebase structure, not thesis content |
| Non-CTU formatting | Any raw markdown, HTML, or non-Typst markup | Thesis must use CTU Typst template |

### 13.2 Required (must be present)

- Academic narrative prose — every paragraph advances the thesis argument
- IEEE citations: `@author2024` syntax, rendered as numbered references `[1]` by CTU template
- CTU formatting: Times New Roman 13pt, 4cm left margin, 1.2 line spacing (from template)
- Table captions above tables, figure captions below figures (CTU standard)
- First-use expansion: "Content-Based Image Retrieval (CBIR)" on first mention in each chapter
- Cross-references: `@sec:label` for sections, `@fig:label` for figures, `@tbl:label` for tables
- Descriptive captions: every figure/table caption explains what the reader should observe or conclude

### 13.3 Per-Chapter Writing Workflow

For each chapter:

1. **Read both sources** — Open `_thesis` and `docs/thesis/` source files for that chapter
2. **Read the spec section** — Review this document's detailed section plan
3. **Write opening paragraph** — Hook the reader, state what this chapter covers
4. **Write section prose** — For each section:
   a. Start from `_thesis` prose as structural model
   b. Verify claims against current implementation (update outdated statements)
   c. Reference `docs/thesis/` only for content that extends scope (security, model config, benchmark results)
   d. Remove all code references, file paths, evidence columns
   e. Add IEEE citations where claims need backing
5. **Insert diagrams** — Place at natural break points with descriptive captions
6. **Write closing paragraph** — Bridge to the next chapter
7. **Build & verify** — `cd thesis && ctu-thesis build`
8. **Fix warnings** — Warnings-as-errors mindset; zero warnings is the goal
9. **Commit** — `feat(thesis): add chapter N — [title]`

---

## 14. File Organization

```
thesis/
├── info.typ                          # Student, thesis, keywords (already fixed)
├── main.typ                          # Rewrite: 7-chapter structure, CTU template
├── .ctu-thesisrc                     # CTU template metadata
├── frontmatter/
│   ├── cover.typ                     # CTU template
│   ├── inner-cover.typ               # CTU template
│   ├── evaluation.typ                # CTU template (student fills)
│   ├── acknowledgements.typ          # CTU template (student fills)
│   ├── table-of-contents.typ         # CTU template
│   ├── list-of-figures.typ           # CTU template
│   ├── list-of-tables.typ            # CTU template
│   ├── abbreviations.typ             # From _thesis info.typ
│   └── abstract.typ                  # New: 200-350 words
├── chapters/
│   ├── part1-introduction.typ        # Heading + include ch1
│   ├── part1/
│   │   └── ch1-introduction.typ      # Sections 1.1-1.6
│   ├── part2-content.typ             # Heading + includes ch2-6
│   ├── part2/
│   │   ├── ch2-background.typ        # Sections 2.1-2.6
│   │   ├── ch3-requirements.typ      # Sections 3.1-3.5
│   │   ├── ch4-architecture.typ      # Sections 4.1-4.6
│   │   ├── ch5-implementation.typ    # Sections 5.1-5.5
│   │   └── ch6-evaluation.typ        # Sections 6.1-6.5
│   ├── part3-conclusion.typ          # Heading + include ch7
│   └── part3/
│       └── ch7-conclusion.typ        # Sections 7.1-7.5
├── images/
│   ├── logo/                         # CTU logo (template asset)
│   └── diagrams/                     # Generated PNG from Makefile
├── backmatter/
│   ├── bibliography.bib              # IEEE references (20+ entries)
│   └── appendices.typ                # Benchmark raw data tables
└── template/
    ├── ctu-styles.typ                # CTU template (keep)
    └── i18n.typ                      # CTU template (keep, English only)

diagrams/                             # At worktree root
├── Makefile                          # Build all diagrams
├── 01-cbir-pipeline.mmd             # Diagram 1
├── 02-use-case.puml                 # Diagram 2
├── 03-c4-context.puml               # Diagram 3
├── 04-c4-container.puml             # Diagram 4
├── 05-c4-component.puml             # Diagram 5
├── 06-bounded-context-map.puml      # Diagram 6
├── 07-erd-core.mmd                   # Diagram 7
├── 08-order-state-machine.puml      # Diagram 8
├── 09-payment-state-machine.puml    # Diagram 9
├── 10-deployment.puml               # Diagram 10
├── 11-ml-pipeline.mmd                # Diagram 11
└── 12-cbir-search-sequence.puml     # Diagram 12
```

---

## 15. Non-Scope (explicit exclusions)

1. Vietnamese translations — English only
2. Code snippets of any language (C#, Python, TypeScript, SQL, shell)
3. 1:1 port of any `_thesis` or `docs/thesis/` file
4. The old confirmation page (not in current CTU template)
5. Cross-module code architecture details (namespace isolation, build targets)
6. Per-module feature listing beyond the summary in Ch3.2
7. Admin panel UI — beyond a brief mention in Ch5.5
8. Frontend component tree or Vue component descriptions
9. Dockerfile or CI/CD pipeline documentation
10. Performance profiling beyond benchmark results
11. Old `_thesis` numbering scheme (Roman numerals) — use CTU template defaults
12. `docs/thesis/13-proposal-options.md` — out of scope entirely
13. Code coverage percentages or test count statistics
14. Database migration scripts or schema DDL
