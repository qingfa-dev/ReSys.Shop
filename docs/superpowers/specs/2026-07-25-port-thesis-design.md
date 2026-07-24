# Design: Port Old Thesis to CTU Typst Template

**Date:** 2026-07-25  
**Branch:** `feature/port-thesis`  
**Status:** Approved

## 1. Goal

Port the existing MSc thesis content (13 chapters in Markdown + 19 Mermaid diagrams + benchmark data) into a new CTU-compliant Typst template, adapted to bachelor level. English language.

## 2. Source Material

| Location | Content | Count |
|----------|---------|-------|
| `docs/thesis/*.md` | Thesis chapters (Markdown) | 13 files |
| `docs/thesis/diagrams/*.mmd` | Mermaid diagrams | 19 files |
| `benchmarks/outputs/thesis/tables/*.typ` | Generated Typst tables with real data | 2 files |
| `benchmarks/outputs/thesis/results/*.json` | Benchmark results | 3 files |

## 3. Target Structure

CTU template uses 3-part structure with Roman numeral front matter and Arabic numeral main content.

### Chapter Mapping

| CTU Part | CTU Chapter | Old Source | Content |
|----------|------------|------------|---------|
| **Part 1: Introduction** | | | |
| | 1.1 Context & Problem | `01-problem-analysis.md` | Problem, objectives, scope |
| | 1.2 Related Work | **[new]** | Literature review (CBIR, modular monoliths, fashion retrieval) |
| | 1.3 Objectives | `01-problem-analysis.md` §objectives | Goals, contributions |
| | 1.4 Methodology | `01-problem-analysis.md` §scope | Research approach, delimitations |
| | 1.5 Outline | **[new]** | Thesis chapter overview |
| **Part 2: Content** | | | |
| | 2.1 Requirements | `02-requirements-analysis.md` | FR/NFR, use cases, roles |
| | 2.2 System Architecture | `03-system-architecture.md` | C4, tech stack, patterns |
| | 2.3 Domain Design | `04-domain-analysis.md` | Aggregates, state machines |
| | 2.4 Database Design | `05-database-design.md` | ERD, pgvector, indexing |
| | 2.5 API Design | `06-api-design.md` | REST endpoints, auth |
| | 2.6 Detailed Design | `07-detailed-design.md` | Sequence/class diagrams |
| | 2.7 Security | `08-security-design.md` | Defense-in-depth |
| | 2.8 Deployment | `09-deployment-design.md` | Aspire, production arch |
| | 2.9 Testing | `10-testing-strategy.md` | Unit/integration/manual |
| | 2.10 Implementation | Extract `## Implementation` sections from old ch3-ch9 | Code highlights, key decisions not covered by dedicated chapters |
| **Part 3: Conclusion** | | | |
| | 3.1 Evaluation | `11-evaluation.md` | Benchmarks, ML metrics, stats |
| | 3.2 Traceability | `12-requirements-traceability-matrix.md` | Req→impl→test matrix |
| | 3.3 Conclusion | **[new]** | Summary, contributions |
| | 3.4 Future Work | **[new]** | Extensions, research directions |

**Dropped:** `13-proposal-options.md` (internal planning, not thesis content).  
**New sections:** Related Work, Implementation highlights, Conclusion, Future Work.

## 4. Conversion Method

### Two-pass extraction + Typst conversion

**Pass 1 — Extract:** Read each old `.md` file. Extract prose, tables, code blocks, lists. Strip Markdown formatting artifacts.

**Pass 2 — Convert:** Convert extracted content to Typst syntax:
- `**bold**` → `[*bold*]`
- `` `code` `` → ` `` `code` `` `
- Markdown tables → Typst `#figure(table(...))`
- Code blocks → `#raw(lang: "...", ...)`
- Lists → `#list(...)`, `#enum(...)`

### Diagram Strategy

19 Mermaid diagrams → Typst native:
- Simple diagrams (use case, state machine, deployment) → Typst `cetz` canvas
- Complex diagrams (C4, sequence, class) → Typst `fletcher` or text-based with boxes
- Fallback: text descriptions with box layouts if rendering fails

### Benchmark Tables

Extract numbers from `benchmarks/outputs/thesis/tables/*.typ`. Recreate with CTU `show figure.where(kind: table)` styling for consistent formatting.

## 5. info.typ Fix

CLI piped values in wrong order. Correct mapping:

```typst
student.name: "Nguyen Thanh Phat"
student.id: "B220001"
student.class: "DI2296A1"
advisor.name: "TS. Tran Thi B"
advisor.title: "TS"
thesis.title: "He thong Thuong mai Dien tu"
thesis.short_title: "He thong TMĐT"
```

## 6. File Structure

```
chapters/
├── part1/
│   ├── 01-context.typ          ← from 01-problem-analysis.md
│   ├── 02-related-work.typ     ← [new] literature review
│   ├── 03-objectives.typ       ← from 01-problem-analysis.md §obj
│   ├── 04-methodology.typ      ← from 01-problem-analysis.md §scope
│   └── 05-outline.typ          ← [new] chapter overview
├── part2/
│   ├── chapter1-requirements.typ
│   ├── chapter1/
│   │   └── 01-requirements.typ  ← from 02-requirements-analysis.md
│   ├── chapter2-architecture.typ
│   ├── chapter2/
│   │   └── 01-architecture.typ  ← from 03-system-architecture.md
│   ├── chapter3-domain.typ
│   ├── chapter3/
│   │   └── 01-domain.typ        ← from 04-domain-analysis.md
│   ├── chapter4-database.typ
│   ├── chapter4/
│   │   └── 01-database.typ      ← from 05-database-design.md
│   ├── chapter5-api.typ
│   ├── chapter5/
│   │   └── 01-api.typ           ← from 06-api-design.md
│   ├── chapter6-detailed.typ
│   ├── chapter6/
│   │   └── 01-detailed.typ      ← from 07-detailed-design.md
│   ├── chapter7-security.typ
│   ├── chapter7/
│   │   └── 01-security.typ      ← from 08-security-design.md
│   ├── chapter8-deployment.typ
│   ├── chapter8/
│   │   └── 01-deployment.typ    ← from 09-deployment-design.md
│   ├── chapter9-testing.typ
│   ├── chapter9/
│   │   └── 01-testing.typ       ← from 10-testing-strategy.md
│   ├── chapter10-implementation.typ
│   └── chapter10/
│       └── 01-implementation.typ ← [merge from ch3-9]
├── part3/
│   ├── 01-evaluation.typ        ← from 11-evaluation.md
│   ├── 02-traceability.typ      ← from 12-requirements-traceability-matrix.md
│   ├── 03-conclusion.typ        ← [new]
│   └── 04-future-work.typ       ← [new]
```

## 7. Execution Order

1. Fix `info.typ` (correct swapped values)
2. Update `main.typ` (add new chapter includes, restructure parts)
3. Port Part 1 chapters (5 files) — convert MD→Typst
4. Port Part 2 chapters (10 files) — convert MD→Typst
5. Port Part 3 chapters (4 files) — convert MD→Typst
6. Convert 19 Mermaid diagrams → Typst native
7. Recreate benchmark tables with CTU styling
8. Write `ctu-thesis build` to verify compilation

## 8. Risks

- **Typst diagram complexity:** C4/sequence diagrams may not render cleanly in Typst. Fallback: text descriptions with box layouts.
- **Cross-references:** Old MD uses `[#ref]` notation. Need to convert to Typst `@label`/`~ref` system.
- **bibliography.bib:** Empty. Need to populate from old thesis references.
- **Page count:** Bachelor thesis has page limits. May need to trim or compress content.

## 9. Verification

```bash
cd .worktrees/feature/port-thesis/thesis
ctu-thesis build              # Compile to PDF
ctu-thesis validate           # Check CTU compliance
```
