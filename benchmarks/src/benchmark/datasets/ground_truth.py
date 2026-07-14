"""Ground-truth builder and stratified split generator.

Parses styles.csv, builds relevance sets, and generates k-fold stratified
splits for the thesis benchmark protocol.
"""
from __future__ import annotations

import json
from dataclasses import dataclass
from pathlib import Path

import numpy as np
import pandas as pd

from benchmark.utils.logging import get_logger

logger = get_logger("datasets.ground_truth")


def build_relevance_sets(df: pd.DataFrame) -> dict[str, set[str]]:
    """Build a relevance set for each product ID.

    Two products are relevant if they share the same masterCategory +
    subCategory. If subCategory is missing/NaN, fall back to masterCategory
    only.

    Args:
        df: DataFrame with at least 'id', 'masterCategory', 'subCategory'.

    Returns:
        Dict mapping product_id -> set of relevant product_ids (excluding
        self).
    """
    df = df.copy()
    df["_relevance_key"] = df.apply(
        lambda row: (
            f"{row['masterCategory']}/{row['subCategory']}"
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
    """Handles metadata loading, relevance building, and stratified splits."""

    df: pd.DataFrame
    min_category_freq: int = 10

    def __post_init__(self) -> None:
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
        n_splits: int = 3,
        seed: int = 42,
        output_dir: Path = Path("outputs/thesis/splits"),
    ) -> list[tuple[Path, Path]]:
        """Generate stratified k-fold splits and save as JSON.

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
        meta_by_id = {
            row["id"]: {
                "image_path": f"images/{row['id']}.jpg",
                "label": (
                    f"{row['masterCategory']}/{row['subCategory']}"
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

            train_path = output_dir / f"fold_{fold_idx}_train.json"
            test_path = output_dir / f"fold_{fold_idx}_test.json"
            train_path.write_text(json.dumps(train_samples, indent=2), encoding="utf-8")
            test_path.write_text(json.dumps(test_samples, indent=2), encoding="utf-8")
            result.append((train_path, test_path))
            logger.info(
                "Fold %d: train=%d, test=%d", fold_idx, len(train_samples), len(test_samples)
            )

        return result
