# Thesis Revision (Evidence-First) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Bring the defended bachelor's thesis to publication quality by resolving every finding from the four-pass review, gated on a final benchmark re-run.

**Architecture:** Three sequential phases. Phase 1 (benchmark code + re-run) produces JSON artifacts that Phase 2 (thesis text) consumes. Phase 3 (polish) assumes stable structure. Each phase gates the next — text cannot be written until numbers are final.

**Tech Stack:** Python 3.12, PyTorch, transformers, torchvision, numpy, psutil (benchmarks); Typst (thesis typesetting); Podman/Docker (pgvector container for pipeline mode).

## Global Constraints

- Python 3.12 (`benchmarks/.python-version`); TDD required for all benchmark code (`benchmarks/AGENTS.md`)
- `uv run ruff check src/` must pass before commit; rules: E, F, I, UP, B, SIM
- `uv run pytest --ignore=src/tests/integration/` green before commit
- Absolute imports only: `from benchmark.X import Y` — never relative
- Seed = 42 everywhere (`MAGIC.SEED`); 3-fold CV (`MAGIC.N_FOLDS_DEFAULT`)
- Thesis builds via `make` in `thesis/` (or the Makefile present); Typst errors = fail
- No code/file references in thesis prose; describe ReSys.Shop as a completed system
- Commit messages: conventional commits (`feat:`, `fix:`, `docs:`, `refactor:`, `chore:`)

---

## File Structure

**New files (created by this plan):**

| File | Responsibility |
|---|---|
| `benchmarks/src/benchmark/models/color_histogram.py` | HSV color histogram adapter (512-dim L2-normalized) |
| `benchmarks/src/benchmark/models/random_baseline.py` | Deterministic random unit vector baseline |
| `benchmarks/src/tests/models/test_color_histogram.py` | TDD tests for color histogram adapter |
| `benchmarks/src/tests/models/test_random_baseline.py` | TDD tests for random baseline adapter |
| `benchmarks/src/benchmark/evaluation/comparison.py` | Pairwise Cohen's d matrix post-run |
| `benchmarks/src/tests/evaluation/test_comparison.py` | TDD tests for Cohen's d matrix |
| `docs/superpowers/plans/2026-07-30-thesis-revision.md` | This plan (meta) |

**Modified files:**

| File | Change |
|---|---|
| `benchmarks/src/benchmark/models/__init__.py` | Register `color-histogram`, `random-baseline` in `_register()` + `get_registry()` |
| `benchmarks/src/benchmark/_constants.py` | Expand `THESIS_MODEL_KEYS` to 13 models |
| `benchmarks/src/benchmark/evaluation/thesis.py` | Add nDCG to `metric_keys`; call pairwise Cohen's d post-run |
| `benchmarks/src/benchmark/datasets/ground_truth.py` | (verify only — multi-label already works via `label_field` param) |
| `thesis/chapters/part1/ch1-introduction.typ` | DSR, remove pre-announced results, cold-start disclaimer |
| `thesis/chapters/part2/ch1-background/*.typ` | Reconcile registry, DeepFashion2, fixes |
| `thesis/chapters/part2/ch2-design/*.typ` | Vector dim, pgvector, use cases, screenshots, etc. |
| `thesis/chapters/part2/ch3-evaluation/*.typ` | Full rewrite from Phase 1 data |
| `thesis/chapters/part3/ch4-conclusion.typ` | Metric count, refreshed numbers |
| `thesis/backmatter/references.typ` | .bib completeness, repo URL |

---

## Phase 1 — Evidence (benchmark code + data)

Phase 1 gates Phase 2: no thesis text can be written until the numbers are final.

---

### Task 1: ColorHistogramModel Adapter

**Files:**
- Create: `benchmarks/src/benchmark/models/color_histogram.py`
- Create: `benchmarks/src/tests/models/test_color_histogram.py`

**Interfaces:**
- Consumes: `benchmark.models.base.EmbeddingModel`, `benchmark._constants.CLI_STR`
- Produces: `ColorHistogramModel` class with `name="ColorHistogram"`, `embedding_dim=512`, deterministic `embed(Image.Image) -> np.ndarray` L2-normalized float32

- [ ] **Step 1: Write the failing test**

```python
# benchmarks/src/tests/models/test_color_histogram.py
"""Tests for the ColorHistogramModel adapter."""
from __future__ import annotations

import numpy as np
import pytest
from PIL import Image

from benchmark.models.color_histogram import ColorHistogramModel


@pytest.fixture
def model() -> ColorHistogramModel:
    m = ColorHistogramModel()
    m.load()
    return m


@pytest.fixture
def red_image() -> Image.Image:
    return Image.new("RGB", (224, 224), color=(200, 40, 40))


@pytest.fixture
def blue_image() -> Image.Image:
    return Image.new("RGB", (224, 224), color=(40, 40, 200))


def test_name(model: ColorHistogramModel) -> None:
    assert model.name == "ColorHistogram"


def test_embedding_dim(model: ColorHistogramModel) -> None:
    assert model.embedding_dim == 512


def test_embed_returns_correct_shape(model: ColorHistogramModel, red_image: Image.Image) -> None:
    vec = model.embed(red_image)
    assert vec.shape == (512,)


def test_embed_returns_float32(model: ColorHistogramModel, red_image: Image.Image) -> None:
    vec = model.embed(red_image)
    assert vec.dtype == np.float32


def test_embed_is_l2_normalized(model: ColorHistogramModel, red_image: Image.Image) -> None:
    vec = model.embed(red_image)
    norm = float(np.linalg.norm(vec))
    assert abs(norm - 1.0) < 1e-5


def test_embed_deterministic(model: ColorHistogramModel, red_image: Image.Image) -> None:
    v1 = model.embed(red_image)
    v2 = model.embed(red_image)
    np.testing.assert_array_equal(v1, v2)


def test_similar_colors_higher_similarity(
    model: ColorHistogramModel, red_image: Image.Image, blue_image: Image.Image,
) -> None:
    """Two shades of red should be more similar to each other than red vs blue."""
    dark_red = Image.new("RGB", (224, 224), color=(180, 30, 30))
    v_red = model.embed(red_image)
    v_dark_red = model.embed(dark_red)
    v_blue = model.embed(blue_image)
    sim_same = float(np.dot(v_red, v_dark_red))
    sim_cross = float(np.dot(v_red, v_blue))
    assert sim_same > sim_cross
```

- [ ] **Step 2: Run test to verify it fails**

```bash
cd benchmarks && uv run pytest src/tests/models/test_color_histogram.py -v
```
Expected: `ModuleNotFoundError: No module named 'benchmark.models.color_histogram'`

- [ ] **Step 3: Write the minimal implementation**

```python
# benchmarks/src/benchmark/models/color_histogram.py
"""Color histogram baseline adapter (no deep learning).

Computes an HSV color histogram flattened to a fixed-dim vector and
L2-normalized. Used as a hand-crafted vision baseline to anchor the
value that deep-learning embeddings add over trivial heuristics.
"""
from __future__ import annotations

import numpy as np
from PIL import Image

from benchmark.models.base import EmbeddingModel
from benchmark.utils.logging import get_logger

logger = get_logger("models.color_histogram")

# Invariant: HSV bins = 8 (H) × 8 (S) × 8 (V) = 512 bins
_H_BINS = 8
_S_BINS = 8
_V_BINS = 8
_EMBEDDING_DIM = _H_BINS * _S_BINS * _V_BINS  # 512


class ColorHistogramModel(EmbeddingModel):
    """HSV color histogram baseline (512-D L2-normalized embeddings)."""

    @property
    def name(self) -> str:
        return "ColorHistogram"

    @property
    def embedding_dim(self) -> int:
        return _EMBEDDING_DIM

    def load(self) -> None:
        # No-op: histogram requires no weights
        logger.info("%s ready (no weights)", self.name)

    def embed(self, image: Image.Image) -> np.ndarray:
        # Transform: Convert to HSV, build 3-D histogram, flatten, L2-normalise
        hsv = np.asarray(image.convert("HSV"), dtype=np.float32)
        h, s, v = hsv[..., 0], hsv[..., 1], hsv[..., 2]
        # Bin: H in [0,180) -> 8 bins, S in [0,255] -> 8 bins, V in [0,255] -> 8 bins
        h_idx = np.clip((h / 180.0 * _H_BINS).astype(np.int32), 0, _H_BINS - 1)
        s_idx = np.clip((s / 256.0 * _S_BINS).astype(np.int32), 0, _S_BINS - 1)
        v_idx = np.clip((v / 256.0 * _V_BINS).astype(np.int32), 0, _V_BINS - 1)
        flat_idx = (h_idx * _S_BINS * _V_BINS) + (s_idx * _V_BINS) + v_idx
        hist = np.bincount(flat_idx.ravel(), minlength=_EMBEDDING_DIM).astype(np.float32)
        # Normalise: L2 to unit length for cosine similarity
        norm = float(np.linalg.norm(hist))
        if norm > 0:
            hist = hist / norm
        return hist
```

- [ ] **Step 4: Run test to verify it passes**

```bash
cd benchmarks && uv run pytest src/tests/models/test_color_histogram.py -v
```
Expected: 7 passed

- [ ] **Step 5: Lint + commit**

```bash
cd benchmarks && uv run ruff check src/benchmark/models/color_histogram.py src/tests/models/test_color_histogram.py
cd /home/ngtphat/Projects/ReSys.Shop && git add benchmarks/src/benchmark/models/color_histogram.py benchmarks/src/tests/models/test_color_histogram.py
git commit -m "feat(benchmarks): add ColorHistogram baseline adapter

512-dim HSV histogram, L2-normalized. Serves as a hand-crafted vision
baseline to anchor the value that deep-learning embeddings add.
Resolves reviewer critique: missing trivial baseline."
```

---

### Task 2: RandomBaselineModel Adapter

**Files:**
- Create: `benchmarks/src/benchmark/models/random_baseline.py`
- Create: `benchmarks/src/tests/models/test_random_baseline.py`

**Interfaces:**
- Consumes: `benchmark.models.base.EmbeddingModel`
- Produces: `RandomBaselineModel` class with `name="RandomBaseline"`, `embedding_dim=512`, deterministic `embed(Image.Image) -> np.ndarray` seeded by image content

- [ ] **Step 1: Write the failing test**

```python
# benchmarks/src/tests/models/test_random_baseline.py
"""Tests for the RandomBaselineModel adapter."""
from __future__ import annotations

import numpy as np
import pytest
from PIL import Image

from benchmark.models.random_baseline import RandomBaselineModel


@pytest.fixture
def model() -> RandomBaselineModel:
    m = RandomBaselineModel()
    m.load()
    return m


@pytest.fixture
def sample_image() -> Image.Image:
    return Image.new("RGB", (224, 224), color=(128, 64, 32))


def test_name(model: RandomBaselineModel) -> None:
    assert model.name == "RandomBaseline"


def test_embedding_dim(model: RandomBaselineModel) -> None:
    assert model.embedding_dim == 512


def test_embed_returns_correct_shape(model: RandomBaselineModel, sample_image: Image.Image) -> None:
    vec = model.embed(sample_image)
    assert vec.shape == (512,)


def test_embed_returns_float32(model: RandomBaselineModel, sample_image: Image.Image) -> None:
    vec = model.embed(sample_image)
    assert vec.dtype == np.float32


def test_embed_is_l2_normalized(model: RandomBaselineModel, sample_image: Image.Image) -> None:
    vec = model.embed(sample_image)
    norm = float(np.linalg.norm(vec))
    assert abs(norm - 1.0) < 1e-5


def test_embed_deterministic_for_same_image(model: RandomBaselineModel, sample_image: Image.Image) -> None:
    v1 = model.embed(sample_image)
    v2 = model.embed(sample_image)
    np.testing.assert_array_equal(v1, v2)


def test_different_images_produce_different_embeddings(model: RandomBaselineModel) -> None:
    img_a = Image.new("RGB", (224, 224), color=(200, 40, 40))
    img_b = Image.new("RGB", (224, 224), color=(40, 40, 200))
    v_a = model.embed(img_a)
    v_b = model.embed(img_b)
    assert not np.array_equal(v_a, v_b)
```

- [ ] **Step 2: Run test to verify it fails**

```bash
cd benchmarks && uv run pytest src/tests/models/test_random_baseline.py -v
```
Expected: `ModuleNotFoundError`

- [ ] **Step 3: Write the minimal implementation**

```python
# benchmarks/src/benchmark/models/random_baseline.py
"""Random ranking baseline adapter (no deep learning).

Produces a seeded random unit vector per image. The seed is derived
from the image bytes so results are deterministic across runs — a
random ranking is the floor any useful retrieval method must beat.
"""
from __future__ import annotations

import hashlib

import numpy as np
from PIL import Image

from benchmark.models.base import EmbeddingModel
from benchmark.utils.logging import get_logger

logger = get_logger("models.random_baseline")

_EMBEDDING_DIM = 512


class RandomBaselineModel(EmbeddingModel):
    """Random unit vector baseline (512-D L2-normalized embeddings)."""

    @property
    def name(self) -> str:
        return "RandomBaseline"

    @property
    def embedding_dim(self) -> int:
        return _EMBEDDING_DIM

    def load(self) -> None:
        # No-op: no weights
        logger.info("%s ready (no weights)", self.name)

    def embed(self, image: Image.Image) -> np.ndarray:
        # Seed: SHA-256 of image bytes -> 32-bit seed for numpy RNG
        raw = image.tobytes()
        digest = hashlib.sha256(raw).digest()
        seed = int.from_bytes(digest[:4], "little")
        rng = np.random.default_rng(seed)
        vec = rng.standard_normal(_EMBEDDING_DIM).astype(np.float32)
        # Normalise: L2 to unit length
        norm = float(np.linalg.norm(vec))
        if norm > 0:
            vec = vec / norm
        return vec
```

- [ ] **Step 4: Run test to verify it passes**

```bash
cd benchmarks && uv run pytest src/tests/models/test_random_baseline.py -v
```
Expected: 7 passed

- [ ] **Step 5: Lint + commit**

```bash
cd benchmarks && uv run ruff check src/benchmark/models/random_baseline.py src/tests/models/test_random_baseline.py
cd /home/ngtphat/Projects/ReSys.Shop && git add benchmarks/src/benchmark/models/random_baseline.py benchmarks/src/tests/models/test_random_baseline.py
git commit -m "feat(benchmarks): add RandomBaseline adapter

Seeded random unit vector (SHA-256 of image bytes). Floor any
useful retrieval method must beat. Resolves: missing random
baseline critique."
```

---

### Task 3: Register Baselines in Model Registry

**Files:**
- Modify: `benchmarks/src/benchmark/models/__init__.py`
- Modify: `benchmarks/src/tests/models/test_registry.py` (add entries)

**Interfaces:**
- Consumes: `ColorHistogramModel`, `RandomBaselineModel` from Tasks 1-2
- Produces: Both models accessible via `REGISTRY["color-histogram"]` and `REGISTRY["random-baseline"]`

- [ ] **Step 1: Add registry entries**

Edit `benchmarks/src/benchmark/models/__init__.py`:

```python
# Add to _register() imports:
from benchmark.models.color_histogram import ColorHistogramModel
from benchmark.models.random_baseline import RandomBaselineModel

# Add to _register() return dict (alphabetical):
return {
    ...existing entries...
    "color-histogram": ColorHistogramModel(),
    "random-baseline": RandomBaselineModel(),
}

# Add to get_registry() imports and return dict with same pattern.
```

- [ ] **Step 2: Verify registry build**

```bash
cd benchmarks && uv run python -c "from benchmark.models import REGISTRY; print(sorted(REGISTRY.keys())); assert 'color-histogram' in REGISTRY; assert 'random-baseline' in REGISTRY"
```
Expected: Both keys present, total 13 models.

- [ ] **Step 3: Update registry tests**

Add to `benchmarks/src/tests/models/test_registry.py`:

```python
def test_registry_contains_baselines() -> None:
    from benchmark.models import REGISTRY
    assert "color-histogram" in REGISTRY
    assert "random-baseline" in REGISTRY
```

- [ ] **Step 4: Run registry tests + commit**

```bash
cd benchmarks && uv run pytest src/tests/models/test_registry.py -v
cd /home/ngtphat/Projects/ReSys.Shop && git add benchmarks/src/benchmark/models/__init__.py benchmarks/src/tests/models/test_registry.py
git commit -m "feat(benchmarks): register color-histogram and random-baseline adapters"
```

---

### Task 4: Expand THESIS_MODEL_KEYS to Full 13

**Files:**
- Modify: `benchmarks/src/benchmark/_constants.py:131-133`

**Interfaces:**
- Consumes: Registry with 13 models (Tasks 1-3)
- Produces: `THESIS_MODEL_KEYS` = 13-element list

- [ ] **Step 1: Update the constant**

Edit `benchmarks/src/benchmark/_constants.py` lines 131-133:

```python
THESIS_MODEL_KEYS: list[str] = [
    "fashion-clip", "clip-vit-b16", "clip-b32", "clip-l14", "clip-generic",
    "siglip", "eva-clip", "dinov2-vits14",
    "resnet-50", "efficientnet-b0", "convnext-tiny",
    "color-histogram", "random-baseline",
]
```

Note: `clip-vit-b16` is the **architecture-matched control** for Fashion-CLIP (both ViT-B/16). Order: CLIP family first (domain-fine-tuning comparison), then other DL models, then baselines last.

- [ ] **Step 2: Update the thesis registry test**

Edit `benchmarks/src/tests/models/test_registry_thesis.py` to assert 13 keys:

```python
def test_thesis_model_keys_count() -> None:
    from benchmark._constants import THESIS_MODEL_KEYS
    assert len(THESIS_MODEL_KEYS) == 13


def test_thesis_model_keys_all_registered() -> None:
    from benchmark._constants import THESIS_MODEL_KEYS
    from benchmark.models import REGISTRY
    for key in THESIS_MODEL_KEYS:
        assert key in REGISTRY, f"THESIS_MODEL_KEYS references unknown model: {key}"
```

- [ ] **Step 3: Run tests + commit**

```bash
cd benchmarks && uv run pytest src/tests/models/test_registry_thesis.py -v
cd /home/ngtphat/Projects/ReSys.Shop && git add benchmarks/src/benchmark/_constants.py benchmarks/src/tests/models/test_registry_thesis.py
git commit -m "feat(benchmarks): expand THESIS_MODEL_KEYS to 13 models

Adds all registered deep-learning models plus color-histogram and
random-baseline. clip-vit-b16 is the architecture-matched control
for Fashion-CLIP. Resolves 'only 4 of 11 evaluated' critique."
```

---

### Task 5: Add nDCG to Thesis Runner Aggregation

**Files:**
- Modify: `benchmarks/src/benchmark/evaluation/thesis.py:139-142, 279-282` (both `_evaluate_model` and `_evaluate_model_with_field`)

**Interfaces:**
- Consumes: `Evaluator.ndcg` (already populated — the metric exists in `metrics/ndcg.py`)
- Produces: `aggregate["ndcg@5"]`, `aggregate["ndcg@10"]`, `aggregate["ndcg@20"]` in thesis results JSON

- [ ] **Step 1: Add nDCG to metric_keys list**

Edit `benchmarks/src/benchmark/evaluation/thesis.py` in `_evaluate_model` (~line 139):

```python
metric_keys = [
    "map",
    "precision@5", "precision@10", "precision@20",
    "recall@5", "recall@10", "recall@20",
    "ndcg@5", "ndcg@10", "ndcg@20",  # <-- ADD
    "latency_mean_ms", "throughput_per_sec",
    "load_time_ms", "index_storage_mb", "ram_mb",
]
```

- [ ] **Step 2: Populate nDCG values in fold results**

Edit `_evaluate_fold` (~line 218). The `metrics` object already has `ndcg: dict[int, float]`:

```python
return {
    ...existing keys...
    "ndcg@5": round(metrics.ndcg.get(5, 0.0), 4),
    "ndcg@10": round(metrics.ndcg.get(10, 0.0), 4),
    "ndcg@20": round(metrics.ndcg.get(20, 0.0), 4),
    ...existing keys...
}
```

- [ ] **Step 3: Repeat for `_evaluate_fold_with_field`** (~line 338) — same 3 nDCG keys.

- [ ] **Step 4: Update thesis tests**

Edit `benchmarks/src/tests/evaluation/test_thesis.py` to assert nDCG keys present in fold results and aggregate.

- [ ] **Step 5: Run thesis tests + commit**

```bash
cd benchmarks && uv run pytest src/tests/evaluation/test_thesis.py -v
cd /home/ngtphat/Projects/ReSys.Shop && git add benchmarks/src/benchmark/evaluation/thesis.py benchmarks/src/tests/evaluation/test_thesis.py
git commit -m "feat(benchmarks): include nDCG@K in thesis runner aggregation

Metric existed in metrics/ndcg.py but was not surfaced in thesis
results. Now reported at K=5,10,20 for both category-only and
multi-label runs. Resolves: nDCG mentioned-but-unreported critique."
```

---

### Task 6: Pairwise Cohen's d Post-Run Comparison

**Files:**
- Create: `benchmarks/src/benchmark/evaluation/comparison.py`
- Create: `benchmarks/src/tests/evaluation/test_comparison.py`
- Modify: `benchmarks/src/benchmark/evaluation/thesis.py` (wire into `run()`)

**Interfaces:**
- Consumes: `stats.cohens_d(group_a, group_b) -> float`, thesis results list
- Produces: `pairwise_cohens_d(results, metric="map") -> dict[str, dict[str, float]]`

- [ ] **Step 1: Write the failing test**

```python
# benchmarks/src/tests/evaluation/test_comparison.py
"""Tests for pairwise Cohen's d comparison."""
from __future__ import annotations

from benchmark.evaluation.comparison import pairwise_cohens_d


def test_pairwise_shape() -> None:
    results = [
        {"model_name": "A", "folds": [{"map": 0.80}, {"map": 0.81}, {"map": 0.79}]},
        {"model_name": "B", "folds": [{"map": 0.85}, {"map": 0.86}, {"map": 0.84}]},
    ]
    matrix = pairwise_cohens_d(results, metric="map")
    assert set(matrix.keys()) == {"A", "B"}
    assert set(matrix["A"].keys()) == {"B"}
    assert set(matrix["B"].keys()) == {"A"}


def test_pairwise_symmetry() -> None:
    results = [
        {"model_name": "A", "folds": [{"map": 0.80}, {"map": 0.81}]},
        {"model_name": "B", "folds": [{"map": 0.85}, {"map": 0.86}]},
    ]
    matrix = pairwise_cohens_d(results, metric="map")
    assert matrix["A"]["B"] == matrix["B"]["A"]


def test_pairwise_zero_for_identical() -> None:
    results = [
        {"model_name": "A", "folds": [{"map": 0.80}, {"map": 0.81}]},
        {"model_name": "B", "folds": [{"map": 0.80}, {"map": 0.81}]},
    ]
    matrix = pairwise_cohens_d(results, metric="map")
    assert matrix["A"]["B"] == 0.0
```

- [ ] **Step 2: Run test to verify it fails**

```bash
cd benchmarks && uv run pytest src/tests/evaluation/test_comparison.py -v
```
Expected: `ModuleNotFoundError`

- [ ] **Step 3: Write the implementation**

```python
# benchmarks/src/benchmark/evaluation/comparison.py
"""Pairwise effect-size comparison across thesis results.

Computes Cohen's d for each model pair on a chosen metric (default:
mAP) using paired fold-level observations.
"""
from __future__ import annotations

from typing import Any

from benchmark.evaluation.stats import cohens_d


def pairwise_cohens_d(
    results: list[dict[str, Any]],
    metric: str = "map",
) -> dict[str, dict[str, float]]:
    """Compute pairwise Cohen's d for a metric across all model pairs.

    Args:
        results: ThesisRunner result dicts (each has model_name + folds).
        metric: Fold-level metric key (e.g., "map", "precision@5").

    Returns:
        Nested dict matrix[a][b] = Cohen's d. Only upper-triangle
        populated (symmetry implied).
    """
    matrix: dict[str, dict[str, float]] = {}
    names = [r["model_name"] for r in results]
    fold_values = {
        r["model_name"]: [f[metric] for f in r["folds"] if metric in f]
        for r in results
    }
    for i, a in enumerate(names):
        matrix[a] = {}
        for b in names[i + 1:]:
            vals_a = fold_values[a]
            vals_b = fold_values[b]
            if len(vals_a) == len(vals_b) and len(vals_a) >= 2:
                matrix[a][b] = round(cohens_d(vals_a, vals_b), 4)
            else:
                matrix[a][b] = 0.0
    return matrix
```

- [ ] **Step 4: Run tests, verify pass**

```bash
cd benchmarks && uv run pytest src/tests/evaluation/test_comparison.py -v
```

- [ ] **Step 5: Wire into ThesisRunner.run()**

Edit `benchmarks/src/benchmark/evaluation/thesis.py` `run()` method. After building `results`, compute Cohen's d and persist:

```python
from benchmark.evaluation.comparison import pairwise_cohens_d

def run(self, model_keys=None) -> list[dict[str, Any]]:
    ...existing code that builds results...

    # Compare: Pairwise Cohen's d on mAP across all model pairs
    cohens_matrix = pairwise_cohens_d(results, metric="map")
    results_dir = self.output_dir / "results"
    results_dir.mkdir(parents=True, exist_ok=True)
    cohens_path = results_dir / "thesis_cohens_d.json"
    cohens_path.write_text(json.dumps(cohens_matrix, indent=2))
    logger.info("Cohen's d matrix -> %s", cohens_path)

    ...existing secondary-label block...
    return results
```

- [ ] **Step 6: Run full thesis test suite + commit**

```bash
cd benchmarks && uv run pytest src/tests/evaluation/ -v
cd /home/ngtphat/Projects/ReSys.Shop && git add benchmarks/src/benchmark/evaluation/comparison.py benchmarks/src/tests/evaluation/test_comparison.py benchmarks/src/benchmark/evaluation/thesis.py
git commit -m "feat(benchmarks): compute pairwise Cohen's d post-thesis run

Persists thesis_cohens_d.json with effect sizes for each model
pair on mAP. Resolves: 'no effect size' critique."
```

---

### Task 7: Run Thesis Benchmark (13 models × 3 folds × 2 label schemes)

**Files:** none (execution only)

**Interfaces:**
- Consumes: Phase 1 Tasks 1-6
- Produces: `outputs/thesis/results/thesis_results.json` (category-only), `outputs/thesis/results/thesis_results_pattern.json` (multi-label, PRIMARY)

- [ ] **Step 1: Verify pipeline health**

```bash
cd benchmarks && uv run ruff check src/ && uv run pytest --ignore=src/tests/integration/
```
Expected: all green.

- [ ] **Step 2: Run category-only thesis benchmark**

```bash
cd benchmarks && uv run benchmark thesis --dataset-root <DATASET_ROOT>
```

Replace `<DATASET_ROOT>` with the actual path to the Fashion Product Images dataset (per `docs/08-replication-guide.md`). This uses `THESIS_MODEL_KEYS` (13 models), 3 folds, seed=42, category-only labels. Produces `outputs/thesis/results/thesis_results.json` + `thesis_cohens_d.json`.

- [ ] **Step 3: Run multi-label thesis benchmark (PRIMARY)**

```bash
cd benchmarks && uv run benchmark thesis --dataset-root <DATASET_ROOT> --secondary-label label_pattern
```

This produces `outputs/thesis/results/thesis_results_pattern.json` with the finer (subCategory + baseColour + pattern) relevance criterion. Per the spec, this is the **primary** evaluation.

- [ ] **Step 4: Inspect outputs**

```bash
cd benchmarks && cat outputs/thesis/results/thesis_cohens_d.json | head -50
cd benchmarks && cat outputs/thesis/results/thesis_results_pattern.json | head -100
```

Verify: 13 models present, nDCG keys populated, ci_95 present, Cohen's d matrix has entries.

- [ ] **Step 5: Diagnose EfficientNet-B0 std dev**

```bash
cd benchmarks && uv run python -c "
import json
data = json.load(open('outputs/thesis/results/thesis_results.json'))
for r in data:
    if r['model_name'] == 'EfficientNet-B0':
        fold_maps = [f['map'] for f in r['folds']]
        print('Per-fold mAPs:', fold_maps)
        print('Std:', r['aggregate']['map']['std'])
"
```

Expected: if std ≈ 0.0007 is genuine (fold mAPs very close), document the cause in thesis text (trivially easy 5-category task). If the per-fold values show larger spread than std suggests, investigate `aggregate_mean_std` and fix.

- [ ] **Step 6: Commit outputs to git (LFS optional)**

```bash
cd /home/ngtphat/Projects/ReSys.Shop && git add benchmarks/outputs/thesis/results/
git commit -m "data(benchmarks): thesis results for 13 models × 2 label schemes

Category-only and multi-label (primary). Includes pairwise
Cohen's d matrix. Gates Phase 2 thesis text rewrite."
```

---

### Task 8: Run Pipeline Mode for RQ3 End-to-End Latency

**Files:** none (execution only)

**Interfaces:**
- Consumes: running pgvector container (see `benchmarks/infra/init.sql`, `benchmarks/docs/08-replication-guide.md` §5)
- Produces: `outputs/pipeline/pipeline_results.json` with end-to-end latency column

- [ ] **Step 1: Start pgvector container**

```bash
cd benchmarks && podman run --rm -d --name pgvector-bench \
  -e POSTGRES_PASSWORD=benchmark -e POSTGRES_USER=benchmark \
  -e POSTGRES_DB=benchmark -p 5432:5432 \
  pgvector/pgvector:pg16
```

- [ ] **Step 2: Initialize schema + wait for ready**

```bash
cd benchmarks && ./infra/wait-for-pg.sh
podman exec -i pgvector-bench psql -U benchmark -d benchmark < infra/init.sql
```

- [ ] **Step 3: Run pipeline mode**

```bash
cd benchmarks && uv run benchmark pipeline --dataset-root <DATASET_ROOT>
```

Produces `outputs/pipeline/pipeline_results.json` with end-to-end latency (inference + HTTP + pgvector query + assembly).

- [ ] **Step 4: Inspect RQ3 latency**

```bash
cd benchmarks && cat outputs/pipeline/pipeline_results.json | python -m json.tool | head -80
```

Verify: end-to-end latency column populated for each model. Note the number for Fashion-CLIP — this is the RQ3 evidence.

- [ ] **Step 5: Tear down container + commit outputs**

```bash
podman stop pgvector-bench
cd /home/ngtphat/Projects/ReSys.Shop && git add benchmarks/outputs/pipeline/
git commit -m "data(benchmarks): pipeline results with end-to-end latency

RQ3 evidence: inference + HTTP + pgvector query + assembly time
for each model. Gates Ch.3 rewrite."
```

---

### Task 9: Generate Typst Tables + Charts

**Files:** none (execution only)

**Interfaces:**
- Consumes: Phase 1 JSON outputs
- Produces: `outputs/thesis/tables/*.typ`, `outputs/thesis/charts/*.png`

- [ ] **Step 1: Generate reports**

```bash
cd benchmarks && uv run benchmark report --format typst
```

- [ ] **Step 2: Verify outputs**

```bash
cd benchmarks && ls outputs/thesis/tables/ outputs/thesis/charts/
```

Expected: `thesis_aggregate.typ`, `thesis_efficiency.typ`, updated charts.

- [ ] **Step 3: Commit**

```bash
cd /home/ngtphat/Projects/ReSys.Shop && git add benchmarks/outputs/thesis/tables/ benchmarks/outputs/thesis/charts/
git commit -m "data(benchmarks): regenerate thesis Typst tables and charts"
```

---

### Phase 1 Gate

**Before starting Phase 2, verify:**

- [ ] `outputs/thesis/results/thesis_results_pattern.json` has 13 models × nDCG + ci_95 + Cohen's d
- [ ] `outputs/pipeline/pipeline_results.json` has end-to-end latency for Fashion-CLIP
- [ ] EfficientNet-B0 std dev diagnosis concluded (documented or fixed)
- [ ] All Phase 1 commits are on the branch

---

## Phase 2 — Structure (Thesis Text)

Phase 2 consumes Phase 1 JSON artifacts. Each chapter rewrite is a separate task.

---

### Task 10: Rewrite Chapter 1 (Introduction)

**Files:**
- Modify: `thesis/chapters/part1/ch1-introduction.typ`

**Interfaces:**
- Consumes: Phase 1 results (for RQ hypotheses framing)
- Produces: Ch.1 with expanded DSR, no pre-announced results, cold-start disclaimer, ONNX comparison

- [ ] **Step 1: Expand DSR methodology to ~1 page**

In the "Research Methodology" section, add:
- Hevner (2004) seven guidelines — map each to a project activity (e.g., "Guideline 1: Design as an Artifact → the polyglot platform with pgvector-integrated CBIR")
- Peffers (2007) six steps — trace the project through each (problem identification → objectives → design → demonstration → evaluation → communication)
- At least one concrete design iteration (e.g., initial pgvector config → benchmark feedback → HNSW parameter tuning)

- [ ] **Step 2: Remove all pre-announced results**

Delete: any sentence that states "Fashion-CLIP achieved X mAP", "15-20% improvement", or cites specific benchmark numbers before Chapter 3. Replace with: candidate-model list, evaluation protocol description, hypothesis framing ("we hypothesize domain-specific fine-tuning improves fashion retrieval over general-purpose models").

- [ ] **Step 3: Add cold-start scope disclaimer**

In the "Cold-start invisibility" paragraph of the Problem Statement, add a 2-sentence scope disclaimer:

> Visual similarity retrieval addresses the *discovery* aspect of cold start (new items can be found by visual query). Personalized *recommendation* — predicting which specific items a user will prefer — requires additional layers (collaborative filtering, click-through feedback) that are out of scope for this thesis.

- [ ] **Step 4: Downgrade "engineering gap" + add ONNX comparison**

In the "Contribution Differentiators" section, replace "engineering gap" framing with "architectural trade-off analysis". Add a paragraph comparing the chosen polyglot sidecar against two alternatives:
- **ONNX Runtime in-process** (no network boundary, but constrains model choice; doesn't support all architectures natively)
- **ML.NET** (no Python ecosystem access; limited vision model zoo)
State the rationale for choosing the sidecar: access to the full PyTorch/HuggingFace ecosystem for pluggable models.

- [ ] **Step 5: State CLIP-generic identity explicitly**

In the "Candidate Models" or "Model Selection" subsection, add:

> `CLIP-generic` refers to OpenAI's `clip-vit-base-patch32` (ViT-B/32 backbone, 512-dim embeddings, pre-trained on 400M WIT image-text pairs). This baseline differs architecturally from Fashion-CLIP (ViT-B/16); Section 3.X isolates the architecture-vs-domain effect by also evaluating CLIP ViT-B/16.

- [ ] **Step 6: Fix preprocessing claim**

Replace any sentence claiming "ImageNet normalization applied uniformly" with:

> Each model uses the preprocessing pipeline from its own model card: CLIP-family models use their respective `CLIPProcessor`, CNN models use ImageNet statistics, and DINOv2 uses its self-supervised preprocessing. This per-model normalization ensures embeddings are valid for each architecture.

- [ ] **Step 7: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/part1/
git commit -m "docs(thesis): rewrite ch1 — DSR expansion, remove pre-announced results, cold-start disclaimer, ONNX comparison"
```

---

### Task 11: Rewrite Chapter 2 Background

**Files:**
- Modify: `thesis/chapters/part2/ch1-background/*.typ`

**Interfaces:**
- Consumes: Phase 1 model registry (13 models)
- Produces: Background with reconciled registry, DeepFashion2, neutralized Fashion-CLIP narrative

- [ ] **Step 1: Reconcile model registry**

Update the model comparison table to match the actual 13-model registry. Columns: Model, Architecture, Dim, Parameters, Training, Notes. Drop phantom ResNet-101/152 references (or add adapters for them). State explicitly which models are benchmark-only vs production-registered.

- [ ] **Step 2: Add DeepFashion2 citation**

In the "Academic Research" subsection of Related Work, add:

> DeepFashion2 (Ge et al., CVPR 2019) extended the original DeepFashion benchmark with dense landmark annotations and in-shop clothes retrieval, establishing the standard sequel benchmark for fashion recognition tasks.

Add `@ge2019deepfashion2` to the bibliography (Task 22).

- [ ] **Step 3: Fix DINOv2 and CNN imprecisions**

Replace:
- "DINOv2 ignores colour" → "DINOv2's self-supervised objective deprioritizes low-level colour features in favour of structural and shape information"
- "CNN processes patches" → "CNN applies convolutional kernels across spatial dimensions of the input tensor"

- [ ] **Step 4: Neutralize Fashion-CLIP narrative**

In the CLIP and Fashion-CLIP sections, present all models with documented trade-offs without an arc toward inevitable selection. Remove phrases like "the natural choice", "clearly superior". Replace with neutral framing: "each model makes different trade-offs; Section 3 evaluates them empirically."

- [ ] **Step 5: Move model selection decision to Ch.3**

Delete or drastically reduce the "Model Selection and Justification" section in Background. Move the selection decision narrative to Ch.3 (Task 15), where the evidence lives. Background retains: candidate-model descriptions, selection criteria (what will be measured), hypothesis framing.

- [ ] **Step 6: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/part2/ch1-background/
git commit -m "docs(thesis): rewrite ch2-background — reconcile registry, DeepFashion2, neutral narrative"
```

---

### Task 12: Rewrite Chapter 2 Design — Part A (Structural Fixes)

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/*.typ` (requirements, use cases, architecture, database, API)

**Interfaces:**
- Consumes: actual DB schema from `service/Api/src/Module/Catalog/Persistence/` and `service/Api/src/Shared/Operational/Persistence/`
- Produces: Design chapter with resolved vector dim, pgvector version, terminology, FR traceability, condensed use cases

- [ ] **Step 1: Resolve vector dimensionality**

Inspect the actual EF Core configuration at `service/Api/src/Module/Catalog/Persistence/Configurations/Products/ImageEmbeddingConfiguration.cs`. Determine:
- What column type is actually used (`vector(512)`? `vector` untyped? per-model columns?)
- How the 11-model variable-dimension claim is actually handled

Update the thesis text to match reality. If the implementation uses `vector(512)` only, remove the claim about storing 384/768/1280/2048-dim vectors. If the implementation uses `vector` untyped, state that explicitly.

- [ ] **Step 2: Fix pgvector version**

Replace all `0.3.2` references with `0.5.0+` (the version that introduced HNSW). Verify the actual deployed version:

```bash
podman exec -i <postgres-container> psql -U <user> -d <db> -c "SELECT extversion FROM pg_extension WHERE extname='vector';"
```

- [ ] **Step 3: Add `SET hnsw.ef_search = 100` to DDL examples**

In the pgvector section, add:

```sql
SET hnsw.ef_search = 100;  -- recall-optimising search parameter
```

- [ ] **Step 4: Unify HNSW latency numbers**

Pick ONE measured value from Phase 1 pipeline results. State with full conditions: "HNSW query latency measured at X ms (p50) on Y vectors, Z hardware, ef_search=100."

- [ ] **Step 5: Fix embedding table naming**

Reconcile `variant_images.embedding` vs `product_image_embeddings.embedding` to match the actual EF Core configuration. Use the correct name throughout.

- [ ] **Step 6: Fix CBIR endpoint path**

Verify the actual deployed path:

```bash
grep -r "search-by-image" service/Api/src/Module/Catalog/Features/ app/Store/src/
```

Unify C# and TypeScript code examples to match the real path.

- [ ] **Step 7: Fix "9 modules vs 8 bounded contexts" terminology**

Add one paragraph defining the relationship: "The platform organises functionality into 9 business modules. Eight of these map to bounded contexts in the DDD sense; the Dashboard module is cross-cutting and not a bounded context. Throughout this thesis, 'module' refers to the deployment/code unit, 'bounded context' to the DDD domain boundary." Use consistently thereafter.

- [ ] **Step 8: Restore FR traceability (core subset)**

Declare a core-FR subset (~15-20 FRs covering the CBIR path + 2-3 other modules). For each core FR, add a "Traces to:" annotation linking to the use case, architecture component, and implementation feature. Appendix the rest.

- [ ] **Step 9: Condense use cases**

Keep 3-5 representative UCs inline with brief description (e.g., one admin CRUD, one customer purchase flow, the CBIR search UC). Move the full 26-UC table to an appendix. Preface with: "Standard exception flows (persistence failure, concurrent modification) are documented once in Appendix X; only domain-specific exceptions appear inline."

- [ ] **Step 10: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/part2/ch2-design/
git commit -m "docs(thesis): rewrite ch2-design part A — pgvector, terminology, traceability, condensed UCs"
```

---

### Task 13: Rewrite Chapter 2 Design — Part B (CBIR, Rationale, Screenshots)

**Files:**
- Modify: `thesis/chapters/part2/ch2-design/04-implementations/*.typ` (ML sidecar, frontend)

**Interfaces:**
- Consumes: Phase 1 pipeline results for CBIR evidence
- Produces: Design chapter with foregrounded CBIR subsection, design-decision rationales, out-of-scope statement, real screenshots

- [ ] **Step 1: Foreground CBIR in dedicated subsection**

Create a new subsection "2.4.4 CBIR Search Pipeline" with:
- Embedding pipeline per supported model (upload → validation → ML sidecar → L2-normalize → pgvector insert)
- Search index configuration (HNSW params, `ef_search`, `model_name` discriminator)
- Query flow (image → embed → pgvector cosine similarity → threshold filter → product dedup → UI render)
- Result quality with 2-3 example images showing top-K results with similarity scores

- [ ] **Step 2: Add 3-5 design-decision rationale paragraphs**

Brief "Why this choice" paragraphs for:
- pgvector over Milvus/Qdrant (transactional consistency, SQL integration)
- CQRS + MediatR for module isolation (in-process messaging, compile-time isolation)
- Redis for hybrid caching (L1 in-process + L2 shared, Hangfire backing)
- Carter over traditional controllers (minimal API surface, module-co-located endpoints)
- Hangfire for background jobs (Redis-backed persistence, cron-style scheduling)

- [ ] **Step 3: Fix pgvector ACID overclaim**

Replace "Vectors and product metadata share the same ACID boundary" with a more precise statement:

> Embeddings and product metadata share the same PostgreSQL instance and transactional boundary on the *write path*. The embedding generation queue (Hangfire) introduces a brief window of inconsistency between product upload and embedding availability; once committed, the embedding and product are atomically consistent.

Also revise the vector-DB comparison table: represent Qdrant/Milvus honestly (both open-source, both free, both support filtering).

- [ ] **Step 4: Add out-of-scope statement**

Add one paragraph:

> The following business domains are explicitly out of scope for this thesis: tax computation (VAT/GST/sales tax), returns and RMA workflows, discounts/promotions/coupon codes, and product reviews/ratings. A production deployment would require these; the thesis focuses on the CBIR contribution and the e-commerce context sufficient to evaluate it.

- [ ] **Step 5: Replace screenshot placeholders**

Capture real screenshots of the running system. For each `// [SCREENSHOT: ...]` placeholder in the thesis:
- Run the system (Aspire + storefront + admin)
- Capture the screen with the tool of your choice (e.g., `gnome-screenshot`, browser devtools)
- Save to `thesis/figures/...` matching the expected path
- Replace the placeholder comment with the actual `#figure(image(...))` call

Priority: CBIR search flow (upload → results grid), admin product management, storefront checkout.

- [ ] **Step 6: Fix duplicated paragraph + typos**

- Delete the duplicated "Compile-Time Module Isolation" paragraph (one of lines 2920/2956)
- Fix "realization" → "realizes" (line ~2820)
- Fix "a unconstrained" → "an unconstrained" (line ~3071)

- [ ] **Step 7: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/part2/ch2-design/ thesis/figures/
git commit -m "docs(thesis): rewrite ch2-design part B — CBIR foreground, rationale, screenshots, typo fixes"
```

---

### Task 14: Rewrite Chapter 3 Evaluation (from Phase 1 data)

**Files:**
- Modify: `thesis/chapters/part2/ch3-evaluation/*.typ`

**Interfaces:**
- Consumes: `outputs/thesis/results/thesis_results_pattern.json` (PRIMARY), `thesis_results.json` (supplementary), `pipeline_results.json` (RQ3), `thesis_cohens_d.json`, generated Typst tables
- Produces: Ch.3 rewritten around 13-model × multi-label primary + proper statistics

- [ ] **Step 1: Replace aggregate/efficiency tables with Phase 1 data**

Copy the regenerated `thesis_aggregate.typ` and `thesis_efficiency.typ` into the thesis. Update any inline numbers to match.

- [ ] **Step 2: Switch to multi-label as PRIMARY**

The "Retrieval Performance" section should now lead with the pattern-label results (`thesis_results_pattern.json`). The category-only results become a supplementary comparison showing how coarse vs fine ground truth affects scores.

- [ ] **Step 3: Replace "±2σ ≈ 95% CI" with bootstrap ci_95**

Replace all confidence-interval language. The code already computes bootstrap CIs at 95% with 10,000 resamples and seed=42 — report these directly as "95% bootstrap CI [lower, upper]".

- [ ] **Step 4: Report per-fold values in appendix table**

Add an appendix table showing per-fold mAP for each model. This addresses the "n=3, SD unstable" concern by making the raw data visible.

- [ ] **Step 5: Report SD for P@K/R@K**

Update the aggregate table to include mean±SD for precision and recall at each K (Phase 1 Task 5 wired this in).

- [ ] **Step 6: Add Cohen's d matrix for key comparisons**

Present a small matrix table for the 4-5 most important comparisons (Fashion-CLIP vs CLIP-ViT-B/16, Fashion-CLIP vs EfficientNet-B0, etc.) with effect-size interpretation ("large" if |d| > 0.8, "medium" if > 0.5, "small" otherwise).

- [ ] **Step 7: Add baseline anchors**

New section: "Baseline Anchors". Report ColorHistogram mAP and RandomBaseline mAP. Interpret: "Deep learning adds X pp over color histogram and Y pp over random ranking, demonstrating that learned embeddings capture visual similarity beyond trivial color matching."

- [ ] **Step 8: Add Fashion-CLIP vs CLIP-ViT-B/16 comparison**

New subsection: "Isolating Architecture from Domain Fine-Tuning". Compare Fashion-CLIP (ViT-B/16, fashion-tuned) vs CLIP-ViT-B/16 (ViT-B/16, generic). The difference isolates the domain-fine-tuning effect from the architecture effect.

- [ ] **Step 9: Add RQ3 end-to-end latency table**

New subsection: "Answer to RQ3: End-to-End Latency". Table with columns: Model, Inference (ms), HTTP Overhead (ms), pgvector Query (ms), Assembly (ms), Total (ms). Use pipeline_results.json.

- [ ] **Step 10: Recompute and fix 5.4% vs 6.1% contradiction**

Verify against Phase 1 data. The correct number is what the data says; the thesis text must match.

- [ ] **Step 11: Fix "~30 relevant items" claim**

Recompute from the ground truth:

```bash
cd benchmarks && uv run python -c "
import json
data = json.load(open('outputs/thesis/splits/fold_0_test.json'))
# Count relevant items per query
..."
```

Update the text to the actual number.

- [ ] **Step 12: Document measurement protocol**

Add a "Measurement Protocol" subsection documenting:
- Seed = 42
- Warmup = 10 runs, benchmark = 100 runs
- Batch size = 64
- Framework versions (PyTorch, torchvision, transformers, Python) — read from `uv.lock`
- HuggingFace model IDs with commit hashes
- Per-model preprocessing pipeline (state it's per-model-card)
- halfvec storage precision

- [ ] **Step 13: Add testing-strategy bridge paragraph**

Between "Testing Strategy" and "Benchmark Protocol" sections, add:

> Functional correctness of the platform was established via unit, integration, and end-to-end testing (Section 2.X). The quantitative benchmark that follows evaluates a different question: which embedding model delivers the best retrieval quality under realistic deployment constraints.

- [ ] **Step 14: Interpret low recall values**

Add 2-3 sentences in the "Retrieval Performance" section:

> Recall@20 of X% is expected given the ground-truth pool: each query has ~N relevant items in a gallery of ~3,300, so exhaustive retrieval at rank 20 captures at most 20/N of the relevant set. For user-facing search, this manifests as high precision in the top results (most shown items are relevant) with incomplete coverage of the catalog (not every relevant item appears).

- [ ] **Step 15: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/part2/ch3-evaluation/
git commit -m "docs(thesis): rewrite ch3 evaluation from Phase 1 data

13 models, multi-label primary, bootstrap CIs, Cohen's d, baseline
anchors, RQ3 end-to-end, measurement protocol. Resolves all
Phase 3 reviewer critiques."
```

---

### Task 15: Rewrite Chapter 4 Conclusion + Back Matter

**Files:**
- Modify: `thesis/chapters/part3/ch4-conclusion.typ`
- Modify: `thesis/backmatter/references.typ` (+ `.bib`)

**Interfaces:**
- Consumes: Phase 1 final numbers, Phase 2 Ch.1-3 rewrites
- Produces: Conclusion with refreshed numbers, verified bibliography, repo URL

- [ ] **Step 1: Fix "five" → "seven" accuracy metrics**

In the Summary of Work, replace "five accuracy metrics" with "seven accuracy metrics (mAP and six rank-based metrics P@5, P@10, P@20, R@5, R@10, R@20)".

- [ ] **Step 2: Refresh all quantitative claims from Phase 1**

Walk through each RQ answer and update numbers to match Phase 1 JSON. Pay special attention to:
- Fashion-CLIP's actual margin over CLIP-ViT-B/16 (the architecture-matched control)
- EfficientNet-B0's mAP (verify the std dev fix held)
- RQ3 end-to-end latency for Fashion-CLIP

- [ ] **Step 3: Replace rhetorical question**

In the Future Work section, replace "Do some models degrade gracefully while others collapse?" with:

> It remains unknown whether some models degrade gracefully at scale while others collapse — a question that requires larger datasets and GPU-equipped hardware to answer.

- [ ] **Step 4: Verify bibliography completeness**

- [ ] Every `@citation_key` in Ch.1-4 has a corresponding entry in `thesis/backmatter/bibliography.bib`
- [ ] IEEE style is correct for all entries (author format, title casing, DOI presence)
- [ ] No orphaned entries (in `.bib` but never cited)
- [ ] Add missing entries: DeepFashion2, ONNX Runtime, Microsoft Aspire

```bash
cd thesis && grep -oE '@[a-zA-Z0-9_-]+' chapters/**/*.typ | sort -u > /tmp/cited.txt
grep -oE '@[a-zA-Z0-9_-]+' backmatter/bibliography.bib | sort -u > /tmp/available.txt
diff /tmp/cited.txt /tmp/available.txt
```

- [ ] **Step 5: Add repository URL + license statement**

In the Introduction (or Conclusion, per preference), add one sentence:

> The complete source code for ReSys.Shop, including the benchmark framework, is available under the [MIT/Apache 2.0] license at <https://github.com/<org>/resys-shop>.

Verify the repo URL is correct and the license file exists in the repo root.

- [ ] **Step 6: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/part3/ thesis/backmatter/
git commit -m "docs(thesis): rewrite ch4 conclusion + verify bibliography

Refreshed numbers from Phase 1, metric count fix, repo URL and
license statement, bibliography completeness verified."
```

---

### Phase 2 Gate

**Before starting Phase 3, verify:**

- [ ] All P0 items from all 4 passes are resolved
- [ ] All P1 items resolved or explicitly deferred with rationale (listed in commit message)
- [ ] `make` in `thesis/` builds clean with no Typst errors
- [ ] No `// [SCREENSHOT: ...]` placeholders remain
- [ ] All `@citation_key` references resolve in the bibliography

---

## Phase 3 — Polish

Phase 3 is a series of global passes over the thesis. Each pass has one focus.

---

### Task 16: Redundancy Elimination Pass

**Files:** all `thesis/chapters/**/*.typ`

- [ ] **Step 1: Identify redundancy targets**

Read the thesis linearly. Flag every instance of:
- Semantic gap (define once in Background, reference thereafter)
- 770B/1T fashion e-commerce statistics (state once)
- Vertical-slice architecture (define once)
- CBIR definition (define once)
- Fashion-CLIP selection narrative (neutral, no arc)

- [ ] **Step 2: Delete or consolidate**

For each redundancy: keep the best instance, replace others with a forward/back reference (e.g., "as defined in Section 2.2").

- [ ] **Step 3: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/
git commit -m "refactor(thesis): redundancy elimination pass"
```

---

### Task 17: De-Textbook Compression Pass

**Files:** all `thesis/chapters/**/*.typ`

- [ ] **Step 1: Identify textbook sections**

Flag sections that explain well-known concepts at length:
- Cosine similarity math (compress to ~1 paragraph + citation)
- CNN basics (compress)
- HNSW internals (compress)
- Monolith vs microservices vs modular monolith (compress)

- [ ] **Step 2: Replace with concise citations**

For each: replace multi-paragraph exposition with "X is described by Author (Year); we use it here for Y." Target ~40-50% reduction in fundamentals.

- [ ] **Step 3: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/
git commit -m "refactor(thesis): de-textbook compression pass (~40% reduction in fundamentals)"
```

---

### Task 18: Terminology Control Pass

**Files:** all `thesis/chapters/**/*.typ`

- [ ] **Step 1: Settle "sidecar" naming**

Pick one: "AI sidecar" / "ML sidecar" / "Python sidecar" / "embedding service". Document the choice in the ubiquitous-language glossary. Use consistently.

- [ ] **Step 2: Settle module/context terminology**

Ensure "9 business modules" and "8 bounded contexts" usage matches the definition added in Task 12.

- [ ] **Step 3: Search-and-replace**

```bash
cd thesis && grep -rn "ML sidecar\|AI sidecar\|Python sidecar\|embedding service" chapters/
```

Replace all with the settled term.

- [ ] **Step 4: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/
git commit -m "refactor(thesis): terminology control pass (settled sidecar + module/context naming)"
```

---

### Task 19: Transition Smoothing Pass

**Files:** all `thesis/chapters/**/*.typ`

- [ ] **Step 1: Walk chapter-to-chapter and section-to-section boundaries**

At each boundary, ask: does the reader know why the next section follows from the previous? If not, add 1-2 bridging sentences.

- [ ] **Step 2: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/
git commit -m "refactor(thesis): transition smoothing pass"
```

---

### Task 20: Grammar Pass

**Files:** all `thesis/chapters/**/*.typ`

- [ ] **Step 1: Run-on sentence pass**

Target: ≤25 words per sentence for technical prose. Break longer sentences at clause boundaries.

- [ ] **Step 2: Negative-connotation pass**

Replace "subjected to" → "underwent" / "was evaluated using". Remove "now" draft markers.

- [ ] **Step 3: Article/grammar pass**

"a" vs "an" before vowel sounds. "realization" → "realizes" (already fixed in Task 13, verify).

- [ ] **Step 4: Tone calibration pass**

Replace overclaims:
- "X is clearly the best choice" → "we selected X because Y"
- "X solves Y" → "X addresses the Y aspect"

- [ ] **Step 5: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/chapters/
git commit -m "refactor(thesis): grammar and tone pass"
```

---

### Task 21: Final Consistency Audit

**Files:** all `thesis/chapters/**/*.typ`, `thesis/backmatter/*.typ`

- [ ] **Step 1: Model count audit**

```bash
cd thesis && grep -rn "11 model\|13 model\|four model" chapters/
```

Ensure "13 models" is used consistently when referring to the benchmark. "4 models" only when referring to the production registry.

- [ ] **Step 2: Training-data figures audit**

ImageNet: "1.2M" vs "1.28M" — pick one and use consistently. Fashion-CLIP fine-tuning: "700K" — verify and cite.

- [ ] **Step 3: Cross-reference audit**

For each `Section X.Y.Z` or `@tbl-...` or `@fig-...` reference, verify the target exists.

```bash
cd thesis && grep -oE '(Section|@tbl-|@fig-)[a-zA-Z0-9._-]+' chapters/**/*.typ | sort -u > /tmp/refs.txt
# Cross-check against actual section/table/figure definitions
```

- [ ] **Step 4: Acronym first-use audit**

For each acronym (CBIR, DSR, HNSW, mAP, ViT, etc.), verify it is defined at first use.

- [ ] **Step 5: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/
git commit -m "refactor(thesis): final consistency audit"
```

---

### Task 22: Bibliography Verification

**Files:** `thesis/backmatter/bibliography.bib`

- [ ] **Step 1: Cited-vs-available diff**

```bash
cd thesis && grep -hoE '@[a-zA-Z0-9_-]+' chapters/**/*.typ | sort -u > /tmp/cited.txt
grep -oE '^@[a-z]+\{([a-zA-Z0-9_-]+)' backmatter/bibliography.bib | sed 's/^@[a-z]*{//' | sort -u > /tmp/available.txt
echo "Cited but missing:" && comm -23 /tmp/cited.txt /tmp/available.txt
echo "In bib but never cited:" && comm -13 /tmp/cited.txt /tmp/available.txt
```

- [ ] **Step 2: Add missing entries**

For each missing entry, add with IEEE-style formatting. Required additions from the reviews: DeepFashion2, ONNX Runtime, Microsoft Aspire, HuggingFace model cards.

- [ ] **Step 3: Remove orphaned entries**

Delete any `.bib` entry not cited anywhere.

- [ ] **Step 4: Verify DOI presence**

For journal/conference papers, verify DOI is present. Books/theses: ISBN or URL.

- [ ] **Step 5: Build + commit**

```bash
cd thesis && make
cd /home/ngtphat/Projects/ReSys.Shop && git add thesis/backmatter/bibliography.bib
git commit -m "docs(thesis): bibliography completeness verified"
```

---

### Phase 3 Gate / Final Verification

- [ ] `make` in `thesis/` builds clean — no Typst errors, no missing figures, no orphaned references
- [ ] `uv run ruff check src/` and `uv run pytest --ignore=src/tests/integration/` in `benchmarks/` green
- [ ] Rendered PDF reviewed visually (no broken layouts, all figures present, tables legible)
- [ ] Spot re-review of Ch.3 using the same reviewer panel's criteria: does the new data support the rewritten claims?

---

## Execution Handoff

Plan complete and saved to `docs/superpowers/plans/2026-07-30-thesis-revision.md`. Two execution options:

**1. Subagent-Driven (recommended)** - I dispatch a fresh subagent per task, review between tasks, fast iteration. Best for this plan because the tasks are independent within each phase and the benchmark re-runs (Tasks 7-9) take wall-clock time that benefits from parallel orchestration.

**2. Inline Execution** - Execute tasks in this session using executing-plans, batch execution with checkpoints. Better if you want to stay in a single conversation context and review diffs inline.

Which approach?
