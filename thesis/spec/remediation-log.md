# Remediation Log

One row per finding from thesis-review-MASTER-FIXLIST.md.

| finding_id | title | status | authoritative_source | files_changed | compile_ok | notes |
|------------|-------|--------|----------------------|---------------|------------|-------|
| T1-1 | Benchmark Table 67/68 vs Appendix A | fixed | thesis_results_category_only.json; thesis_results.json | 05-retrieval-performance.typ, thesis_aggregate.typ, thesis_efficiency.typ, 5 PNG charts | yes | Table 67 regenerated from JSON; diagram tables + PNG charts regenerated; make charts added to Makefile |
| T1-2 | "Eleven models" claim | fixed | benchmarks/src/benchmark/models/__init__.py:44-56 (11) | ml-sidecar.typ (Table 55) | yes | Review/rewrite said "six"; REJECTED per CON-001. Registry has 11; Table 55 expanded to list all 11. "Eleven" wording preserved everywhere. |
| T1-3 | Fashion-CLIP improvement % (15-20/5.4/6.1) | fixed | thesis_results_category_only.json | 04-model-selection.typ, 07-model-comparison.typ, ch4-conclusion.typ | yes | 3x "15 to 20%"→2.13%, 1x "6.1%"→2.13%, 4x "5.4%"→2.13% |
| T1-4 | Fabricated citations [6],[27] | fixed | real papers (DOIs) | bibliography.bib | yes | chia2022fashionclip: Scientific Reports 2022, 8 authors; wu2019fashioniq: CVPR 2021, 7 authors |
| T1-5 | "Nine bounded contexts" | fixed | Table 47 (8 rows) | 01-system-overview.typ | yes | "nine"→"eight" |
| T1-6 | 88 FRs / nine modules | fixed | Tables 10-17 sum = 87 | requirements.typ, ch2-design.typ, 05-api-design.typ | yes | "88/nine"→"87/eight", Dashboard dropped from FR list |
| T1-7 | "Near-zero P@20" | fixed | Appendix A.2/A.3 (~0.30) | ch4-conclusion.typ | yes | "near-zero"→"reduces substantially to ~0.30" |
| T1-8 | CBIR endpoint 3 URLs | fixed | Carter routes (code-verified) | 05-api-design.typ, ml-sidecar.typ, f1-visual-search.typ, cbir-search-sequence.puml | yes | Reconciled to api/storefront/catalog/products/images/search |
| T1-9 | Variable Vector Dimensions vs vector(512) | fixed | ImageEmbeddingConfiguration.cs:19 (code-verified) | 04-database-design.typ | yes | Reframed as "Fixed Vector Dimensions" with current constraint noted |
| T1-10 | Phantom "Section 2.1.5" | fixed | thesis TOC | 04-database-design.typ, data-persistence.typ | yes | "Section 2.1.5"→"Section 1.4.2" (HNSW/IVFFlat comparison) |
| T1-11 | Four vs five UI states | fixed | Table 58 (4 rows) | f1-visual-search.typ | yes | "five"→"four" |
| T1-12 | Thesis Outline chapter numbering | fixed | real TOC | ch1-introduction.typ | yes | Rewritten to match three-part structure |
| T2-13 | PostgreSQL 16 vs 17 | fixed | container tag pg17-trixie | 04-benchmark-protocol.typ | yes | "PostgreSQL 16"→"17" |
| T2-14 | pgvector 0.3.2 vs 0.7.0 | fixed | Chapter 3 (0.7.0) | technology-stack.typ | yes | "0.3.2"→"0.7.0" |
| T2-15 | Permission-string format | fixed | service code (PermissionMetadata, 4-part dots) | 7 files across ch2-design | yes | Standardized to domain.category.resource.action (4-part, dots) |
| T2-16 | "Eight use cases" storefront | fixed | §2.4.5.2.1-8 (9) | frontend-ux.typ | yes | "eight"→"nine" |
| T2-17 | Accuracy metrics count 3/5/7 | fixed | Table 65 (3 families) | 04-benchmark-protocol.typ, ch4-conclusion.typ | yes | Standardized to "3 families × 3 depths = 7 columns" |
| T2-18 | Pinterest 30% stat | fixed | pinterest2023visual ref | ch1-introduction.typ, ch1-background | yes | Softened to qualitative claim without unsupported 30% |
| T2-19 | Table 70 traceability mis-citations | fixed | section audit | ch4-conclusion.typ | yes | 6 rows fixed (§2.2.4→§2.3.4, §3.5→§3.7, +4 more) |
| T2-20 | EfficientNet-B0 3.4% vs 7.7% | fixed | thesis_results_category_only.json | 04-model-selection.typ | yes | "3.4%"→"4.65%" |
| T2-21 | DeepFashion missing co-author | fixed | real paper (Shi Qiu) | bibliography.bib | yes | Added "Qiu, Shi" to liu2016deepfashion |
| T2-22 | Sequential selection claim | fixed | benchmarks split script (seed 42, stratified) | b-dataset.typ | yes | "sequentially"→"stratified random sampling" |
| T3-1..6 | Polish items | fixed | | 4 files | yes | Section 2.2.2→2.2, support actors added, Chapter 6→3 ×2 |
| S4-1 | Frontend-UX screenshots (Option C, 46 shots) | planned | plan/feature-thesis-screenshots-1.md (verified against Admin/Store SPA source, Audit 2026-08-20) | plan/feature-thesis-screenshots-1.md; screenshots/ dir (to create); f1-f10 .typ (to edit); remediation-log.md; verify_remediation.py | yes | 35 kept + 3 new + 8 restored = 46; 6 fabricated screenshots removed; capture via Playwright at 1920x1200 @2x, fullPage PNG; target ~169 pages <=170. See plan for task matrix. |
