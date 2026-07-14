import json
from pathlib import Path

import pandas as pd
import pytest

from benchmark.datasets.ground_truth import GroundTruth, build_relevance_sets


def test_build_relevance_sets():
    df = pd.DataFrame({
        "id": ["1", "2", "3", "4"],
        "masterCategory": ["A", "A", "B", "B"],
        "subCategory": ["X", "X", "Y", "Y"],
    })
    relevance = build_relevance_sets(df)
    assert relevance["1"] == {"2"}
    assert relevance["2"] == {"1"}
    assert relevance["3"] == {"4"}
    assert relevance["4"] == {"3"}


def test_build_relevance_fallback_to_master_only():
    df = pd.DataFrame({
        "id": ["1", "2", "3"],
        "masterCategory": ["A", "A", "A"],
        "subCategory": ["X", pd.NA, "X"],
    })
    relevance = build_relevance_sets(df)
    # Item 2 has NaN subCategory, so falls back to masterCategory only ("A")
    # Items 1 and 3 have subCategory "X", so their key is "A/X"
    # Therefore item 2 is only relevant to itself (excluded) — no other item has key "A"
    assert relevance["2"] == set()  # no other item has pure masterCategory "A"
    # Items 1 and 3 share "A/X"
    assert relevance["1"] == {"3"}
    assert relevance["3"] == {"1"}


def test_generate_splits(tmp_path: Path):
    df = pd.DataFrame({
        "id": [f"{i}" for i in range(30)],
        "masterCategory": ["A"] * 15 + ["B"] * 15,
        "subCategory": ["X"] * 30,
    })
    gt = GroundTruth(df, min_category_freq=5)
    splits = gt.generate_splits(n_splits=3, seed=42, output_dir=tmp_path)
    assert len(splits) == 3
    for fold_idx, (train_path, test_path) in enumerate(splits):
        train = json.loads(train_path.read_text())
        test = json.loads(test_path.read_text())
        assert len(train) > 0
        assert len(test) > 0
        # No overlap
        train_ids = {s["product_id"] for s in train}
        test_ids = {s["product_id"] for s in test}
        assert not train_ids & test_ids


def test_ground_truth_missing_id_column():
    df = pd.DataFrame({"masterCategory": ["A"]})
    with pytest.raises(ValueError, match="styles.csv must contain an 'id' column"):
        GroundTruth(df)
