# Chapter 3 Benchmark Audit — Which Claims Are Defensible?

Verdict per claim, verified against the **committed authoritative outputs**:
`benchmarks/outputs/thesis_catonly` (category-only) · `thesis_6models` (category+colour) ·
`thesis_catpat` (category+colour+pattern). These MATCH the thesis and are reproducible.
The stale `outputs_5k` / `outputs_5k_split` runs (FashionCLIP mAP 0.9051, 4 models only)
are NOT the source of the thesis numbers and must not be used to "correct" them.

## 1. Where the numbers live (verified)
| Ground-truth scheme | Committed output dir | Fashion-CLIP mAP (verified) |
|---|---|---|
| Category-only | `thesis_catonly` | 0.9336 |
| Category + colour | `thesis_6models` | 0.2439 |
| Category + colour + pattern | `thesis_catpat` | 0.2071 |

All six models' category-only mAP (±SD) in Chapter 3, Appendix A, and the abstract match
`thesis_catonly` exactly: Fashion-CLIP 0.9336±0.0060 · DINOv2 0.9299±0.0058 · CLIP B/16
0.9202±0.0043 · CLIP B/32 0.9184±0.0060 · ResNet-50 0.9132±0.0057 · EfficientNet-B0
0.9077±0.0076. P@K / R@K and the efficiency table (latency/throughput/load/storage) also
match the auto-generated `tables/thesis_aggregate.typ` / `thesis_efficiency.typ`.

## 2. Claim-by-claim verdict

### (a) DEFENSIBLE — experimentally demonstrated (keep as-is)
- Six-model ranking and all reported mAP / P@K / R@K values: match committed outputs. ✓
- 3-fold stratified CV, category-only binary relevance: documented in protocol; per-fold
  breakdown in Appendix A matches the aggregates. ✓
- Ground-truth sensitivity: absolute mAP collapses from ~0.93 (category-only) to ~0.20–0.25
  (colour) to ~0.16–0.21 (colour+pattern), and the ranking reorders (DINOv2 collapses under
  fine attributes; CLIP family stays robust). All three scheme values verified. This is the
  thesis's most valuable and honest finding — preserve it. ✓
- "highest observed mean mAP" phrasing: correct and appropriately qualified. ✓
- Efficiency headline ratios that check out: EfficientNet-B0 vs CLIP B/16 5.5× (235.5/42.6
  = 5.53) ✓; EffB0 vs FashionCLIP throughput 1.5× (21.4/14.2 = 1.51) ✓; mAP 97.2%
  (0.9077/0.9336) ✓; latency 37.5% (42.6/113.6) ✓; 2.67× latency increase (113.6/42.6) ✓;
  load 118.3 ms "less than a third" of ResNet 385.6 ms (0.307) ✓; storage ratios 1.6× /
  4.0–5.4× ✓. These are internally and externally consistent.
- "The pluggable model architecture … switch via one environment variable": supported by
  implementation (EMBEDDING_MODEL) and the multi-scheme benchmark. Defensible as an
  implementation property, not a research contribution. ✓

### (b) DEFENSIBLE WITH QUALIFICATION (keep, but wording must stay qualified)
- **Fashion-CLIP "highest" / recommendation:** fine AS LONG AS it stays "highest observed
  mean" with the 0.40% vs DINOv2 gap and 3-fold power caveat visible. Do not upgrade to
  "statistically significant superiority."
- **RAM figures:** thesis discloses they are NOT instrumented (psutil unreliable on the
  Linux host) and are estimates from parameter counts (~100 MB EffB0 to ~600 MB CLIP). This
  disclosure makes the claim defensible. FIX: Chapter 3 lists ranges while Appendix A lists
  "N/A" — standardize so they don't appear contradictory (both should state "estimated, not
  measured").
- **"Eleven candidates it supports":** defensible — the registry supports 11; 6 selected.
  Keep. (A prior REVIEW.md claimed six; that is now resolved — the framework config/tests
  assert 11.)
- **Deployment recommendation (Fashion-CLIP for quality, EfficientNet-B0 for CPU):** well
  grounded in the measured latency/accuracy trade-off. Defensible with the CPU-only caveat.
- **Content-based visual recommendation terminology:** correct and must be preserved (the
  system does NOT do collaborative filtering / personalization / session-based rec).

### (c) OVERCLAIMS TO FIX (independent of page cutting)
1. **CRITICAL — Statistical-bound error (§3.5 and §3.7).** The thesis claims Fashion-CLIP's
   ±2SD lower bound (0.9336 − 0.0120 = **0.9216**) "exceeds the upper bounds of the two CNN
   models only marginally." It does NOT. It overlaps ALL of them:
   - DINOv2 upper 0.9415 · CLIP B/16 0.9288 · CLIP B/32 0.9304 · ResNet 0.9246 ·
     EfficientNet 0.9229 — all above 0.9216.
   Reword to: Fashion-CLIP's lower bound **overlaps the upper bounds of every other model**,
   so on category-only retrieval the top tier is statistically indistinguishable. The current
   "non-overlapping-bounds heuristic" paragraph also lists the ResNet/EfficientNet numbers
   inconsistently with this.
2. **MAJOR — Efficiency ratio error (§3.6).** "42.6 ms is 2.2× faster than DINOv2 ViT-S/14
   (126.3 ms)" — actual 126.3/42.6 = **2.96 ≈ 3.0×**. Change 2.2× → ~3.0×.
3. **MAJOR — Throughput referent mislabeled (§3.6).** "throughput of 21.4 img/s is … 2.1×
   higher than the two CLIP variants' lower range." The CLIP variants' throughputs are 4.0
   and 11.9 (ratios 5.4× and 1.8×). The 2.1× figure is 21.4/10.2 = 2.10, i.e. vs DINOv2/ResNet,
   NOT the CLIP variants. Correct the referent or the number.
4. **MAJOR — RQ3 "independent scaling and fault isolation were achieved."** The benchmark
   demonstrates latency + model separation on CPU, not service failure, independent instance
   scaling, resource isolation, recovery, or concurrent-load throughput. Unless such
   experiments exist, classify this as an architecture/design property: "the architecture
   supports independent scaling and fault isolation; this was validated only at the latency /
   model-separation level." Split demonstrated vs design property.
5. **MAJOR — "production-ready" (§3.7 finding 6).** Downgrade to "production-oriented" /
   "deployment-feasible" / "feasible for small-scale deployment." The work supports the
   weaker claim.
6. **MODERATE — RAM Ch3 vs Appendix A inconsistency** (ranges vs N/A) — reconcile (see b).

### (d) DESIGN PROPERTY vs DEMONSTRATED (for any rewrite)
| Claim | Classification |
|---|---|
| Sub-second end-to-end latency on CPU | Demonstrated (latency + pgvector query latency) |
| Model switching via env var | Demonstrated (implementation + multi-scheme runs) |
| Independent scaling | Design property (not benchmarked) |
| Fault isolation | Design property (not benchmarked) |
| ACID consistency / transaction integrity | Design property (not stress-tested) |
| Production viability at millions of items | Projection / extrapolation, not measured |

## 3. Numerical-consistency audit (cross-location)
- **Consistent:** abstract, §1, Chapter 3 main body, Appendix A, and Part 3 conclusion all
  use 0.9336 / 0.9299 / 0.9202 / 0.9184 / 0.9132 / 0.9077 and the efficiency figures
  consistently. The old REVIEW.md "biggest issue" (Ch3 vs Appendix A mismatch) is resolved
  in the current source. ✓
- **Verified against raw data:** per-fold (0.9274/0.9340/0.9394 → 0.9336±0.0060 for
  Fashion-CLIP) checks out. ✓
- **Fix list:** the 2.2×→3.0× ratio; the "2.1× CLIP variants" referent; the statistical-bound
  paragraph; RAM disclosure consistency. These are the only numeric/claim issues found.

## 4. Reproducibility hygiene (recommendation)
The stale `outputs_5k` / `outputs_5k_split` directories contain different, non-authoritative
numbers (FashionCLIP mAP 0.9051) and only 4 models. They could be mistaken for the thesis
source. Recommend (optionally) documenting which output dirs are authoritative, or moving
stale ones aside, so a future reader/reproducer can't conflate them. This is a repo-hygiene
note, not a thesis-claim fix.

## 5. Bottom line for Q3
- Most headline claims are **defensible and reproducible** from committed outputs.
- **Five concrete fixes** are required before submission: (1) the statistical-bound overclaim
  [CRITICAL], (2) the 2.2×→3.0× latency ratio [MAJOR], (3) the mislabeled "2.1× CLIP
  variants" throughput referent [MAJOR], (4) RQ3 scaling/fault-isolation wording [MAJOR],
  (5) "production-ready" → "production-oriented" [MAJOR], plus (6) RAM disclosure consistency
  [MODERATE].
- The ground-truth sensitivity analysis, dataset-imbalance limitation, and
  statistical-power caveat are the thesis's strongest, most honest elements — **do not
  compress them.**
