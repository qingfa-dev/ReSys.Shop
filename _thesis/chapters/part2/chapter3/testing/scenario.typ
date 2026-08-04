=== Evaluation Scenarios

#import "../../../../template/ctu-styles.typ": figure-placeholder

The system is evaluated under three distinct operational scenarios to ensure it meets both functional and non-functional requirements.

// #figure(
//   figure-placeholder("Test Scenarios Diagram"),
//   caption: [Evaluation Workflow: Partitioning testing into Cold-Start, Concurrency, and Transactional Integrity phases.],
// )

==== Scenario A: Cold-Start vs. Warm-State Inference
*Objective:* Quantify the impact of the "Model Warmup" strategy implemented in `lifespan.py`.
- *Flow:*
  1. *Cold State:* Service Boot $\to$ First Request (triggers Lazy Load of 500MB weights) $\to$ Response.
  2. *Warm State:* Pre-warmed Memory $\to$ Request $\to$ Instant Response.
- *Metric:* P99 Latency (ms).

==== Scenario B: High-Concurrency Retrieval
*Objective:* Stress-test the Vector Database (`pgvector`) under load.
- *Configuration:* 50 concurrent virtual users performing `SearchByImage` operations.
- *Constraint:* System must maintain $< 500$ms latency at 100 RPS.
- *Implementation:* Simulated using `pytest-benchmark` against the `GET /search` endpoint.

==== Scenario C: End-to-End Transaction Integrity
*Objective:* Verify that the "Visual Search $\to$ Cart $\to$ Checkout" funnel operates without data loss.

```typescript
// E2E Validation Flow
START User Session
  1. UPLOAD "Blue Dress.jpg" -> Visual Search API
  2. SELECT Item #1 (ID: 123)
  3. POST /cart/items { id: 123 }
  4. POST /auth/login (Role: Guest -> User)
  5. ASSERT Cart.Contains(123) // Validation Point
END Session
```

// #figure(
//   figure-placeholder("E2E Validation Flow"),
//   caption: [Transactional flow verifying data persistence across the Guest-to-User State transition.],
// )

=== Evaluation Environment

All experiments were conducted on a standardized reference implementation to ensure fairness.

#figure(
  table(
    columns: (1fr, 1fr),
    stroke: 0.5pt,
    [*Component*], [*Specification*],
    [GPU Runtime], [NVIDIA RTX 4090 (24GB VRAM)],
    [CPU], [AMD Ryzen 9 7950X],
    [Database], [PostgreSQL 16 + pgvector 0.5.1],
    [Orchestrator], [.NET Aspire (Docker Compose mode)],
  ),
  caption: [Hardware and Software Environment for Benchmarks.],
  kind: table,
)

