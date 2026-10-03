# 03 — Metrics

Every metric the benchmark computes, explained with examples.

## Two Categories

| Category | Metrics | Question Answered |
|----------|---------|-------------------|
| **Retrieval Effectiveness** | Precision@K, Recall@K, mAP, nDCG@K | "How good is the model at finding similar items?" |
| **Operational Performance** | Latency, Throughput, Load Time, RAM, Storage | "How practical is this model for production?" |

---

## Retrieval Effectiveness Metrics

These assume you have a **ground truth** — a list of which items are "similar" for each query.

### Precision@K

**Definition:** Of the top-K results the model returned, what fraction are actually relevant (similar)?

**Formula:** `Precision@K = (number of relevant items in top-K) / K`

**Range:** 0.0 to 1.0

**Example:**

You search for a "black T-shirt." The model returns 5 items:

| Rank | Item | Relevant? |
|------|------|-----------|
| 1 | Black T-shirt | ✅ Yes |
| 2 | White T-shirt | ✅ Yes |
| 3 | Black Dress | ❌ No |
| 4 | Blue T-shirt | ✅ Yes |
| 5 | Red Sneakers | ❌ No |

- Relevant items in top-5: 3
- Precision@5 = 3/5 = **0.60**

**Intuition:** If Precision@10 = 0.70, then 7 out of 10 results are actually similar. Higher is better.

**When it's useful:** When you show a fixed number of results to users (e.g., "top 10 similar items").

---

### Recall@K

**Definition:** Of all the relevant items in the entire gallery, what fraction did the model find in the top-K?

**Formula:** `Recall@K = (number of relevant items in top-K) / (total relevant items in gallery)`

**Range:** 0.0 to 1.0

**Example:**

You search for a "black T-shirt." There are 50 T-shirts total in the gallery. The model's top-20 results contain 10 T-shirts.

- Recall@20 = 10/50 = **0.20**

**Intuition:** If Recall@20 = 0.80, the model found 80% of all similar items within the top 20. Higher is better.

**When it's useful:** When you want to know "did we find most of the good stuff?" Important for completeness.

**The difference between Precision and Recall:**
- **Precision** = "Of what we showed, how much was good?"
- **Recall** = "Of all the good stuff, how much did we find?"

There's always a trade-off. Showing more results (higher K) usually increases Recall but may decrease Precision.

---

### mAP (mean Average Precision)

**Definition:** A single number that summarizes the quality of the **entire ranked list**, not just top-K.

**How it works:**
1. For each query, compute Average Precision (AP):
   - Look at each position in the ranked list where a relevant item appears
   - Compute Precision at that position
   - Average those precision values
2. Average AP across all queries → mAP

**Range:** 0.0 to 1.0

**Example:**

Ranked results for one query (✅ = relevant):

| Rank | Relevant? | Precision at this rank |
|------|-----------|------------------------|
| 1 | ✅ | 1/1 = 1.00 |
| 2 | ❌ | — |
| 3 | ✅ | 2/3 = 0.67 |
| 4 | ❌ | — |
| 5 | ✅ | 3/5 = 0.60 |

AP = average of precision at relevant ranks = (1.00 + 0.67 + 0.60) / 3 = **0.756**

mAP = average AP across all 5,000 queries.

**Intuition:** mAP rewards models that put relevant items **near the top**. A model that finds relevant items at rank #1, #2, #3 scores higher than one that finds them at rank #18, #19, #20.

**Why it's the primary metric:**
- Summarizes the entire precision-recall trade-off
- Standard in CBIR (Content-Based Image Retrieval) literature
- Single number makes comparison easy

**When to use it:** When you care about ranking quality overall, not just a fixed cutoff.

---

### nDCG@K (Normalized Discounted Cumulative Gain)

**Definition:** Like Precision@K, but rewards relevant items more if they appear **early** in the ranking.

**How it works:**
- Relevant item at rank #1 = full credit (1.0)
- Relevant item at rank #2 = slightly less credit (1/log₂(3) ≈ 0.63)
- Relevant item at rank #10 = much less credit (1/log₂(11) ≈ 0.30)
- Then normalize by the best possible score

**Range:** 0.0 to 1.0

**Intuition:** Getting the right answer at #1 is much better than getting it at #20. nDCG captures this intuition mathematically.

**When to use it:** When the **order** of results matters a lot (e.g., e-commerce where top results get 90% of clicks).

**In the thesis:** Reported for completeness, but mAP is the primary metric.

---

## Operational Performance Metrics

### Latency (ms/image)

**Definition:** Time to embed one image, in milliseconds.

**How measured:**
1. Run 10 "warmup" images (to warm up GPU caches)
2. Run 100 timed inferences
3. Report mean and standard deviation

**Why it matters:** Users won't wait. If embedding takes 500ms, the "similar items" feature feels sluggish.

**Typical values:**
- CPU: 50-200ms per image
- GPU: 5-20ms per image

**Thesis reporting:** Mean ± SD across 3 folds.

---

### Throughput (images/sec)

**Definition:** How many images can be embedded per second when processing in batches.

**How measured:**
1. Create batches of 64 images
2. Run 10 batches
3. `throughput = total_images / total_time`

**Why it matters:** Determines server capacity. If throughput is 100 images/sec, one GPU can handle 100 product uploads per second.

**Latency vs Throughput:**
- **Latency** = "How long for one image?" (user experience)
- **Throughput** = "How many images total?" (server capacity)

Batching improves throughput but doesn't reduce single-image latency.

---

### Model Load Time

**Definition:** Time to download weights and initialize the model, in seconds.

**Why it matters:**
- Affects server startup time
- Some models download 1GB+ of weights
- Fashion-CLIP and CLIP-generic need to download from HuggingFace on first run

**Typical values:**
- ResNet-50: 2-5 seconds (weights cached after first run)
- Fashion-CLIP: 5-15 seconds (larger model)

---

### RAM Footprint

**Definition:** Peak memory usage while running inference, in megabytes (MB).

**How measured:** Sample `psutil.Process().memory_info().rss` during a batch inference.

**Why it matters:** Determines server cost. More RAM = more expensive cloud instances.

**Typical values:**
- ResNet-50: ~1,500 MB
- Fashion-CLIP: ~2,000 MB
- EfficientNet-B0: ~800 MB

---

### Index Storage (MB per 1,000 embeddings)

**Definition:** How much disk space 1,000 embedding vectors occupy.

**How computed:**
```
storage_per_1k = (embeddings.nbytes / 1024 / 1024) / (N / 1000)
```

Where `embeddings.nbytes` is the raw numpy array size, and N is the total number of embeddings.

**Why it matters:** At 1 million products, storage differences matter:
- 512-d model: ~2,000 MB
- 2048-d model: ~8,000 MB

**Typical values:**

| Model | Dimension | Storage per 1K |
|-------|-----------|----------------|
| Fashion-CLIP | 512 | ~2.0 MB |
| CLIP-generic | 512 | ~2.0 MB |
| EfficientNet-B0 | 1280 | ~5.0 MB |
| ResNet-50 | 2048 | ~8.0 MB |

---

## How to Read the Results Table

The thesis produces a table like this:

| Model | Precision@5 | Recall@5 | Precision@10 | Recall@10 | mAP |
|-------|-------------|----------|--------------|-----------|-----|
| Fashion-CLIP | 0.72 ± 0.02 | 0.15 ± 0.01 | 0.65 ± 0.02 | 0.28 ± 0.02 | 0.82 ± 0.01 |
| ResNet-50 | 0.55 ± 0.03 | 0.12 ± 0.01 | 0.48 ± 0.03 | 0.22 ± 0.02 | 0.68 ± 0.02 |

**How to interpret:**
- **Mean (first number):** Average performance across 3 folds
- **± SD (second number):** How consistent the model was. Small SD = reliable.
- **Fashion-CLIP vs ResNet-50:** Fashion-CLIP has higher mAP (0.82 vs 0.68) and lower SD (0.01 vs 0.02), meaning it's both better and more consistent.

---

## Metric Selection Guide

| If you care about... | Use this metric |
|---------------------|-----------------|
| "Are the top results good?" | Precision@K |
| "Did we find most similar items?" | Recall@K |
| "Overall ranking quality" | mAP |
| "Order matters a lot" | nDCG@K |
| "User experience / responsiveness" | Latency |
| "Server capacity / cost" | Throughput, RAM, Storage |
| "Startup time" | Load Time |

---

## Why mAP Is Primary

The thesis uses mAP as the primary comparison metric because:

1. **Comprehensive:** Summarizes the entire precision-recall curve, not just one K value
2. **Standard:** Used in virtually all CBIR papers since 2010
3. **Single number:** Easy to compare across models
4. **Thesis-ready:** Directly supports the hypotheses (H1-H4)

Precision@K and Recall@K are reported as secondary metrics to show performance at specific cutoffs that might be used in the actual product (e.g., "show top 10 similar items").
