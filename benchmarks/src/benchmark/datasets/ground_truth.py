"""Ground-truth builder and stratified split generator.

Parses styles.csv, builds relevance sets based on category + colour keys,
and generates k-fold stratified splits for the thesis benchmark protocol.

Edge cases:
- Missing or NaN subCategory / baseColour fall back to coarser grouping.
- Categories with fewer than ``min_category_freq`` samples are grouped into
  ``"Other"`` to avoid tiny strata.
- Self-relevance (a product being relevant to itself) is excluded from
  each relevance set.
"""
from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path

import numpy as np
import pandas as pd

from benchmark._constants import DFLT, MAGIC, PAT
from benchmark.utils.logging import get_logger

logger = get_logger("datasets.ground_truth")


def normalize_colour(raw: str | float | None) -> str:
    """Map a raw baseColour string to a broad visual colour group.

    The Fashion Product Images dataset uses 46 distinct colour labels
    (e.g. ``"Blue"``, ``"Navy Blue"``, ``"Turquoise Blue"``).  These
    differ in marketing terms but not in human visual perception — a
    user searching for a "Blue T-shirt" expects Navy Blue results too.

    Returns the raw string unchanged if it does not match any group.
    Missing/NaN values return ``"Unknown"``.
    """
    if pd.isna(raw) or not isinstance(raw, str) or not raw.strip():
        return "Unknown"
    c = raw.strip()

    if any(t in c.lower() for t in ("black", "charcoal")):
        return "Black"
    if any(t in c.lower() for t in ("white", "off white", "cream")):
        return "White"
    if any(t in c.lower() for t in ("blue", "navy", "turquoise", "teal", "aqua", "sea green", "sky")):
        return "Blue"
    if any(t in c.lower() for t in ("red", "maroon", "burgundy", "rust", "coral", "magenta", "rose", "mauve", "peach")):
        return "Red"
    if any(t in c.lower() for t in ("pink", "lavender")):
        return "Pink"
    if any(t in c.lower() for t in ("olive", "lime", "green", "khaki")):
        return "Green"
    if any(t in c.lower() for t in ("purple")):
        return "Purple"
    if any(t in c.lower() for t in ("grey", "gray", "silver", "charcoal")):
        return "Grey"
    if any(t in c.lower() for t in ("orange")):
        return "Orange"
    if any(t in c.lower() for t in ("multi")):
        return "Multi"
    if any(t in c.lower() for t in ("brown", "coffee", "tan", "beige", "taupe", "nude", "khaki", "mushroom", "copper", "bronze", "gold", "yellow", "mustard", "lemon")):
        return "Brown/Yellow"

    logger.debug("Unmapped colour label: %s", c)
    return c


def build_relevance_sets(df: pd.DataFrame) -> dict[str, set[str]]:
    """Build a relevance set for each product ID.

    Two products are relevant if they share the same masterCategory +
    subCategory + *normalised* colour (see :func:`normalize_colour`).
    If subCategory is missing/NaN, fall back to masterCategory only.

    Colour normalisation merges visually similar labels (e.g. ``"Blue"``,
    ``"Navy Blue"``, ``"Turquoise Blue"`` → ``"Blue"``), so the ground
    truth reflects perceptual visual similarity rather than exact
    marketing-label matching.  This is the sweet spot between the
    original category-only scheme (too broad) and raw-colour matching
    (too strict — 21 % of items had no colour-mate).

    Args:
        df: DataFrame with at least ``'id'``, ``'masterCategory'``,
            ``'subCategory'``, ``'baseColour'`` columns.

    Returns:
        Dict mapping product_id -> set of relevant product_ids (excluding
        self).
    """
    df = df.copy()
    if "baseColour" in df.columns:
        df["_norm_colour"] = df["baseColour"].apply(normalize_colour)
    else:
        df["_norm_colour"] = "Unknown"
    df["_relevance_key"] = df.apply(
        lambda row: (
            f"{row['masterCategory']}/{row['subCategory']}/{row['_norm_colour']}"
            if pd.notna(row.get("subCategory"))
            else str(row["masterCategory"])
        ),
        axis=1,
    )

    relevance: dict[str, set[str]] = {}
    grouped = df.groupby("_relevance_key")["id"].apply(set)
    for pid in df["id"]:
        key = df.loc[df["id"] == pid, "_relevance_key"].iloc[0]
        group = set(grouped.get(key, set()))
        group.discard(pid)
        relevance[str(pid)] = group

    return relevance


@dataclass
class GroundTruth:
    """Handles metadata loading, relevance building, and stratified splits.

    Attributes:
        df: Product metadata DataFrame with ``id`` and ``masterCategory``
            columns.
        min_category_freq: Minimum samples per category. Rare categories are
            grouped into ``"Other"``.
    """

    df: pd.DataFrame
    min_category_freq: int = MAGIC.MIN_CATEGORY_FREQ

    def __post_init__(self) -> None:
        """Validate input and group rare categories into ``"Other"``.

        Raises:
            ValueError: If the ``id`` column is missing from the DataFrame.
        """
        if "id" not in self.df.columns:
            raise ValueError("styles.csv must contain an 'id' column")
        # Group rare categories into "Other"
        counts = self.df["masterCategory"].value_counts()
        rare = counts[counts < self.min_category_freq].index
        if len(rare) > 0:
            self.df = self.df.copy()
            self.df.loc[self.df["masterCategory"].isin(rare), "masterCategory"] = "Other"
            logger.info("Grouped %d rare categories into 'Other'", len(rare))

    def generate_splits(
        self,
        n_splits: int = MAGIC.N_FOLDS_DEFAULT,
        seed: int = MAGIC.SEED,
        output_dir: Path = DFLT.SPLITS_DIR,
    ) -> list[tuple[Path, Path]]:
        """Generate stratified k-fold splits and save as JSON.

        Shuffles indices within each category stratum to preserve class
        distribution across folds, then writes train/test JSON files.

        Args:
            n_splits: Number of folds.
            seed: Random seed for reproducibility.
            output_dir: Where to write split JSON files.

        Returns:
            List of (train_path, test_path) tuples for each fold.
        """
        output_dir.mkdir(parents=True, exist_ok=True)
        rng = np.random.default_rng(seed)

        # Shuffle within each stratum then split
        categories = self.df["masterCategory"].unique()
        fold_indices: list[list[int]] = [[] for _ in range(n_splits)]

        for cat in categories:
            cat_df = self.df[self.df["masterCategory"] == cat].reset_index(drop=True)
            indices = cat_df.index.to_numpy().copy()
            rng.shuffle(indices)
            splits = np.array_split(indices, n_splits)
            for fold_idx, split in enumerate(splits):
                fold_indices[fold_idx].extend(cat_df.iloc[split]["id"].tolist())

        # Build full id -> metadata mapping
        self.df["_norm_colour"] = (self.df["baseColour"].apply(normalize_colour)
                                     if "baseColour" in self.df.columns
                                     else "Unknown")
        meta_by_id = {
            row["id"]: {
                "image_path": PAT.IMAGE_PATH.format(product_id=row['id']),
                "label": (
                    f"{row['masterCategory']}/{row['subCategory']}/{row['_norm_colour']}"
                    if pd.notna(row.get("subCategory"))
                    else str(row["masterCategory"])
                ),
                "product_id": str(row["id"]),
            }
            for _, row in self.df.iterrows()
        }

        all_ids = set(self.df["id"].tolist())
        result: list[tuple[Path, Path]] = []

        for fold_idx in range(n_splits):
            test_ids = set(fold_indices[fold_idx])
            train_ids = all_ids - test_ids

            train_samples = [meta_by_id[pid] for pid in sorted(train_ids) if pid in meta_by_id]
            test_samples = [meta_by_id[pid] for pid in sorted(test_ids) if pid in meta_by_id]

            train_path = output_dir / PAT.FOLD_TRAIN.format(fold_idx=fold_idx)
            test_path = output_dir / PAT.FOLD_TEST.format(fold_idx=fold_idx)
            train_path.write_text(json.dumps(train_samples, indent=2), encoding="utf-8")
            test_path.write_text(json.dumps(test_samples, indent=2), encoding="utf-8")
            result.append((train_path, test_path))
            logger.info(
                "Fold %d: train=%d, test=%d", fold_idx, len(train_samples), len(test_samples)
            )

        return result
