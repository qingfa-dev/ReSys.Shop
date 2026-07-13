# Proposal Options for Remaining Thesis Questions

This document provides **2–3 alternative proposals** for each remaining pending question. Each proposal includes: (a) what to do, (b) effort level, (c) examiner impact, and (d) recommended context (BSc/MSc/PhD).

---

## Q1: Thesis Level (BSc / MSc / PhD)

This is the **root decision** — it determines depth for all other questions.

### Proposal A: BSc (Bachelor's Thesis)
**Approach**: Focus on **implementation and architecture demonstration**. Literature review is brief (5–8 papers). Evaluation is functional correctness + basic metrics.

| Aspect | Depth |
|--------|-------|
| Literature Review | 5–8 papers; survey of e-commerce architectures + CBIR |
| Design Rationale | Decision tables + 1-paragraph justification per major choice |
| Diagrams | Mermaid/textual acceptable; formal UML optional |
| Evaluation | Unit test pass rates + manual API test coverage + basic performance |
| ML Evaluation | Qualitative demo ("search returns visually similar items") |
| User Study | Not required |
| Page Count | 40–60 pages |

**Pros**: Fastest to complete; implementation speaks for itself.
**Cons**: Less competitive if applying for research positions.
**When to choose**: If your program requires a working system with documentation.

---

### Proposal B: MSc (Master's Thesis) ⭐ RECOMMENDED DEFAULT
**Approach**: Focus on **architectural process + measurable evaluation**. Literature review is systematic (15–25 papers). Evaluation includes quantitative metrics.

| Aspect | Depth |
|--------|-------|
| Literature Review | 15–25 papers; systematic comparison of monolith vs microservices, CBIR methods, vector DB options |
| Design Rationale | Full prose per decision (2–3 paragraphs) with alternatives considered and rejected |
| Diagrams | Formal Mermaid/PlantUML required for C4 and sequence diagrams |
| Evaluation | Test coverage ≥70%, integration test pass rate, response time benchmarks, ML Recall@K/Precision@K |
| ML Evaluation | Quantitative: measure Recall@K and Precision@K on a ground-truth dataset of 100 images |
| User Study | Optional; if included: 5 participants + SUS questionnaire |
| Page Count | 80–120 pages |

**Pros**: Demonstrates research methodology; strong for job applications.
**Cons**: Requires more time for literature review and evaluation.
**When to choose**: If your program expects a research contribution or you plan to publish.

---

### Proposal C: PhD (Doctoral Dissertation)
**Approach**: Focus on **novel contribution** — either a new architectural pattern, a new CBIR technique, or a systematic evaluation framework.

| Aspect | Depth |
|--------|-------|
| Literature Review | 50+ papers; full systematic review with PRISMA methodology |
| Design Rationale | Must prove novelty — why existing patterns insufficient |
| Diagrams | Full formal UML; may need custom notation for novel pattern |
| Evaluation | Statistical significance testing (t-tests, ANOVA), confidence intervals, cross-validation |
| ML Evaluation | Comparison against 3+ baseline models (e.g., ResNet, VGG, CLIP-generic) with statistical testing |
| User Study | Required: ≥15 participants, A/B testing, eye-tracking optional |
| Page Count | 200+ pages |

**Pros**: Publishable as journal/conference papers.
**Cons**: Scope far exceeds current codebase; likely needs 12+ months.
**When to choose**: Only if this is a funded PhD with extension time.

---

## Q3: Requirement Numbering (R-001 Style)

### Proposal A: Formal Hierarchical Numbering ⭐ RECOMMENDED
**Format**: `CAT-FR-01`, `ID-FR-02`, `ORD-NFR-03`

**Implementation**:
- Update Chapter 2 with formal IDs
- Cross-reference in every design chapter: "This sequence diagram realizes **CAT-FR-01** and **CAT-FR-06**"
- RTM (Chapter 12) uses these IDs as primary keys

**Pros**: Professional; traceability is examinable; IEEE 29148 compliant.
**Cons**: Requires updating all existing tables and adding cross-references.
**Effort**: Medium (2–3 hours of editing).

---

### Proposal B: Simple Module Prefix Only
**Format**: Use existing prefix style (`CAT-FR-01`) but **don't cross-reference** throughout the document.

**Implementation**:
- Keep IDs in Chapter 2 tables
- Mention informally in design chapters: "The Create Product feature (see Catalog requirements)"

**Pros**: Faster; less maintenance if chapters are reorganized.
**Cons**: Examiners must manually search for traceability.
**Effort**: Low (already partially done).

---

### Proposal C: No Numbering — Narrative Only
**Format**: Remove all IDs; describe requirements in prose paragraphs.

**Implementation**:
- Rewrite §2.1 as prose instead of tables
- Mention requirements by name only

**Pros**: Reads like a essay; good for humanities-influenced examiners.
**Cons**: SE examiners penalize lack of traceability; almost never recommended for technical theses.
**Effort**: Low but risky.

---

## Q6: Microservices Justification Depth

### Proposal A: Table + 1 Paragraph Prose ⭐ RECOMMENDED
**What**: Keep the existing decision table (modular monolith vs microservices vs clean architecture) and add a **2–3 paragraph rationale** after it.

**Content of prose**:
- Paragraph 1: Why microservices were rejected (ops complexity > value at thesis scale)
- Paragraph 2: Why clean architecture per-module was rejected (build time, project count)
- Paragraph 3: Why modular monolith wins (single deployable, ACID checkout, demonstrability)

**Pros**: Demonstrates critical thinking without excessive length.
**Cons**: Some examiners want deeper analysis.
**Effort**: Low (already drafted; needs minor expansion).

---

### Proposal B: Full Section with SWOT Analysis
**What**: Add a dedicated §3.1.1.4 "Architectural Decision Record (ADR)" using the MADR template.

**Template**:
```
# ADR-001: Modular Monolith over Microservices
## Context: Single-team thesis project, 8 business domains
## Decision: Use modular monolith with namespace isolation
## Consequences:
- Positive: ACID transactions, single debug session, fast builds
- Negative: Cannot scale modules independently, single failure domain
## Status: Accepted
```

**Pros**: Very professional; ADRs are industry standard (GitHub, AWS use them).
**Cons**: Adds 2–3 pages; may be overkill for BSc.
**Effort**: Medium.

---

### Proposal C: Reference External Authority
**What**: Cite a well-known source (e.g., Newman, *Monolith to Microservices* Chapter 2) and align your decision with their recommendations.

**Pros**: Authority by citation; examiners respect external validation.
**Cons**: Requires reading the cited chapter carefully to ensure alignment.
**Effort**: Low if you have the book.

---

## Q7: Class Diagrams — Per-Aggregate vs. Full Assembly

### Proposal A: Per-Aggregate Class Diagrams ⭐ RECOMMENDED
**What**: Create 4–6 focused class diagrams, one per major aggregate.

**Diagrams to include**:
1. **Product aggregate** — Product, Variant, VariantImage, ProductOptionType, OptionType, OptionValue, Classification, Taxon, Taxonomy
2. **Order aggregate** — Order, LineItem, Adjustment
3. **Payment aggregate** — PaymentIntent, PaymentCapture, PaymentMethod
4. **Identity aggregate** — User, Role, UserClaim, UserRole, PermissionContext
5. **Result + Pipeline types** — Already exists as `class-result-pipeline.mmd`

**Pros**: Readable; each fits on one page; shows depth without overwhelming.
**Cons**: Doesn't show cross-aggregate relationships.
**Effort**: Medium (4–6 Mermaid files).

---

### Proposal B: Full Module Assembly Diagram
**What**: One giant diagram of all domain entities and their relationships.

**Pros**: Comprehensive; shows the full picture.
**Cons**: Unreadable in print (would span 3+ pages); examiners skip it.
**Effort**: High; almost certainly requires a tool like Visual Studio Class Designer or Enterprise Architect.
**Verdict**: ❌ Not recommended.

---

### Proposal C: No Class Diagrams — Use ERD Instead
**What**: Rely on the ERD (`erd-core.mmd`) for structural relationships and describe behavior in sequence diagrams.

**Pros**: ERDs are standard for database-centric theses; less maintenance.
**Cons**: Doesn't show method signatures or inheritance hierarchies.
**Effort**: None (already done).
**When to choose**: If your examiner is database-oriented rather than OO-oriented.

---

## Q10: Database Normalization Discussion

### Proposal A: Brief Paragraph ⭐ RECOMMENDED
**What**: Add 1 paragraph to Chapter 5 explaining normalization decisions.

**Draft paragraph**:
> "The database schema is designed in **Third Normal Form (3NF)** with the exception of the `Order.Total` field, which is a **computed denormalized column** storing the sum of `ItemTotal + AdjustmentTotal + ShipmentTotal`. This denormalization is intentional: it avoids recalculating the total on every read query, which is a frequent operation in order listing and checkout confirmation. The trade-off is that `Total` must be recalculated whenever line items or adjustments are modified — this is enforced by the `Order.Method.Checkout.cs` domain method, not by a database trigger, keeping business logic in the domain layer per DDD principles."

**Pros**: Shows awareness of normalization; justifies intentional denormalization.
**Cons**: None — this is standard practice.
**Effort**: 10 minutes.

---

### Proposal B: Formal Normalization Proof
**What**: For each major table, prove it satisfies 3NF (or BCNF) by showing no partial/transitive dependencies.

**Pros**: Extremely rigorous; impresses theoretically-minded examiners.
**Cons**: Tedious; adds 3–4 pages of formal notation that most SE examiners skim.
**Effort**: High.
**When to choose**: If your thesis is database-focused or your examiner is a database researcher.

---

### Proposal C: Skip Normalization Discussion
**What**: Assume 3NF is implicit and don't mention it.

**Pros**: Saves space.
**Cons**: SE examiners expect at least a passing acknowledgment of normalization; skipping it looks naive.
**Verdict**: ❌ Not recommended.

---

## Q14/Q15: Threat Model (STRIDE) and GDPR

### Proposal A: Layered Controls Table Only ⭐ RECOMMENDED FOR BSc/MSc
**What**: Keep Chapter 8 as-is — the security controls table (JWT, rate limiting, headers, upload guards) is sufficient.

**Rationale**: A full STRIDE threat model is 10+ pages. For a thesis where security is a supporting concern (not the primary contribution), the controls table demonstrates awareness without dominating the document.

**Pros**: Focused; proportional to security's role in the thesis.
**Cons**: May not satisfy security-focused examiners.

---

### Proposal B: STRIDE Lite + GDPR Checklist
**What**: Add 1 page: a STRIDE-per-element table for the 3 most critical elements (Order, PaymentIntent, JWT Token).

| Element | Spoofing | Tampering | Repudiation | Info Disclosure | DoS | Elevation |
|---------|----------|-----------|-------------|-----------------|-----|-----------|
| Order | Session fixation | Price tampering | No audit log | Leak to other users | Cart exhaustion | Admin impersonation |
| PaymentIntent | Webhook spoofing | Amount modification | No receipt | ClientSecret leak | Stripe retry storm | Refund abuse |
| JWT Token | Token forgery | Claims tampering | N/A | Secret exposure | N/A | Role escalation |

Plus a ½-page GDPR checklist: data retention (soft deletion), right to erasure (`IsDeleted` flag), lawful basis (consent for marketing, contract for orders).

**Pros**: Demonstrates security thinking without excessive length.
**Cons**: Still adds 2 pages.
**Effort**: Medium.
**When to choose**: If your program has a security module or your examiner has a security background.

---

### Proposal C: Full STRIDE + Privacy Impact Assessment
**What**: 5-page formal threat model with data flow diagrams, attack trees, and a Privacy Impact Assessment (PIA).

**Pros**: Publication-ready security analysis.
**Cons**: Major scope expansion; security becomes a parallel thesis topic.
**Effort**: Very high (1–2 weeks).
**When to choose**: PhD only, or if security is your declared contribution.

---

## Q20: Coverage Target

### Proposal A: 70% Statement Coverage ⭐ RECOMMENDED
**What**: State 70% as the target; measure with `coverlet` and report actual numbers in Chapter 11.

**Rationale**: 70% is achievable for domain logic + handlers. Integration tests add structural coverage even if not measured by coverlet. This is a realistic target for a thesis timeline.

**Pros**: Achievable; honest.
**Cons**: Some industrial standards expect 80%.

---

### Proposal B: 80% Statement Coverage
**What**: State 80%; add tests to reach it.

**Rationale**: Industry standard (many CI pipelines gate on 80%).

**Pros**: Impressive number.
**Cons**: May require writing tests for trivial properties (getters/setters) just to hit the number — examiners see through this.

---

### Proposal C: No Numeric Target — Qualitative Coverage
**What**: Describe coverage by feature domain instead of percentage.

> "All checkout, payment, and authentication handlers have unit and integration tests. Inventory, shipping, and profile modules have unit tests for core domain methods but lack integration coverage — these are listed as future work."

**Pros**: Honest; avoids gaming the metric.
**Cons**: Some programs require a numeric threshold.

---

## Q21: Benchmarks — Now or Later?

### Proposal A: Methodology in Draft, Numbers in Final ⭐ RECOMMENDED
**What**: Draft includes the evaluation framework (metrics, datasets, procedures). Final includes actual numbers.

**Rationale**: Draft evaluation is judged on methodology soundness, not results. Results come after implementation stabilizes.

**Pros**: Parallelizes work; you can write Chapters 10–11 before the system is fully benchmarked.
**Cons**: Final submission requires actual runs.

---

### Proposal B: Run Now + Include in Draft
**What**: Execute benchmarks now and populate Chapter 11.

**Commands**:
```bash
dotnet test /p:CollectCoverage=true   # Coverage numbers
cd service/Embedding && uv run pytest  # Python tests
cd app/Admin && pnpm run test:unit     # Frontend tests
```

**Pros**: Draft is complete; no last-minute crunch.
**Cons**: If you refactor, numbers change and must be re-run.

---

### Proposal C: No Benchmarks — Argumentative Evaluation
**What**: Argue that the architecture's quality is demonstrated by structural properties (module isolation, explicit error handling, testability) rather than runtime metrics.

**Pros**: Philosophically valid (Shaw & Garlan argue architecture is about form, not just performance).
**Cons**: Weak unless your examiner is an architecture theorist.
**When to choose**: If benchmarks are technically infeasible (e.g., no access to GPU for Fashion-CLIP benchmarking).

---

## Q22: ML Statistical Analysis

### Proposal A: Recall@K + Precision@K with Standard Deviation ⭐ RECOMMENDED
**What**: On a ground-truth dataset of 100 fashion images with human-labeled similarity groups:
- Query each image against the catalog
- Measure top-20 retrieval overlap with labeled group
- Report mean ± SD across all queries

**Pros**: Standard CBIR evaluation; doesn't require advanced statistics.
**Cons**: Less rigorous than significance testing.

---

### Proposal B: Comparison with Baseline + Significance Testing
**What**: Compare Fashion-CLIP against a baseline (e.g., ResNet-50 image embeddings) using paired t-test or Wilcoxon signed-rank test.

**Pros**: Demonstrates that your chosen model is statistically superior.
**Cons**: Requires running a second model; more computation.
**When to choose**: If ML is the primary thesis contribution.

---

### Proposal C: Qualitative Demonstration Only
**What**: Show 3 example queries with top-5 results and describe visual similarity subjectively.

**Pros**: No computation needed; acceptable for BSc.
**Cons**: Not quantitative; weak for MSc+.

---

## Q23: User Study

### Proposal A: Skip User Study — Technical Evaluation Only ⭐ RECOMMENDED FOR ARCHITECTURE-FOCUSED THESIS
**What**: The thesis contribution is architectural (modular monolith, vertical slices, CBIR integration). User evaluation is unnecessary.

**Pros**: Focused; avoids scope creep.
**Cons**: If your title mentions "user experience" or "usability," examiners expect a study.

---

### Proposal B: Lightweight SUS Questionnaire (5 Participants)
**What**: 5-minute System Usability Scale (10 questions, 1–5 Likert scale) administered to 5 classmates.

**Pros**: Adds 1 page; minimal effort; demonstrates awareness of usability.
**Cons**: 5 participants is not statistically significant.
**When to choose**: If your examiner specifically asks about usability.

---

### Proposal C: Task-Based Testing (10 Participants + Metrics)
**What**: Recruit 10 participants. Measure:
- Task completion time (search by image → add to cart → checkout)
- Error rate (failed checkouts, confusion points)
- SUS score

**Pros**: Strong evidence of system usability.
**Cons**: 1–2 weeks of scheduling and analysis; requires ethics approval at many institutions.
**When to choose**: If HCI is a secondary contribution or your program requires it.

---

## Q24: RTM Test Case IDs (TC-001 Style)

### Proposal A: Add Formal TC IDs to RTM ⭐ RECOMMENDED
**Format**: `TC-{Module}-{Sequence}`
- `TC-CAT-001` = Create Product unit test
- `TC-CAT-002` = Create Product integration test
- `TC-CAT-003` = Search by image manual API test

**Implementation**: Update Chapter 12 RTM with a "Test Case ID" column.

**Pros**: IEEE 829 compliant; professional traceability.
**Cons**: Adds maintenance if tests are renamed.
**Effort**: Low (1 hour of editing).

---

### Proposal B: Use File Paths as Identifiers
**Format**: Use existing file paths instead of synthetic IDs.
- Test: `Module.UnitTests/Catalog/Features/Admin/Products/Create/CreateProduct.Tests.cs`

**Pros**: No extra IDs to maintain; directly traceable.
**Cons**: Verbose; breaks if files move.

---

### Proposal C: No Test Case IDs — Keep as-is
**Format**: Keep the current RTM without TC IDs.

**Pros**: Less work.
**Cons**: Less formal; examiners may ask for more structure.

---

## Q25: Actual Coverage Numbers from Coverlet

### Proposal A: Populate for Final Submission Only ⭐ RECOMMENDED
**What**: Leave `[TODO]` in draft; run `dotnet test /p:CollectCoverage=true` before final submission and populate Chapter 11 + RTM.

**Pros**: Numbers are fresh and accurate at submission time.
**Cons**: Risk of forgetting.

---

### Proposal B: Populate Now with Caveat
**What**: Run coverlet now, populate numbers, and add a footnote: "Coverage measured on commit `abc1234`; subject to change as tests are added."

**Pros**: Draft is complete.
**Cons**: Numbers may drift; footnote looks defensive.

---

### Proposal C: Never Populate — Explain Why
**What**: State: "Coverage is opt-in via `/p:CollectCoverage=true` and measured per-test-run. A single static number is less informative than the per-module coverage table in the RTM, which reflects test distribution."

**Pros**: Intellectually honest.
**Cons**: May not satisfy programs with mandatory coverage thresholds.

---

## Decision Quick-Reference

| If you are... | Choose these proposals |
|----------------|------------------------|
| **BSc, time-constrained** | A for thesis level, B for numbering, A for microservices, A for class diagrams, A for normalization, A for security, C for coverage, C for benchmarks, C for ML stats, A for user study |
| **MSc, standard timeline** | **B for thesis level (default)**, A for numbering, A for microservices, A for class diagrams, A for normalization, A/B for security, A for coverage, A for benchmarks, A for ML stats, A for user study |
| **MSc, aiming for publication** | B for thesis level, A for numbering, B for microservices (ADR), A for class diagrams, A for normalization, B for security, B for coverage, B for benchmarks, B for ML stats, B/C for user study |
| **PhD** | C for thesis level, A for numbering, B/C for microservices, A for class diagrams, B for normalization, C for security, B for coverage, B for benchmarks, B for ML stats, C for user study |

---

## Evidence

- This document is generated from the pending `[ASK USER]` items in `docs/thesis/README.md`
- All proposals are grounded in software engineering thesis standards (IEEE 830, ISO 42010, Shaw & Garlan)
