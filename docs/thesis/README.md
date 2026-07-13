# ReSys.Shop — Thesis Design Documentation

This directory contains thesis-oriented design documentation emphasizing **analysis, design decisions, and justification** rather than operational how-to guides.

## Document Index

| # | Document | Focus | Priority |
|---|----------|-------|----------|
| 01 | [Problem Analysis](01-problem-analysis.md) | Problem statement, objectives, scope, stakeholders | ★★★★★ |
| 02 | [Requirements Analysis](02-requirements-analysis.md) | Functional/non-functional requirements, business rules, use cases, user roles | ★★★★★ |
| 03 | [System Architecture](03-system-architecture.md) | Architectural style, C4 diagrams, technology stack, design patterns, data flow | ★★★★★ |
| 04 | [Domain Analysis](04-domain-analysis.md) | Domain model, aggregates, entities, value objects, state machines, business rules | ★★★★★ |
| 05 | [Database Design](05-database-design.md) | ERD, schema organization, pgvector integration, indexing strategy, migrations | ★★★★★ |
| 06 | [API Design](06-api-design.md) | REST endpoints, request/response models, auth/authz, OpenAPI, HTTP test artifacts | ★★★★☆ |
| 07 | [Detailed Design](07-detailed-design.md) | Sequence diagrams (checkout, image search), class diagrams, ML service workflow | ★★★★★ |
| 08 | [Security Design](08-security-design.md) | Authentication, authorization, input validation, rate limiting, secrets management | ★★★☆☆ |
| 09 | [Deployment Design](09-deployment-design.md) | Aspire orchestration, service defaults, conceptual production architecture | ★★★☆☆ |
| 10 | [Testing Strategy](10-testing-strategy.md) | Unit/integration/manual testing, mocking, coverage, known gaps | ★★★★★ |
| 11 | [Evaluation](11-evaluation.md) | Architectural compliance, functional correctness, performance, ML metrics, discussion | ★★★★★ |
| 12 | [Requirements Traceability Matrix](12-requirements-traceability-matrix.md) | Bidirectional req → design → impl → test mapping | ★★★★☆ |
| 13 | [Proposal Options](13-proposal-options.md) | 2–3 alternative answers for every pending question | ★★★★☆ |

## Suggested Thesis Structure

These documents map to a typical software engineering thesis as follows:

```
Introduction
├─ Background → Chapter 1 (Problem Analysis)
├─ Problem Statement → Chapter 1
├─ Objectives → Chapter 1
└─ Contributions → Chapter 1 + Chapter 11

Literature Review
├─ E-commerce systems, CBIR, Recommendation systems → [External literature]
└─ Deep learning, Vector databases → [External literature]

Requirements Analysis
├─ Functional requirements → Chapter 2
├─ Non-functional requirements → Chapter 2
├─ Use cases → Chapter 2
└─ User roles → Chapter 2

System Design
├─ Overall architecture → Chapter 3
├─ C4 diagrams → Chapter 3
├─ Technology stack → Chapter 3 + Chapter 5
├─ Design patterns → Chapter 3
└─ Data flow → Chapter 3 + Chapter 7

Domain Design
├─ Domain model → Chapter 4
├─ ERD → Chapter 5
├─ Aggregate boundaries → Chapter 4
├─ State machines → Chapter 4
└─ Business rules → Chapter 4 + Chapter 2

Detailed Design
├─ Sequence diagrams → Chapter 7
├─ Class diagrams → Chapter 7
├─ API design → Chapter 6
├─ Database schema → Chapter 5
└─ ML service workflow → Chapter 7

Implementation
├─ Backend (.NET) → Chapters 3, 4, 5, 6, 7
├─ Frontend (Vue) → Chapters 3, 6
├─ ML service (FastAPI) → Chapters 3, 5, 7
└─ Infrastructure (Aspire) → Chapters 3, 9

Testing and Evaluation
├─ Unit / integration / system testing → Chapter 10
├─ Performance testing → Chapter 11
├─ ML evaluation (Recall@K, Precision@K) → Chapter 11
└─ Discussion → Chapter 11

Conclusion and Future Work
└─ Chapter 11
```

## Diagrams Checklist

| Diagram | Location | Status | Mermaid File |
|---------|----------|--------|--------------|
| C4 Context Diagram | Chapter 3 | ✅ Formal Mermaid | [`diagrams/c4-context.mmd`](diagrams/c4-context.mmd) |
| C4 Container Diagram | Chapter 3 | ✅ Formal Mermaid | [`diagrams/c4-container.mmd`](diagrams/c4-container.mmd) |
| C4 Component Diagram | Chapter 3 | ✅ Formal Mermaid | [`diagrams/c4-component.mmd`](diagrams/c4-component.mmd) |
| Deployment Diagram | Chapter 9 | ✅ Formal Mermaid | [`diagrams/deployment.mmd`](diagrams/deployment.mmd) |
| Use Case Diagram | Chapter 2 | ✅ Formal Mermaid | [`diagrams/use-case.mmd`](diagrams/use-case.mmd) |
| Sequence Diagram (Create Product) | Chapter 7 | ✅ Formal Mermaid | [`diagrams/sequence-create-product.mmd`](diagrams/sequence-create-product.mmd) |
| Sequence Diagram (Checkout) | Chapter 7 | ✅ Formal Mermaid | [`diagrams/sequence-checkout.mmd`](diagrams/sequence-checkout.mmd) |
| Sequence Diagram (Image Search) | Chapter 7 | ✅ Formal Mermaid | [`diagrams/sequence-image-search.mmd`](diagrams/sequence-image-search.mmd) |
| Class Diagram (Result + Pipeline) | Chapter 7 | ✅ Formal Mermaid | [`diagrams/class-result-pipeline.mmd`](diagrams/class-result-pipeline.mmd) |
| Class Diagram (Product Aggregate) | Chapter 4 | ✅ Formal Mermaid | [`diagrams/class-product-aggregate.mmd`](diagrams/class-product-aggregate.mmd) |
| Class Diagram (Order Aggregate) | Chapter 4 | ✅ Formal Mermaid | [`diagrams/class-order-aggregate.mmd`](diagrams/class-order-aggregate.mmd) |
| Class Diagram (Payment Aggregate) | Chapter 4 | ✅ Formal Mermaid | [`diagrams/class-payment-aggregate.mmd`](diagrams/class-payment-aggregate.mmd) |
| Class Diagram (Identity Aggregate) | Chapter 4 | ✅ Formal Mermaid | [`diagrams/class-identity-aggregate.mmd`](diagrams/class-identity-aggregate.mmd) |
| ERD (Core Business) | Chapter 5 | ✅ Formal Mermaid | [`diagrams/erd-core.mmd`](diagrams/erd-core.mmd) |
| State Machine (Order) | Chapter 4 | ✅ Formal Mermaid | [`diagrams/state-order.mmd`](diagrams/state-order.mmd) |
| State Machine (Payment) | Chapter 4 | ✅ Formal Mermaid | [`diagrams/state-payment.mmd`](diagrams/state-payment.mmd) |
| ML Pipeline Diagram | Chapter 7 | ✅ Formal Mermaid | [`diagrams/ml-pipeline.mmd`](diagrams/ml-pipeline.mmd) |
| Bounded Context Map | Chapter 4 | ✅ Formal Mermaid | [`diagrams/bounded-context-map.mmd`](diagrams/bounded-context-map.mmd) |

> **Note**: All diagrams are rendered as **Mermaid** (`.mmd`) files. They can be viewed in any Mermaid-compatible viewer (GitHub, GitLab, Notion, VS Code extensions) or converted to SVG/PNG using the Mermaid CLI (`mmdc`). Textual/ASCII versions remain in the chapter documents for readability.

## How to Use These Documents

1. **Start with Chapter 1** to understand the problem space and justify the architectural choices.
2. **Cross-reference Chapters 3-5** during the system design section of the thesis.
3. **Use Chapter 7** for the detailed design section with sequence diagrams.
4. **Chapter 11** provides the evaluation framework and honest discussion of limitations.

## Relationship to `docs/codebase/`

The `docs/codebase/` directory contains operational documentation (STACK, ARCHITECTURE, CONVENTIONS, TESTING, CONCERNS) produced by the `acquire-codebase-knowledge` process. These thesis documents synthesize that operational knowledge into **analysis and justification** suitable for academic examination.

- `docs/codebase/` = "How the system works"
- `docs/thesis/` = "Why the system was designed this way and how we know it works"

## Evidence Principle

Every claim in these documents is traceable to:
- A source file path and line number
- A configuration file value
- A terminal command output
- A git commit message

Unknowns are marked `[TODO]`. Intent-dependent decisions are marked `[ASK USER]`.

## Completed Actions

| Action | Status | Evidence |
|--------|--------|----------|
| Generate formal Mermaid diagrams | ✅ Done | 18 `.mmd` files in `diagrams/` |
| Add formal scope boundary section (§1.4) | ✅ Done | `01-problem-analysis.md` updated with Scope and Delimitations |
| Add bounded context map (§4.1a) | ✅ Done | `04-domain-analysis.md` + `bounded-context-map.mmd` |
| Add ubiquitous language glossary (§4.6) | ✅ Done | `04-domain-analysis.md` with 20+ defined terms |
| Create Requirements Traceability Matrix | ✅ Done | `12-requirements-traceability-matrix.md` |
| Provide multi-proposal options for pending questions | ✅ Done | `13-proposal-options.md` with 2–3 alternatives per question |
| Apply recommended proposals to documents | ✅ Done | Ch 1 (MSc level), Ch 3 (justification prose), Ch 5 (normalization), Ch 8 (STRIDE table), Ch 10 (70% coverage), Ch 11 (ML metrics + skip user study), Ch 12 (TC IDs), 4 class diagrams |

## Outstanding Questions ([ASK USER])

✅ **All 25 questions are now resolved.** The only remaining `[TODO]` items are **quantitative benchmarks** that must be measured on the final codebase snapshot before submission:

| Task | When | How |
|------|------|-----|
| Run `dotnet test /p:CollectCoverage=true` and populate coverage percentages | Final submission | `dotnet test /p:CollectCoverage=true` |
| Execute Fashion-CLIP benchmark (100-image ground-truth dataset) | Final submission | Python script: generate embeddings → query pgvector → measure Recall@20 / Precision@20 |
| Measure end-to-end checkout latency | Final submission | Integration test with `Stopwatch` or load test with `k6` |

---

## Resolution Log

| # | Question | Decision | Evidence |
|---|----------|----------|----------|
| 1 | **Thesis level**: BSc, MSc, or PhD? | **MSc** | `01-problem-analysis.md:1` |
| 2 | **Scope documentation format**: Formal boundary section? | Formal §1.4 with In-Scope / Out-of-Scope / Justification | `01-problem-analysis.md:§1.4` |
| 3 | **Requirement numbering**: Formal R-001 cross-references? | `CAT-FR-01` / `TC-CAT-001` hierarchical IDs | Ch 2 tables + Ch 12 RTM |
| 4 | **Accessibility/GDPR**: Specific NFRs expected? | **Not required** — architecture-focused, not compliance | `01-problem-analysis.md:§1.4` |
| 5 | **Diagram format**: Formal UML or ASCII? | Formal **Mermaid** (18 files) | `diagrams/*.mmd` |
| 6 | **Microservices justification**: Table + prose? | Decision table + **3 paragraphs** of design rationale | `03-system-architecture.md:§3.1.1` |
| 7 | **UML class diagram**: All aggregates? | **4 per-aggregate diagrams** (Product, Order, Payment, Identity) | `diagrams/class-*-aggregate.mmd` |
| 8 | **DDD expectations**: BC maps, UL glossary? | Bounded Context Map + **20-term UL Glossary** | `04-domain-analysis.md:§4.1a, §4.6` |
| 9 | **ERD format**: Formal or textual? | Formal **Mermaid** ER diagram | `diagrams/erd-core.mmd` |
| 10 | **Normalization discussion**: 3NF/BCNF? | **1 paragraph** explaining 3NF + denormalization of `order.total` | `05-database-design.md:§5.1a` |
| 11 | **OpenAPI spec**: Include `openapi.json`? | **No** — representative samples in Ch 6 | `06-api-design.md:§6.3` |
| 12 | **API standards**: JSON:API, OData, HAL? | **Not required** — Level 2 REST sufficient | `06-api-design.md:§6.1` |
| 13 | **Sequence diagram format**: Formal UML? | Formal **Mermaid** sequence diagrams (3 flows) | `diagrams/sequence-*.mmd` |
| 14 | **Full class diagram**: Entire Module assembly? | **No** — representative per-aggregate diagrams | `diagrams/class-*-aggregate.mmd` |
| 15 | **Threat model**: STRIDE-per-element? | **Full STRIDE table** for Order, PaymentIntent, JWT | `08-security-design.md:§8.1a` |
| 16 | **GDPR privacy-by-design**: Data retention docs? | **Omitted** — not top concern | `04-domain-analysis.md:§4.2` |
| 17 | **Deployment diagram**: Cloud vendor icons? | **Conceptual** deployment (generic containers) | `09-deployment-design.md:§9.1.3` |
| 18 | **Cloud platform target**: AWS/Azure/GCP? | **Generic** — no vendor named | `09-deployment-design.md:§9.1.3` |
| 19 | **Test plan matrix**: Req × test levels? | Full **RTM** | `12-requirements-traceability-matrix.md` |
| 20 | **Coverage target**: Expected percentage? | **≥70%** statement coverage | `10-testing-strategy.md:§10.6` |
| 21 | **Benchmarks**: Run now or methodology only? | **Methodology for draft; numbers for final** | `11-evaluation.md:§11.1` |
| 22 | **ML statistics**: Confidence intervals? | **Recall@K + Precision@K with mean ± SD** | `11-evaluation.md:§11.5` |
| 23 | **User study**: SUS/task-based testing? | **Skipped** — architectural contribution | `11-evaluation.md:§11.6` |
| 24 | **RTM test case IDs**: TC-001 style? | **Formal TC-{Module}-{Seq} IDs** | `12-requirements-traceability-matrix.md` |
| 25 | **Actual coverage numbers**: Populate from coverlet? | **[TODO — Final Submission]** | `10-testing-strategy.md:§10.6` |
