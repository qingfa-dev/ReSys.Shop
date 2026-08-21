# Reviewer 4 — Perspective Report (Software Architecture / Industry)

**Persona:** Practitioner software architect who has shipped polyglot ML-in-production systems.
**Paper:** Building a Fashion E-Commerce Application with Recommendation and Image-Based Product Search
**Lens:** Deployability, pattern soundness, practical impact, operational realism.

## Strengths
- **Clean polyglot isolation.** The Python FastAPI sidecar behind an X-API-Key on an internal Docker network, with the .NET backend owning transactional logic, is exactly the right shape for keeping PyTorch out of the CLR. The strategy-pattern `ModelManager` switchable via `EMBEDDING_MODEL` is a genuinely good, reusable pattern (§2.4.4, §1.5.6).
- **pgvector ACID argument is the best contribution.** §1.4.5 / §1.4.6 correctly identify that co-locating embeddings and product rows in one PostgreSQL transaction eliminates the dual-store stale-index class of bugs. For SME-scale catalogues this is a strong, pragmatic call.
- **VSA + Carter + MediatR + FluentValidation** is a coherent, modern .NET stack; the 262-endpoint claim (Table 57) signals real breadth. Diagrams (C4, ERD, sequence) are present and readable.
- Two-tier HybridCache (L1 in-process + L2 Redis) and Hangfire for embedding queues are sensible production touches.

## Issues

### [MAJOR] P1 — RQ3 "architecture viability" is asserted, not demonstrated under load
- **Where:** §3.7.4 Answer to RQ3 (line 7996): *"end-to-end search latency stays under one second… independent scaling and fault isolation were achieved."*
- **Why it matters:** All latency numbers are **single-image inference on one laptop** (i7-1165G7). There is no concurrency test, no throughput-under-load measurement, no fault-injection beyond "sidecar restart" in the functional test suite (§3.3.2). "Independent scaling" and "fault isolation" are *design properties*, not *measured* ones. Calling them "achieved" overclaims.
- **Fix:** Either (a) add a lightweight load test (e.g., k6/vegeta against the search endpoint, reporting p50/p95 latency and error rate at, say, 10/50 concurrent users), or (b) rewrite RQ3's answer as "the design *supports* independent scaling and fault isolation; interactive latency was confirmed for single-query operation, with load behaviour left to future work." Do not claim measured properties you did not measure.

### [MODERATE] P2 — Latency budget inconsistency
- **Where:** §1.3.4.3 sets a sub-300 ms total-response target; §3.7.4 claims "<1 second" end-to-end. Fashion-CLIP inference alone is 96.8 ms plus IVFFlat (2.7–6.5 ms) plus network + .NET overhead — the 300 ms target is not clearly met once round-trips and serialization are included.
- **Fix:** Reconcile the target (pick one, e.g., "<1 s interactive" as the stated objective) and show the breakdown that supports it.

### [MINOR] P3 — "Production" index choice under-specified
- HNSW is "designated for production" but only IVFFlat was benchmarked (65–72% recall@10). For a real deploy you'd want HNSW numbers or an explicit "prototype-scale only" caveat. The §1.4.3 HNSW recall claims (">95% @ <10 ms, up to 10 M vectors") are cited from [12], not measured here.

### [MINOR] P4 — Scalability ceiling acknowledged but deployment guidance thin
- §1.4.6 correctly notes pgvector's "millions of vectors" ceiling. The deployment recommendation (§3.7.2) should state the explicit catalogue-size breakpoint at which a dedicated vector DB becomes necessary, so an SME adopter knows when to migrate.

## Practical-impact verdict
This is the manuscript's strongest dimension. As an **engineering reference implementation** it is credible and reusable. The only real weakness is treating design-time properties as measured results (P1). A short load test or honest re-wording would make the architecture claims bullet-proof.

| Dimension | Score |
|---|---|
| Architecture soundness | 78 |
| Deployability | 72 |
| Practical impact | 75 |
| Operational realism | 65 |
