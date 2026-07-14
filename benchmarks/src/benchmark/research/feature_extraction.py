"""Research feature extraction helpers that build on the benchmark generator."""
from __future__ import annotations

import json
from dataclasses import asdict
from pathlib import Path

import numpy as np

from benchmark.embeddings.generator import EmbeddingGenerator, EmbeddingResult
from benchmark.models import get_registry
from benchmark.research.datasets import ResearchDataset
from benchmark.utils.logging import get_logger

logger = get_logger("research.feature_extraction")


def extract_and_save_features(
    model_key: str,
    dataset_root: Path,
    output_dir: Path = Path("outputs/research"),
    train_split_file: Path | None = None,
    test_split_file: Path | None = None,
    combined_split_file: Path | None = None,
    batch_size: int = 64,
    device: str = "auto",
    use_cache: bool = True,
    dataset_name: str = "research",
) -> Path:
    """Extract embeddings for a research dataset and write a feature bundle."""
    registry = get_registry(device=device)
    if model_key not in registry:
        raise ValueError(f"Unknown model key: {model_key}")

    dataset = ResearchDataset(
        dataset_root=dataset_root,
        train_split_file=train_split_file,
        test_split_file=test_split_file,
        combined_split_file=combined_split_file,
    )
    dataset.load()

    model = registry[model_key]
    generator = EmbeddingGenerator(
        model=model,
        dataset=dataset,
        batch_size=batch_size,
        use_cache=use_cache,
    )
    result = generator.generate(dataset_name=dataset_name)
    return save_research_features(result, output_dir=output_dir)


def save_research_features(result: EmbeddingResult, output_dir: Path) -> Path:
    output_dir = output_dir / "features"
    output_dir.mkdir(parents=True, exist_ok=True)

    ids = [sample.product_id for sample in result.samples]
    labels = [sample.label for sample in result.samples]
    splits = [sample.split for sample in result.samples]
    image_paths = [str(sample.image_path) for sample in result.samples]
    metadata = [
        {
            "product_id": sample.product_id,
            "label": sample.label,
            "split": sample.split,
            "image_path": str(sample.image_path),
        }
        for sample in result.samples
    ]

    output_path = output_dir / f"{result.model_slug}_features.npz"
    np.savez_compressed(
        output_path,
        embeddings=result.embeddings,
        ids=np.array(ids, dtype=str),
        labels=np.array(labels, dtype=str),
        splits=np.array(splits, dtype=str),
        image_paths=np.array(image_paths, dtype=str),
    )

    metadata_path = output_dir / f"{result.model_slug}_features.json"
    metadata_path.write_text(json.dumps(metadata, indent=2), encoding="utf-8")

    logger.info("Saved research features to %s", output_path)
    return output_path


def load_research_features(path: Path) -> tuple[np.ndarray, np.ndarray, np.ndarray, np.ndarray]:
    data = np.load(path, allow_pickle=False)
    return (
        data["embeddings"],
        data["ids"],
        data["labels"],
        data["splits"],
    )
