# thesis/ — CTU Bachelor Thesis (Typst)

Typst 0.15.1 thesis "Fashion E-commerce with CBIR & Model Benchmarking" — bilingual EN/VI, CTU (Can Tho University) format. Content mirrors the ReSys.Shop platform (see repo-root `AGENTS.md`).

## Build & verify

```bash
typst compile main.typ        # -> main.pdf  (must run after ANY .typ change; compile fails on missing image refs)
typst watch main.typ          # live reload
make all                      # render diagram sources -> PNG (NOT the PDF build; diagrams only)
```

- `main.pdf` and `thesis-en.pdf` are committed to git (no gitignore; only `thesis/images/diagrams/*.png` is ignored at root). `thesis-en.pdf` is a stale snapshot — don't treat as source of truth.
- No CI or lint for this folder; `typst compile` is the whole verification step.

## Structure & wiring

- `main.typ` is the only entrypoint: imports `info.typ` + `template/` (ctu-styles, i18n), includes frontmatter, then parts 1–3 via aggregators `chapters/part{1,2,3}-*.typ`, then backmatter.
- Chapter pattern: `chapters/part2/chN-name/chN-name.typ` = `=` chapter + intro bullets, includes feature files `fN-name.typ` (each `==` section) which include numbered files `fN/NN-topic.typ` (`===`/`====`). Always create a matching aggregator when adding content files.
- Heading numbering is set per-part in `main.typ` (Part 1: Roman "I."; Parts 2–3: "1.1") with `#counter(heading)` resets. Never set numbering in chapter files. Level-3 headings are hidden from TOC in Part 1 only.
- `info.typ` holds all metadata: student/advisor/committee, title, keywords, and the abbreviations tuples — one per language; **add new abbreviations to BOTH `en` and `vi` tuples**. `template/i18n.typ` (`term(lang, key)`) holds CTU standard labels; `settings.primary_lang = "en"`.

## Conventions

- Figures: source `figures/chapters/part2/chN-name/fN-name/diagrams/P{part}S{ch}.{sec}_{name}.{mmd|puml}` → PNG beside source, referenced in chapters via `#figure(image("../../../../figures/...", width: 100%), caption: [...]) <fig-name>`; cross-ref as `@fig-...` / `@tbl-...`. Keep the `P{part}S{part}.{ch}.{sec}_` naming scheme.
- Citations: Typst `@key` syntax against `backmatter/bibliography.bib` (IEEE, ≥15 refs required). Multiple cites as `@a @b`.
- Compliance constraints (`compliance.json`): Times New Roman 13pt, margins L4/R2.5/T2.5/B2.5cm, line spacing 1.2, paragraph indent 1cm, abstract 200–350 words, 3–5 keywords. Headings: L1 14pt uppercase centered, L2 13pt uppercase, L3 13pt.
- `frontmatter/abstract.typ` hardcodes benchmark numbers (Fashion-CLIP mAP 0.8788, etc.) — keep in sync with `ch3-evaluation` when results change.

## Gotchas

- `chapters/part2/ch2-design/` has a stale duplicate: `04-implementation/` (singular, NOT included by `ch2-design.typ`) vs `04-implementations/` (plural, live). However the ml-pipeline diagram files live under `figures/.../04-implementation/diagrams/` and ARE referenced by live content — before deleting either, grep for `04-implementation` across `chapters/` and `figures/`.
- Makefile diagram quirks: PlantUML output PNG name is derived from `@startuml <name>` (make renames it to the source base name); the ERD-core mermaid rule uses `-w 2400 -s 1 -t neutral` while others use `-w 4800 -s 2 -t default`. Requires `mmdc` (mermaid-cli) + `java`; bundled `plantuml.jar` is committed.
- `review/pass{1..4}/06-editorial-decision.md` are EIC decisions from prior review passes (academic-paper-reviewer skill) — address their open findings before claiming completion.
- `.ctu-thesisrc` and `compliance.json` are tooling/config inputs (auto_renumber, format checks) — edit only if the tooling demands it.
