# ML Benchmarking Glossary for Non-ML Developers

A plain-English explanation of the machine learning and evaluation terms used in the thesis benchmark project. No math background required.

---

## The Big Picture

**What we are doing:** We have 5,000 fashion product images. We want to find out which of 4 AI models is best at "given this image, find similar images." To do this fairly, we run a standardized experiment and measure the results.

---

## 1. The Dataset

### Image
A picture file (`.jpg`). In our case, product photos like "red dress" or "black sneakers."

### Dataset
A collection of images. We have two:
- **Small dataset** (~5,000 images) — used for the benchmark experiments.
- **Large dataset** (~44,000 images) — used later to fill the shop with demo products.

### Metadata
Information *about* each image, stored in `styles.csv`. For example:

```csv
id,masterCategory,subCategory,articleType,baseColour
1163,Apparel,Topwear,T-shirt,Black
1165,Apparel,Topwear,T-shirt,Blue
```

This tells us image `1163` is a black T-shirt. We use this to decide which images are "similar."

### Ground Truth
What counts as "correct" or "similar" for a given image. In our benchmark:
- Two images are **relevant** (similar) if they share the same `masterCategory` + `subCategory`.
- Example: a black T-shirt and a blue T-shirt are both `Apparel` + `Topwear` → they are relevant to each other.
- A T-shirt and a sneaker are *not* relevant.

> **Analogy:** Ground truth is like the answer key on a test. It tells us "for image X, the correct similar images are Y and Z."

---

## 2. The Models

### Model / AI Model
A pre-trained neural network that turns an image into a list of numbers (an **embedding**). We did not train these models — we download them from HuggingFace or PyTorch Hub.

### Embedding (also: Vector, Feature Vector)
A list of numbers that represents an image. Think of it as a "fingerprint" for the image.

Example: a 512-dimensional embedding looks like `[0.12, -0.05, 0.88, ..., 0.03]` with 512 numbers.

> **Analogy:** If images were people, an embedding is their height, weight, age, and 509 other measurements. Similar people have similar measurements.

### Embedding Dimension
How many numbers are in the embedding. Different models produce different lengths:

| Model | Dimension |
|-------|-----------|
| Fashion-CLIP | 512 |
| CLIP-generic | 512 |
| EfficientNet-B0 | 1280 |
| ResNet-50 | 2048 |

### Model Adapter
A thin Python wrapper that loads a model and provides two methods:
- `load()` — download weights and initialize
- `embed(image)` — turn one image into an embedding vector

The benchmark project needs adapters for each model. Your production sidecar already has these models working; the benchmark just needs its own wrappers.

### Pre-trained
The model was already trained on millions of images by someone else (OpenAI, Facebook, etc.). We just download and use it. We do **not** train anything.

### Fashion-Specific vs Generic
- **Fashion-CLIP** was fine-tuned on fashion images. It "knows" about clothes, colors, styles.
- **CLIP-generic** was trained on general internet images (cats, cars, food, etc.). It knows about everything but may be weaker on fashion details.
- **ResNet-50 / EfficientNet** were trained on ImageNet (1,000 general object categories). They are classic "computer vision" baselines.

---

## 3. Retrieval

### Query
The image we start with. "Find images similar to **this** one."

### Gallery
The pool of images we search through. "Search **all these** images and return the most similar ones."

### Retrieval
The act of searching the gallery for images similar to the query.

### Similarity Score
A number from -1 to 1 that says "how alike are these two embeddings?" 1.0 = identical, 0.0 = unrelated, -1.0 = opposite.

We use **cosine similarity** — a math formula that compares two embedding vectors. Because our vectors are L2-normalized, cosine similarity is just the dot product.

### Top-K
The best K results. "Top-10" means the 10 most similar images.

### Exclude Self
When the query image is also in the gallery, we skip it so the model doesn't just return the exact same image as the #1 result.

---

## 4. Splits and Cross-Validation

### Train/Test Split
Dividing the dataset into two groups:
- **Train (Gallery)** — images the model can "recommend"
- **Test (Query)** — images we use as queries to test the model

> **Analogy:** Train = the books in a library. Test = the books a customer brings in and says "find me similar ones."

### K-Fold Cross-Validation
Instead of one fixed split, we do multiple splits and average the results. This makes the experiment more reliable.

**How 3-fold works:**

```
All 5,000 images
├─ Fold 0: Test = images 0-1666,    Train = images 1667-4999
├─ Fold 1: Test = images 1667-3333, Train = images 0-1666 + 3334-4999
└─ Fold 2: Test = images 3334-4999, Train = images 0-3333
```

Each image gets to be a query exactly once. Each image gets to be in the gallery exactly twice.

**Why do this?**
- If we only use one split, we might get lucky or unlucky.
- With 3 folds, we average the results. If a model is truly better, it wins on all 3 folds.
- It's like running the experiment 3 times with different random groups.

### Stratified Split
When splitting, we make sure each fold has the same proportion of categories. If 30% of all images are "Apparel/Topwear," then each fold also has ~30% "Apparel/Topwear."

This prevents a fold from having zero T-shirts by accident, which would break the experiment.

### Minimum Frequency Threshold
If a category has fewer than 10 images, we group it into an "Other" bucket before splitting. Otherwise, a category with 2 images might get split as 1 train + 1 test, which is too small to be meaningful.

---

## 5. Evaluation Metrics

These measure "how good is the model at finding similar images?"

### Relevant Item
An image that shares the same `masterCategory` + `subCategory` as the query. These are the "correct answers."

### Retrieved Item
An image the model returned in its top-K results.

### Precision@K
> Of the top-K images the model returned, what fraction are actually relevant?

Formula: `relevant_retrieved / K`

Example: Top-5 results contain 3 relevant items → Precision@5 = 3/5 = 0.60

> **Analogy:** You ask for 5 movie recommendations. 3 are good. Precision@5 = 60%.

### Recall@K
> Of all the relevant images in the gallery, what fraction did the model find in the top-K?

Formula: `relevant_retrieved / total_relevant_in_gallery`

Example: There are 50 relevant T-shirts in the gallery. The model found 10 in the top-20 → Recall@20 = 10/50 = 0.20

> **Analogy:** There are 50 hidden treasure chests in a field. You found 10. Recall = 20%.

### mAP (mean Average Precision)
A single number that summarizes how well the model ranks relevant items.

- It looks at the full ranked list, not just top-K.
- It rewards models that put relevant items near the top.
- Range: 0.0 (worst) to 1.0 (perfect).

> **Analogy:** mAP is like a student's GPA — one number that summarizes overall performance across all tests.

### nDCG@K (Normalized Discounted Cumulative Gain)
Like Precision@K, but it penalizes relevant items that appear late in the ranking. Item #1 being relevant is worth more than item #20 being relevant.

> **Analogy:** A search engine. The #1 result being correct is great. The #20 result being correct is barely helpful.

---

## 6. Efficiency Metrics

These measure "how fast/expensive is the model?"

### Latency
Time to embed one image, in milliseconds (ms).
- **Mean latency:** average time
- **p50:** 50th percentile (median)
- **p95:** 95th percentile (worst 5% are slower than this)
- **p99:** 99th percentile (worst 1%)

> **Analogy:** How long does it take to scan one item at the checkout?

### Throughput
Images processed per second (images/sec). Batch processing is faster per image than single-image processing.

> **Analogy:** How many items can the checkout scan in one minute?

### Warmup Runs
The first few inferences are slower because the model is still initializing GPU memory, loading caches, etc. We run 10 "warmup" images before timing, so the measurements reflect steady-state speed.

### Model Load Time
Time to download weights and initialize the model, in milliseconds. This only happens once at startup.

### RAM Footprint
How much memory the model uses while running, in megabytes (MB).

### Index Storage
How much disk space the embeddings take up. We report "MB per 1,000 embeddings" so the number is easy to compare across models.

| Model | Dimension | Storage per 1K |
|-------|-----------|----------------|
| Fashion-CLIP | 512 | ~2.0 MB |
| ResNet-50 | 2048 | ~8.0 MB |

---

## 7. Statistical Analysis

### Mean ± SD (Standard Deviation)
- **Mean:** average value across the 3 folds
- **SD:** how much the fold results vary. Small SD = consistent. Large SD = one fold was very different.

Example: `mAP = 0.82 ± 0.01` means the model scored ~0.82 every time, very consistent.

### Cohen's d (Effect Size)
A standardized measure of "how big is the difference between two models?"

| d value | Interpretation |
|---------|---------------|
| 0.2 | Small difference |
| 0.5 | Medium difference |
| 0.8+ | Large difference |

Example: Fashion-CLIP mAP = 0.82, ResNet-50 mAP = 0.70, Cohen's d = 1.2 → **large effect**, Fashion-CLIP is clearly better.

> **Why use this instead of p-values?** Cohen's d tells you *how big* the difference is, not just whether it's "statistically significant."

### Bootstrap 95% CI (Confidence Interval)
A range that says "we're 95% confident the true mean is between X and Y."

Example: `mAP = 0.82, 95% CI [0.80, 0.84]` → the true score is probably between 0.80 and 0.84.

> **How it works:** We resample the fold results 10,000 times with replacement and see where most of the averages fall. It's like shaking the data to see how stable it is.

### Paired t-test (OMITTED in our design)
A statistical test that asks "is the difference between two models real, or just luck?" We **omit** this because with only 3 folds, the test is underpowered — it cannot reliably detect true differences.

> **Analogy:** Flipping a coin 3 times and getting 2 heads doesn't prove the coin is biased. You need more flips.

---

## 8. Benchmark Pipeline Terms

### Cache
Storing embeddings on disk so we don't recompute them every time. Cached by `(model_name, dataset_name)`.

### Batch Inference
Processing multiple images at once instead of one by one. GPUs are much faster at batches.

Example: Embedding 64 images in one batch is faster than 64 separate single-image calls.

### L2 Normalization
Scaling an embedding vector so its length is exactly 1.0. This makes cosine similarity calculations simple and fair.

> **Analogy:** Converting all measurements to percentages so you can compare them fairly.

### Float32
A 32-bit floating point number. Standard for ML embeddings. Each number takes 4 bytes.

### Pareto Frontier
A chart that plots **accuracy** (mAP) vs **speed** (latency). Models in the top-left are best: high accuracy, low latency.

```
Latency (ms) ▲
             │      ● Bad model (slow, inaccurate)
             │   ● Okay model
             │● Best model (fast, accurate)
             └──────────────────► mAP
```

A model is **Pareto-optimal** if no other model is both faster and more accurate.

---

## 9. File/Project Terms

### Adapter
See **Model Adapter** above. A wrapper that lets the benchmark use a model.

### Split File
A JSON file listing which images are in the train set and which are in the test set for a given fold.

Example: `fold_0_test.json` contains 1,667 image IDs that are queries for fold 0.

### Typst
A document formatting language (like LaTeX but simpler). Our benchmark generates `.typ` files with result tables that you can include directly in your thesis.

### styles.csv
The metadata file from Kaggle. Contains product info for all images.

---

## Quick Reference: The 4 Models Compared

| Model | Type | Why Include It? |
|-------|------|-----------------|
| **Fashion-CLIP** | Vision-Language, fashion-tuned | Expected best performer on fashion |
| **CLIP-generic** | Vision-Language, general | Tests if generic CLIP is good enough |
| **EfficientNet-B0** | CNN, efficient | Best speed/accuracy trade-off? |
| **ResNet-50** | CNN, classic baseline | Widely used baseline in literature |

---

## Quick Reference: The Metrics Table

| What we measure | Why it matters |
|-----------------|----------------|
| Precision@K | Are the top results actually similar? |
| Recall@K | Did we find most of the similar items? |
| mAP | Overall ranking quality |
| Latency | How long per image? (user experience) |
| Throughput | How many images per second? (scalability) |
| Load time | How long to start the model? |
| Storage | How much disk per 1K embeddings? (cost) |
| RAM | How much memory? (server cost) |

---

*Generated for the ReSys.Shop thesis benchmark project. Last updated: 2026-07-15*
