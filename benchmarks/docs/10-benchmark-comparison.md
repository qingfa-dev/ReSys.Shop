# 10 — Benchmark Comparison: Old vs New Ground Truth, Thesis vs Pipeline

Full comparative analysis of three benchmark runs, explained for both academic
evaluators and non-ML developers.

---

## Executive Summary (Non-ML Developer Version)

We compared AI models that automatically find similar-looking fashion products
in an online shop. The goal: **which model should power the "show me items like
this" feature?**

We ran three experiments with different configurations:

| Experiment | Dataset | Ground truth rule | Retrieval method | What it measures |
|---|---|---|---|---|
| **A. Old Thesis** | 44K images | Same category (any T-shirt ≈ any T-shirt) | NumPy in-memory | Category prediction (easy) |
| **B. Old Pipeline** | 44K images | Same category | PostgreSQL + pgvector | Production-like retrieval |
| **C. New Thesis** | 5K images | Same category **+ same colour group** | NumPy in-memory | **Visual similarity** (hard) |

**Key finding**: When we switched from "same category" to "same category + same
colour" as the definition of "similar," the best model's accuracy dropped from
75% to 25%. This isn't because the models are bad — it's because the old rule
was measuring the wrong thing. The new rule actually measures visual similarity.

**FashionCLIP** is the best model in all three experiments. **EfficientNet-B0**
is 2.6× faster while being nearly as accurate. For production, start with
FashionCLIP; switch to EfficientNet-B0 if latency is critical.

### Quick Decision Matrix

| If you need... | Use this model | Why |
|---|---|---|
| Best accuracy (visual similarity) | FashionCLIP | 24.5% mAP, best colour+type matching |
| Best speed | EfficientNet-B0 | 37.8ms/image, 30.2 images/sec, 0.220 mAP |
| Best storage efficiency | FashionCLIP | 512-d vectors, 1.95 MB per 1K images |
| Easiest to deploy | CLIP-generic | 512-d vectors, good accuracy, lots of docs |

---

## 1. What Are We Measuring?

### For Non-ML Developers

Imagine you run a clothing store website. A customer uploads a photo of a black
T-shirt and clicks "Find Similar." Your system must:

1. Convert the photo into numbers (an "embedding") using an AI model
2. Compare those numbers to every product in your catalogue
3. Show the top-20 closest matches

The question is: **which AI model does this best?**

To answer this, we need a way to score each model. We do this by building a
"ground truth" — a list that says, for every product in our catalogue, which
other products should be considered "similar." Then we check: did the model's
top-20 results include the items on that list?

**The ground truth IS the test.** If the ground truth says "a black T-shirt and
a blue T-shirt are similar," then a model that returns blue T-shirts for a black
T-shirt query would score 100%. But that's wrong — a real customer searching
with a black T-shirt doesn't want blue T-shirts.

So the ground truth must be carefully designed to match real user expectations.
This document explains how we designed it, tested three versions, and what we
learned.

### For Academic Readers

This benchmark evaluates **content-based image retrieval (CBIR)** for fashion
e-commerce. Four pre-trained embedding models are compared across 5,000 fashion
product images using a 3-fold stratified cross-validation protocol.

The primary contribution is methodological: we demonstrate that the choice of
ground-truth relevance rule fundamentally changes both the absolute and relative
performance of embedding models. The standard category-based ground truth
(masterCategory/subCategory) conflates taxonomic membership with visual
similarity, producing inflated metrics (mAP = 0.75, P@5 = 0.78) that do not
reflect real-world retrieval quality.

We introduce a **colour-normalized visual similarity ground truth** (§2) that
maps 46 raw colour labels to 11 perceptual colour categories and uses the
two-part relevance key `subCategory/normalizedColour`. Under this ground truth,
mAP drops to 0.25 — a 3× reduction that reflects the genuine difficulty of
fine-grained visual matching. The model ranking also shifts: CLIP-generic moves
from 4th to 2nd, suggesting vision-language pretraining benefits from colour
discrimination tasks.

A secondary contribution is the comparison between **in-memory cosine retrieval**
(thesis mode) and **pgvector IVFFlat-indexed retrieval** (pipeline mode). The
production pipeline introduces a recall gap of ~0.35 at K=20 (pgvector recall@20
= 0.65 vs exact cosine recall@20 = ~1.0), representing the trade-off between
query latency and retrieval completeness in a deployed system.

---

## 2. Ground Truth Evolution

### 2.1 Experiment A: Category-Only (Old Thesis, 44K Images)

**Rule**: Two products are "similar" if they share `masterCategory/subCategory`.

```
Query: Black T-shirt (Apparel/Topwear)
Relevant: ALL T-shirts, regardless of colour
          Blue T-shirt    → ✅ similar
          Red T-shirt     → ✅ similar
          Black shoe      → ❌ not similar (different subCategory)
          Black dress     → ❌ not similar (different subCategory)
```

**Problem**: This measures "is it a T-shirt?" — not "does it look like this?"
A model that learns "T-shirts go together, shoes go together" achieves high
mAP without understanding colour, pattern, or style at all.

### 2.2 Experiment B: Category-Only + pgvector (Old Pipeline, 44K Images)

Same ground truth as A, but uses PostgreSQL with pgvector IVFFlat indexing for
retrieval instead of in-memory NumPy. This adds a real-world factor:
approximate nearest-neighbour search in a database introduces a recall gap
between exact cosine similarity and indexed retrieval.

### 2.3 Experiment C: Category + Normalized Colour (New Thesis, 5K Images)

**Rule**: Two products are "similar" if they share `subCategory/normalizedColour`.

**Colour normalization** maps 46 raw labels to 11 perceptual groups:

| Normalized | Raw labels folded in |
|---|---|
| Black | Black, Charcoal |
| White | White, Off White, Cream |
| Blue | Blue, Navy Blue, Turquoise Blue, Teal, Aqua, Sky Blue, … |
| Red | Red, Maroon, Burgundy, Coral, Magenta, Rose, Mauve, … |
| Green | Green, Olive, Lime Green |
| Grey | Grey, Silver |
| Brown/Yellow | Brown, Tan, Beige, Khaki, Gold, Yellow, Mustard, Nude, … |

```
Query: Black T-shirt (Topwear/Black)
Relevant: ONLY black-coloured T-shirts
          Black T-shirt   → ✅ similar
          Navy Blue T-shirt → ✅ similar (normalized to Blue... wait, Navy→Blue)
          Black polo       → ✅ similar (both Topwear/Black)
          Red T-shirt      → ❌ not similar (different colour)
          Blue T-shirt     → ❌ not similar (different colour)
          Black shoe       → ❌ not similar (different subCategory)
```

Wait — Navy normalizes to Blue. So a Navy Blue T-shirt would have key
`Topwear/Blue`, NOT `Topwear/Black`. That means a Black T-shirt query and a
Navy T-shirt result are NOT similar under this rule. This is a deliberate
choice — while Navy and Black are both dark, they are distinct colour categories.

---

## 3. Results: Full Comparison

### 3.1 Retrieval Effectiveness (mAP — primary metric)

| Model | A. Old Thesis (mAP) | B. Old Pipeline (mAP) | C. New Thesis (mAP) |
|-------|--------------------|-----------------------|---------------------|
| FashionCLIP | 0.746 ± 0.009 | 0.879 ± 0.002 | **0.245 ± 0.004** |
| CLIP-generic | 0.703 ± 0.022 | 0.834 ± 0.004 | **0.231 ± 0.006** |
| EfficientNet-B0 | 0.720 ± 0.016 | 0.816 ± 0.001 | **0.220 ± 0.006** |
| ResNet-50 | 0.715 ± 0.026 | 0.812 ± 0.005 | **0.209 ± 0.004** |

**What this means (non-ML)**: In experiment A, FashionCLIP scored 74.6% — meaning
about 75% of its top results were correct. But "correct" meant "in the same
category" (any T-shirt counts). In experiment C, the score drops to 24.5% —
now "correct" means "same category AND same colour." The 24.5% is the real
number — when a user searches with a black T-shirt, FashionCLIP puts a same-colour
T-shirt in the top results about 1 in 4 times.

**What this means (academic)**: The 3× mAP reduction (0.75 → 0.25) is not a model
failure — it is a ground-truth calibration. Under the original rule, the
effective upper bound on mAP is determined by the model's category
discrimination ability, which saturates quickly (mAP = 0.75–0.88). Under the
colour-normalized rule, the upper bound is determined by the model's
fine-grained colour+type discrimination, which has substantially more headroom
for improvement. This makes the benchmark a more useful instrument for future
model comparison.

### 3.2 Precision@K and Recall@K (Exp. C — New Thesis)

| Model | P@5 | P@10 | P@20 | R@5 | R@10 | R@20 |
|-------|-----|------|------|-----|------|------|
| FashionCLIP | 0.429 | 0.390 | 0.351 | 0.065 | 0.104 | 0.167 |
| CLIP-generic | 0.412 | 0.374 | 0.333 | 0.057 | 0.095 | 0.152 |
| EfficientNet-B0 | 0.393 | 0.363 | 0.327 | 0.052 | 0.088 | 0.141 |
| ResNet-50 | 0.382 | 0.349 | 0.315 | 0.050 | 0.084 | 0.136 |

**For non-ML developers**: 

- **P@5 = 0.43** means: when FashionCLIP shows 5 results, ~2.2 of them are
  actually the same type AND colour as the query. The other ~2.8 are wrong.
- **R@20 = 0.17** means: out of all same-colour-same-type items in the
  catalogue, FashionCLIP finds only 17% of them. 83% are missed.
- This is low, but reflects reality: with ~16 same-colour items per type
  scattered across 3,332 gallery items, finding them is genuinely hard.

### 3.3 Operational Performance

| Model | Embed Time (ms) | Load Time (s) | Storage (MB/1K) | Throughput (img/s) |
|-------|-----------------|---------------|-----------------|-------------------|
| FashionCLIP | 96.8 | 5.26 | 1.95 | 18.5 |
| CLIP-generic | 86.6 | 6.85 | 1.95 | 21.4 |
| EfficientNet-B0 | **37.8** | **0.11** | 4.88 | **30.2** |
| ResNet-50 | 61.9 | 0.37 | 7.81 | 13.5 |

**For non-ML developers**:

- EfficientNet-B0 is the fastest: 38ms to process one image (about 26 images per second per CPU core)
- FashionCLIP loads in 5.3 seconds on startup — acceptable for a server that runs 24/7
- ResNet-50 stores 4× more data per image (8 MB vs 2 MB per 1,000 images) — matters at scale
- For a catalogue of 100,000 products: FashionCLIP needs ~195 MB storage; ResNet-50 needs ~781 MB

### 3.4 Hypothesis Testing (Exp. C)

| Hypothesis | Prediction | Observed | Cohen's d | Verdict |
|-----------|-----------|----------|-----------|---------|
| H1: FashionCLIP > all on mAP | FashionCLIP has highest mAP | 0.245 vs 0.231 (CLIP-generic) | 1.88–6.50 | ✅ Supported |
| H2: EfficientNet-B0 best mAP/ms | Best efficiency metric | 0.220 mAP at 37.8ms (5.8 mAP/ms) vs FashionCLIP 0.245 at 96.8ms (2.5 mAP/ms) | — | ✅ Supported |
| H3: ResNet-50 highest storage | 4× storage of 512-d models | 7.81 MB/1K vs 1.95 MB/1K (4.0×) | — | ✅ Supported |
| H4: CLIP-generic > CNNs | CLIP beats EfficientNet & ResNet | mAP 0.231 > 0.220 > 0.209 | — | ✅ Supported |

---

## 4. Model Ranking Shift

The ranking changed between old and new ground truth:

| Rank | Old (category only) | New (category + colour) |
|------|--------------------|------------------------|
| 1 | FashionCLIP (0.746) | FashionCLIP (0.245) |
| 2 | EfficientNet-B0 (0.720) | **CLIP-generic (0.231)** ⬆ |
| 3 | ResNet-50 (0.715) | EfficientNet-B0 (0.220) ⬇ |
| 4 | CLIP-generic (0.703) | ResNet-50 (0.209) |

**CLIP-generic moved from 4th to 2nd.** Under the old rule, CLIP-generic was
indistinguishable from the CNNs. Under the new colour-sensitive rule, it clearly
outperforms them. This suggests that **vision-language pretraining (CLIP)
provides better colour discrimination** than ImageNet-pretrained CNNs — a
finding that was invisible under the category-only ground truth.

As fashion retrieval is inherently colour-sensitive (a user searching with a
red dress does not want blue dresses), this ranking shift has practical
importance for model selection.

---

## 5. Thesis vs Pipeline: Retrieval Engine Comparison

### 5.1 What They Measure

| | Thesis Mode | Pipeline Mode |
|---|---|---|
| **Retrieval engine** | In-memory NumPy cosine similarity | PostgreSQL + pgvector (IVFFlat index) |
| **Retrieval type** | Exact nearest neighbour | Approximate nearest neighbour |
| **Query latency** | O(N×D) per query, no DB overhead | Indexed search, SQL query overhead |
| **Relevance ground truth** | Split file labels | Same split file labels |
| **Extra metrics** | None | pgvector index build time, pgvector query latency, pgvector recall@K |
| **Suitable for** | Academic evaluation, model comparison | Production readiness assessment |

### 5.2 pgvector Recall Gap (Old Pipeline, 44K Dataset)

The pipeline benchmark measures how much retrieval accuracy is lost when moving
from exact cosine similarity to pgvector approximate search:

| Model | Exact cosine P@5 | pgvector R@5 | pgvector R@10 | pgvector R@20 | Gap (R@20) |
|-------|-----------------|--------------|---------------|---------------|------------|
| FashionCLIP | 0.930 | 0.761 | 0.715 | 0.652 | — |
| CLIP-generic | 0.903 | 0.729 | 0.686 | 0.622 | — |
| EfficientNet-B0 | 0.890 | 0.690 | 0.645 | 0.583 | — |
| ResNet-50 | 0.884 | 0.000 | 0.000 | 0.000 | ⚠️ 2048-d too large |

**ResNet-50 cannot use IVFFlat indexing** because pgvector's IVFFlat
implementation has a 2000-dimension limit. ResNet-50 embeddings are 2048-d.
This means ResNet-50 must use exact search (seqscan) in production, incurring
linear scan cost.

The pgvector recall@K (~0.58–0.65) represents the fraction of exact-cosine
nearest neighbours that the IVFFlat index also retrieves. The ~0.35 gap between
exact and approximate recall is the price of indexed search speed — acceptable
for production where query latency matters more than perfect recall.

### 5.3 Production Pipeline Timings (Old Pipeline, 44K Dataset)

| Model | pgvector query latency | Index build time | Ingestion time |
|-------|----------------------|-----------------|---------------|
| FashionCLIP | 2.7 ms | 0.25 s | 3.4 s |
| CLIP-generic | 2.9 ms | 0.24 s | 3.3 s |
| EfficientNet-B0 | 6.5 ms | 0.55 s | 8.2 s |
| ResNet-50 | 0.0 ms (no index) | 0.0 s (no index) | — |

pgvector queries complete in 2.7–6.5 ms — well within acceptable bounds for
real-time search. Index build time is sub-second, allowing fast catalogue
updates. Ingestion time (embedding generation + pgvector INSERT) is 3–8 seconds
per fold-batch, reflecting the cost of database write throughput.

---

## 6. Why the Old Results Were Misleading

### 6.1 The Category-Only Problem

Under the original ground truth, "similar" meant "same `masterCategory/subCategory`."
This produced a 47-group relevance structure averaging 945 items per group:

```
Any Apparel/Topwear → Any Apparel/Topwear  ✅ relevant
Black T-shirt      → Blue T-shirt          ✅ (same category, different colour — WRONG)
Black T-shirt      → White T-shirt         ✅ (same category, different colour — WRONG)
Black T-shirt      → Black dress           ❌ (different subCategory — correct)
Black T-shirt      → Black shoe            ❌ (different subCategory — correct)
```

The model only needed to learn category boundaries — "is this a T-shirt or a
shoe?" — which is a coarse classification task that most pre-trained models
solve easily. This produced **inflated metrics** (mAP = 0.75, P@5 = 0.78) that
did not reflect real fashion retrieval quality.

An e-commerce user searching with a black T-shirt who gets 5 blue T-shirts in
response would think the feature is broken — even though the benchmark scored
that as 100% correct. The benchmark was measuring the wrong thing.

### 6.2 The P@20/R@20 Zero Bug

In the old thesis results, all P@20 and R@20 values were 0.0. This was a bug —
not a real result. The evaluation pipeline had a computation error that affected
only K=20. This bug was fixed as part of the ground truth refactoring.
The new results show valid P@20 and R@20 values across all models.

---

## 7. The Colour-Normalized Ground Truth

### 7.1 Why Normalize Colours?

The Fashion Product Images dataset contains 46 distinct `baseColour` labels:

```
Black, Charcoal, White, Off White, Cream, Blue, Navy Blue, Dark Blue,
Light Blue, Sky Blue, Turquoise Blue, Turquoise, Teal, Aqua, Sea Green,
Red, Maroon, Burgundy, Rust, Peach, Coral, Magenta, Rose, Mauve,
Pink, Lavender, Green, Olive, Lime Green, Purple, Grey, Silver,
Orange, Multi, Brown, Coffee Brown, Mushroom Brown, Tan, Beige,
Khaki, Nude, Taupe, Copper, Bronze, Gold, Yellow, Mustard, Lemon
```

These are **marketing labels**, not perceptual measurements. "Navy Blue" and
"Blue" are visually similar but produce different relevance keys. Using raw
labels creates 857 groups with 21% solo items — products that have no
colour-mate in the catalogue.

Normalization reduces these to 11 perceptual groups following Berlin & Kay
(1969) basic colour terms:

| Normalized | Raw labels folded in | Items |
|---|---|---|
| Black | Black, Charcoal | 9,728 |
| White | White, Off White, Cream | 6,110 |
| Blue | Blue, Navy Blue, Turquoise Blue, Teal, Aqua, Sky Blue, … | 6,896 |
| Red | Red, Maroon, Burgundy, Rust, Peach, Coral, Magenta, Rose, Mauve | 3,342 |
| Green | Green, Olive, Lime Green | 2,553 |
| Grey | Grey, Silver | 4,059 |
| Pink | Pink, Lavender | 1,888 |
| Purple | Purple | 1,960 |
| Orange | Orange | 530 |
| Brown/Yellow | Brown, Tan, Beige, Khaki, Gold, Yellow, Mustard, Lemon, Nude, Taupe | 4,724 |
| Multi | Multi | 394 |

### 7.2 Why Only 2 of 6 Attributes

The dataset has 6 categorical attributes. Only 2 are visual:

| Attribute | Visual? | Reason |
|-----------|---------|--------|
| `subCategory` | ✅ Yes | Defines the shape/type of the item |
| `baseColour` | ✅ Yes (after normalization) | The most prominent visual feature |
| `articleType` | ⚠️ Partial | Too granular — separates visually similar items (Tshirts vs Shirts vs Polos) |
| `usage` | ❌ No | Marketing label — "Casual" T-shirt looks the same as "Sports" T-shirt |
| `season` | ❌ No | Same item tagged differently — "Summer" black T-shirt = "Winter" black T-shirt |
| `gender` | ❌ No | Demographic label — Men's black T-shirt looks similar to Women's black T-shirt |

Adding any non-visual attribute **degrades** the benchmark — it creates
false negatives by splitting visually identical items into separate relevance
groups. Detailed analysis in `docs/09-visual-similarity-attributes.md`.

---

## 8. Practical Recommendations

### 8.1 Which Model for Production?

Three criteria matter in production:

1. **Retrieval quality** (mAP)
2. **Latency** (ms/image)
3. **Storage cost** (MB per 1,000 embeddings)

| Model | mAP | Latency | Storage/1K | Best for… |
|-------|-----|---------|------------|-----------|
| FashionCLIP | 0.245 | 96.8ms | 1.95 MB | Maximum accuracy |
| CLIP-generic | 0.231 | 86.6ms | 1.95 MB | Good accuracy, open-source |
| EfficientNet-B0 | 0.220 | **37.8ms** | 4.88 MB | Latency-sensitive apps |
| ResNet-50 | 0.209 | 61.9ms | 7.81 MB | Not recommended |

**Recommendation**: Deploy **FashionCLIP** as the default model. It provides the
best visual similarity retrieval across all metrics. If query latency must be
below 50ms (e.g., real-time search suggestions), fall back to
**EfficientNet-B0** which achieves 2.6× lower latency at a 0.025 mAP penalty.

**ResNet-50 is not recommended** for production CBIR: it has the lowest mAP,
highest storage cost (4× of 512-d models), and cannot use pgvector IVFFlat
indexing (2048-d exceeds the 2000-d limit).

### 8.2 How to Read the Numbers

For a non-ML stakeholder evaluating these results:

| Question | Answer |
|----------|--------|
| "Is 24.5% mAP good?" | It's the honest number. The old 74.6% was misleading. 24.5% means the model gets ~1 in 4 results right when you search with a photo. This is realistic — fashion similarity is genuinely hard. |
| "Which model should we use?" | FashionCLIP for best quality. EfficientNet-B0 if speed matters more than a few percentage points of accuracy. |
| "How much does storage matter?" | At 100K products, FashionCLIP uses ~195 MB for embeddings. ResNet-50 uses ~781 MB. The difference matters at scale. |
| "Can we improve these numbers?" | Yes — the benchmark shows that even the best model has substantial headroom (75% of results are not correct). Fine-tuning on fashion data or adding attribute-aware re-ranking would help. |

### 8.3 Reproducing These Results

```bash
# Create 5K subset
cd benchmarks
head -5001 data/raw/fashion-product-images-small/styles.csv > data/raw/fashion-5k/styles.csv
ln -s "$(pwd)/data/raw/fashion-product-images-small/images" data/raw/fashion-5k/images

# Run thesis benchmark (CPU)
uv run benchmark thesis \
  --dataset-root data/raw/fashion-5k \
  --folds 3 --seed 42 --device cpu

# Run pipeline benchmark (requires PostgreSQL + pgvector)
docker compose -f old/ReSys.Research/docker-compose.yml up -d
uv run benchmark pipeline \
  --dataset-root data/raw/fashion-5k \
  --folds 3 --seed 42 --device cpu \
  --conn-string 'postgresql://benchmark:benchmark@localhost:5433/benchmark'

# Compare results
cat outputs/thesis/results/thesis_results.json
cat outputs/pipeline/results/pipeline_results.json
```

---

## Glossary

| Term | Plain English | Technical |
|------|--------------|-----------|
| **Embedding** | A list of 512 numbers that describes an image. Two images with similar lists look similar. | A dense float32 vector produced by the penultimate layer of a neural network, L2-normalized to unit length. |
| **Ground truth** | The "answer key" — which items should be considered similar to which. | A binary relevance labeling function R(q, g) → {0, 1}. |
| **mAP** | A single score (0–1) for how good the model is at ranking similar items. Higher = better. | Mean Average Precision — average of precision scores at each relevant retrieval position, averaged across queries. |
| **P@K** | Of the top K results, what fraction are actually similar? | Precision at cutoff K — |relevant ∩ retrieved| / K. |
| **R@K** | Of all similar items in the catalogue, what fraction did the model find? | Recall at cutoff K — |relevant ∩ retrieved| / |relevant|. |
| **Cohen's d** | How much better is one model than another? d > 0.8 = big difference. | Standardized mean difference — (μ₁ − μ₂) / σ_pooled. |
| **3-fold CV** | Run the test 3 times, each time using a different third as the test set. Averages out luck. | K-fold cross-validation with K=3, stratified by category proportion. |
| **Colour normalization** | Turn "Navy Blue" → "Blue" so that visually identical colours are grouped together. | Mapping of 46 raw baseColour labels to 11 perceptual colour categories. |
| **pgvector** | A PostgreSQL plugin that lets you search by "closest match" instead of exact text match. | PostgreSQL extension providing `vector` type and `<=>` cosine distance operator with IVFFlat indexing. |
| **IVFFlat** | A speed trick for database search — pre-groups similar items so you only search a fraction of the catalogue. | Inverted File with Flat compression — approximate nearest neighbour index with configurable list count. |

---

## References

1. **ReSys.Shop Benchmark Documentation**:
   - `benchmarks/docs/01-overview.md` — project overview
   - `benchmarks/docs/03-metrics.md` — metric definitions with examples
   - `benchmarks/docs/05-datasets.md` — dataset structure and preparation
   - `benchmarks/docs/06-thesis-protocol.md` — thesis evaluation protocol
   - `benchmarks/docs/08-visual-similarity-pipeline.md` — pipeline explanation
   - `benchmarks/docs/09-visual-similarity-attributes.md` — attribute analysis

2. **Source Code**:
   - `src/benchmark/datasets/ground_truth.py` — `_normalize_colour()`, `build_relevance_sets()`
   - `src/benchmark/retrieval/cosine.py` — in-memory cosine retrieval
   - `src/benchmark/retrieval/pgvector.py` — `PgvectorRetriever`
   - `src/benchmark/evaluation/thesis.py` — `ThesisRunner`
   - `src/benchmark/evaluation/pipeline.py` — `PipelineRunner`

3. **Result Data**:
   - `outputs/thesis.v1/` — Old category-only thesis results (44K images)
   - `outputs/pipeline.v1/` — Old category-only pipeline results (44K images)
   - `outputs/thesis/` — New colour-normalized thesis results (5K images)
   - `benchmarks/docs/09-documentation-review.md` — 5-reviewer documentation audit

4. **Academic**:
   - Berlin, B. & Kay, P. (1969). *Basic Color Terms.* UC Press.
   - Zheng, L. et al. (2017). "SIFT Meets CNN." IEEE TPAMI.
   - Radford, A. et al. (2021). "Learning Transferable Visual Models." ICML 2021.
   - Chia et al. (2022). "Contrastive Language-Image Pre-Training for Fashion." SIGIR 2022.
