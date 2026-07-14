"""CLI: uv run benchmark research [COMMANDS]"""
from __future__ import annotations

from pathlib import Path
from typing import Annotated

import typer
from rich.console import Console

from benchmark.research.db import PgvectorBenchmark
from benchmark.research.evaluation import evaluate_features
from benchmark.research.feature_extraction import extract_and_save_features
from benchmark.research.reports import generate_research_report
from benchmark.utils.logging import setup_logging

app = typer.Typer(
    name="research",
    help="Research-focused commands for feature extraction, evaluation, and PGVector benchmarking.",
)
console = Console()


@app.command()
def extract_features(
    model: Annotated[str, typer.Option("--model", "-m", help="Model key from the benchmark registry.", show_default=True)] = "fashion-clip",
    dataset_root: Annotated[Path, typer.Option("--dataset-root", "-d", help="Root path for the dataset.")] = Path("data/raw/deepfashion"),
    train_split_file: Annotated[Path | None, typer.Option("--train-split-file", help="Train split JSON file.")] = None,
    test_split_file: Annotated[Path | None, typer.Option("--test-split-file", help="Test split JSON file.")] = None,
    combined_split_file: Annotated[Path | None, typer.Option("--combined-split-file", help="Combined split JSON file.")] = None,
    output: Annotated[Path, typer.Option("--output", "-o", help="Research output directory.", show_default=True)] = Path("outputs/research"),
    batch_size: Annotated[int, typer.Option("--batch-size", help="Batch size for embedding extraction.", show_default=True)] = 64,
    device: Annotated[str, typer.Option("--device", help="Device to run models on.", show_default=True)] = "auto",
    no_cache: Annotated[bool, typer.Option("--no-cache", help="Skip the embedding cache.")] = False,
    log_level: Annotated[str, typer.Option("--log-level", help="Logging level.", show_default=True)] = "INFO",
) -> None:
    """Extract research embeddings and save them as feature bundles."""
    setup_logging(level=log_level)
    path = extract_and_save_features(
        model_key=model,
        dataset_root=dataset_root,
        output_dir=output,
        train_split_file=train_split_file,
        test_split_file=test_split_file,
        combined_split_file=combined_split_file,
        batch_size=batch_size,
        device=device,
        use_cache=not no_cache,
    )
    console.print(f"[green]Saved research features to {path}[/green]")


@app.command()
def evaluate(
    features_file: Annotated[Path, typer.Option("--features-file", "-f", help="Feature bundle .npz file.")] = Path("outputs/research/features/fashion-clip_features.npz"),
    k: Annotated[str, typer.Option("--k", help="Comma-separated K values for evaluation.", show_default=True)] = "1,5,10",
    output: Annotated[Path, typer.Option("--output", "-o", help="Research report output directory.", show_default=True)] = Path("outputs/research/reports"),
    log_level: Annotated[str, typer.Option("--log-level", help="Logging level.", show_default=True)] = "INFO",
) -> None:
    """Evaluate a saved research feature bundle and generate summary reports."""
    setup_logging(level=log_level)
    ks = [int(x.strip()) for x in k.split(",") if x.strip()]
    metrics = evaluate_features(features_file, ks=ks)
    if "Model" not in metrics:
        metrics["Model"] = features_file.stem.replace("_features", "")
    paths = generate_research_report([metrics], output_dir=output)
    console.print(f"[green]Research evaluation complete. Reports written to {output}[/green]")
    for name, path in paths.items():
        console.print(f"  - {name}: {path}")


@app.command()
def pgvector_benchmark(
    conn_string: Annotated[str, typer.Option("--conn-string", help="PostgreSQL connection string.", show_default=True)] = "postgresql://research_user:research_password@localhost:5433/research_sandbox",
    model: Annotated[str, typer.Option("--model", help="Benchmark model key.", show_default=True)] = "fashion-clip",
    top_k: Annotated[int, typer.Option("--top-k", help="Number of results to query.", show_default=True)] = 10,
    num_queries: Annotated[int, typer.Option("--num-queries", help="Number of database queries to run.", show_default=True)] = 100,
    log_level: Annotated[str, typer.Option("--log-level", help="Logging level.", show_default=True)] = "INFO",
) -> None:
    """Run a PGVector latency benchmark for a model stored in Postgres."""
    setup_logging(level=log_level)
    with PgvectorBenchmark(conn_string=conn_string) as runner:
        stats = runner.benchmark_model(model, num_queries=num_queries, top_k=top_k)
    console.print(f"[green]PGVector benchmark complete for {model}[/green]")
    for key, value in stats.items():
        console.print(f"  {key}: {value}")


@app.command()
def validate_hnsw(
    conn_string: Annotated[str, typer.Option("--conn-string", help="PostgreSQL connection string.", show_default=True)] = "postgresql://research_user:research_password@localhost:5433/research_sandbox",
    model: Annotated[str, typer.Option("--model", help="Benchmark model key.", show_default=True)] = "fashion-clip",
    top_k: Annotated[int, typer.Option("--top-k", help="Number of results to compare.", show_default=True)] = 10,
    num_queries: Annotated[int, typer.Option("--num-queries", help="Number of queries to validate.", show_default=True)] = 50,
    log_level: Annotated[str, typer.Option("--log-level", help="Logging level.", show_default=True)] = "INFO",
) -> None:
    """Validate PGVector HNSW approximate search recall against exact query results."""
    setup_logging(level=log_level)
    with PgvectorBenchmark(conn_string=conn_string) as runner:
        stats = runner.validate_hnsw(model, num_queries=num_queries, top_k=top_k)
    console.print(f"[green]HNSW validation complete for {model}[/green]")
    for key, value in stats.items():
        console.print(f"  {key}: {value}")
