# Thesis Design Decisions — Complete Resolution Guide

This document records every architectural and scope decision made during the ReSys.Shop thesis documentation process. For each decision, three options were evaluated against the criteria of **examiner impact**, **effort required**, and **alignment with the thesis contribution** (software architecture: modular monolith, vertical slices, explicit error handling, CBIR integration).

---

## Q1: Thesis Level (BSc / MSc / PhD)

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **BSc** — implementation focus, brief literature review (5–8 papers), functional correctness + basic metrics, 40–60 pages | Fastest to complete; implementation quality speaks for itself; low literature burden | Less competitive for research positions; may not satisfy programs expecting research methodology | No |
| **B** | **MSc** — architectural process + systematic evaluation, 15–25 papers, quantitative metrics (coverage ≥70%, ML benchmarks), 80–120 pages | Demonstrates research methodology; strong for job applications and further study; matches project scope (8 modules, CBIR, full test suite) | Requires more time for literature review and benchmark execution | ✅ **Yes** |
| **C** | **PhD** — novel algorithmic or architectural contribution, 50+ papers, statistical significance testing, 200+ pages | Publishable as journal/conference papers; highest academic prestige | Scope far exceeds current codebase; would require 12+ months and novel contribution beyond integration work | No |

**Decision**: **B — MSc (Master of Science in Software Engineering)**

**Rationale**: The project scope (8 business modules, Fashion-CLIP CBIR integration, comprehensive test suite) provides sufficient depth for a Master's thesis without requiring novel algorithmic invention. The evaluation can be systematic (test coverage, ML metrics, architectural compliance audit) rather than statistically groundbreaking. A PhD would require inventing a new architectural pattern or a new CBIR technique, neither of which is within the current scope.

**Evidence**: `01-problem-analysis.md:1` — "Master of Science in Software Engineering"

---

## Q2: Scope Documentation Format

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Formal §1.4 "Scope and Delimitations"** with In-Scope deliverables table, Out-of-Scope table, and justification paragraph citing SE standards | Professional; signals discipline; examiners penalize scope ambiguity; demonstrates understanding of project boundaries | Requires ~2 hours of writing; must be updated if scope shifts | ✅ **Yes** |
| **B** | Simple table in §1.3 (no separate section, no justification) | Faster to write; less maintenance | Looks naive; examiners may ask "why was X excluded?" without written justification | No |
| **C** | Mention scope only in passing within the introduction | Minimal effort | Unacceptable for SE theses; demonstrates lack of planning | No |

**Decision**: **A — Formal §1.4 with tables and justification paragraph**

**Rationale**: A scope section is a standard SE thesis requirement. The justification paragraph explains *why* each out-of-scope item was deferred (e.g., CI/CD shifts focus from architecture to operations; YARP gateway adds ops complexity without research value). This preemptively answers examiner questions and demonstrates professional project management.

**Evidence**: `01-problem-analysis.md:§1.4`

---

## Q3: Requirement Numbering

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Formal hierarchical IDs** (`CAT-FR-01`, `ID-FR-02`, `ORD-NFR-03`) with cross-references in design chapters and Test Case IDs (`TC-CAT-001`) in the RTM | IEEE 29148 compliant; bidirectional traceability (req ↔ design ↔ test); professional standard for SE documentation | Requires updating all existing tables and adding cross-references; maintenance if chapters reorganize | ✅ **Yes** |
| **B** | Module prefix only (`Catalog-01`) but no cross-referencing throughout the document | Faster; less maintenance if structure changes | Examiners must manually search for traceability; weakens the "structured process" argument | No |
| **C** | Narrative only — describe requirements in prose without IDs | Reads like an essay; humanities-influenced examiners may prefer it | SE examiners penalize lack of traceability; almost never recommended for technical theses | No |

**Decision**: **A — Formal hierarchical numbering with Test Case IDs**

**Rationale**: Requirement traceability is a core SE competency. IEEE 29148 (Requirements Engineering) mandates unique identifiers for demonstrable coverage. The `TC-{Module}-{Seq}` convention (e.g., `TC-CAT-001`, `TC-ORD-002`) maps every requirement to a concrete test artifact, which is exactly what examiners look for when evaluating "did you follow a structured process?"

**Evidence**: Chapter 2 tables + `12-requirements-traceability-matrix.md`

---

## Q4: Accessibility (WCAG) and GDPR — Should These Appear as Explicit NFRs?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | Add 1-page GDPR section (data retention, right to erasure, lawful basis) and 1-page accessibility section (keyboard navigation, screen reader support) to Chapter 2 | Demonstrates compliance awareness; satisfies interdisciplinary examiners (law/ethics panels) | Adds ~2 pages that do not advance the architectural argument; GDPR is a legal concern, not a design contribution | No |
| **B** | Mention GDPR and accessibility as **out-of-scope constraints** in §1.4 with a sentence explaining the architecture focus | Acknowledges the concerns without dedicating chapters to them; honest about scope boundaries | May not satisfy examiners from law or HCI backgrounds | ✅ **Yes** |
| **C** | Ignore entirely unless the examiner specifically raises it | Minimal effort | Looks like an oversight; could trigger a "why didn't you consider privacy?" question at viva | No |

**Decision**: **B — Documented as out-of-scope in §1.4**

**Rationale**: The primary contribution of this thesis is software architecture — modular monolith, vertical slices, explicit error handling, and CBIR integration. GDPR and WCAG are important but orthogonal to these contributions. They are operational/legal compliance concerns that would require a separate thesis to address properly. By documenting them as consciously deferred, the thesis demonstrates scope discipline rather than negligence.

**Evidence**: `01-problem-analysis.md:§1.4` — Out of Scope table includes "CI/CD Pipeline" and "GDPR/privacy compliance" as explicitly deferred items

---

## Q5: Diagram Format — ASCII/Text, Mermaid, or PlantUML?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Formal Mermaid** (`.mmd` files) — renderable in GitHub, GitLab, VS Code, Notion; convertible to SVG/PNG via CLI | Standard syntax; version-controllable; free; renders natively in modern tools; no proprietary software required | Slightly less feature-rich than PlantUML for complex layouts; requires `mmdc` CLI for PDF export | ✅ **Yes** |
| **B** | PlantUML/Structurizr — richer notation, supports C4 natively | More mature ecosystem; better C4 support; prettier default styling | Requires Java runtime; external tooling dependency; not natively rendered in GitHub | No |
| **C** | ASCII/text diagrams embedded in Markdown | Zero tooling; works in any text editor; sufficient for drafts | Unprofessional for final submission; examiners skip them; hard to read cardinality and relationships | No |

**Decision**: **A — 18 formal Mermaid diagrams**

**Rationale**: Mermaid is the pragmatic choice for a thesis that lives in version control. It renders in GitHub (no export step needed for sharing with supervisor), converts to publication-quality SVG via a single CLI command (`mmdc`), and requires no external dependencies beyond Node.js. The 18 diagrams cover all required diagram types (C4 Context/Container/Component, Deployment, Use Case, 3 Sequence diagrams, 5 Class diagrams, ERD, 2 State Machines, ML Pipeline, Bounded Context Map).

**Evidence**: `diagrams/*.mmd`

---

## Q6: Microservices Justification — How Much Depth?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Decision table** (modular monolith vs microservices vs clean architecture) + **3 paragraphs** of prose rationale | Demonstrates critical thinking without excessive length; proportional to the decision's importance; examiners reward "why" not just "what" | Some examiners want deeper analysis (e.g., ADR template) | ✅ **Yes** |
| **B** | Full ADR (Architectural Decision Record) using MADR template — Context, Decision, Consequences, Status | Very professional; ADRs are industry standard (GitHub, AWS); demonstrates formal decision discipline | Adds 2–3 pages; may be overkill for BSc; MADR is more common in industry docs than academic theses | No |
| **C** | Reference external authority only (e.g., cite Newman, *Monolith to Microservices*) without own reasoning | Authority by citation; low effort | Weakens the thesis — it looks like you borrowed someone else's conclusion rather than reasoning through it yourself | No |

**Decision**: **A — Table + 3 paragraphs of design rationale**

**Rationale**: A decision table addresses the *what* (which alternatives were considered). The 3 paragraphs address the *why* (why microservices introduce accidental complexity for a single-developer thesis; why per-module assemblies create build overhead; why modular monolith optimizes for demonstrability and ACID consistency). This combination satisfies examiners who want both breadth of awareness and depth of reasoning.

**Evidence**: `03-system-architecture.md:§3.1.1`

---

## Q7: Class Diagrams — Per-Aggregate or Full Assembly?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **4–6 focused class diagrams**, one per major aggregate (Product, Order, Payment, Identity) | Readable; each fits on one page; shows depth without overwhelming; easy to update if domain model changes | Does not show cross-aggregate relationships at a glance | ✅ **Yes** |
| **B** | One giant diagram of all domain entities and relationships | Comprehensive; shows the full picture | Unreadable in print (spans 3+ pages); examiners skip it; requires Enterprise Architect or similar tool; high maintenance when any entity changes | No |
| **C** | No class diagrams — rely on ERD for structural relationships and sequence diagrams for behavior | ERDs are standard for DB-centric theses; less maintenance | Doesn't show method signatures, inheritance, or behavioral responsibilities; weak for OO-oriented examiners | No |

**Decision**: **A — 4 per-aggregate class diagrams**

**Rationale**: Per-aggregate diagrams are the standard practice in DDD documentation (Vernon, *Implementing Domain-Driven Design*). Each diagram fits on a single page, shows the entities, value objects, enums, and key methods within one consistency boundary, and is maintainable when the model evolves. The 4 chosen aggregates (Product, Order, Payment, Identity) represent the most complex and most examinable domains in the system.

**Evidence**: `diagrams/class-product-aggregate.mmd`, `class-order-aggregate.mmd`, `class-payment-aggregate.mmd`, `class-identity-aggregate.mmd`

---

## Q8: DDD Expectations — Bounded Context Map and Ubiquitous Language?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Both** — Bounded Context Map (showing 8 contexts + integration pattern) + Ubiquitous Language Glossary (20+ defined terms) | Demonstrates full DDD literacy; map shows architectural boundaries; glossary prevents "translation friction" between domain experts and examiners | Adds ~3 pages; requires careful maintenance if terms change | ✅ **Yes** |
| **B** | Bounded Context Map only | Shows architectural decomposition; satisfies diagram-hungry examiners | Glossary is what makes DDD *actionable* — without it, the map is just boxes | No |
| **C** | Neither — mention DDD in passing without formal artifacts | Minimal effort | Examiner who knows DDD (e.g., read Evans or Vernon) will penalize the lack of concrete artifacts | No |

**Decision**: **A — Both BC Map and UL Glossary**

**Rationale**: If the thesis claims to use Domain-Driven Design (which it does — Chapter 4 is titled "Domain Analysis"), the examiner expects to see DDD artifacts. A Bounded Context Map without a Ubiquitous Language is like a class diagram without method names — it shows structure but not meaning. The glossary of 20+ terms (Product, Variant, Master Variant, CheckoutState, Embedding, CBIR, etc.) ensures every domain term used in the thesis is precisely defined and traceable to a class or enum in the codebase.

**Evidence**: `04-domain-analysis.md:§4.1a` (BC Map), `§4.6` (UL Glossary)

---

## Q9: ERD Format — Formal Diagram or Textual?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Formal Mermaid ER diagram** (`erDiagram` syntax) with cardinality, keys, and relationships | Visually clear; standard notation; examiners can read cardinality at a glance; fits in thesis PDF when rendered to SVG | Requires rendering step for PDF; Mermaid ER syntax is newer and less mature than class diagram syntax | ✅ **Yes** |
| **B** | External tool (dbdiagram.io, draw.io, Enterprise Architect) | More mature ER notation; richer styling options; better print quality | Requires proprietary or web-based tool; not version-controllable; link may break before submission | No |
| **C** | Textual ASCII representation in Markdown | Zero tooling; works everywhere | Unprofessional; cardinality is hard to read; relationships get messy with >10 entities | No |

**Decision**: **A — Formal Mermaid ER diagram**

**Rationale**: Database design is a core SE thesis chapter. A visual ERD communicates schema structure far more effectively than prose. Mermaid's `erDiagram` syntax supports cardinality (`||--o{`), primary keys, and foreign keys — everything needed for a thesis-grade ERD. The diagram is version-controllable alongside the code and renders in the same toolchain as all other diagrams.

**Evidence**: `diagrams/erd-core.mmd`

---

## Q10: Database Normalization — Discuss 3NF/BCNF?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **1 paragraph** explaining 3NF compliance + intentional denormalization of `order.total` | Shows awareness of normalization; justifies the only denormalization; standard practice for SE theses | Does not satisfy theoretically-minded examiners who want formal proofs | ✅ **Yes** |
| **B** | Formal normalization proof (prove 3NF or BCNF for each major table by showing no partial/transitive dependencies) | Extremely rigorous; impresses database researchers | Tedious; adds 3–4 pages of formal notation that most SE examiners skim; overkill unless DB is the primary contribution | No |
| **C** | Skip normalization discussion entirely | Saves space | SE examiners expect at least passing acknowledgment of normalization; skipping it looks naive | No |

**Decision**: **A — Brief paragraph on 3NF + `order.total` denormalization**

**Rationale**: A single paragraph is the sweet spot. It demonstrates that the designer understands Third Normal Form (no repeating groups, no transitive dependencies, every non-key attribute depends on the key) while also showing the maturity to intentionally denormalize for performance. The `order.total` field is the classic example: it is computed by the domain method `Order.Method.Checkout.cs` and stored redundantly to avoid recalculation during high-frequency reads (order listing, admin dashboard). The trade-off is justified and centralized in the domain layer, not scattered in SQL triggers.

**Evidence**: `05-database-design.md:§5.1a`

---

## Q11: OpenAPI Specification — Include Exported `openapi.json`?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | Include a full exported `openapi.json` in the thesis appendix | Comprehensive; shows the complete API surface; useful as a reference | 5,000+ lines of JSON; unreadable in a PDF thesis; examiners skip it; does not demonstrate design *thinking* | No |
| **B** | **Representative endpoint samples** (3–4 examples) with request/response models inline in Chapter 6 | Readable; demonstrates API design thinking; shows error envelope, auth headers, and DTO structure; directly traceable to requirements | Does not show the *entire* API; may require examiner to trust that other endpoints follow the same pattern | ✅ **Yes** |
| **C** | Describe endpoints in prose only (no examples) | Flexible; easy to write | Examiners want concrete evidence; prose alone does not demonstrate REST design competency | No |

**Decision**: **B — Representative samples in Chapter 6**

**Rationale**: An exported OpenAPI spec is a *reference document*, not a *design document*. The thesis must demonstrate design decisions (why `Result<T>` envelope, why JWT Bearer, why `X-CSRF-TOKEN` header) — and this is best done through 3–4 representative endpoints: Create Product (admin, authenticated, validation), Search by Image (storefront, multipart upload, CBIR), and Checkout (critical path, transaction, nested state machine). These examples show the full range of API design concerns without drowning the reader in JSON.

**Evidence**: `06-api-design.md:§6.3`

---

## Q12: API Standards — JSON:API, OData, HAL?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | Adhere to JSON:API or HAL (hypermedia) specification | Industry standard for mature APIs; enables HATEOAS; impressive to API-focused examiners | Adds significant complexity (link generation, relationship serialization); not required for thesis scope; overkill for internal API | No |
| **B** | **Level 2 REST** (Richardson Maturity Model) — HTTP verbs + resources + standard status codes | Sufficient for thesis demonstrability; matches the actual implementation (Carter minimal APIs with `MapGet`/`MapPost`); pragmatic | Does not demonstrate hypermedia design; may look simplistic to API specialists | ✅ **Yes** |
| **C** | Not discussed — assume REST is implicit | Minimal effort | Examiners who know Richardson's model will ask "what level?" and expect an explicit answer | No |

**Decision**: **B — Level 2 REST is sufficient**

**Rationale**: The Richardson Maturity Model defines 4 levels: Level 0 (RPC over HTTP), Level 1 (Resources), Level 2 (HTTP verbs), Level 3 (Hypermedia / HATEOAS). ReSys.Shop operates at Level 2: it uses proper HTTP verbs (`GET`, `POST`, `PUT`, `DELETE`), returns appropriate status codes (`201 Created`, `400 Validation`, `409 Conflict`), and structures URIs around resources (`/api/admin/catalog/admin/products`). Level 3 (JSON:API, HAL) adds hypermedia links and relationship embedding — valuable for public APIs, but unnecessary for a thesis backend where the frontends are co-developed and the contract is stable.

**Evidence**: `06-api-design.md:§6.1`

---

## Q13: Sequence Diagram Format — ASCII or Formal UML?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Formal Mermaid sequence diagrams** with actors, lifelines, activation bars, alt/else fragments | Standard UML notation; shows exact message flow; supports conditional logic (`alt Validation fails`); renderable in GitHub/VS Code | Requires rendering for PDF; Mermaid sequence syntax has some rendering quirks with complex loops | ✅ **Yes** |
| **B** | PlantUML sequence diagrams | Better rendering quality; more mature sequence diagram support; supports notes and dividers natively | Requires Java; not version-controllable alongside code in the same way | No |
| **C** | ASCII/text sequence diagrams | Zero tooling; sufficient for drafts | Unprofessional; activation bars and alt fragments are nearly impossible to read; examiners skip them | No |

**Decision**: **A — 3 Mermaid sequence diagrams**

**Rationale**: Sequence diagrams are the most important behavioral diagrams in a thesis. They show the *dynamic* interaction between components — exactly what examiners need to verify that the architecture actually works. The 3 chosen flows (Create Product, Checkout, Image Search) cover the full range of system behavior: a standard CRUD admin feature, the highest-stakes business flow (money-moving), and the ML-powered CBIR flow. Each diagram includes conditional logic (`alt Validation fails`), nested command dispatch (`ISender.Send`), and external service calls (Stripe, Python sidecar).

**Evidence**: `diagrams/sequence-create-product.mmd`, `sequence-checkout.mmd`, `sequence-image-search.mmd`

---

## Q14: Full Class Diagram for Entire Module Assembly?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | Create one giant diagram of all 50+ domain entities across all 8 modules | Comprehensive; shows everything at a glance | Unreadable in print (would span 3+ pages at readable font size); requires specialized tool (Visual Studio Class Designer, Enterprise Architect); unmaintainable when any entity changes | No |
| **B** | **Representative per-aggregate diagrams only** (4–6 focused diagrams) | Readable; each fits on one page; shows depth without overwhelming; maintainable | Does not show cross-aggregate relationships in one view | ✅ **Yes** |
| **C** | Skip class diagrams entirely — use ERD and sequence diagrams instead | ERD covers structure; sequence diagrams cover behavior | Class diagrams show *behavioral responsibilities* (methods) and inheritance that ERDs cannot express; skipping them weakens the OO design argument | No |

**Decision**: **B — 4 per-aggregate class diagrams**

**Rationale**: A class diagram of 50+ entities is a "wall of text" in visual form — examiners skip it. Four focused diagrams (Product aggregate, Order aggregate, Payment aggregate, Identity aggregate) each tell a coherent story. The Product diagram shows the variant hierarchy and embedding. The Order diagram shows the checkout state machine and total calculation. The Payment diagram shows the capture/refund lifecycle. The Identity diagram shows the permission-based authorization model. Together they cover the most examinable parts of the domain.

**Evidence**: `diagrams/class-product-aggregate.mmd`, `class-order-aggregate.mmd`, `class-payment-aggregate.mmd`, `class-identity-aggregate.mmd`

---

## Q15: Threat Model — STRIDE-Per-Element or Layered Controls?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | Full STRIDE-per-element table for 3 critical elements (Order, PaymentIntent, JWT) mapping all 6 threat categories | Very thorough; demonstrates structured security thinking; impresses security-focused examiners | Adds ~3–4 pages; overkill for an architecture thesis where security is a supporting concern, not the primary contribution; most SE examiners will skim it | No |
| **B** | **Defense-in-depth layered controls table** (network, auth, authz, transport, input, data, observability) + note that exhaustive STRIDE is prepared on request | Focused on architectural security controls (the thesis contribution); proportional; honest about scope boundaries | May not satisfy examiners with a security background who expect formal threat modeling | ✅ **Yes** |
| **C** | No threat discussion at all | Saves space | SE examiners expect at least awareness of security threats; complete omission looks naive | No |

**Decision**: **B — Layered controls table + deferred STRIDE**

**Rationale**: The thesis contribution is software architecture, not security research. A full STRIDE analysis (18 threat rows × mitigations × gaps) adds significant length without advancing the primary argument about modular monoliths, vertical slices, or CBIR. The layered controls table in §8.1 already demonstrates defense-in-depth awareness. A note in §8.1a explicitly states that exhaustive STRIDE is prepared and can be appended if the examiner requests it — this is the professional middle ground.

**Evidence**: `08-security-design.md:§8.1a`

---

## Q16: GDPR Privacy-by-Design — Data Retention and Erasure Documentation?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | Add a 1-page "Privacy Impact Assessment" subsection in Chapter 8 explaining soft-deletion rationale, retention policy, and erasure procedure | Demonstrates privacy awareness; satisfies GDPR-conscious examiners | Adds length without advancing the architectural contribution; privacy law is outside the thesis domain | No |
| **B** | Add a brief paragraph in §1.4 (Scope) noting that full GDPR compliance is deferred | Acknowledges the concern; honest about boundaries | May still trigger questions if the examiner is privacy-focused | No |
| **C** | **Omitted** — soft-deletion design (`IsDeleted` flags) is documented in the domain model only; no separate privacy framing | Keeps focus on architecture; `IsDeleted` is a technical pattern, not a GDPR mechanism | If the examiner is from a law/ethics panel, they may question the omission | ✅ **Yes** |

**Decision**: **C — Not included as a dedicated section**

**Rationale**: The system uses soft deletion (`IsDeleted`, `DeletedAtUtc`, `DeletedBy`) on all business entities. This is documented as a cross-cutting domain concern in Chapter 4 (Domain Analysis) — it enables audit trails and recoverable deletion. However, framing it as "GDPR compliance" would imply a legal analysis that the thesis does not provide. The thesis is a software engineering document, not a legal one. The soft-deletion pattern is justified by operational needs (audit, recovery), not by GDPR Article 17 (right to erasure).

**Evidence**: `04-domain-analysis.md:§4.2` (aggregate tables show soft deletion fields)

---

## Q17: Deployment Diagram — Formal Cloud Vendor Icons or Conceptual?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | Formal deployment diagram with AWS/Azure/GCP-specific icons, load balancer types, managed service names | Looks professional; demonstrates cloud literacy; useful if targeting a DevOps role | Misleading — the system has no Dockerfiles, no CI/CD, and no cloud deployment; naming a vendor implies commitment not implemented | No |
| **B** | **Conceptual block diagram** showing generic containers (API, Python sidecar, PostgreSQL, Redis, CDN) without vendor naming | Honest about thesis scope; focuses on component topology rather than vendor specifics; sufficient for demonstrating deployment thinking | Less visually impressive than vendor-specific diagrams | ✅ **Yes** |
| **C** | No deployment diagram at all | Saves space | Deployment architecture is a standard SE thesis chapter; omitting it weakens the "complete system" argument | No |

**Decision**: **B — Conceptual deployment diagram**

**Rationale**: Since the thesis explicitly defers Dockerfiles, CI/CD, and production deployment (§1.4 Out of Scope), naming a cloud vendor (AWS, Azure, GCP) would be misleading. A conceptual diagram showing generic containers, load balancing, and data tiers is honest about the current state while still demonstrating awareness of production topology. The diagram shows horizontal scaling of API containers, read replicas for PostgreSQL, and CDN hosting for static SPA bundles — all standard practices without vendor lock-in.

**Evidence**: `09-deployment-design.md:§9.1.3` + `diagrams/deployment.mmd`

---

## Q18: Cloud Platform Target — Name a Vendor?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | Target AWS (EKS, RDS, S3, ElastiCache) | Concrete; examiners can evaluate against real services | Commitment not implemented; thesis has no Terraform/CloudFormation; adds ops complexity without research value | No |
| **B** | Target Azure (AKS, Azure SQL, Blob Storage) | Concrete; good if program is Microsoft-aligned | Same as AWS — commitment without implementation | No |
| **C** | **Generic** — no vendor named; describe "a cloud environment with container orchestration" | Honest; avoids false commitment; focuses on architecture (what components exist and how they interact) rather than operations (how to provision them) | Less specific; may not satisfy examiners who want concrete deployment evidence | ✅ **Yes** |

**Decision**: **C — Generic, no vendor named**

**Rationale**: The deployment design in Chapter 9 is conceptual because the actual implementation stops at local development (Aspire orchestration). There are no Terraform files, no Kubernetes manifests, no Dockerfiles, and no CI/CD pipeline. Naming a cloud vendor would create an expectation that the thesis does not fulfill. The conceptual diagram is sufficient to show that the *architecture* is deployable (stateless API containers, separate Python sidecar, external PostgreSQL and Redis) without pretending the *operations* are solved.

**Evidence**: `09-deployment-design.md:§9.1.3`

---

## Q19: Test Plan Matrix — Requirements × Test Levels?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Full Requirements Traceability Matrix** mapping 40 FRs + 10 NFRs → design chapters → implementation files → test files → status | IEEE 829 compliant; bidirectional traceability; demonstrates complete coverage awareness; professional standard | High maintenance; must be updated if requirements or tests change; ~6 pages | ✅ **Yes** |
| **B** | Simple checklist (requirements listed, tests checked off) | Faster; less maintenance | Weak traceability — no mapping to design or implementation; looks like a todo list rather than engineering documentation | No |
| **C** | No matrix — describe testing approach in prose only | Minimal effort | Examiners penalize lack of traceability; prose cannot demonstrate coverage completeness | No |

**Decision**: **A — Full RTM with 40 FRs, 10 NFRs, coverage summary, and gaps table**

**Rationale**: The RTM is the single most important evidence of "structured process" that an examiner evaluates. It proves that every requirement has a design artifact, an implementation file, and a test file. The gaps table (Inventory/Shipping/Profile modules have minimal tests) is equally important — it shows honesty and provides a roadmap for future work. IEEE 829 (Test Documentation) explicitly recommends traceability matrices for this purpose.

**Evidence**: `12-requirements-traceability-matrix.md`

---

## Q20: Coverage Target — Expected Percentage?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **70% statement coverage** for backend (Module + Shared assemblies) | Achievable; honest; avoids gaming the metric with trivial property tests; sufficient for thesis demonstrability | Some industrial standards gate on 80%; may look low to examiners from industry | ✅ **Yes** |
| **B** | 80% statement coverage | Industry standard; impressive number | May require writing meaningless tests (auto-property getters/setters) just to hit the number; examiners see through this | No |
| **C** | No numeric target — describe coverage qualitatively by feature domain | Avoids metric gaming; honest about uneven test distribution | Many programs require a numeric threshold; looks evasive | No |

**Decision**: **A — ≥70% statement coverage**

**Rationale**: 70% is the pragmatic sweet spot. It ensures all domain methods, handlers, and validators have at least one test path without requiring trivial tests for `public string Name { get; set; }` properties. The current estimated coverage is ~60–70% (higher in Ordering/Payment, lower in Inventory/Shipping/Profile). The RTM honestly documents this gap. A 70% target is defensible: "We prioritized testing the money-moving and customer-facing modules; inventory and shipping are tested at the API level via integration tests rather than unit tests."

**Evidence**: `10-testing-strategy.md:§10.6`

---

## Q21: Benchmarks — Run Now or Methodology Only?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Methodology in draft; numbers in final** — describe evaluation framework (metrics, datasets, procedures) now; run benchmarks before submission | Numbers reflect final system state; parallelizes work (write evaluation chapter while implementation continues); standard practice | Risk of forgetting to run benchmarks; final crunch if results are poor | ✅ **Yes** |
| **B** | Run all benchmarks now and include in draft | Draft is complete; no last-minute work | If you refactor after benchmarking, numbers change and must be re-run; footnotes about "commit hash" look defensive | No |
| **C** | No benchmarks — argue that architecture quality is demonstrated by structural properties (module isolation, explicit error handling) rather than runtime metrics | Philosophically valid (Shaw & Garlan); works if benchmarks are technically infeasible | Weak unless examiner is an architecture theorist; most SE examiners expect some quantitative evidence | No |

**Decision**: **A — Methodology for draft; numbers for final submission**

**Rationale**: Draft theses are evaluated on methodology soundness, not on results. The evaluation chapter should answer: "How would you know if this works?" — and that is a methodology question. The actual numbers come after the implementation stabilizes. This approach also allows the student to write Chapters 10–11 in parallel with final implementation work.

**Evidence**: `11-evaluation.md:§11.1`

---

## Q22: ML Statistical Analysis — Confidence Intervals or Mean ± SD?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Recall@K + Precision@K with mean ± standard deviation** across a 100-image ground-truth dataset | Standard CBIR evaluation; easy to compute; sufficient for MSc; interpretable | Less rigorous than significance testing; does not prove superiority over baselines | ✅ **Yes** |
| **B** | Comparison against baseline (e.g., ResNet-50) + paired t-test or Wilcoxon signed-rank test | Demonstrates that Fashion-CLIP is statistically superior; publication-ready | Requires running a second model; more computation; statistical testing adds complexity without advancing the architectural thesis | No |
| **C** | Qualitative demonstration only (show 3 example queries with top-5 results) | No computation needed; acceptable for BSc | Not quantitative; weak for MSc; examiners expect measurable evidence | No |

**Decision**: **A — Mean ± SD across 100-image dataset**

**Rationale**: For a thesis where the ML component is integrated (not invented), the standard evaluation is sufficient. Recall@20 measures "of the 9 similar items in the group, how many were retrieved in top-20?" Precision@20 measures "of the 20 retrieved items, how many are actually similar?" Reporting mean ± SD across 100 queries shows consistency. A standard deviation < 0.15 indicates reliable retrieval; higher variance flags edge cases (unusual patterns, accessories) that the model struggles with — useful discussion material.

**Evidence**: `11-evaluation.md:§11.5`

---

## Q23: User Study — SUS / Task-Based Testing?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Skip entirely** — thesis contribution is architectural (modular monolith, vertical slices, CBIR), not HCI | Keeps focus sharp; avoids scope creep; honest about contribution boundaries | If thesis title mentions "user experience" or "usability," examiners will expect it | ✅ **Yes** |
| **B** | Lightweight SUS appendix (10 questions, 5-point Likert, 5 volunteer participants) | Minimal effort (~1 page); demonstrates awareness of usability; signals professionalism | 5 participants is not statistically significant; may look tokenistic | No |
| **C** | Full task-based testing (10 participants, time-to-completion, error rate, SUS) | Strong evidence of system usability; required if HCI is a secondary contribution | 1–2 weeks of scheduling; may require ethics approval; shifts focus away from architecture | No |

**Decision**: **A — Not included**

**Rationale**: The thesis evaluates software architecture — specifically, whether a modular monolith with vertical slices and explicit error handling can support a CBIR-powered e-commerce system. Usability is a quality attribute of the frontend, not of the architecture. The Storefront and Admin SPAs exist as proof-of-concept clients that exercise the API; their UX refinement is deferred to future work. If an examiner specifically demands usability evidence, a lightweight SUS appendix can be added without expanding the core scope.

**Evidence**: `11-evaluation.md:§11.6`

---

## Q24: RTM Test Case IDs — Formal `TC-001` Style?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Formal TC IDs** using `TC-{Module}-{Sequence}` (e.g., `TC-CAT-001`, `TC-ORD-002`) | IEEE 829 compliant; professional traceability; easy to sort and reference; each test case has a unique identity | Adds maintenance if tests are renamed or reorganized | ✅ **Yes** |
| **B** | Use file paths as identifiers (e.g., `Module.UnitTests/Catalog/Features/.../CreateProduct.Tests.cs`) | No synthetic IDs to maintain; directly traceable | Verbose; breaks if files move; not sortable by sequence | No |
| **C** | No test case IDs — keep the RTM without a synthetic identifier column | Less work | Less formal; examiners may ask for more structure; harder to reference specific tests in discussion | No |

**Decision**: **A — Formal `TC-{Module}-{Seq}` IDs**

**Rationale**: A test case ID is the bridge between "I tested this" and "here is the proof." Without IDs, the RTM reads as a list of file paths. With IDs, it becomes an auditable registry. The `TC-CAT-001` convention groups tests by module (Catalog, Ordering, Payment) and sequence, making it easy to spot gaps (e.g., "TC-INV-001 through TC-INV-005 exist, but TC-INV-006 is missing — that's the stock transfer test we still need to write").

**Evidence**: `12-requirements-traceability-matrix.md`

---

## Q25: Actual Coverage Numbers — Populate from Coverlet?

| Option | Description | Pros | Cons | Selected? |
|--------|-------------|------|------|-----------|
| **A** | **Populate at final submission** via `dotnet test /p:CollectCoverage=true` on the final codebase snapshot | Numbers are fresh and accurate; reflect the actual system state at evaluation time; no risk of drift | Risk of forgetting; requires a final build-and-test run before submission deadline | ✅ **Yes** |
| **B** | Populate now with a footnote: "Coverage measured on commit `abc1234`; subject to change" | Draft is complete; no last-minute work | Numbers may drift; footnote looks defensive; requires updating if tests are added | No |
| **C** | Never populate — explain that coverage is opt-in and per-run | Intellectually honest; avoids false precision | May not satisfy programs with mandatory coverage thresholds; looks evasive | No |

**Decision**: **A — Populate at final submission**

**Rationale**: Coverage percentages are meaningless if measured on an intermediate snapshot. A student might write "75% coverage" in the draft, then add 10 new tests before submission and actually achieve 82% — or remove some tests and drop to 68%. The `[TODO — Final Submission]` marker is the honest and professional approach: the methodology is fully described, the target (≥70%) is stated, and the exact number will be measured when the codebase is frozen.

**Evidence**: `10-testing-strategy.md:§10.6`, `11-evaluation.md:§11.1`

---

## Summary: All 25 Decisions

| # | Question | Selected | Key Rationale |
|---|----------|----------|---------------|
| 1 | Thesis level | **MSc** | Matches project scope; PhD requires novel algorithmic contribution |
| 2 | Scope format | Formal §1.4 | Demonstrates project boundary discipline |
| 3 | Requirement numbering | `CAT-FR-01` / `TC-CAT-001` | IEEE 29148 traceability |
| 4 | Accessibility/GDPR | **Omitted** | Architecture-focused, not compliance-focused |
| 5 | Diagram format | **Mermaid** (18 files) | Version-controllable; renders in GitHub |
| 6 | Microservices justification | Table + 3 paragraphs | Critical thinking without excessive length |
| 7 | Class diagrams | 4 per-aggregate | Readable; maintainable; standard DDD practice |
| 8 | DDD artifacts | BC Map + UL Glossary | Required if claiming DDD in thesis |
| 9 | ERD format | **Mermaid** | Standard ER notation; version-controllable |
| 10 | Normalization | 3NF + denormalization paragraph | Shows awareness + maturity |
| 11 | OpenAPI spec | **No** — samples only | Spec is reference, not design evidence |
| 12 | API standards | Level 2 REST | Sufficient for thesis; hypermedia overkill |
| 13 | Sequence diagrams | **Mermaid** (3 flows) | Standard UML; shows dynamic behavior |
| 14 | Full class diagram | **No** | Giant diagrams are unreadable |
| 15 | Threat model | Controls table only | STRIDE prepared but deferred unless requested |
| 16 | GDPR privacy | **Omitted** | Soft deletion documented; no legal analysis |
| 17 | Deployment diagram | Conceptual | Honest about no production deployment |
| 18 | Cloud vendor | **Generic** | No false commitment to unimplemented ops |
| 19 | Test plan matrix | **Full RTM** | IEEE 829; demonstrates coverage awareness |
| 20 | Coverage target | **70%** | Achievable; honest; avoids metric gaming |
| 21 | Benchmarks | Methodology now, numbers final | Standard practice; numbers must reflect final state |
| 22 | ML statistics | Mean ± SD | Standard CBIR evaluation for MSc |
| 23 | User study | **Skipped** | Architectural thesis, not HCI |
| 24 | TC IDs | `TC-{Module}-{Seq}` | Professional traceability |
| 25 | Coverage numbers | **[TODO Final]** | Measured on frozen codebase |

---

## Remaining Work Before Submission

| Task | Status | Action | When |
|------|--------|--------|------|
| Run `dotnet test /p:CollectCoverage=true` and populate percentages | ⏳ | Execute command; extract `cobertura` JSON; update Ch 10, Ch 11, RTM | Final submission |
| Execute Fashion-CLIP benchmark (100-image ground-truth dataset) | ⏳ | Build dataset → generate embeddings → query pgvector → compute Recall@20 / Precision@20 | Final submission |
| Measure end-to-end checkout latency | ⏳ | Integration test with `Stopwatch` or `k6` load test | Final submission |
| Render Mermaid diagrams to SVG/PNG | ⏳ | `mmdc -i file.mmd -o file.svg` for all 18 diagrams | Before PDF assembly |
| Literature review citations | ⏳ | Add 15–25 papers to Ch 1–3; cite Evans, Vernon, Newman, Shaw & Garlan, Richardson, Microsoft STRIDE | Draft completion |
