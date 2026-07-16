# Benchmark Documentation Index

Complete documentation for the ReSys.Shop fashion image retrieval benchmark.

## Quick Navigation

| Document | What You'll Learn |
|----------|-----------------|
| [01 — Overview](01-overview.md) | What this benchmark is, why it exists, and what it measures |
| [02 — Models](02-models.md) | All supported models explained, with recommendations on which to use |
| [03 — Metrics](03-metrics.md) | Every metric explained with examples and intuition |
| [04 — Pipeline](04-pipeline.md) | How data flows from raw images to final reports |
| [05 — Datasets](05-datasets.md) | Available datasets and how to prepare them |
| [06 — Thesis Protocol](06-thesis-protocol.md) | The academic evaluation protocol for the CTU thesis |
| [07 — References](07-references.md) | Academic papers, tools, and further reading |
| [08 — Replication Guide](08-replication-guide.md) | Step-by-step to replicate all results (thesis + pipeline + pgvector) |
| [09 — Results](09-benchmark-results.md) | Full benchmark results — pipeline (5K, pgvector) + thesis (in-memory) |
| [09 — Visual Similarity Attributes](09-visual-similarity-attributes.md) | Analysis of 18 attribute combinations; colour normalisation rationale |
| [10 — Benchmark Comparison](10-benchmark-comparison.md) | 3-way comparison: category-only → cat+colour → cat+colour+pattern |
| [10 — Visual Similarity Pipeline](10-visual-similarity-pipeline.md) | End-to-end visual similarity pipeline architecture |
| [11 — Enriched Dataset](11-enriched-dataset.md) | Enriched dataset usage; dual-label (pattern) evaluation |
| [12 — Final Review](12-final-review.md) | Comprehensive documentation + methodology audit after all changes |
| [Directory Map](codebase/DIRECTORY_MAP.md) | Every folder and file explained with priorities |

## For New Users

Start here: [01 — Overview](01-overview.md) → [08 — Replication Guide](08-replication-guide.md) → [02 — Models](02-models.md) → [03 — Metrics](03-metrics.md) → [10 — Benchmark Comparison](10-benchmark-comparison.md)

## For Thesis Writers

Focus on: [08 — Replication Guide](08-replication-guide.md) → [06 — Thesis Protocol](06-thesis-protocol.md) → [03 — Metrics](03-metrics.md) → [10 — Benchmark Comparison](10-benchmark-comparison.md)

## External Resources

- [ML Benchmarking Glossary](../../docs/superpowers/specs/2026-07-15-ml-benchmarking-glossary.md) — Plain-English definitions of all ML terms
- [Enriched Benchmark Spec](../../docs/superpowers/specs/2026-07-15-enriched-benchmark-design.md) — Design document for enriched dataset + 3-way comparison
- [Code Review](codebase/CODE_REVIEW.md) — Full code review: 21 findings (bugs, perf, nits)
- Project `README.md` — Quick start commands

---

*This documentation lives in `benchmarks/docs/` and is versioned with the codebase.*
