"""Centralized constants for the benchmark codebase.

All hardcoded values — magic numbers, string keys, configuration defaults,
path patterns, constraints, and result codes — are declared here before use.
Never inline a value that appears more than once or represents a tunable
parameter.

Categories
----------
MagicNumbers   — numeric literals (batch sizes, seeds, K cut-offs, etc.)
Strings        — string literals (metric keys, split names, CLI sentinels)
Defaults       — default paths, flags, log levels, connection strings
Patterns       — file name templates, glob patterns, format strings
Constraints    — bounds, limits, thresholds, dimension caps
ResultCodes    — exit codes, status codes, error codes
"""
from __future__ import annotations

from dataclasses import dataclass, field
from pathlib import Path


@dataclass(frozen=True)
class MagicNumbers:
    SEED: int = 42
    BATCH_SIZE: int = 64
    DEFAULT_K_VALUES: list[int] = field(default_factory=lambda: [1, 5, 10, 20])
    DEFAULT_THESIS_K_VALUES: list[int] = field(default_factory=lambda: [5, 10, 20])
    WARMUP_RUNS: int = 10
    BENCHMARK_RUNS: int = 100
    MAX_LATENCY_SAMPLES: int = 200
    MS_CONVERSION: float = 1000.0
    MIN_CATEGORY_FREQ: int = 10
    BOOTSTRAP_CONFIDENCE: float = 0.95
    BOOTSTRAP_RESAMPLES: int = 10_000
    N_FOLDS_DEFAULT: int = 3
    METRIC_DECIMALS: int = 4
    LATENCY_DECIMALS: int = 1
    N_QUANTILES: int = 100
    P50_INDEX: int = 49
    P95_INDEX: int = 94
    P99_INDEX: int = 98


@dataclass(frozen=True)
class FAISS:
    N_LISTS: int = 100
    N_PROBE: int = 10
    IVFFLAT_MIN_FACTOR: int = 39


@dataclass(frozen=True)
class Chart:
    FIG_SIZE_PRECISION: tuple = (7, 4.5)
    FIG_SIZE_LATENCY: tuple = (8, 4.5)
    PNG_DPI: int = 150
    BAR_WIDTH: float = 0.25
    MAP_X_LIMIT_MULTIPLIER: float = 1.15
    MAP_X_LIMIT_ABS: float = 1.05


@dataclass(frozen=True)
class Strings:
    MAP: str = "map"
    PRECISION: str = "precision"
    RECALL: str = "recall"
    NDCG: str = "ndcg"
    LATENCY: str = "latency"
    P50_MS: str = "p50_ms"
    P95_MS: str = "p95_ms"
    P99_MS: str = "p99_ms"
    MEAN_MS: str = "mean_ms"
    STD_MS: str = "std_ms"
    MIN_MS: str = "min_ms"
    MAX_MS: str = "max_ms"
    N_SAMPLES: str = "n_samples"
    ELAPSED_MS: str = "elapsed_ms"
    MODEL: str = "model"
    DATASET: str = "dataset"
    LATENCY_MS: str = "latency_ms"
    THROUGHPUT: str = "throughput_per_sec"
    LABEL: str = "label"


@dataclass(frozen=True)
class SPLITS:
    TRAIN: str = "train"
    TEST: str = "test"
    VAL: str = "val"
    OTHER: str = "Other"


@dataclass(frozen=True)
class CLI:
    ALL: str = "all"
    AUTO: str = "auto"
    LIST: str = "list"
    STATS: str = "stats"
    CLEAR: str = "clear"
    CSV: str = "csv"
    JSON: str = "json"
    MARKDOWN: str = "markdown"
    TYPST: str = "typst"
    CHARTS: str = "charts"
    CPU: str = "cpu"
    CUDA: str = "cuda"
    MPS: str = "mps"


@dataclass(frozen=True)
class DATASET_FIELDS:
    ID: str = "id"
    IMAGE_PATH: str = "image_path"
    LABEL: str = "label"
    PRODUCT_ID: str = "product_id"
    SPLIT: str = "split"
    MASTER_CATEGORY: str = "masterCategory"
    SUB_CATEGORY: str = "subCategory"
    BASE_COLOUR: str = "baseColour"
    SECONDARY_LABEL: str = "label_pattern"
    PATTERN: str = "pattern"


@dataclass(frozen=True)
class PLACEHOLDERS:
    MISSING_TYPST: str = "---"
    MISSING_MD: str = "—"
    BEST_MODEL_FALLBACK: str = "N/A"


THESIS_MODEL_KEYS: list[str] = [
    "fashion_clip", "resnet50", "efficientnet_b0", "clip_generic",
]

FILE_ENCODING: str = "utf-8"

PALETTE: list[str] = [
    "#E63946",
    "#457B9D",
    "#1D3557",
    "#2A9D8F",
    "#E9C46A",
]


@dataclass(frozen=True)
class Defaults:
    DATASET_ROOT: Path = field(default=Path("data/raw/deepfashion"))
    SPLIT_FILE: Path = field(default=Path("data/splits/deepfashion/test.json"))
    CACHE_DIR: Path = field(default=Path("data/cache"))
    OUTPUTS_ROOT: Path = field(default=Path("outputs"))
    METRICS_DIR: Path = field(default=Path("outputs/metrics"))
    REPORTS_DIR: Path = field(default=Path("outputs/reports"))
    TABLES_DIR: Path = field(default=Path("outputs/tables"))
    FIGURES_DIR: Path = field(default=Path("outputs/figures"))
    THESIS_DIR: Path = field(default=Path("outputs/thesis"))
    PIPELINE_DIR: Path = field(default=Path("outputs/pipeline"))
    EMBEDDINGS_DIR: Path = field(default=Path("outputs/embeddings"))
    SPLITS_DIR: Path = field(default=Path("outputs/thesis/splits"))
    DATASET_NAME: str = "deepfashion"
    LOG_LEVEL: str = "INFO"
    ROOT_LOGGER: str = "benchmark"
    CONN_STRING: str = "postgresql://benchmark:benchmark@localhost:5432/benchmark"
    USE_CACHE: bool = True


@dataclass(frozen=True)
class Patterns:
    CACHE_NPZ: str = "{model_slug}__{dataset_name}.npz"
    STORAGE_NPZ: str = "{model_slug}.npz"
    FOLD_TRAIN: str = "fold_{fold_idx}_train.json"
    FOLD_TEST: str = "fold_{fold_idx}_test.json"
    IMAGE_PATH: str = "images/{product_id}.jpg"
    PER_MODEL_JSON: str = "{slug}.json"
    NPZ_GLOB: str = "*.npz"
    JSON_GLOB: str = "*.json"


@dataclass(frozen=True)
class OutputFiles:
    COMPARISON_JSON: str = "benchmark.json"
    CSV: str = "benchmark.csv"
    MARKDOWN: str = "summary.md"
    THESIS_RESULTS: str = "thesis_results.json"
    PIPELINE_RESULTS: str = "pipeline_results.json"
    PRECISION_TYP: str = "precision.typ"
    RECALL_TYP: str = "recall.typ"
    NDCG_TYP: str = "ndcg.typ"
    LATENCY_TYP: str = "latency.typ"
    MAP_SUMMARY_TYP: str = "map_summary.typ"
    THESIS_AGGREGATE_TYP: str = "thesis_aggregate.typ"
    THESIS_EFFICIENCY_TYP: str = "thesis_efficiency.typ"
    PIPELINE_PRODUCTION_TYP: str = "pipeline_production.typ"


@dataclass(frozen=True)
class LogFiles:
    RUN: str = "benchmark.log"
    THESIS: str = "thesis.log"
    PIPELINE: str = "pipeline.log"


@dataclass(frozen=True)
class Constraints:
    MAX_LATENCY_SAMPLES: int = 200
    MIN_CATEGORY_FREQ: int = 10
    IVFFLAT_MIN_FACTOR: int = 39
    MIN_FOLDS_FOR_BOOTSTRAP: int = 3
    BYTES_TO_MB: float = 1024.0 * 1024.0


@dataclass(frozen=True)
class ResultCodes:
    EXIT_FAILURE: int = 1
    HTTP_OK: int = 200
    HTTP_BAD_REQUEST: int = 400
    HTTP_UNAUTHORIZED: int = 401
    HTTP_FORBIDDEN: int = 403
    HTTP_NOT_FOUND: int = 404
    HTTP_CONFLICT: int = 409
    HTTP_INTERNAL_ERROR: int = 500


MAGIC = MagicNumbers()
STR = Strings()
SPLIT = SPLITS()
CLI_STR = CLI()
FIELD = DATASET_FIELDS()
PLACEHOLDER = PLACEHOLDERS()
DFLT = Defaults()
PAT = Patterns()
OUT = OutputFiles()
LOG = LogFiles()
CONST = Constraints()
EXIT = ResultCodes()
FAISS_PARAMS = FAISS()
CHART = Chart()

__all__ = [
    "MAGIC", "STR", "SPLIT", "CLI_STR", "FIELD", "PLACEHOLDER",
    "DFLT", "PAT", "OUT", "LOG", "CONST", "EXIT", "FAISS_PARAMS", "CHART",
    "THESIS_MODEL_KEYS", "FILE_ENCODING", "PALETTE",
    "MagicNumbers", "Strings", "SPLITS", "CLI", "DATASET_FIELDS", "PLACEHOLDERS",
    "Defaults", "Patterns", "OutputFiles", "LogFiles", "Constraints", "ResultCodes",
    "FAISS", "Chart",
]
