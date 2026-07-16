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
        "baseColour": ["Red", "Red", "Blue", "Blue"],
    })
    relevance = build_relevance_sets(df)
    assert relevance["1"] == {"2"}
    assert relevance["2"] == {"1"}
    assert relevance["3"] == {"4"}
    assert relevance["4"] == {"3"}


def test_build_relevance_with_base_colour():
    """Different colours within same category should NOT be relevant."""
    df = pd.DataFrame({
        "id": ["1", "2", "3"],
        "masterCategory": ["A", "A", "A"],
        "subCategory": ["X", "X", "X"],
        "baseColour": ["Red", "Blue", "Red"],
    })
    relevance = build_relevance_sets(df)
    assert relevance["1"] == {"3"}  # "A/X/Red"
    assert relevance["3"] == {"1"}  # "A/X/Red"
    assert relevance["2"] == set()  # "A/X/Blue" — no other blue items


def test_build_relevance_fallback_to_master_only():
    df = pd.DataFrame({
        "id": ["1", "2", "3"],
        "masterCategory": ["A", "A", "A"],
        "subCategory": ["X", pd.NA, "X"],
    })
    relevance = build_relevance_sets(df)
    # Item 2 has NaN subCategory; no baseColour → falls back to masterCategory only ("A")
    # Items 1 and 3 have subCategory "X" but no baseColour → fall back to "A/X"
    assert relevance["2"] == set()  # no other item has pure masterCategory "A"
    # Items 1 and 3 share "A/X"
    assert relevance["1"] == {"3"}
    assert relevance["3"] == {"1"}


def test_generate_splits(tmp_path: Path):
    df = pd.DataFrame({
        "id": [f"{i}" for i in range(30)],
        "masterCategory": ["A"] * 15 + ["B"] * 15,
        "subCategory": ["X"] * 30,
        "baseColour": ["Black"] * 30,
    })
    gt = GroundTruth(df, min_category_freq=5)
    splits = gt.generate_splits(n_splits=3, seed=42, output_dir=tmp_path)
    assert len(splits) == 3
    for _fold_idx, (train_path, test_path) in enumerate(splits):
        train = json.loads(train_path.read_text())
        test = json.loads(test_path.read_text())
        assert len(train) > 0
        assert len(test) > 0
        # No overlap
        train_ids = {s["product_id"] for s in train}
        test_ids = {s["product_id"] for s in test}
        assert not train_ids & test_ids
        # Labels now include baseColour
        for s in train + test:
            assert s["label"] in ("A/X/Black", "B/X/Black")


def test_ground_truth_missing_id_column():
    df = pd.DataFrame({"masterCategory": ["A"]})
    with pytest.raises(ValueError, match="styles.csv must contain an 'id' column"):
        GroundTruth(df)


def test_build_sample_meta_with_pattern():
    from benchmark.datasets.ground_truth import GroundTruth, normalize_colour

    norm = normalize_colour("Navy Blue")
    row = pd.Series({
        "id": "123",
        "masterCategory": "Apparel",
        "subCategory": "Tshirts",
        "baseColour": "Navy Blue",
        "_norm_colour": norm,
        "pattern": "Solid",
    })
    meta = GroundTruth._build_sample_meta(row, has_pattern=True)
    assert meta["label"] == f"Apparel/Tshirts/{norm}"
    assert meta["label_pattern"] == f"Apparel/Tshirts/{norm}/Solid"
    assert meta["product_id"] == "123"
    assert meta["image_path"] == "images/123.jpg"


def test_build_sample_meta_pattern_unknown_fallback():
    row = pd.Series({
        "id": "456",
        "masterCategory": "Footwear",
        "subCategory": "Sneakers",
        "baseColour": "Black",
        "_norm_colour": "Black",
        "pattern": "Unknown",
    })
    meta = GroundTruth._build_sample_meta(row, has_pattern=True)
    assert meta["label_pattern"] == meta["label"]


def test_build_sample_meta_no_pattern_column():
    row = pd.Series({
        "id": "789",
        "masterCategory": "Accessories",
        "subCategory": "Belts",
        "baseColour": "Brown",
        "_norm_colour": "Brown/Yellow",
    })
    meta = GroundTruth._build_sample_meta(row, has_pattern=False)
    assert "label_pattern" not in meta
    assert "label" in meta
