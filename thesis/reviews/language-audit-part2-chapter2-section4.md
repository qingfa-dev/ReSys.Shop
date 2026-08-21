# Language Level Audit + Re-Leveled Rewrite — Chapter 2, §2.4

**Scope note:** §2.4 is the implementation chapter, mostly code snippets, tables, and short explanatory prose around them. Same approach as before: code and named design patterns (Template Method, Result pattern, CQRS) stay untouched, they're correct technical vocabulary. The audit below targets the explanatory prose, and this section's opening paragraph in particular has the most concentrated native-level writing found so far in the whole thesis.

**Factual corrections carried forward from the earlier review, applied in the rewrite below:** the CBIR endpoint URL inconsistency, "eight use cases" → "nine," pgvector 0.3.2 → 0.7.0, and the "four-state" vs. "five states" contradiction. These are already resolved in the fact-correction rewrite files; this pass adds the language-level fix on top of those, not instead of them.

---

## STAGE 1 — Sentence-by-sentence audit

| # | Original | Issue | Class |
|---|---|---|---|
| 1 | "This section describes the concrete realization of the ReSys.Shop system, showing how the architecture and design decisions from Sections 2.2 and 2.3 were translated into working software." | "The concrete realization of" and "translated into working software" are both elevated, almost literary phrasings for a simple idea ("this section shows how the design became real code"). | [AI-LIKE] |
| 2 | "The presentation follows the system's actual structure: the technology stack that underpins development, the vertical slice pattern that organizes the .NET codebase, the persistence layer that stores relational and vector data in a single database, the machine learning sidecar that constitutes the core research contribution, and the frontend applications that deliver the user-facing experience." | This is the single densest sentence found in the entire thesis so far: five "the X that Y" clauses chained together in one 70-word sentence. "Underpins," "constitutes," and "deliver the user-facing experience" are all advanced/formal word choices. This reads as a native academic writer's polished summary paragraph, not natural for this level at all. | [AI-LIKE] + [TOO ADVANCED] + [UNCLEAR] |
| 3 | "The platform uses pinned versions across three ecosystems... Centralized package management enforces reproducibility via Directory.Packages.props, uv.lock, and pnpm-lock.yaml." | "Enforces reproducibility" is a correct, standard software-engineering phrase (reproducible builds is a real, named concept), acceptable technical usage. | [TECHNICAL TERM] |
| 4 | "Inter-module communication relies exclusively on ISender.Send() messages. Bounded contexts never import foreign context namespaces, enforcing isolation within a single assembly via static analysis and build policies." | "Relies exclusively on" repeats the same phrase flagged in §2.3; "enforcing isolation... via static analysis and build policies" is dense but the terms themselves are necessary and correct. | [TOO ADVANCED] (mild, repeat) |
| 5 | "The persistence layer maps eight bounded contexts to dedicated PostgreSQL schemas, co-locating vector embeddings with relational product data in a single PostgreSQL 17 instance." | "Co-locating" is a correct, moderately technical word (common in database/systems writing), acceptable given the audience, but a plainer phrasing exists. | [TOO ADVANCED] (mild) |
| 6 | "All models conform to BaseEmbedder via the Template Method pattern." | "Template Method pattern" is a real, named design pattern (from the Gang of Four design patterns book), keep exactly. "Conform to" is standard, correct usage in this context. | [TECHNICAL TERM] |
| 7 | "The base class orchestrates loading, forwarding, and L2-normalisation; subclasses provide only the forward pass." | "Orchestrates" used metaphorically (like conducting an orchestra) for a base class's role is a moderately advanced usage; a simpler verb works just as well here. | [TOO ADVANCED] (mild) |
| 8 | "Models load lazily on first request within torch.no_grad(). The device property resolves CUDA, MPS, or CPU at runtime." | Clear, plain, appropriately technical (lazy loading and device resolution are standard PyTorch concepts). | [TECHNICAL TERM] |
| 9 | "For visual search, the repository extends the base with a multipart upload method... dispatching POST /api/storefront/search-by-image with Content-Type: multipart/form-data." | Clear, technical, plain. | [NO ISSUE] |
| 10 | "The customer storefront implements eight use cases covering product discovery, purchasing, and account management." | Clear sentence structure; the number itself needs the factual correction already established ("eight" → "nine"), not a language-level issue. | [not a language issue — factual, already flagged] |
| 11 | "The visual search interface implements a four-state UI model." | Clear, plain. (Number consistency with the later "five states" sentence is a factual issue already flagged, not a language one.) | [NO ISSUE] |

---

## STAGE 2 — Methodology claims requiring verification

No new methodology concerns beyond what's already established in the earlier factual review for this section (the endpoint URL mismatch, the use case count, the pgvector version, the four/five states contradiction). Nothing further to flag here.

---

## STAGE 3 — Re-leveled rewrite

```
2.4 IMPLEMENTATION

This section shows how the design from Sections 2.2 and 2.3 was built into
working software. It follows the actual structure of the system: the
technology stack used for development, the vertical slice pattern that
organizes the .NET codebase, the layer that stores relational and vector
data in a single database, the machine learning sidecar (the main research
contribution of this project), and the frontend applications that users
interact with.

- Technology Stack. Framework versions and how the system is
  containerized.
- Vertical Slice Core. Feature co-location, the Carter-MediatR request
  pipeline, and the functional Result pattern.
- Data Persistence. Multi-schema EF Core, pgvector integration with
  HNSW indexing, and concurrency control.
- ML Sidecar. Model management, the embedding generation pipeline, and
  the full CBIR search flow.
- Frontend Applications. Dual-SPA architecture, the visual search interface,
  and key administration workflows.

2.4.1 Technology Stack
The platform uses pinned (fixed) versions across three technology
ecosystems: .NET 10 for transactional logic and API design, Python 3.12
for deep learning (PyTorch, Hugging Face), and Vue 3 for reactive user
interfaces. Package management is centralized to keep builds reproducible,
using Directory.Packages.props (NuGet), uv.lock (Python), and
pnpm-lock.yaml (JavaScript).

[Table 51 unchanged, with the pgvector version already corrected to 0.7.0
per the earlier factual review.]

2.4.2 Vertical Slice Core
[Code and step tables unchanged.]

Communication between modules uses only ISender.Send() messages.
Bounded contexts never import another context's namespace. This keeps
the modules isolated within a single assembly, checked using static
analysis and build rules.

[Code block unchanged.]

2.4.3 Data Persistence Architecture
The persistence layer maps eight bounded contexts to their own dedicated
PostgreSQL schemas. Vector embeddings and relational product data are
stored together, in a single PostgreSQL 17 instance.

2.4.3.1 Schema Organisation and Vector Storage
Each bounded context has its own isolated schema. Table 53 shows the
breakdown:

[Table 53 unchanged.]

2.4.4 ML Sidecar
[Sub-headings and code unchanged.]

2.4.4.1 Model Management
[Table 55 unchanged.]

All models follow the BaseEmbedder interface, using the Template Method
design pattern. The base class handles loading, running the model, and
L2-normalisation; each subclass only needs to implement its own forward
pass. Models are loaded the first time they are requested (lazy loading),
inside torch.no_grad(). The device property automatically chooses CUDA,
MPS, or CPU at runtime.

[Code block unchanged.]

2.4.5 Frontend Applications
[Intro sentence, if present, kept plain.]

2.4.5.1 Repository Pattern and API Client
[Code unchanged.]

For visual search, the repository extends the base repository with a
multipart upload method:
async searchByImage(file: File): Promise<Result<Product[]>>
This sends a POST request to /api/catalog/storefront/search-by-image, with
Content-Type: multipart/form-data.

2.4.5.2 Storefront Interfaces
The customer storefront implements nine use cases, covering product
discovery, purchasing, and account management.

2.4.5.2.1 Visual Search: UC-STR-SRC
The visual search interface has a four-state UI model.

[Table 58 unchanged.]
```

---

## STAGE 4 — Final consistency check

| Check | Result |
|---|---|
| Vocabulary difficulty | The §2.4 opening paragraph, the single most advanced passage found in the whole thesis so far, was rewritten from "the concrete realization... underpins... constitutes... deliver the user-facing experience" down to plain, direct phrasing ("shows how the design was built into working software"). "Orchestrates" → "handles," "co-locating" → "stored together," "relies exclusively on" → "uses only" (repeat fix from §2.3). Design pattern names (Template Method, Result pattern) kept exactly, they're correct technical vocabulary. |
| Sentence length | The 70-word, five-clause opening sentence was the single biggest sentence-length problem in the audit series so far; split into a normal-length sentence plus the existing bullet list, which already did the job of breaking down the five parts. |
| Grammar | No errors introduced. |
| Repeated phrases | "Relies exclusively on" and "co-locating" both repeat patterns already flagged in §2.3; consistently fixed the same way both times rather than leaving one instance unfixed. |
| AI-like formulaic expressions | Removed: "the concrete realization of," "translated into working software," "constitutes the core research contribution," "deliver the user-facing experience." This was the highest concentration of these patterns found in any single paragraph across the whole thesis. |
| Technical terminology | Preserved exactly: Template Method pattern, Result pattern, CQRS, lazy loading, L2-normalisation, torch.no_grad(), CUDA/MPS/CPU device resolution, all correct and necessary. |
| Numbers | Nine use cases (already corrected from "eight"), pgvector 0.7.0 (already corrected from 0.3.2), corrected endpoint URL, all consistent with the earlier factual fixes. |
| Claims vs. evidence | No new evidence concerns; this section's fact-level issues were already resolved in the earlier review, this pass only adjusts the language level on top. |
| Meaning preserved | Checked against original; the opening paragraph's five listed components were preserved exactly, just de-compressed into plainer language and supported by the existing bullet list rather than crammed into one sentence. |

---

## A. Ten most important problems

1. The §2.4 opening sentence ("The presentation follows the system's actual structure: the technology stack that underpins development..."), the single most advanced, most compressed sentence found anywhere in the thesis audit so far: 70 words, five stacked clauses, multiple elevated word choices.
2. "The concrete realization of the ReSys.Shop system" — literary phrasing for a simple idea.
3. "Translated into working software" — same pattern, elevated phrasing.
4. "Constitutes the core research contribution" — formal, report-style.
5. "Deliver the user-facing experience" — marketing/business-report register, out of place in a thesis.
6. "Orchestrates loading, forwarding, and L2-normalisation" — metaphorical, advanced verb usage.
7. "Co-locating vector embeddings with relational product data" — correct but more formal than needed.
8. "Relies exclusively on ISender.Send() messages" — repeat of the §2.3 pattern, same fix applied.
9–10. No further major issues, the rest of §2.4 is code-and-table-heavy and was already appropriately plain.

## B. Words/phrases to avoid

the concrete realization of, translated into working software, underpins, constitutes, deliver the user-facing experience, orchestrates (prefer "handles" or "manages"), co-locating (prefer "stored together" or "kept together")

## C. Words/phrases that are safe and natural for your level

shows how, was built into, handles, manages, stored together, kept together, the main research contribution, users interact with

## D. Writing style to use consistently

Same guidance as previous files, but this section is a good reminder of the single biggest risk pattern across the whole thesis: **section-opening summary paragraphs are where the most advanced, most compressed writing tends to appear**, likely because these paragraphs try to preview several ideas at once and reach for a more "impressive" register to do it. Whenever you're writing a paragraph that introduces or summarizes what a whole section covers, that's exactly the moment to slow down and use short, plain sentences, one idea per sentence, rather than trying to pack everything into one elegant-sounding sentence. A bullet list (which this thesis already uses well elsewhere) is almost always a better tool for that job than a single dense sentence.

---

Ready for Chapter 3 (Testing and Evaluation) next, same three-stage process.
