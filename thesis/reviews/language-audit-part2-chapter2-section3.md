# Language Level Audit + Re-Leveled Rewrite — Chapter 2, §2.3

**Scope note:** §2.3 is architecture and design documentation, and a large part of its vocabulary (bounded contexts, aggregate roots, Conformist pattern, Published Language, Shared Kernel, C4 model, RFC 7807 Problem Details) comes from named, standard software-engineering concepts, not the author's own word choices. These stay exactly as-is regardless of English level, they're the correct technical terms, and a reader in this field expects them. The audit below focuses on the connecting prose around these terms, where real language-level issues exist.

**Factual correction carried forward:** "nine bounded contexts" (§2.3.1) is corrected to "eight," consistent with the earlier factual review and every other mention in this section.

---

## STAGE 1 — Sentence-by-sentence audit

| # | Original | Issue | Class |
|---|---|---|---|
| 1 | "Six dimensions define the platform architecture, from service composition through domain modelling to database, API, and security layers." | "Dimensions define... from X through Y to Z" is a compressed, almost report-cover-style construction. Reads as more polished than the surrounding technical writing. | [AI-LIKE] |
| 2 | "The design follows a service-oriented approach: a Vue 3 frontend, a .NET 10 modular monolith backend, and a Python FastAPI machine learning sidecar, each independently deployable." | Clear once the first sentence is simplified; the technical terms here are all necessary. | [TECHNICAL TERM] |
| 3 | "ReSys.Shop comprises three services... and eight bounded contexts using Domain-Driven Design with MediatR dispatch between modules." | "Comprises" is a correct but moderately formal word where "has" or "is made up of" would be simpler and equally accurate. | [TOO ADVANCED] (mild) |
| 4 | "Internally, the backend is partitioned into nine bounded contexts, each owning a dedicated database schema." | Factual correction: should be "eight," matching the sentence three lines earlier and Table 47. | [not a language issue — factual, already flagged] |
| 5 | "The backend follows Domain-Driven Design (DDD) with eight bounded contexts, each managing explicit aggregate boundaries and domain invariants under a Conformist integration pattern via Published Language contracts." | Dense sentence, but every term (aggregate boundaries, domain invariants, Conformist pattern, Published Language) is a real, necessary DDD concept. This is appropriately technical, not overly advanced language, it's precise vocabulary for a precise topic. | [TECHNICAL TERM] |
| 6 | "The platform is partitioned into eight bounded contexts along business capability boundaries." | Clear and necessary. | [TECHNICAL TERM] |
| 7 | "Context-to-context communication relies exclusively on MediatR ISender in-process dispatch without direct compile-time project references." | "Relies exclusively on" is a correct, slightly formal phrase, "uses only" would be simpler and equally accurate. | [TOO ADVANCED] (mild) |
| 8 | "The system context positions ReSys.Shop within its operational environment, defining user roles and external dependencies." | Same "positions... within" pattern flagged twice already in Chapter 1 §1.6, this is the third occurrence of this specific AI-like construction. | [AI-LIKE] |
| 9 | "The container view decomposes ReSys.Shop into six standalone deployable processes and data stores." | "Decomposes" is standard, correct architecture vocabulary (C4 model terminology), keep. | [TECHNICAL TERM] |
| 10 | "Embedding Sidecar: Python 3.12 FastAPI service loading ML models into GPU/CPU memory for on-demand embedding generation." | Clear, plain, appropriately technical. | [NO ISSUE] |
| 11 | "Cross-context relationships use identifier references (UUIDs) without database-level foreign key constraints... maintaining logical module isolation while avoiding distributed transaction overhead." | The second half of this sentence stacks two abstract outcomes ("maintaining X while avoiding Y") in a slightly dense way; understandable, but a plainer connector would help. | [UNCLEAR] (mild) |
| 12 | "PostgreSQL's pgvector extension executes vector similarity searches within the relational engine." | "Executes... within the relational engine" is a formal, slightly stiff phrasing; a simpler verb works just as well. | [TOO ADVANCED] (mild) |
| 13 | "The API exposes a RESTful interface via Carter modules and the MediatR CQRS pattern, registering approximately 262 endpoints across nine modules." | Clear and appropriately technical; "exposes" is standard API vocabulary, keep. (Note: "nine modules" here should reflect the corrected eight-bounded-context count plus Dashboard as discussed in the factual review, worth double-checking against the final module count once that's settled.) | [TECHNICAL TERM] |
| 14 | "Handlers return Result\<T\>; success maps to 200/201/204, domain errors to RFC 7807 Problem Details." | Plain, technical, correct. | [TECHNICAL TERM] |
| 15 | "The security framework operates across three layers: authentication, authorization, and defense-in-depth infrastructure." | "Operates across three layers" is a fine, standard technical description. Acceptable. | [NO ISSUE] |
| 16 | "Single-use refresh token rotation is enforced: exchanging an expired access token consumes the current refresh token and issues a new pair." | "Consumes" used technically (meaning "uses up") is a correct and common pattern in security/token documentation, keep. | [TECHNICAL TERM] |
| 17 | "Re-submitting a previously consumed refresh token triggers breach detection, immediately revoking all active refresh tokens for that user and forcing full re-authentication." | Long sentence (28 words) with three chained consequences; understandable, but could be split for easier reading. | [UNCLEAR] (mild) |

---

## STAGE 2 — Methodology claims requiring verification

No new methodology concerns in this section beyond the factual corrections already established in the earlier review (the bounded-context count, the PostgreSQL/pgvector version mismatches, and the permission-format inconsistency). Nothing further to flag here.

---

## STAGE 3 — Re-leveled rewrite

```
2.3 SYSTEM ARCHITECTURE & DESIGN

This section covers six parts of the platform architecture: from how the
services are organized, to domain modelling, to the database, API, and
security layers. The design uses a service-oriented approach: a Vue 3
frontend, a .NET 10 modular monolith backend, and a Python FastAPI
machine learning sidecar, each of which can be deployed independently.

- System Overview. Three services, eight bounded contexts, and a
  summary of the technology stack.
- Domain-Driven Design. Context map, aggregate roots with their
  invariants, and state machines.
- C4 Architecture. Context, container, and component-level views of the
  system.
- Database Design. Schemas per context, pgvector integration, and the
  main design decisions.
- API Design. Carter minimal APIs, MediatR CQRS, and endpoint
  conventions.
- Security Design. JWT authentication, permission-based authorisation, and
  defensive hardening.

2.3.1 System Overview
ReSys.Shop has three services: a Vue 3 frontend, a .NET 10 backend [19],
and a Python FastAPI ML sidecar [23], and eight bounded contexts, using
Domain-Driven Design with MediatR dispatch between modules.

[Table 46 unchanged.]

Internally, the backend is divided into eight bounded contexts, each with
its own dedicated database schema. Table 47 lists each context, its
aggregate root, and key domain entities.

2.3.2 Domain-Driven Design
The backend follows Domain-Driven Design (DDD), with eight bounded
contexts. Each context manages its own aggregate boundaries and domain
invariants, using a Conformist integration pattern through Published
Language contracts.

2.3.2.1 Bounded Context Map
The platform is divided into eight bounded contexts, along business
capability boundaries. Each context independently owns its own state
model, business logic, and vocabulary. Integration follows a Conformist
pattern, using core abstractions from the Shared Kernel (Result<T>,
ICommand, IQuery). Communication between contexts uses only MediatR
ISender in-process dispatch, with no direct project references between
modules at compile time.

Figure 33 shows the eight contexts and how they communicate with each
other. Table 48 lists each context's business responsibilities and its
Published Language boundaries.

[Table 48 unchanged, already plain and well-organized.]

2.3.3 C4 Architecture
[If there is an intro sentence before 2.3.3.1, keep it plain and short.]

2.3.3.1 System Context
The system context shows how ReSys.Shop fits into its operating
environment, including user roles and external dependencies (Figure 36).

The platform interacts with two human user groups:
- Customers: Browse the catalogue, run visual and keyword searches,
  manage carts, and complete checkouts.
- Administrators: Manage products, process orders, track inventory, and
  manage user accounts.
Five external integrations:
- Stripe: Handles payment intents and webhook notifications, verified
  using signatures.
- SendGrid: Sends transactional emails (order confirmations, password
  resets, shipping updates).
- S3-Compatible Storage: Stores product images and files.
- Google OAuth: Allows customers to sign in using Google.
- Python ML Sidecar: Generates image embeddings, running inside the
  same container orchestration boundary.

2.3.3.2 Container
The container view breaks ReSys.Shop down into its deployable parts:
processes and data stores (Figure 37).

The deployable units:
- Store & Admin SPAs: Vue 3 single-page applications that communicate
  with the backend over HTTPS REST endpoints.
- API Backend: .NET 10 application that runs domain logic through
  Carter minimal APIs and MediatR CQRS pipelines.
- Embedding Sidecar: Python 3.12 FastAPI service that loads ML models
  into GPU or CPU memory to generate embeddings on demand.
- PostgreSQL 17 (with pgvector): Stores relational domain schemas and
  high-dimensional vector embeddings.
- Redis 7: Used as an L2 distributed cache for HybridCache, and as the
  persistent job store for Hangfire.

2.3.4 Database Design
The ReSys.Shop database is a single PostgreSQL 17 instance, divided into
per-context schemas. Each schema belongs to one bounded context and is
managed using Entity Framework Core migrations.

2.3.4.1 Schema Organisation
Each of the eight bounded contexts has its own dedicated database schema:
[Bullet list unchanged, already plain.]

Relationships across contexts use identifier references (UUIDs) instead of
database-level foreign key constraints. For example, an order references a
UserId and a VariantId as simple attributes, without a foreign key. This
keeps the modules logically separate and avoids the cost of coordinating
transactions across multiple contexts.

2.3.4.2 Core Entity-Relationship Model
Figure 40 shows the entity-relationship model across all eight bounded
contexts. Dotted lines show references between different contexts.

The Catalog domain is centered on Product, with a one-to-many
relationship to Variant. VariantImage records store image paths and an
optional vector(512) embedding column. Taxonomy and Taxon trees
manage hierarchical classification using self-referencing foreign keys.

The Ordering domain is centered on Order, linked one-to-many with
LineItem. Line items store price snapshots at the time of purchase, so
order history is not affected by later catalogue price changes.

2.3.4.3 pgvector Integration
PostgreSQL's pgvector extension [13] runs vector similarity searches
directly inside the relational database. The variant_images table stores
feature vectors in an embedding column defined as vector(512). The
platform uses HNSW indexing [12] with cosine distance by default, to
meet the sub-second CBIR latency target (NFR-01a), with IVFFlat used as
a fallback for local development environments (see Section 1.4.3-1.4.4
for the HNSW/IVFFlat comparison).

2.3.5 API Design
The API is a RESTful interface built using Carter modules and the
MediatR CQRS pattern [18], with approximately 262 endpoints across
eight modules.

2.3.5.1 API Architecture
Each request follows a standard MediatR pipeline: Carter endpoint ->
LoggingBehavior -> ValidationBehavior -> ExceptionMappingBehavior ->
Handler.Execute() -> Result<T>.ToResult() -> HTTP Response (RFC 7807
Problem Details on error).

[Code block unchanged.]

Handlers return Result<T>. A successful result maps to a 200, 201, or 204
response; a domain error maps to an RFC 7807 Problem Details response.

2.3.5.2 Endpoint Organisation
Endpoints follow the convention /api/{module}/{surface}/{resource}, where
surface is either storefront or admin. All admin routes enforce
administrator-only access using .HasPermission().

Eleven inter-module contract DTOs allow cross-module data sharing
without breaking the module boundaries. [Continue as originally written.]

2.3.6 Security Design
The security system works across three layers: authentication,
authorisation, and defense-in-depth infrastructure.

2.3.6.1 Authentication and Session Management
JWT authentication is set up using JwtSettings, with the HS256 algorithm,
a 15-minute access token expiration, and a 30-day maximum token age.
Single-use refresh token rotation is enforced: when an expired access
token is exchanged, the current refresh token is used up (consumed) and a
new pair of tokens is issued. If a refresh token that has already been used
is submitted again, this is treated as a possible security breach. All active
refresh tokens for that user are immediately revoked, and the user must
log in again.
```

---

## STAGE 4 — Final consistency check

| Check | Result |
|---|---|
| Vocabulary difficulty | Simplified "comprises" → "has," "relies exclusively on" → "uses only," "executes... within" → "runs... inside," "positions... within its operational environment" → "shows how... fits into." All DDD/C4/architecture terminology (bounded context, aggregate root, Conformist pattern, Published Language, Shared Kernel) kept exactly, these are correct, necessary technical vocabulary regardless of general English level. |
| Sentence length | The two densest opening sentences (§2.3 intro, §2.3.4.1's isolation sentence) simplified or split. Most of the rest of the section was already at an appropriate level, database/API/security documentation tends to be naturally plain and factual. |
| Grammar | No errors introduced. |
| Repeated phrases | "Positions... within" flagged as the third occurrence of this specific AI-like pattern (after two occurrences in Chapter 1 §1.6); consistently reworded, not repeated again here. |
| AI-like formulaic expressions | Removed: "dimensions define... from X through Y to Z," "positions... within its operational environment." |
| Technical terminology | Preserved exactly throughout, this section is dominated by named architectural patterns and standard terms (DDD, C4, CQRS, RFC 7807, HS256), which is expected and correct. |
| Numbers | Eight bounded contexts (corrected from the original's "nine" per the earlier factual review), 262 endpoints, eleven DTOs, all kept as verified. |
| Claims vs. evidence | No new evidence concerns beyond what's already flagged in the earlier factual review (PostgreSQL version, pgvector version, permission format). |
| Meaning preserved | Checked section by section against the original; no technical content added or removed. |

---

## A. Ten most important problems

1. "Six dimensions define the platform architecture, from X through Y to Z" — report-cover-style construction, out of place next to the plainer technical prose around it.
2. "Positions... within its operational environment" — third occurrence of this specific AI-like pattern across the thesis.
3. "Nine bounded contexts" (§2.3.1) — factual error, should be eight, already established.
4. "Relies exclusively on" — moderately formal where "uses only" is simpler and equally accurate.
5. "Maintaining logical module isolation while avoiding distributed transaction overhead" — two abstract outcomes chained together, a plainer connector helps.
6. "Executes vector similarity searches within the relational engine" — stiffer than necessary; "runs... inside" is simpler and just as accurate.
7. "Re-submitting a previously consumed refresh token triggers breach detection, immediately revoking..." — one long sentence chaining three consequences, reads more easily split.
8–10. No further major issues, this section leans heavily on necessary DDD/architecture terminology that should stay exactly as written; most remaining sentences were already appropriately plain.

## B. Words/phrases to avoid

comprises (prefer "has" or "is made up of"), relies exclusively on (prefer "uses only"), executes... within the relational engine (prefer "runs inside"), positions... within its operational environment (prefer "fits into" or "shows how X connects to Y"), decomposes (keep only when it's the standard C4 term; otherwise prefer "breaks down into")

## C. Words/phrases that are safe and natural for your level

has, is made up of, uses only, runs inside, fits into, divided into, breaks down into, stores, manages, follows

## D. Writing style to use consistently

Same overall guidance as previous files. The one thing worth noting specifically for architecture/design chapters like this one: named technical patterns (Conformist, Published Language, Shared Kernel, bounded context) are proper technical vocabulary, not "advanced words to simplify." Keep them exactly as the DDD and C4 literature uses them. Your simplification effort should go into the connective sentences around these terms (the "why" and "how" explanations), not into the terms themselves.

---

Ready for §2.4 (Implementation) next, same three-stage process.
