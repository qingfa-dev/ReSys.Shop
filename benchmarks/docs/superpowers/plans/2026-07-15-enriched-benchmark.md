# Enriched Dataset + Dual-Label Benchmark — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Integrate visual attributes from per-product JSON metadata into the benchmark via a new `benchmark enrich` CLI command, and add side-by-side primary/secondary evaluation so models can be compared on `subCategory/norm_colour` vs `subCategory/norm_colour/pattern` relevance schemes without changing the evaluator.

**Architecture:** New standalone script `scripts/enrich_dataset.py` merges CSV + JSON into enriched dual-label split files. `ThesisRunner` gains optional secondary-evaluation pass controlled by `--secondary-label`. `FashionDataset.load()` accepts `label_field` parameter for label-swapping. The evaluator (`Evaluator.evaluate_split`) is untouched.

**Tech Stack:** Python 3.12, pandas, JSON, Typer CLI. No new dependencies.

## Global Constraints

- Python >=3.12, uv package manager, ruff lint rules (E, F, I, UP, B, SIM)
- Absolute imports with `benchmark.` prefix only
- snake_case files and functions, PascalCase classes
- Tests in `src/tests/`, mirrored structure from `src/benchmark/`
- All changes must pass `uv run pytest --ignore=src/tests/integration/ -q` before commit
- TDD: write failing test first, then minimal implementation

---

### Task 1: Rename `_normalize_colour` to public `normalize_colour`

**Files:**
- Modify: `src/benchmark/datasets/ground_truth.py:28,93,130,179`
- Modify: `src/tests/datasets/test_ground_truth.py`

**Interfaces:**
- Produces: `normalize_colour(raw: str | float | None) -> str` (was `_normalize_colour`)
- Consumers: Tasks 2, 3 (enrichment script imports this)

- [ ] **Step 1: Rename function definition and update internal callers**

In `src/benchmark/datasets/ground_truth.py:28`, change:

```python
def _normalize_colour(raw: str | float | None) -> str:
```

to:

```python
def normalize_colour(raw: str | float | None) -> str:
```

Then update the three internal call sites. In `build_relevance_sets()` at line 93:
```python
df["_norm_colour"] = df["baseColour"].apply(_normalize_colour)
```
→
```python
df["_norm_colour"] = df["baseColour"].apply(normalize_colour)
```

In `generate_splits()` at line 179:
```python
self.df["_norm_colour"] = (self.df["baseColour"].apply(_normalize_colour) ...
```
→
```python
self.df["_norm_colour"] = (self.df["baseColour"].apply(normalize_colour) ...
```

In `meta_by_id` dict at line 184, update the lambda to call `normalize_colour` (already applied on line 179, so this is a precomputed column — no change needed for the lambda itself, but ensure `_norm_colour` is used consistently).

- [ ] **Step 2: Run tests to verify no regressions**

```bash
cd benchmarks && uv run pytest src/tests/datasets/test_ground_truth.py -v
```
Expected: 5 passed

- [ ] **Step 3: Commit**

```bash
git add src/benchmark/datasets/ground_truth.py
git commit -m "refactor: rename _normalize_colour to public normalize_colour"
```

---

### Task 2: Add `label_field` parameter to `FashionDataset.load()`

**Files:**
- Modify: `src/benchmark/datasets/loader.py:68-84`
- Create: `src/tests/datasets/test_loader_label_field.py`

**Interfaces:**
- Produces: `FashionDataset.load(label_field: str = "label")` — reads `item[label_field]` instead of hardcoded `item["label"]`
- Consumers: Task 5 (ThesisRunner secondary pass)

- [ ] **Step 1: Write failing test**

Create `src/tests/datasets/test_loader_label_field.py`:

```python
import json
from pathlib import Path
from benchmark.datasets.loader import FashionDataset


def test_load_with_custom_label_field(tmp_path: Path):
    split_file = tmp_path / "split.json"
    split_file.write_text(json.dumps([
        {"image_path": "images/1.jpg", "label": "Topwear/Blue",
         "label_pattern": "Topwear/Blue/Checked", "product_id": "1"},
        {"image_path": "images/2.jpg", "label": "Topwear/Blue",
         "label_pattern": "Topwear/Blue/Solid", "product_id": "2"},
    ]))

    dataset_root = tmp_path / "images"
    dataset_root.mkdir()

    # Default label_field
    ds1 = FashionDataset(dataset_root=tmp_path, split_file=split_file, split="test")
    ds1.load()
    assert ds1.samples[0].label == "Topwear/Blue"

    # Custom label_field
    ds2 = FashionDataset(dataset_root=tmp_path, split_file=split_file, split="test")
    ds2.load(label_field="label_pattern")
    assert ds2.samples[0].label == "Topwear/Blue/Checked"
    assert ds2.samples[1].label == "Topwear/Blue/Solid"
```

- [ ] **Step 2: Run test to verify it fails**

```bash
uv run pytest src/tests/datasets/test_loader_label_field.py::test_load_with_custom_label_field -v
```
Expected: FAIL — `FashionDataset.load()` got unexpected keyword argument `label_field`

- [ ] **Step 3: Implement `label_field` parameter**

In `src/benchmark/datasets/loader.py:68`, change:

```python
def load(self) -> None:
    """Parse the split JSON and build the internal sample list.

    Reads the split file as UTF-8 JSON, then constructs ``Sample``
    instances with paths rooted at ``dataset_root``.
    """
    logger.info("Loading %s split from %s", self.split, self.split_file)
    raw = json.loads(self.split_file.read_text(encoding=FILE_ENCODING))
    self._samples = [
        Sample(
            image_path=self.dataset_root / item[FIELD.IMAGE_PATH],
            label=item[FIELD.LABEL],
            product_id=item[FIELD.PRODUCT_ID],
            split=self.split,
        )
        for item in raw
    ]
    logger.info("Loaded %d samples", len(self._samples))
```

to:

```python
def load(self, label_field: str = FIELD.LABEL) -> None:
    """Parse the split JSON and build the internal sample list.

    Reads the split file as UTF-8 JSON, then constructs ``Sample``
    instances with paths rooted at ``dataset_root``.

    Args:
        label_field: JSON key for the relevance label. Defaults to
            ``"label"``.  Set to ``"label_pattern"`` for the secondary
            (pattern-aware) evaluation pass.
    """
    logger.info("Loading %s split from %s (label=%s)", self.split, self.split_file, label_field)
    raw = json.loads(self.split_file.read_text(encoding=FILE_ENCODING))
    self._samples = [
        Sample(
            image_path=self.dataset_root / item[FIELD.IMAGE_PATH],
            label=item[label_field],
            product_id=item[FIELD.PRODUCT_ID],
            split=self.split,
        )
        for item in raw
    ]
    logger.info("Loaded %d samples", len(self._samples))
```

- [ ] **Step 4: Run test to verify it passes**

```bash
uv run pytest src/tests/datasets/test_loader_label_field.py -v
```
Expected: PASS

- [ ] **Step 5: Run full test suite**

```bash
uv run pytest --ignore=src/tests/integration/ -q
```
Expected: all existing tests pass (the parameter has a default so no callers break)

- [ ] **Step 6: Commit**

```bash
git add src/benchmark/datasets/loader.py src/tests/datasets/test_loader_label_field.py
git commit -m "feat: add label_field parameter to FashionDataset.load()"
```

---

### Task 3: Create enrichment script `scripts/enrich_dataset.py`

**Files:**
- Create: `scripts/enrich_dataset.py`
- Create: `src/tests/scripts/test_enrich.py`
- Modify: `src/benchmark/_constants.py` (add PATTERN constant)

**Interfaces:**
- Consumes: `normalize_colour` from `benchmark.datasets.ground_truth` (Task 1)
- Produces: enriched CSV at `output/styles.csv` and dual-label split JSON files at `output/splits/fold_*_*.json`
- CLI: `benchmark enrich` command wired in Task 4

- [ ] **Step 1: Add `SECONDARY_LABEL_FIELD` constant**

In `src/benchmark/_constants.py`, add to `DATASET_FIELDS`:

```python
@dataclass(frozen=True)
class DATASET_FIELDS:
    # ... existing fields ...
    SECONDARY_LABEL: str = "label_pattern"
    PATTERN: str = "pattern"
```

- [ ] **Step 2: Write enrichment script**

Create `scripts/enrich_dataset.py`:

```python
#!/usr/bin/env python
"""Merge CSV product metadata with per-product JSON articleAttributes.

Produces an enriched dataset with two label schemes per sample:
  label         — subCategory/normalizedColour  (primary)
  label_pattern — subCategory/normalizedColour/Pattern  (secondary)

Usage:
    uv run python scripts/enrich_dataset.py \\
        --json-styles data/raw/fashion-product-images/styles/ \\
        --csv data/raw/fashion-product-images-small/styles.csv \\
        --output data/raw/fashion-enriched-5k \\
        --subset 5000
"""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any


def parse_args() -> argparse.Namespace:
    p = argparse.ArgumentParser(description="Enrich dataset with JSON articleAttributes")
    p.add_argument("--json-styles", required=True, type=Path,
                   help="Directory of per-product JSON files")
    p.add_argument("--csv", required=True, type=Path,
                   help="CSV with id, subCategory, baseColour columns")
    p.add_argument("--output", required=True, type=Path,
                   help="Output directory for enriched dataset")
    p.add_argument("--subset", type=int, default=0,
                   help="Limit to first N products (0 = all)")
    p.add_argument("--folds", type=int, default=3,
                   help="Number of CV folds")
    p.add_argument("--seed", type=int, default=42,
                   help="Random seed for split generation")
    return p.parse_args()


def extract_pattern(data: dict[str, Any]) -> str:
    aa = data.get("articleAttributes", {})
    if not isinstance(aa, dict):
        return "Unknown"
    return str(aa.get("Pattern", "Unknown")).strip() or "Unknown"


def main() -> None:
    args = parse_args()

    # Import dependencies after arg parsing (lazy — CLI may be fast)
    import pandas as pd
    import numpy as np
    from benchmark.datasets.ground_truth import normalize_colour

    # 1. Read CSV → lookup by id
    print(f"Reading CSV: {args.csv}")
    csv_df = pd.read_csv(args.csv, on_bad_lines="warn")
    csv_rows = {}
    for _, row in csv_df.iterrows():
        pid = str(row["id"])
        csv_rows[pid] = {
            "masterCategory": str(row.get("masterCategory", "")),
            "subCategory": str(row.get("subCategory", "")),
            "articleType": str(row.get("articleType", "")),
            "baseColour": str(row.get("baseColour", "")),
            "season": str(row.get("season", "")),
            "year": str(row.get("year", "")),
            "usage": str(row.get("usage", "")),
            "gender": str(row.get("gender", "")),
            "productDisplayName": str(row.get("productDisplayName", "")),
        }

    # 2. Read JSON files, merge with CSV
    json_dir: Path = args.json_styles
    json_files = sorted(json_dir.glob("*.json"))
    if args.subset > 0:
        json_files = json_files[:args.subset]

    enriched: list[dict[str, str]] = []
    for jf in json_files:
        try:
            d = json.loads(jf.read_text(encoding="utf-8"))
        except (json.JSONDecodeError, OSError):
            continue
        data = d.get("data", {})
        pid = str(data.get("id", ""))
        if not pid or pid not in csv_rows:
            continue
        row = csv_rows[pid]
        pattern = extract_pattern(data)
        enriched.append({
            "id": pid,
            "masterCategory": row["masterCategory"],
            "subCategory": row["subCategory"],
            "articleType": row["articleType"],
            "baseColour": row["baseColour"],
            "season": row["season"],
            "year": row["year"],
            "usage": row["usage"],
            "gender": row["gender"],
            "productDisplayName": row["productDisplayName"],
            "pattern": pattern,
        })

    if not enriched:
        print("ERROR: No enriched rows produced. Check --json-styles and --csv paths.")
        sys.exit(1)

    enriched_df = pd.DataFrame(enriched)
    print(f"Enriched {len(enriched_df)} rows")

    # 3. Write enriched CSV
    args.output.mkdir(parents=True, exist_ok=True)
    csv_out = args.output / "styles.csv"
    enriched_df.to_csv(csv_out, index=False)
    print(f"Wrote enriched CSV: {csv_out}")

    # 4. Generate stratified splits with dual labels
    rng = np.random.default_rng(args.seed)
    categories = enriched_df["masterCategory"].unique()
    fold_indices: list[list[int]] = [[] for _ in range(args.folds)]
    for cat in categories:
        cat_df = enriched_df[enriched_df["masterCategory"] == cat].reset_index(drop=True)
        indices = cat_df.index.to_numpy().copy()
        rng.shuffle(indices)
        splits_arr = np.array_split(indices, args.folds)
        for fi, s in enumerate(splits_arr):
            fold_indices[fi].extend(cat_df.iloc[s]["id"].tolist())

    # Build dual-label sample entries
    enriched_df["_nc"] = enriched_df["baseColour"].apply(normalize_colour)
    meta_by_id = {}
    for _, row in enriched_df.iterrows():
        pid = row["id"]
        sc = str(row["subCategory"])
        nc = str(row["_nc"])
        pat = str(row.get("pattern", "Unknown"))
        primary = f"{sc}/{nc}"
        secondary = f"{sc}/{nc}/{pat}" if pat != "Unknown" else primary
        meta_by_id[pid] = {
            "image_path": f"images/{pid}.jpg",
            "label": primary,
            "label_pattern": secondary,
            "product_id": str(pid),
        }

    all_ids = set(enriched_df["id"].tolist())
    splits_dir = args.output / "splits"
    splits_dir.mkdir(parents=True, exist_ok=True)
    for fi in range(args.folds):
        test_ids = set(str(i) for i in fold_indices[fi])
        train_ids = all_ids - test_ids
        train = [meta_by_id[pid] for pid in sorted(train_ids) if pid in meta_by_id]
        test = [meta_by_id[pid] for pid in sorted(test_ids) if pid in meta_by_id]
        (splits_dir / f"fold_{fi}_train.json").write_text(json.dumps(train, indent=2), encoding="utf-8")
        (splits_dir / f"fold_{fi}_test.json").write_text(json.dumps(test, indent=2), encoding="utf-8")
        print(f"Fold {fi}: train={len(train)}, test={len(test)}")

    print(f"Done. Output: {args.output}")


if __name__ == "__main__":
    main()
```

- [ ] **Step 3: Write test for enrichment script**

Create `src/tests/scripts/test_enrich.py`:

```python
import json
from pathlib import Path
from scripts.enrich_dataset import extract_pattern


def test_extract_pattern_solid():
    data = {"articleAttributes": {"Pattern": "Solid"}}
    assert extract_pattern(data) == "Solid"


def test_extract_pattern_missing():
    assert extract_pattern({}) == "Unknown"
    assert extract_pattern({"articleAttributes": {}}) == "Unknown"


def test_extract_pattern_empty_string():
    data = {"articleAttributes": {"Pattern": ""}}
    assert extract_pattern(data) == "Unknown"


def test_enrich_integration(tmp_path: Path):
    """End-to-end: create mini JSON + CSV, run enrichment, verify output."""
    import subprocess, sys

    # Create mini CSV
    csv_path = tmp_path / "mini.csv"
    csv_path.write_text("id,masterCategory,subCategory,articleType,baseColour,season,year,usage,gender,productDisplayName\n"
                        "1,Apparel,Topwear,Tshirts,Blue,Summer,2012,Casual,Men,Blue T-shirt\n"
                        "2,Apparel,Topwear,Shirts,Blue,Fall,2011,Casual,Men,Navy Blue Shirt\n")

    # Create mini JSON files
    json_dir = tmp_path / "json_styles"
    json_dir.mkdir()
    (json_dir / "1.json").write_text(json.dumps({"data": {"id": 1, "articleAttributes": {"Pattern": "Solid"}}}))
    (json_dir / "2.json").write_text(json.dumps({"data": {"id": 2, "articleAttributes": {"Pattern": "Checked"}}}))

    out_dir = tmp_path / "enriched"
    result = subprocess.run([
        sys.executable, "scripts/enrich_dataset.py",
        "--json-styles", str(json_dir),
        "--csv", str(csv_path),
        "--output", str(out_dir),
        "--subset", "2",
    ], capture_output=True, text=True)
    assert result.returncode == 0, result.stderr

    # Verify enriched CSV
    csv_out = out_dir / "styles.csv"
    assert csv_out.exists()

    # Verify split files have dual labels
    split_file = out_dir / "splits" / "fold_0_test.json"
    samples = json.loads(split_file.read_text())
    assert len(samples) > 0
    assert "label" in samples[0]
    assert "label_pattern" in samples[0]
    if samples[0]["product_id"] == "1":
        assert samples[0]["label"] == "Topwear/Blue"
        assert samples[0]["label_pattern"] == "Topwear/Blue/Solid"
```

- [ ] **Step 4: Run enrichment test**

```bash
uv run pytest src/tests/scripts/test_enrich.py -v
```
Expected: PASS (integration test creates real files)

- [ ] **Step 5: Run with real data to verify**

```bash
uv run python scripts/enrich_dataset.py \
    --json-styles data/raw/fashion-product-images/styles/ \
    --csv data/raw/fashion-product-images-small/styles.csv \
    --output data/raw/fashion-enriched-5k \
    --subset 5000
```

Verify output files exist:
```bash
ls data/raw/fashion-enriched-5k/styles.csv
ls data/raw/fashion-enriched-5k/splits/fold_0_test.json
```
Check a sample has dual labels:
```bash
uv run python3 -c "import json; d=json.load(open('data/raw/fashion-enriched-5k/splits/fold_0_test.json')); print(d[0])"
```
Expected: `{'image_path': 'images/...', 'label': 'Topwear/Blue', 'label_pattern': 'Topwear/Blue/...', 'product_id': '...'}`

- [ ] **Step 6: Link images**

```bash
rm -rf data/raw/fashion-enriched-5k/images
ln -s "$(pwd)/data/raw/fashion-product-images-small/images" data/raw/fashion-enriched-5k/images
```

- [ ] **Step 7: Commit**

```bash
git add scripts/enrich_dataset.py src/tests/scripts/test_enrich.py src/benchmark/_constants.py
git commit -m "feat: add enrichment script for JSON articleAttributes"
```

---

### Task 4: Add `benchmark enrich` CLI command

**Files:**
- Modify: `src/benchmark/cli/benchmark.py`

**Interfaces:**
- Consumes: `scripts/enrich_dataset.py` (subprocess call)
- Produces: `benchmark enrich` Typer command

- [ ] **Step 1: Add enrich command**

In `src/benchmark/cli/benchmark.py`, add after existing imports and before `# ── run command`:

```python
# ── enrich command ──────────────────────────────────────────────────────

@app.command()
def enrich(
    json_styles: Annotated[Path, typer.Option("--json-styles",
        help="Directory of per-product JSON files from the full Kaggle dataset.",
        exists=True, file_okay=False, dir_okay=True)] = ...,
    csv: Annotated[Path, typer.Option("--csv",
        help="CSV with id, subCategory, baseColour columns.",
        exists=True, file_okay=True, dir_okay=False)] = ...,
    output: Annotated[Path, typer.Option("--output", "-o",
        help="Output directory for enriched dataset.")] = Path("data/raw/fashion-enriched"),
    subset: Annotated[int, typer.Option("--subset",
        help="Limit to first N products (0 = all).")] = 0,
    folds: Annotated[int, typer.Option("--folds",
        help="Number of cross-validation folds.")] = MAGIC.N_FOLDS_DEFAULT,
    seed: Annotated[int, typer.Option("--seed",
        help="Random seed.")] = MAGIC.SEED,
) -> None:
    """Enrich dataset with visual attributes from JSON articleAttributes.

    Merges the CSV product metadata with per-product JSON files from the
    full Kaggle Fashion Product Images dataset, extracting
    ``articleAttributes.Pattern`` for a secondary (pattern-aware) relevance
    scheme.  Produces dual-label split JSON files with both ``label``
    (primary) and ``label_pattern`` (secondary) fields.

    Example::

        uv run benchmark enrich \\
            --json-styles data/raw/fashion-product-images/styles/ \\
            --csv data/raw/fashion-product-images-small/styles.csv \\
            --output data/raw/fashion-enriched-5k \\
            --subset 5000
    """
    import subprocess, sys

    cmd = [
        sys.executable, str(Path(__file__).resolve().parent.parent.parent.parent / "scripts" / "enrich_dataset.py"),
        "--json-styles", str(json_styles),
        "--csv", str(csv),
        "--output", str(output),
        "--subset", str(subset),
        "--folds", str(folds),
        "--seed", str(seed),
    ]
    result = subprocess.run(cmd, capture_output=False)
    if result.returncode != 0:
        raise typer.Exit(code=EXIT.EXIT_FAILURE)
```

Note: The path resolution `Path(__file__).resolve().parent.parent.parent.parent` walks up from `src/benchmark/cli/benchmark.py` (4 levels) to the project root `benchmarks/`.

- [ ] **Step 2: Verify CLI help**

```bash
uv run benchmark enrich --help
```
Expected: shows `--json-styles`, `--csv`, `--output`, `--subset`, `--folds`, `--seed` options.

- [ ] **Step 3: Run enrich command**

```bash
uv run benchmark enrich \
    --json-styles data/raw/fashion-product-images/styles/ \
    --csv data/raw/fashion-product-images-small/styles.csv \
    --output data/raw/fashion-enriched-5k \
    --subset 5000
```

Expected: creates `data/raw/fashion-enriched-5k/styles.csv` and `data/raw/fashion-enriched-5k/splits/*.json`.

- [ ] **Step 4: Commit**

```bash
git add src/benchmark/cli/benchmark.py
git commit -m "feat: add benchmark enrich CLI command"
```

---

### Task 5: Add secondary label evaluation to `ThesisRunner`

**Files:**
- Modify: `src/benchmark/evaluation/thesis.py`
- Create: `src/tests/evaluation/test_thesis_secondary.py`

**Interfaces:**
- Consumes: `FashionDataset.load(label_field=...)` from Task 2
- Produces: `ThesisRunner(secondary_label=None)` — silent backward-compatible change
- New method: `run_secondary(model_keys, results_dir)` → writes `thesis_results_pattern.json`

- [ ] **Step 1: Write failing test**

Create `src/tests/evaluation/test_thesis_secondary.py`:

```python
import json
from pathlib import Path
from unittest.mock import MagicMock, patch
import numpy as np
import pytest
from benchmark.evaluation.thesis import ThesisRunner


@pytest.fixture
def dual_label_dataset(tmp_path: Path) -> Path:
    root = tmp_path / "ds"
    root.mkdir()
    import pandas as pd
    df = pd.DataFrame({
        "id": [str(i) for i in range(20)],
        "masterCategory": ["A"] * 10 + ["B"] * 10,
        "subCategory": ["X"] * 20,
        "baseColour": ["Black"] * 20,
    })
    df.to_csv(root / "styles.csv", index=False)

    # Create dual-label split files
    splits_dir = root / "splits"
    splits_dir.mkdir()
    for fold in range(3):
        for split_name, ids in [("train", [str(i) for i in range(14)]),
                                  ("test", [str(i) for i in range(14, 20)])]:
            samples = [{
                "image_path": f"images/{pid}.jpg",
                "label": f"X/Black",
                "label_pattern": f"X/Black/Solid",
                "product_id": pid,
            } for pid in ids]
            (splits_dir / f"fold_{fold}_{split_name}.json").write_text(json.dumps(samples))
    return root


def test_thesis_runner_secondary_label_writes_pattern_file(dual_label_dataset: Path, tmp_path: Path):
    fake_model = MagicMock()
    fake_model.name = "Fake"
    fake_model.slug = "fake"
    fake_model.load = MagicMock()
    fake_model.embed_batch = MagicMock(return_value=np.random.rand(2, 64).astype(np.float32))
    fake_model.embed = MagicMock(return_value=np.random.rand(64).astype(np.float32))

    output_dir = tmp_path / "output"
    with patch("benchmark.evaluation.thesis.get_registry") as mock_reg:
        mock_reg.return_value = {"fake": fake_model}
        runner = ThesisRunner(
            dataset_root=dual_label_dataset,
            output_dir=output_dir,
            folds=3, seed=42, device="cpu",
            use_cache=False, batch_size=2,
            secondary_label="label_pattern",
        )
        results = runner.run(model_keys=["fake"])

    assert len(results) == 1
    # Primary results
    primary = output_dir / "results" / "thesis_results.json"
    assert primary.exists()
    primary_data = json.loads(primary.read_text())
    assert len(primary_data) == 1

    # Secondary results
    secondary = output_dir / "results" / "thesis_results_pattern.json"
    assert secondary.exists()
    secondary_data = json.loads(secondary.read_text())
    assert len(secondary_data) == 1
    assert secondary_data[0]["model_name"] == "Fake"
```

- [ ] **Step 2: Run test to verify it fails**

```bash
uv run pytest src/tests/evaluation/test_thesis_secondary.py -v
```
Expected: FAIL — `TypeError: ThesisRunner.__init__() got an unexpected keyword argument 'secondary_label'`

- [ ] **Step 3: Implement secondary label support in ThesisRunner**

In `src/benchmark/evaluation/thesis.py`, modify `__init__`:

```python
class ThesisRunner:
    """Evaluates thesis models with k-fold cross-validation."""

    def __init__(
        self,
        dataset_root: Path,
        output_dir: Path = Path("outputs/thesis"),
        k_values: list[int] | None = None,
        folds: int = MAGIC.N_FOLDS_DEFAULT,
        seed: int = MAGIC.SEED,
        device: str = "auto",
        use_cache: bool = True,
        batch_size: int = MAGIC.BATCH_SIZE,
        secondary_label: str | None = None,  # NEW
    ) -> None:
        self.dataset_root = dataset_root
        self.output_dir = output_dir
        self.k_values = k_values or list(MAGIC.DEFAULT_THESIS_K_VALUES)
        self.folds = folds
        self.seed = seed
        self.device = device
        self.use_cache = use_cache
        self.batch_size = batch_size
        self._secondary_label = secondary_label  # NEW
        self._registry = get_registry(device=device)
```

Then add a private method `_run_with_label_field` that mirrors the core evaluation loop but loads datasets with a specific `label_field`:

```python
def _run_with_label_field(
    self,
    keys: list[str],
    splits: list[tuple[Path, Path]],
    label_field: str,
) -> list[dict[str, Any]]:
    """Evaluate all models using a specific label field from split JSON."""
    results: list[dict[str, Any]] = []
    for key in keys:
        if key not in self._registry:
            logger.error("Model %s not in registry, skipping", key)
            continue
        model = self._registry[key]
        model_result = self._evaluate_model_with_field(
            model, splits, label_field,
        )
        results.append(model_result)
    return results
```

And `_evaluate_model_with_field` — identical to `_evaluate_model` but passes `label_field` to `_evaluate_fold`:

```python
def _evaluate_model_with_field(
    self,
    model,
    splits: list[tuple[Path, Path]],
    label_field: str,
) -> dict[str, Any]:
    """Evaluate one model across all folds with a custom label field."""
    logger.info("Evaluating %s [%s] ...", model.name, label_field)

    fold_results: list[dict[str, Any]] = []
    fold_map_scores: list[float] = []

    t0 = time.perf_counter()
    model.load()
    load_time_ms = (time.perf_counter() - t0) * MAGIC.MS_CONVERSION

    for fold_idx, (train_path, test_path) in enumerate(splits):
        logger.info("  Fold %d ...", fold_idx)
        fold_result = self._evaluate_fold_with_field(
            model, train_path, test_path, fold_idx, load_time_ms, label_field,
        )
        fold_results.append(fold_result)
        fold_map_scores.append(fold_result["map"])

    # Aggregate
    aggregate: dict[str, dict[str, float]] = {}
    metric_keys = ["map", "precision@5", "precision@10", "precision@20",
                   "recall@5", "recall@10", "recall@20",
                   "latency_mean_ms", "throughput_per_sec",
                   "load_time_ms", "index_storage_mb", "ram_mb"]
    for mk in metric_keys:
        vals = [f[mk] for f in fold_results if mk in f]
        if vals:
            aggregate[mk] = aggregate_mean_std(vals)

    if len(fold_map_scores) >= 3:
        ci_lower, ci_upper = bootstrap_ci(fold_map_scores, seed=self.seed)
        aggregate["map"]["ci_95"] = [ci_lower, ci_upper]

    return {
        "model_name": model.name,
        "model_slug": model.slug,
        "folds": fold_results,
        "aggregate": aggregate,
    }
```

And `_evaluate_fold_with_field` — identical to `_evaluate_fold` but calls `ds.load(label_field=label_field)` on both datasets:

```python
def _evaluate_fold_with_field(
    self, model, train_path, test_path, fold_idx, load_time_ms, label_field,
) -> dict[str, Any]:
    query_ds = FashionDataset(
        dataset_root=self.dataset_root, split_file=test_path, split=SPLIT.TEST,
    )
    query_ds.load(label_field=label_field)
    gallery_ds = FashionDataset(
        dataset_root=self.dataset_root, split_file=train_path, split=SPLIT.TRAIN,
    )
    gallery_ds.load(label_field=label_field)

    # Generate embeddings
    query_gen = EmbeddingGenerator(
        model=model, dataset=query_ds,
        batch_size=self.batch_size, use_cache=self.use_cache,
    )
    gallery_gen = EmbeddingGenerator(
        model=model, dataset=gallery_ds,
        batch_size=self.batch_size, use_cache=self.use_cache,
    )
    query_result = query_gen.generate(dataset_name=f"fold_{fold_idx}_test")
    gallery_result = gallery_gen.generate(dataset_name=f"fold_{fold_idx}_train")

    evaluator = Evaluator(
        dataset=query_ds, k_values=self.k_values, measure_efficiency=False,
    )
    metrics = evaluator.evaluate_split(
        query_result=query_result, gallery_result=gallery_result,
        dataset_name=f"fold_{fold_idx}",
    )

    sample_images = self._load_sample_images(query_ds.samples, max_n=MAGIC.MAX_LATENCY_SAMPLES)
    latency_stats = measure_latency(model, sample_images,
                                     warmup_runs=MAGIC.WARMUP_RUNS, benchmark_runs=MAGIC.BENCHMARK_RUNS)
    throughput = measure_throughput(model, sample_images[:MAGIC.BATCH_SIZE],
                                     batch_size=MAGIC.BATCH_SIZE, num_batches=10)
    ram_mb = self._measure_peak_ram(model, sample_images[:MAGIC.BATCH_SIZE])
    total_storage_mb = query_result.embeddings.nbytes / CONST.BYTES_TO_MB

    return {
        "fold": fold_idx,
        "map": round(metrics.map_score, 4),
        "precision@5": round(metrics.precision.get(5, 0.0), 4),
        "precision@10": round(metrics.precision.get(10, 0.0), 4),
        "precision@20": round(metrics.precision.get(20, 0.0), 4),
        "recall@5": round(metrics.recall.get(5, 0.0), 4),
        "recall@10": round(metrics.recall.get(10, 0.0), 4),
        "recall@20": round(metrics.recall.get(20, 0.0), 4),
        "latency_mean_ms": round(latency_stats.mean, 2),
        "latency_std_ms": round(latency_stats.std, 2),
        "throughput_per_sec": round(throughput, 2),
        "load_time_ms": round(load_time_ms, 2),
        "index_storage_mb": round(total_storage_mb, 2),
        "ram_mb": round(ram_mb, 2),
    }
```

Modify `run()` to call secondary evaluation when `_secondary_label` is set, at the end after the primary loop:

```python
def run(
    self,
    model_keys: list[str] | None = None,
) -> list[dict[str, Any]]:
    keys = model_keys or THESIS_MODEL_KEYS
    logger.info("Starting thesis benchmark: %d models, %d folds", len(keys), self.folds)

    styles_csv = self.dataset_root / "styles.csv"
    if not styles_csv.exists():
        logger.error("styles.csv not found at %s", styles_csv)
        raise FileNotFoundError(f"styles.csv not found: {styles_csv}")

    df = pd.read_csv(styles_csv, on_bad_lines="warn")
    gt = GroundTruth(df, min_category_freq=MAGIC.MIN_CATEGORY_FREQ)
    splits = gt.generate_splits(
        n_splits=self.folds, seed=self.seed,
        output_dir=self.output_dir / "splits",
    )

    # Primary evaluation (always runs)
    results: list[dict[str, Any]] = []
    for key in keys:
        if key not in self._registry:
            logger.error("Model %s not in registry, skipping", key)
            continue
        model = self._registry[key]
        model_result = self._evaluate_model(model, splits)
        results.append(model_result)

    # Secondary evaluation (opt-in)
    if self._secondary_label:
        logger.info("Running secondary evaluation with label field: %s", self._secondary_label)
        secondary_results = self._run_with_label_field(keys, splits, self._secondary_label)
        results_dir = self.output_dir / "results"
        results_dir.mkdir(parents=True, exist_ok=True)
        secondary_path = results_dir / "thesis_results_pattern.json"
        secondary_path.write_text(json.dumps(secondary_results, indent=2))
        logger.info("Secondary results → %s", secondary_path)

    return results
```

- [ ] **Step 4: Run test to verify it passes**

```bash
uv run pytest src/tests/evaluation/test_thesis_secondary.py -v
```
Expected: PASS

- [ ] **Step 5: Run existing tests**

```bash
uv run pytest src/tests/evaluation/test_thesis.py -v
```
Expected: all existing tests pass (backward compatible — `secondary_label` defaults to None)

- [ ] **Step 6: Commit**

```bash
git add src/benchmark/evaluation/thesis.py src/tests/evaluation/test_thesis_secondary.py
git commit -m "feat: add secondary label evaluation to ThesisRunner"
```

---

### Task 6: Add `--secondary-label` flag to thesis CLI

**Files:**
- Modify: `src/benchmark/cli/benchmark.py:212-232` (thesis command signature)

- [ ] **Step 1: Add CLI flag**

In `src/benchmark/cli/benchmark.py`, add after the `seed` parameter in the thesis command signature (line 231):

```python
    secondary_label: Annotated[str | None, typer.Option("--secondary-label",
        help="Secondary label field for pattern-aware evaluation "
             "(e.g., 'label_pattern'). When set, a second evaluation pass "
             "produces thesis_results_pattern.json.")] = None,
```

Then add `secondary_label=secondary_label` to the `ThesisRunner(...)` constructor call (line 266-275):

```python
    runner = ThesisRunner(
        dataset_root=dataset_root,
        output_dir=output,
        k_values=top_k,
        folds=folds,
        seed=seed,
        device=device,
        use_cache=not no_cache,
        batch_size=batch_size,
        secondary_label=secondary_label,  # NEW
    )
```

- [ ] **Step 2: Verify CLI help**

```bash
uv run benchmark thesis --help
```
Expected: shows `--secondary-label TEXT` option.

- [ ] **Step 3: Run on enriched 5K dataset**

```bash
uv run benchmark thesis \
    --dataset-root data/raw/fashion-enriched-5k \
    --secondary-label label_pattern \
    --folds 3 --seed 42 --device cpu
```

Expected: produces `outputs/thesis/results/thesis_results.json` AND `outputs/thesis/results/thesis_results_pattern.json`.

- [ ] **Step 4: Verify both files have different results**

```bash
uv run python3 -c "
import json
p = json.load(open('outputs/thesis/results/thesis_results.json'))
s = json.load(open('outputs/thesis/results/thesis_results_pattern.json'))
print(f'Primary mAP:   {p[0][\"aggregate\"][\"map\"][\"mean\"]:.4f}')
print(f'Secondary mAP: {s[0][\"aggregate\"][\"map\"][\"mean\"]:.4f}')
"
```

Expected: different mAP values (secondary should be lower because pattern matching is stricter).

- [ ] **Step 5: Commit**

```bash
git add src/benchmark/cli/benchmark.py
git commit -m "feat: add --secondary-label flag to thesis CLI"
```

---

### Task 7: Full test suite + lint

- [ ] **Step 1: Run all tests**

```bash
cd benchmarks && uv run pytest --ignore=src/tests/integration/ -q
```

Expected: all tests pass.

- [ ] **Step 2: Lint**

```bash
uv run ruff check src/
```

Fix any violations.

- [ ] **Step 3: Commit any lint fixes**

```bash
git commit -am "chore: lint fixes"
```

---

### Task 8: Update documentation

**Files:**
- Modify: `benchmarks/docs/09-visual-similarity-attributes.md`
- Modify: `benchmarks/docs/06-thesis-protocol.md`
- Modify: `benchmarks/docs/05-datasets.md`
- Modify: `benchmarks/docs/10-benchmark-comparison.md`
- Create: `benchmarks/docs/11-enriched-dataset.md`

- [ ] **Step 1: Add pattern attribute section to attribute doc**

In `benchmarks/docs/09-visual-similarity-attributes.md`, add after the colour normalization section (§4):

```markdown
## 2.8 Pattern Attribute from Per-Product JSON

The full Fashion Product Images dataset includes 44,446 per-product JSON files
(`styles/{product_id}.json`) containing a nested `articleAttributes` object.
The `Pattern` key holds one of:

| Pattern | Coverage | Visual Meaning |
|---------|----------|---------------|
| Solid | 36% | Single colour, no visible pattern |
| Printed | 26% | Graphic/text print |
| Checked | 16% | Checked/plaid pattern |
| Striped | 15% | Horizontal/vertical stripes |
| Self Design | 5% | Subtle tone-on-tone pattern |
| Unknown | 52% | No pattern data available |

Pattern is the highest-coverage, highest-visual-impact attribute beyond colour.
A checked shirt is visually very different from a solid shirt of the same colour.

The `benchmark enrich` command merges these JSON attributes with the CSV metadata,
producing dual-label split files with both `label` (primary: subCategory/colour)
and `label_pattern` (secondary: subCategory/colour/pattern).  See
[11 — Enriched Dataset](11-enriched-dataset.md) for usage.
```

- [ ] **Step 2: Add dual-label section to thesis protocol**

In `benchmarks/docs/06-thesis-protocol.md`, add after the "Results Template" section:

```markdown
### Dual-Label Evaluation

When using the enriched dataset (`benchmark enrich`), pass `--secondary-label label_pattern`
to run a second evaluation pass comparing subCategory/colour vs subCategory/colour/pattern:

```bash
uv run benchmark thesis \
    --dataset-root data/raw/fashion-enriched-5k \
    --secondary-label label_pattern \
    --folds 3
```

This produces two result files:
- `thesis_results.json` — primary evaluation (category + colour)
- `thesis_results_pattern.json` — secondary evaluation (category + colour + pattern)

The secondary evaluation reuses model embeddings from the primary pass
(no re-inference).  Comparing the two reveals whether model rankings
change when pattern awareness is required.
```

- [ ] **Step 3: Add enriched dataset section**

In `benchmarks/docs/05-datasets.md`, add a new section:

```markdown
### Enriched Dataset (JSON Attributes)

To unlock visual attributes beyond the CSV (Pattern, Sleeve Length, Fabric), use:

```bash
uv run benchmark enrich \
    --json-styles data/raw/fashion-product-images/styles/ \
    --csv data/raw/fashion-product-images-small/styles.csv \
    --output data/raw/fashion-enriched-5k \
    --subset 5000
```

This produces `fashion-enriched-5k/` with `styles.csv` (enriched), `splits/`
(dual-label JSON), and an `images/` symlink.  See [11 — Enriched Dataset](11-enriched-dataset.md).
```

- [ ] **Step 4: Create enriched dataset guide**

Create `benchmarks/docs/11-enriched-dataset.md`:

```markdown
# 11 — Enriched Dataset

Complete guide for enriching the benchmark dataset with visual attributes
from per-product JSON metadata.

## Prerequisites

Download the **full** Fashion Product Images dataset from Kaggle
([paramaggarwal/fashion-product-images-dataset](https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-dataset)).
Extract it so you have:

```
data/raw/fashion-product-images/
├── images/          # 44K+ JPEG product photos
└── styles/          # 44K+ per-product JSON files
    ├── 10000.json
    ├── 10001.json
    └── ...
```

You also need the small CSV dataset (`fashion-product-images-small/styles.csv`).

## Enrichment

```bash
uv run benchmark enrich \
    --json-styles data/raw/fashion-product-images/styles/ \
    --csv data/raw/fashion-product-images-small/styles.csv \
    --output data/raw/fashion-enriched-5k \
    --subset 5000
```

## Output Structure

```
fashion-enriched-5k/
├── styles.csv              # Enriched: id, masterCategory, …, pattern
├── images/                 # → symlink to original images
└── splits/
    ├── fold_0_test.json     # { "label": "Topwear/Blue",
    │                          "label_pattern": "Topwear/Blue/Checked" }
    ├── fold_0_train.json
    ├── ...
```

## Running Dual-Label Evaluation

```bash
uv run benchmark thesis \
    --dataset-root data/raw/fashion-enriched-5k \
    --secondary-label label_pattern \
    --folds 3 --seed 42 --device cpu
```

## Extracting Other Attributes

The enrichment script extracts `articleAttributes.Pattern` by default.
To add Sleeve Length, Fabric, or Fit in the future, modify
`scripts/enrich_dataset.py::extract_pattern()` to also extract those keys.
Each new attribute adds a new `label_<attr>` field to the split JSON.
```

- [ ] **Step 5: Commit docs**

```bash
git add benchmarks/docs/
git commit -m "docs: enriched dataset + dual-label documentation"
```

---

