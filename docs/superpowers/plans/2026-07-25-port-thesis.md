# Port Old Thesis to CTU Typst Template — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Port 13 chapters (Markdown) + 19 Mermaid diagrams + benchmark data from old thesis into CTU-compliant Typst template at `.worktrees/feature/port-thesis/thesis/`.

**Architecture:** Two-pass conversion — extract Markdown content, convert to Typst syntax. Diagrams rewritten as Typst-native (cetz/fletcher or text-based). Benchmark tables recreated with CTU styling.

**Tech Stack:** Typst, ctu-thesis CLI, Mermaid (source reference), Python benchmark outputs.

## Global Constraints

- CTU format: Times New Roman 13pt, margins L4cm/R2.5cm/T2.5cm/B2.5cm, line spacing 1.2
- Warnings-as-errors: `ctu-thesis build` must compile cleanly
- English language throughout
- Bachelor thesis level (adapt MSc content down)
- `info.typ` values must be corrected before any content work
- bibliography.bib must be populated with real references
- Diagrams must be Typst-native (no Mermaid in final output)

---

## Phase 1: Fix Template Foundation

### Task 1: Fix info.typ Values

**Files:**
- Modify: `.worktrees/feature/port-thesis/thesis/info.typ`

**Context:** The `ctu-thesis init` CLI piped arguments in wrong order. All student/advisor/thesis fields are swapped.

- [ ] **Step 1: Replace info.typ with corrected values**

```typst
// ============================================================================
// CTU THESIS INFORMATION CONFIGURATION
// Can Tho University - College of Information and Communication Technology
// ============================================================================

#let info = (
  en: (
    student: (
      name: "Nguyen Thanh Phat",
      id: "B220001",
      class: "DI2296A1",
      major: "INFORMATION TECHNOLOGY",
      program: "High-Quality Program",
    ),
    advisor: (
      name: "TS. Tran Thi B",
      title: "TS",
    ),
    thesis: (
      title: "He thong Thuong mai Dien tu voi Tim kiem Anh Cong nghe",
      short_title: "He thong TMĐT",
      date: "July 2026",
      location: "Can Tho",
      degree: "BACHELOR OF ENGINEERING",
    ),
    keywords: (
      "e-commerce",
      "content-based image retrieval",
      "modular monolith",
      "fashion retrieval",
      "embedding models",
    ),
    committee: (
      chairman: "Dr. Chairman Name",
      reviewer: "Dr. Reviewer Name",
      advisor: "TS. Tran Thi B",
    ),
    abbreviations: (
      ("API", "Application Programming Interface"),
      ("CTU", "Can Tho University"),
      ("ICT", "Information and Communication Technology"),
      ("UI/UX", "User Interface/User Experience"),
      ("HTTP", "Hypertext Transfer Protocol"),
      ("CBIR", "Content-Based Image Retrieval"),
      ("CQRS", "Command Query Responsibility Segregation"),
      ("MediatR", "Mediator library for .NET"),
      ("EF Core", "Entity Framework Core"),
      ("JWT", "JSON Web Token"),
      ("pgvector", "PostgreSQL vector extension"),
    ),
  ),
  vi: (
    student: (
      name: "Nguyen Thanh Phat",
      id: "B220001",
      class: "DI2296A1",
      major: "CONG NGHE THONG TIN",
      program: "Chat luong cao",
    ),
    advisor: (
      name: "TS. Tran Thi B",
      title: "TS",
    ),
    thesis: (
      title: "He thong Thuong mai Dien tu voi Tim kiem Anh Cong nghe",
      short_title: "He thong TMĐT",
      date: "Thang 07/2026",
      location: "Can Tho",
      degree: "KY SU",
    ),
    keywords: (
      "thuong mai dien tu",
      "tim kiem anh cong nghe",
      "kien truc don mo",
      "tim kiem anh thoi trang",
      "mo hinh embedding",
    ),
    committee: (
      chairman: "TS. Ten Chu Tich",
      reviewer: "TS. Ten Phan Bien",
      advisor: "TS. Tran Thi B",
    ),
    abbreviations: (
      ("API", "Giao dien lap trinh ung dung"),
      ("CTU", "Dai hoc Can Tho"),
      ("CNTT-TT", "Cong nghe Thong tin va Truyen thong"),
      ("UI/UX", "Giao dien/Trai nguoi dung"),
      ("HTTP", "Giao thuc truyen tai sieu van ban"),
      ("CBIR", "Tim kiem anh cong nghe"),
      ("CQRS", "Phan cach lenh truy van"),
      ("MediatR", "Thu vien trung gian cho .NET"),
      ("EF Core", "Entity Framework Core"),
      ("JWT", "JSON Web Token"),
      ("pgvector", "Phan mo rong vector cua PostgreSQL"),
    ),
  ),
)

// ============================================================================
// GLOBAL SETTINGS (CTU STANDARD — Decision 4125/QĐ-ĐHCT 2024)
// ============================================================================
#let settings = (
  primary_lang: "en",

  // CTU Official Colors
  border_color: rgb(0, 51, 153), // CTU Blue (#003399)
  accent_color: rgb(0, 83, 159), // CTU Accent (#00539F)

  // CTU Format Requirements (2025-2026)
  format: (
    font: "Times New Roman",
    font_size: 13pt,
    line_spacing: 1.2,
    margins: (
      left: 4cm,
      right: 2.5cm,
      top: 2.5cm,
      bottom: 2.5cm,
    ),
    paragraph_indent: 1cm,
    abstract_words: (200, 350),
  ),
)
```

- [ ] **Step 2: Verify info.typ compiles**

Run: `cd .worktrees/feature/port-thesis/thesis && ctu-thesis build 2>&1 | head -20`
Expected: No errors related to info.typ fields.

- [ ] **Step 3: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/info.typ
git commit -m "fix(info): correct swapped values from CLI pipe order"
```

---

### Task 2: Restructure main.typ for 13-Chapter Port

**Files:**
- Modify: `.worktrees/feature/port-thesis/thesis/main.typ`

**Context:** Current main.typ has 3-part structure with placeholder chapters. Need to add includes for all 21 new chapter files.

- [ ] **Step 1: Rewrite main.typ with full chapter structure**

```typst
// ============================================================================
// CTU GRADUATION THESIS - MAIN FILE
// Can Tho University Format — Decision 4125/QĐ-ĐHCT (2024)
// ============================================================================

// 1. CONFIGURATION & IMPORTS
#import "info.typ": *
#import "template/ctu-styles.typ": ctu-styles
#import "template/i18n.typ": term

// 2. GLOBAL SETTINGS
#let lang = if settings.primary_lang in ("en", "vi") { settings.primary_lang } else { "en" }

// 3. DOCUMENT SETUP (CTU Format)
#show: doc => ctu-styles(doc, lang: lang)

// Document Metadata
#set document(
  title: info.at(lang, default: info.en).thesis.title,
  author: info.at(lang, default: info.en).student.name,
  keywords: info.at(lang, default: info.en).keywords,
)

// ============================================================================
// 4. FRONT MATTER (Roman numerals i, ii, iii...)
// ============================================================================
#set page(numbering: "i")
#counter(page).update(1)

// Cover Pages
#import "frontmatter/cover.typ": cover-page
#import "frontmatter/inner-cover.typ": inner-cover-page

#cover-page(lang: lang)
#inner-cover-page(lang: lang)

// Evaluation & Acknowledgements
#include "frontmatter/evaluation.typ"
#include "frontmatter/acknowledgements.typ"

// Lists (TOC, LOF, LOT)
#include "frontmatter/table-of-contents.typ"
#pagebreak()

#include "frontmatter/list-of-figures.typ"
#pagebreak()

#include "frontmatter/list-of-tables.typ"
#pagebreak()

// Abbreviations & Abstract
#include "frontmatter/abbreviations.typ"
#include "frontmatter/abstract.typ"

// ============================================================================
// 5. MAIN CONTENT (Arabic numerals 1, 2, 3...)
// ============================================================================
#set page(numbering: "1")
#counter(page).update(1)
#set heading(numbering: "1.1.1.1")

// Helper for Part Headings
#let part-heading(body) = {
  pagebreak()
  v(2cm)
  heading(level: 1, numbering: none, outlined: true)[#body]
}

// PART 1: INTRODUCTION
#part-heading[#term(lang, "part") 1: INTRODUCTION]
#counter(heading).update(1)
#include "chapters/part1-introduction.typ"

// PART 2: THESIS CONTENT
#part-heading[#term(lang, "part") 2: THESIS CONTENT]
#include "chapters/part2-content.typ"

// PART 3: CONCLUSION
#part-heading[#term(lang, "part") 3: CONCLUSION AND FUTURE WORK]
#counter(heading).step()
#include "chapters/part3-conclusion.typ"

// ============================================================================
// 6. BACK MATTER
// ============================================================================

// REFERENCES (IEEE Style — CTU Standard)
#pagebreak()
#bibliography("backmatter/bibliography.bib", title: term(lang, "ref"), style: "ieee")

// APPENDICES
#pagebreak()
#set page(numbering: none)
#counter(heading).update(0)
#set heading(numbering: "A.1")
#include "backmatter/appendices.typ"
```

- [ ] **Step 2: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/main.typ
git commit -m "refactor(main): restructure for full 13-chapter port"
```

---

## Phase 2: Port Part 1 — Introduction (5 files)

### Task 3: Port Chapter 1.1 — Context and Problem Statement

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part1/01-context.typ`
- Source: `docs/thesis/01-problem-analysis.md` §1.1-1.2

**Interfaces:**
- Produces: Typst file with `== Context and Problem Statement` heading

- [ ] **Step 1: Read source content**

Read `docs/thesis/01-problem-analysis.md` lines 1-35 (Background + Problem Statement).

- [ ] **Step 2: Convert MD→Typst**

Convert:
- `**Master of Science in Software Engineering**` → `[*Master of Science in Software Engineering*]`
- Numbered lists `1. **text**` → `#enum(numbering: "1.")[*text*]`
- Markdown tables → Typst `#figure(table(...))`
- Regular paragraphs → plain Typst text

- [ ] **Step 3: Write 01-context.typ**

```typst
== Context and Problem Statement

This thesis is submitted in partial fulfillment of the requirements for the degree of [*Bachelor of Engineering in Information Technology*]. It presents the complete analysis, design, implementation, and evaluation of ReSys.Shop — a fashion e-commerce platform with Content-Based Image Retrieval (CBIR) capabilities, featuring a comparative evaluation of multiple pretrained visual feature extraction models.

Fashion e-commerce represents one of the most competitive and technically demanding domains in online retail. Consumers expect rich visual experiences, personalized recommendations, and seamless checkout flows across multiple devices. Traditional text-based search often fails in fashion because shoppers struggle to articulate visual preferences (e.g., "a dress like this but in blue").

ReSys.Shop addresses three distinct problems simultaneously:

#enum(numbering: "1.")[
  [*The user-facing problem*]: How can a fashion e-commerce platform provide intuitive visual search using modern machine learning techniques?
][
  [*The engineering problem*]: How can a complex e-commerce system be architected to maintain modularity, testability, and operational clarity as it scales across 8 business domains?
][
  [*The ML evaluation problem*]: Which pretrained visual feature extraction model (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic) offers the optimal balance of retrieval effectiveness and operational performance for fashion CBIR?
]

== Problem Statement

Existing fashion e-commerce platforms typically fall into one of two categories:

#enum(numbering: "1.")[
  [*Monolithic platforms*] (e.g., early Shopify, Magento) that become unmaintainable as business logic interleaves across features
][
  [*Microservice platforms*] that introduce excessive operational overhead for small-to-medium teams, with distributed-transaction complexity for e-commerce workflows
]

Neither approach optimally serves a research context where rapid iteration on ML-powered features must coexist with stable transactional domains, the system must be demonstrable as a single deployable unit, and code quality must be examinable and justifiable.

#figure(
  caption: [Specific technical gaps identified],
  table(
    columns: 3,
    align: (left, left, left),
    table.header(
      [*Gap*], [*Evidence from prior art*], [*Consequence*],
    ),
    [Exception-driven error handling], [Typical ASP.NET controllers throw exceptions for validation failures], [Unpredictable control flow],
    [Anemic domain models], [EF entities are data bags with no behavior], [Business rules scattered across services],
    [Horizontal layering], [Controllers → Services → Repositories → Entities], [Changes touch 4+ files],
    [Tight module coupling], [Services directly reference other modules' repositories], [Cannot test modules in isolation],
    [Missing vector search], [Standard SQL databases cannot perform similarity search], [Fashion image search requires separate infrastructure],
    [No model comparison for CBIR], [Prior art selects embedding models arbitrarily], [Suboptimal model may be deployed],
  )
) <tab:gaps>
```

- [ ] **Step 4: Verify compilation**

Run: `cd .worktrees/feature/port-thesis/thesis && ctu-thesis build 2>&1 | tail -5`
Expected: Build succeeds.

- [ ] **Step 5: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part1/01-context.typ
git commit -m "feat(port): add ch1.1 context and problem statement"
```

---

### Task 4: Port Chapter 1.2 — Related Work (New Section)

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part1/02-related-work.typ`

**Context:** This is a new section not in the old thesis. Must write a literature review covering CBIR, modular monoliths, and fashion retrieval. Pull references from `benchmarks/docs/07-references.md`.

- [ ] **Step 1: Read references source**

Read `benchmarks/docs/07-references.md` for bibliography entries.

- [ ] **Step 2: Write 02-related-work.typ**

```typst
== Related Work

=== Content-Based Image Retrieval

Content-Based Image Retrieval (CBIR) systems retrieve images from a database based on visual similarity rather than textual metadata. Early CBIR systems relied on hand-crafted features such as color histograms, texture descriptors (Gabor filters), and shape representations (SIFT, SURF). These approaches suffered from the semantic gap — low-level visual features do not map reliably to high-level human concepts.

Deep learning transformed CBIR by learning visual representations directly from data. Convolutional Neural Networks (CNNs) such as ResNet and EfficientNet extract rich feature vectors that capture semantic content. More recently, Vision Transformers (ViT) and contrastive learning approaches like CLIP have enabled zero-shot visual retrieval by learning joint visual-linguistic representations.

=== Fashion Image Retrieval

Fashion-specific retrieval presents unique challenges: fine-grained visual differences (sleeve length, pattern, neckline), style consistency across views, and the need to match across diverse product categories. Fashion-CLIP extends CLIP with fashion-domain pretraining, achieving superior retrieval on fashion datasets compared to generic models.

Prior work in fashion CBIR typically selects a single embedding model without empirical comparison. This thesis addresses that gap by evaluating four models (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic) on both retrieval effectiveness and operational performance.

=== Modular Monolith Architecture

The modular monolith pattern combines the simplicity of a single deployable unit with the modularity benefits of microservices. Each business domain lives in an isolated module with explicit boundaries, communicating via in-process message dispatch rather than network calls.

Vertical slice architecture further refines this by organizing code around feature actions rather than technical layers. Each feature (e.g., "Create Product") is cohesively implemented in a single folder containing handler, endpoint, request, response, and validator — eliminating the cross-cutting concerns of traditional horizontal layering.

=== Vector Database Integration

PostgreSQL with the pgvector extension enables similarity search directly within the relational database, eliminating the need for a separate vector database. This approach leverages existing SQL tooling, transactions, and operational knowledge while providing cosine similarity and nearest-neighbor search on embedding vectors.

=== Summary

This thesis contributes a dual evaluation: (a) architectural patterns for modular e-commerce systems, and (b) empirical comparison of embedding models for fashion CBIR. The work bridges software engineering rigor with machine learning evaluation methodology.
```

- [ ] **Step 3: Add bibliography entries to backmatter/bibliography.bib**

```bibtex
@book{shaw2012software,
  author = {Shaw, Mary and Garlan, David},
  title = {Software Architecture: Perspectives on an Emerging Practice},
  year = {2012},
  publisher = {Prentice Hall},
}

@article{radenovic2019fine,
  author = {Radenovic, Filip and Tolias, Giorgos and Chum, Ondrej},
  title = {Fine-tuning CNN Image Retrieval with No Human Annotation},
  journal = {IEEE Transactions on Pattern Analysis and Machine Intelligence},
  year = {2019},
}

@article{li2023fashion,
  author = {Li, Chunyuan and Gan, Zhiwei and Yang, Zheng and Yang, Jianwei and Li, Lei and Wang, Li and Gao, Jianfeng},
  title = {Multimodal Pretraining with Language for Fashion},
  journal = {arXiv preprint},
  year = {2023},
}

@article{he2016deep,
  author = {He, Kaiming and Zhang, Xiangyu and Ren, Shaoqing and Sun, Jian},
  title = {Deep Residual Learning for Image Recognition},
  journal = {IEEE Conference on Computer Vision and Pattern Recognition},
  year = {2016},
}

@article{tan2019efficientnet,
  author = {Tan, Mingxing and Le, Quoc V.},
  title = {EfficientNet: Rethinking Model Scaling for CNNs},
  journal = {International Conference on Machine Learning},
  year = {2019},
}

@article{radford2021learning,
  author = {Radford, Alec and Kim, Jong Wook and Hallacy, Chris and Ramesh, Aditya and Goh, Gabriel and Agarwal, Sandhini and Sastry, Girish and Askell, Amanda and Mishkin, Pamela and Clark, Jack and others},
  title = {Learning Transferable Visual Models From Natural Language Supervision},
  journal = {International Conference on Machine Learning},
  year = {2021},
}
```

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part1/02-related-work.typ thesis/backmatter/bibliography.bib
git commit -m "feat(port): add ch1.2 related work and initial bibliography"
```

---

### Task 5: Port Chapter 1.3 — Objectives

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part1/03-objectives.typ`
- Source: `docs/thesis/01-problem-analysis.md` §1.3

- [ ] **Step 1: Read source**

Read `docs/thesis/01-problem-analysis.md` lines 36-57.

- [ ] **Step 2: Convert and write 03-objectives.typ**

Convert primary/secondary objectives to Typst enum lists. Replace "Master of Science" with "Bachelor of Engineering".

- [ ] **Step 3: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part1/03-objectives.typ
git commit -m "feat(port): add ch1.3 objectives"
```

---

### Task 6: Port Chapter 1.4 — Methodology

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part1/04-methodology.typ`
- Source: `docs/thesis/01-problem-analysis.md` §1.4-1.6

- [ ] **Step 1: Read source**

Read `docs/thesis/01-problem-analysis.md` lines 58-121.

- [ ] **Step 2: Convert and write 04-methodology.typ**

Convert scope, delimitations, stakeholders to Typst. Drop the `[ASK USER]` section (lines 124-127). Convert tables to Typst `#figure(table(...))`.

- [ ] **Step 3: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part1/04-methodology.typ
git commit -m "feat(port): add ch1.4 methodology"
```

---

### Task 7: Port Chapter 1.5 — Outline

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part1/05-outline.typ`

**Context:** New section — brief overview of thesis chapter structure.

- [ ] **Step 1: Write 05-outline.typ**

```typst
== Thesis Outline

This thesis is organized into three parts:

*Part 1: Introduction* provides the problem context, related work in content-based image retrieval and modular architecture, research objectives, and methodology.

*Part 2: Thesis Content* presents the system design and implementation across ten chapters: requirements analysis, system architecture, domain design, database design, API design, detailed design, security design, deployment design, testing strategy, and implementation highlights.

*Part 3: Conclusion* evaluates the system against research objectives, traces requirements to implementation, summarizes contributions, and identifies future work directions.
```

- [ ] **Step 2: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part1/05-outline.typ
git commit -m "feat(port): add ch1.5 thesis outline"
```

---

### Task 8: Update Part 1 Index

**Files:**
- Modify: `.worktrees/feature/port-thesis/thesis/chapters/part1-introduction.typ`

- [ ] **Step 1: Verify includes match created files**

Current content should already include all 5 files. Verify no missing includes.

- [ ] **Step 2: Commit (if changed)**

---

## Phase 3: Port Part 2 — Content (10 chapters)

### Task 9: Port Chapter 2.1 — Requirements Analysis

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter1-requirements.typ`
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter1/01-requirements.typ`
- Source: `docs/thesis/02-requirements-analysis.md` (188 lines)

- [ ] **Step 1: Read source**

Read full file. Identify FR/NFR tables, use cases, user roles.

- [ ] **Step 2: Convert to Typst**

Convert all Markdown tables to Typst `#figure(table(...))`. Convert use case descriptions to prose with `#enum(...)` lists. Preserve section hierarchy (`==`, `===`).

- [ ] **Step 3: Write chapter1-requirements.typ (index)**

```typst
= CHAPTER 1: REQUIREMENTS ANALYSIS

#include "chapter1/01-requirements.typ"
```

- [ ] **Step 4: Write chapter1/01-requirements.typ**

Full converted content from `02-requirements-analysis.md`.

- [ ] **Step 5: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2/chapter1-requirements.typ thesis/chapters/part2/chapter1/
git commit -m "feat(port): add ch2.1 requirements analysis"
```

---

### Task 10: Port Chapter 2.2 — System Architecture

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter2-architecture.typ`
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter2/01-architecture.typ`
- Source: `docs/thesis/03-system-architecture.md` (256 lines)

- [ ] **Step 1: Read source**

Read full file. Contains C4 diagram references, tech stack table, design patterns.

- [ ] **Step 2: Convert to Typst**

Replace Mermaid C4 references with placeholder text descriptions (diagrams ported separately in Phase 5). Convert tech stack table. Convert patterns section.

- [ ] **Step 3: Write files**

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2/chapter2-architecture.typ thesis/chapters/part2/chapter2/
git commit -m "feat(port): add ch2.2 system architecture"
```

---

### Task 11: Port Chapter 2.3 — Domain Design

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter3-domain.typ`
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter3/01-domain.typ`
- Source: `docs/thesis/04-domain-analysis.md` (248 lines)

- [ ] **Step 1: Read source**

Contains bounded contexts, aggregates, state machines, business rules.

- [ ] **Step 2: Convert to Typst**

Replace Mermaid state machine references with text descriptions. Convert bounded context list.

- [ ] **Step 3: Write files**

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2/chapter3-domain.typ thesis/chapters/part2/chapter3/
git commit -m "feat(port): add ch2.3 domain design"
```

---

### Task 12: Port Chapter 2.4 — Database Design

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter4-database.typ`
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter4/01-database.typ`
- Source: `docs/thesis/05-database-design.md` (247 lines)

- [ ] **Step 1: Read source**

Contains ERD references, schema, pgvector, indexing.

- [ ] **Step 2: Convert to Typst**

Replace Mermaid ERD with text table description. Convert indexing strategy.

- [ ] **Step 3: Write files**

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2/chapter4-database.typ thesis/chapters/part2/chapter4/
git commit -m "feat(port): add ch2.4 database design"
```

---

### Task 13: Port Chapter 2.5 — API Design

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter5-api.typ`
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter5/01-api.typ`
- Source: `docs/thesis/06-api-design.md` (229 lines)

- [ ] **Step 1: Read source**

Contains REST endpoints, request/response models, auth.

- [ ] **Step 2: Convert to Typst**

Convert endpoint tables. Convert request/response examples to code blocks.

- [ ] **Step 3: Write files**

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2/chapter5-api.typ thesis/chapters/part2/chapter5/
git commit -m "feat(port): add ch2.5 API design"
```

---

### Task 14: Port Chapter 2.6 — Detailed Design

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter6-detailed.typ`
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter6/01-detailed.typ`
- Source: `docs/thesis/07-detailed-design.md` (293 lines)

- [ ] **Step 1: Read source**

Contains sequence diagrams, class diagrams, ML workflow.

- [ ] **Step 2: Convert to Typst**

Replace Mermaid sequence/class references with text descriptions (diagrams ported in Phase 5).

- [ ] **Step 3: Write files**

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2/chapter6-detailed.typ thesis/chapters/part2/chapter6/
git commit -m "feat(port): add ch2.6 detailed design"
```

---

### Task 15: Port Chapter 2.7 — Security Design

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter7-security.typ`
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter7/01-security.typ`
- Source: `docs/thesis/08-security-design.md` (188 lines)

- [ ] **Step 1: Read source**

Contains defense-in-depth controls, auth/authz, validation.

- [ ] **Step 2: Convert to Typst**

Convert security controls table. Convert auth flow description.

- [ ] **Step 3: Write files**

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2/chapter7-security.typ thesis/chapters/part2/chapter7/
git commit -m "feat(port): add ch2.7 security design"
```

---

### Task 16: Port Chapter 2.8 — Deployment Design

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter8-deployment.typ`
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter8/01-deployment.typ`
- Source: `docs/thesis/09-deployment-design.md` (102 lines)

- [ ] **Step 1: Read source**

Smallest chapter. Contains Aspire orchestration, production arch.

- [ ] **Step 2: Convert to Typst**

Replace Mermaid deployment diagram reference. Convert Aspire description.

- [ ] **Step 3: Write files**

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2/chapter8-deployment.typ thesis/chapters/part2/chapter8/
git commit -m "feat(port): add ch2.8 deployment design"
```

---

### Task 17: Port Chapter 2.9 — Testing Strategy

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter9-testing.typ`
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter9/01-testing.typ`
- Source: `docs/thesis/10-testing-strategy.md` (185 lines)

- [ ] **Step 1: Read source**

Contains unit/integration/manual testing, mocking, coverage.

- [ ] **Step 2: Convert to Typst**

Convert testing table. Convert coverage targets.

- [ ] **Step 3: Write files**

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2/chapter9-testing.typ thesis/chapters/part2/chapter9/
git commit -m "feat(port): add ch2.9 testing strategy"
```

---

### Task 18: Port Chapter 2.10 — Implementation Highlights

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter10-implementation.typ`
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part2/chapter10/01-implementation.typ`

**Context:** New chapter — extract "Implementation" sections from old ch3-ch9 that aren't covered by dedicated chapters.

- [ ] **Step 1: Scan old chapters for Implementation sections**

Search `docs/thesis/03-system-architecture.md` through `09-deployment-design.md` for `## Implementation` or `### Implementation` sections.

- [ ] **Step 2: Merge into new chapter**

Extract implementation highlights: key code patterns, configuration decisions, technology choices that are implementation-specific (not design-level).

- [ ] **Step 3: Write files**

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2/chapter10-implementation.typ thesis/chapters/part2/chapter10/
git commit -m "feat(port): add ch2.10 implementation highlights"
```

---

### Task 19: Update Part 2 Index

**Files:**
- Modify: `.worktrees/feature/port-thesis/thesis/chapters/part2-content.typ`

- [ ] **Step 1: Update includes for 10 chapters**

```typst
#include "chapter1-requirements.typ"
#include "chapter2-architecture.typ"
#include "chapter3-domain.typ"
#include "chapter4-database.typ"
#include "chapter5-api.typ"
#include "chapter6-detailed.typ"
#include "chapter7-security.typ"
#include "chapter8-deployment.typ"
#include "chapter9-testing.typ"
#include "chapter10-implementation.typ"
```

- [ ] **Step 2: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part2-content.typ
git commit -m "refactor(port): update part2 index for 10 chapters"
```

---

## Phase 4: Port Part 3 — Conclusion (4 files)

### Task 20: Port Chapter 3.1 — Evaluation

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part3/01-evaluation.typ`
- Source: `docs/thesis/11-evaluation.md` (406 lines — largest chapter)

- [ ] **Step 1: Read source**

Contains benchmark methodology, ML metrics, statistical analysis, results discussion.

- [ ] **Step 2: Convert to Typst**

Convert evaluation tables. Replace Mermaid diagram references. Convert statistical formulas to Typst math notation.

- [ ] **Step 3: Include benchmark tables**

Read `benchmarks/outputs/thesis/tables/thesis_aggregate.typ` and `thesis_efficiency.typ`. Extract numbers, recreate with CTU styling.

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part3/01-evaluation.typ
git commit -m "feat(port): add ch3.1 evaluation"
```

---

### Task 21: Port Chapter 3.2 — Requirements Traceability

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part3/02-traceability.typ`
- Source: `docs/thesis/12-requirements-traceability-matrix.md` (177 lines)

- [ ] **Step 1: Read source**

Contains bidirectional traceability matrix.

- [ ] **Step 2: Convert to Typst**

Convert matrix table to Typst `#figure(table(...))`.

- [ ] **Step 3: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part3/02-traceability.typ
git commit -m "feat(port): add ch3.2 traceability matrix"
```

---

### Task 22: Write Chapter 3.3 — Conclusion (New)

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part3/03-conclusion.typ`

- [ ] **Step 1: Write conclusion**

```typst
== Conclusion

This thesis presented the design, implementation, and evaluation of ReSys.Shop — a fashion e-commerce platform with Content-Based Image Retrieval capabilities.

The primary contributions are:

#enum(numbering: "1.")[
  [*Modular monolith architecture*] demonstrating that 8 self-contained business modules can communicate exclusively via in-process message dispatch, achieving module isolation while maintaining single-unit deployability.
][
  [*Vertical-slice feature organization*] showing that co-locating handler, endpoint, request, response, and validator per feature reduces cross-cutting concerns and improves testability.
][
  [*Explicit error handling*] through the Result<T> type system, eliminating exception-driven control flow and making all failure paths traceable.
][
  [*Comparative ML evaluation*] of 4 embedding models (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic) on fashion image retrieval, with Fashion-CLIP achieving the highest mAP (0.7455) while EfficientNet-B0 offered the best latency-throughput balance.

The system validates that a modular monolith with vertical slices can support both stable transactional domains and rapidly iterating ML-powered features without architectural compromise.
```

- [ ] **Step 2: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part3/03-conclusion.typ
git commit -m "feat(port): add ch3.3 conclusion"
```

---

### Task 23: Write Chapter 3.4 — Future Work (New)

**Files:**
- Create: `.worktrees/feature/port-thesis/thesis/chapters/part3/04-future-work.typ`

- [ ] **Step 1: Write future work**

```typst
== Future Work

Several extensions emerge from this thesis:

#enum(numbering: "1.")[
  [*Recommendation engine*]: Collaborative filtering or hybrid approaches combining CBIR with user behavior data for personalized recommendations.
][
  [*Model fine-tuning*]: Custom training of embedding models on the fashion dataset to improve domain-specific retrieval accuracy.
][
  [*Multi-tenancy*]: Extending the single-store architecture to support multiple merchants with isolated data and configuration.
][
  [*CI/CD pipeline*]: Automated build, test, and deployment workflows for production readiness.
][
  [*Mobile native clients*]: iOS and Android applications using the same REST API backend.
][
  [*Extended evaluation*]: Larger ground-truth datasets, user study with real shoppers, and A/B testing of retrieval quality.
]
```

- [ ] **Step 2: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part3/04-future-work.typ
git commit -m "feat(port): add ch3.4 future work"
```

---

### Task 24: Update Part 3 Index

**Files:**
- Modify: `.worktrees/feature/port-thesis/thesis/chapters/part3-conclusion.typ`

- [ ] **Step 1: Update includes**

```typst
#include "01-evaluation.typ"
#include "02-traceability.typ"
#include "03-conclusion.typ"
#include "04-future-work.typ"
```

- [ ] **Step 2: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part3-conclusion.typ
git commit -m "refactor(port): update part3 index for 4 chapters"
```

---

## Phase 5: Convert Mermaid Diagrams to Typst Native

### Task 25: Convert Simple Diagrams (Use Case, State Machine, Deployment)

**Files:**
- Modify: `thesis/chapters/part2/chapter1/01-requirements.typ` (use-case)
- Modify: `thesis/chapters/part2/chapter3/01-domain.typ` (state machines)
- Modify: `thesis/chapters/part2/chapter8/01-deployment.typ` (deployment)

**Diagrams to convert:**
- `use-case.mmd` (87 lines) → inline in ch2.1 requirements
- `state-order.mmd` (40 lines) → inline in ch2.3 domain
- `state-payment.mmd` (35 lines) → inline in ch2.3 domain
- `deployment.mmd` (61 lines) → inline in ch2.8 deployment

- [ ] **Step 1: Convert use-case.mmd**

Add to `01-requirements.typ` as Typst text-based diagram with actor-use case mappings.

- [ ] **Step 2: Convert state machines**

Add to `01-domain.typ` as Typst state transition tables or text-based flow.

- [ ] **Step 3: Convert deployment diagram**

Add to `01-deployment.typ` as Typst box-and-arrow diagram.

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/
git commit -m "feat(diagrams): convert use case, state machines, deployment to Typst"
```

---

### Task 26: Convert C4 Diagrams (Context, Container, Component)

**Files:**
- Modify: `thesis/chapters/part2/chapter2/01-architecture.typ`

**Diagrams:**
- `c4-context.mmd` (34 lines)
- `c4-container.mmd` (49 lines)
- `c4-component.mmd` (65 lines)

- [ ] **Step 1: Convert C4 diagrams**

Add to `01-architecture.typ` as Typst box diagrams with labeled arrows. Use `#rect()` and `#line()` for boxes and connections.

- [ ] **Step 2: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/
git commit -m "feat(diagrams): convert C4 context, container, component to Typst"
```

---

### Task 27: Convert Sequence Diagrams

**Files:**
- Modify: `thesis/chapters/part2/chapter6/01-detailed.typ`

**Diagrams:**
- `sequence-checkout.mmd` (57 lines)
- `sequence-create-product.mmd` (55 lines)
- `sequence-image-search.mmd` (52 lines)

- [ ] **Step 1: Convert sequence diagrams**

Add to `01-detailed.typ` as Typst lifeline diagrams or text-based step sequences.

- [ ] **Step 2: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/
git commit -m "feat(diagrams): convert sequence diagrams to Typst"
```

---

### Task 28: Convert Class Diagrams

**Files:**
- Modify: `thesis/chapters/part2/chapter6/01-detailed.typ` (embedding models, result pipeline)
- Modify: `thesis/chapters/part2/chapter3/01-domain.typ` (identity, order, payment, product aggregates)

**Diagrams (6 files, largest 129 lines):**
- `class-embedding-models.mmd` → ch2.6 detailed design
- `class-identity-aggregate.mmd` → ch2.3 domain
- `class-order-aggregate.mmd` → ch2.3 domain
- `class-payment-aggregate.mmd` → ch2.3 domain
- `class-product-aggregate.mmd` → ch2.3 domain
- `class-result-pipeline.mmd` → ch2.6 detailed design

- [ ] **Step 1: Convert class diagrams**

Add to respective .typ files as Typst class tables or text-based UML.

- [ ] **Step 2: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/
git commit -m "feat(diagrams): convert class diagrams to Typst"
```

---

### Task 29: Convert Remaining Diagrams

**Files:**
- Modify: `thesis/chapters/part2/chapter4/01-database.typ` (ERD)
- Modify: `thesis/chapters/part2/chapter3/01-domain.typ` (bounded context map)
- Modify: `thesis/chapters/part2/chapter6/01-detailed.typ` (ML pipeline)

**Diagrams:**
- `erd-core.mmd` (133 lines)
- `bounded-context-map.mmd` (47 lines)
- `ml-pipeline.mmd` (71 lines)

- [ ] **Step 1: Convert ERD**

Add to `01-database.typ` as Typst entity-relationship table.

- [ ] **Step 2: Convert bounded context map**

Add to `01-domain.typ` as Typst text-based context map.

- [ ] **Step 3: Convert ML pipeline**

Add to `01-detailed.typ` as Typst flow diagram.

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/
git commit -m "feat(diagrams): convert ERD, context map, ML pipeline to Typst"
```

---

## Phase 6: Benchmark Tables and Bibliography

### Task 30: Recreate Benchmark Tables with CTU Styling

**Files:**
- Modify: `thesis/chapters/part3/01-evaluation.typ`

**Data sources:**
- `benchmarks/outputs/thesis/tables/thesis_aggregate.typ` (FashionCLIP mAP=0.7455, etc.)
- `benchmarks/outputs/thesis/tables/thesis_efficiency.typ` (latency, throughput, storage, RAM)

- [ ] **Step 1: Extract numbers from source tables**

Extract all values from the two generated Typst tables.

- [ ] **Step 2: Recreate with CTU styling**

Use `#figure(table(...))` with CTU formatting:
- 12pt font for captions
- Single line spacing for captions
- `#term(lang, "table")` for label
- Proper column alignment

- [ ] **Step 3: Verify table references**

Ensure `<tab:thesis-aggregate>` and `<tab:thesis-efficiency>` labels are preserved for cross-references.

- [ ] **Step 4: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/chapters/part3/01-evaluation.typ
git commit -m "feat(tables): recreate benchmark tables with CTU styling"
```

---

### Task 31: Populate bibliography.bib

**Files:**
- Modify: `.worktrees/feature/port-thesis/thesis/backmatter/bibliography.bib`

**Context:** Bibliography.bib is empty. Need entries from old thesis references.

- [ ] **Step 1: Read old thesis references**

Read `benchmarks/docs/07-references.md` for full bibliography.

- [ ] **Step 2: Convert to BibTeX**

Convert all references to BibTeX format. Ensure IEEE style compatibility.

- [ ] **Step 3: Commit**

```bash
cd .worktrees/feature/port-thesis
git add thesis/backmatter/bibliography.bib
git commit -m "feat(bib): populate bibliography from old thesis references"
```

---

## Phase 7: Build Verification and Cleanup

### Task 32: Full Build Verification

**Files:** All thesis files

- [ ] **Step 1: Run ctu-thesis build**

```bash
cd .worktrees/feature/port-thesis/thesis
ctu-thesis build 2>&1
```

Expected: Clean compilation, no errors.

- [ ] **Step 2: Run ctu-thesis validate**

```bash
ctu-thesis validate 2>&1
```

Expected: CTU compliance checks pass.

- [ ] **Step 3: Fix any compilation errors**

If errors found, fix them and re-run.

- [ ] **Step 4: Commit fixes**

```bash
cd .worktrees/feature/port-thesis
git add thesis/
git commit -m "fix(port): resolve compilation errors from full build"
```

---

### Task 33: Cleanup Stale Files

- [ ] **Step 1: Remove _thesis directory if exists**

```bash
rm -rf .worktrees/feature/port-thesis/_thesis
```

- [ ] **Step 2: Update .gitignore if needed**

Ensure `.worktrees/` is gitignored in main repo.

- [ ] **Step 3: Commit**

```bash
cd .worktrees/feature/port-thesis
git add -A
git commit -m "chore(port): cleanup stale files and gitignore"
```

---

## Summary

| Phase | Tasks | Files Created/Modified |
|-------|-------|----------------------|
| 1: Fix Foundation | 2 | info.typ, main.typ |
| 2: Part 1 Intro | 6 | 5 chapter files + index |
| 3: Part 2 Content | 11 | 10 chapter files + index |
| 4: Part 3 Conclusion | 5 | 4 chapter files + index |
| 5: Diagrams | 5 | 19 diagrams converted |
| 6: Tables + Bib | 2 | 2 tables + bibliography |
| 7: Verify + Cleanup | 2 | Build fixes |
| **Total** | **33** | **~35 files** |
