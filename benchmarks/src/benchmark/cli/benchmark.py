"""CLI entry point — ``uv run benchmark <command> [OPTIONS]``.

Commands
--------
run         Run embedding + retrieval + metric evaluation for one or more models.
thesis      Run the thesis benchmark (4 models × 3-fold CV).
pipeline    Run the production pipeline benchmark (thesis + pgvector).
report      Generate JSON / CSV / Markdown / Typst reports from stored results.
cache       Inspect or clear the embedding cache.

Edge cases:
- Unknown model keys print error with available keys and exit with code 1.
- Missing image paths are logged as warnings but do not abort the run.
- Cache directory operations handle non-existent directories gracefully.
- Results directory not found during ``report`` exits with code 1.
"""
from __future__ import annotations

import json
from pathlib import Path
from typing import Annotated

import typer
from rich.console import Console
from rich.table import Table

from benchmark._constants import (
    CLI_STR,
    DFLT,
    EXIT,
    FAISS_PARAMS,
    FILE_ENCODING,
    LOG,
    MAGIC,
    PLACEHOLDER,
    SPLIT,
)
from benchmark.utils.logging import get_logger, setup_logging
from benchmark.utils.random_seed import set_seed

app = typer.Typer(
    name="benchmark",
    no_args_is_help=True,
    help="Fashion retrieval benchmark — compare image embedding models.",
)
console = Console()
logger = get_logger("cli")


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
    import subprocess
    import sys

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

# ── run command ─────────────────────────────────────────────────────────

@app.command(name="run")
def run(
    dataset_root: Annotated[Path, typer.Option("--dataset-root", "-d",
        help="Path to the raw dataset directory.")] = DFLT.DATASET_ROOT,
    split_file: Annotated[Path, typer.Option("--split-file", "-s",
        help="JSON split file (see datasets/loader.py for format).")] = DFLT.SPLIT_FILE,
    gallery_split_file: Annotated[Path | None, typer.Option("--gallery-split-file",
        help="Optional gallery/train split file. When provided, enables split-aware "
             "evaluation (query from --split-file, gallery from this file). "
             "Academically correct protocol for thesis results.")] = None,
    models: Annotated[str, typer.Option("--models", "-m",
        help="Comma-separated model keys, or 'all'.")] = CLI_STR.ALL,
    k: Annotated[str, typer.Option("--k",
        help="Comma-separated K values for P@K / R@K / nDCG@K.")] = "1,5,10,20",
    batch_size: Annotated[int, typer.Option("--batch-size",
        help="Images per forward pass.")] = MAGIC.BATCH_SIZE,
    no_cache: Annotated[bool, typer.Option("--no-cache",
        help="Disable embedding cache; always recompute.")] = False,
    no_latency: Annotated[bool, typer.Option("--no-latency",
        help="Skip latency / throughput measurement.")] = False,
    output: Annotated[Path, typer.Option("--output", "-o",
        help="Root output directory.", show_default=True)] = DFLT.OUTPUTS_ROOT,
    dataset_name: Annotated[str, typer.Option("--dataset-name", "--name",
        help="Dataset label used in reports and cache keys.", show_default=True)] = DFLT.DATASET_NAME,
    device: Annotated[str, typer.Option("--device",
        help="Device to run models on (cpu, cuda, mps, auto).", show_default=True)] = "auto",
    seed: Annotated[int, typer.Option("--seed",
        help="Random seed for reproducibility.", show_default=True)] = MAGIC.SEED,
    log_level: Annotated[str, typer.Option("--log-level", show_default=True)] = DFLT.LOG_LEVEL,
) -> None:
    """Run the full benchmark pipeline.

    Loads the dataset, generates embeddings (or loads from cache), runs
    exact cosine retrieval, and computes Precision@K, Recall@K, nDCG,
    mAP, latency, and throughput. Produces JSON, CSV, Markdown, Typst
    tables, and matplotlib charts.

    Example::

        uv run benchmark run \\
            --dataset-root data/raw/deepfashion \\
            --split-file data/splits/deepfashion/test.json \\
            --models fashion-clip,clip-b32 \\
            --k 1,5,10,20
    """
    setup_logging(level=log_level, log_file=output / "logs" / LOG.RUN)
    set_seed(seed)

    from benchmark.datasets.loader import FashionDataset
    from benchmark.datasets.validators import validate_dataset
    from benchmark.evaluation.benchmark import BenchmarkRunner
    from benchmark.models import REGISTRY
    from benchmark.reporting import (
        generate_all_charts,
        write_all_tables,
        write_comparison_json,
        write_csv,
        write_markdown,
        write_model_json,
    )

    # ── resolve model keys ────────────────────────────────────────────────────
    all_keys = list(REGISTRY.keys())
    model_keys = all_keys if models == CLI_STR.ALL else [k.strip() for k in models.split(",")]
    unknown = [k for k in model_keys if k not in REGISTRY]
    if unknown:
        console.print(f"[red]Unknown model keys: {unknown}[/red]")
        console.print(f"Available: {all_keys}")
        raise typer.Exit(code=EXIT.EXIT_FAILURE)


    top_k = [int(v) for v in k.split(",")]
    # ── print run config ──────────────────────────────────────────────────────
    config_table = Table(title="Benchmark Configuration", show_header=False)
    config_table.add_column("Key", style="bold")
    config_table.add_column("Value")
    config_table.add_row("Models", ", ".join(model_keys))
    config_table.add_row("K values", str(top_k))
    config_table.add_row("Dataset root", str(dataset_root))
    config_table.add_row("Split file", str(split_file))
    if gallery_split_file:
        config_table.add_row("Gallery split", str(gallery_split_file))
        config_table.add_row("Mode", "split-aware (query/gallery)")
    config_table.add_row("Batch size", str(batch_size))
    config_table.add_row("Cache", "disabled" if no_cache else "enabled")
    config_table.add_row("Latency", "disabled" if no_latency else "enabled")
    config_table.add_row("Seed", str(seed))
    console.print(config_table)

    # ── load dataset ──────────────────────────────────────────────────────────
    dataset = FashionDataset(dataset_root=dataset_root, split_file=split_file, split=SPLIT.TEST)
    dataset.load()

    gallery_dataset: FashionDataset | None = None
    if gallery_split_file:
        gallery_dataset = FashionDataset(
            dataset_root=dataset_root,
            split_file=gallery_split_file,
            split=SPLIT.TRAIN,
        )
        gallery_dataset.load()

    errors = validate_dataset(dataset)
    if errors:
        console.print(f"[yellow]⚠ {len(errors)} missing images — they will be skipped[/yellow]")

    # ── run ───────────────────────────────────────────────────────────────────
    runner = BenchmarkRunner(
        dataset=dataset,
        k_values=top_k,
        batch_size=batch_size,
        use_cache=not no_cache,
        measure_efficiency=not no_latency,
        save_embeddings=True,
        dataset_name=dataset_name,
        device=device,
        gallery_dataset=gallery_dataset,
    )
    all_metrics = runner.run(model_keys=model_keys)

    # ── save per-model JSON ───────────────────────────────────────────────────
    metrics_dir = output / "metrics"
    for m in all_metrics:
        write_model_json(m, output_dir=metrics_dir)

    # ── generate all reports ──────────────────────────────────────────────────
    reports_dir = output / "reports"
    tables_dir  = output / "tables"
    figures_dir = output / "figures"

    write_comparison_json(all_metrics, output_dir=reports_dir)
    write_csv(all_metrics, k_values=top_k, output_dir=reports_dir)
    write_markdown(all_metrics, k_values=top_k, output_dir=reports_dir, dataset_name=dataset_name)
    write_all_tables(all_metrics, k_values=top_k, output_dir=tables_dir)
    generate_all_charts(all_metrics, k_values=top_k, output_dir=figures_dir)

    # ── print summary table ───────────────────────────────────────────────────
    summary = Table(title="Results", show_header=True, header_style="bold cyan")
    summary.add_column("Model")
    summary.add_column("mAP", justify="right")
    for kv in top_k:
        summary.add_column(f"P@{kv}", justify="right")
    summary.add_column("p50 (ms)", justify="right")

    for m in sorted(all_metrics, key=lambda x: x.map_score, reverse=True):
        row = [
            m.model_name,
            f"{m.map_score:.4f}",
            *[f"{m.precision.get(kv, 0):.4f}" for kv in top_k],
            f"{m.latency.get('p50_ms', 0):.1f}" if m.latency else PLACEHOLDER.MISSING_MD,
        ]
        summary.add_row(*row)

    console.print(summary)
    console.print(f"\n[green]✓ All outputs written to {output}/[/green]")


# ── thesis command ────────────────────────────────────────────────────────────

@app.command()
def thesis(
    dataset_root: Annotated[Path, typer.Option("--dataset-root", "-d",
        help="Path to the raw dataset directory.")] = DFLT.DATASET_ROOT,
    models: Annotated[str, typer.Option("--models", "-m",
        help="Comma-separated model keys, or 'all'.")] = "all",
    folds: Annotated[int, typer.Option("--folds",
        help="Number of cross-validation folds.")] = MAGIC.N_FOLDS_DEFAULT,
    k: Annotated[str, typer.Option("--k",
        help="Comma-separated K values for P@K / R@K.")] = "5,10,20",
    batch_size: Annotated[int, typer.Option("--batch-size",
        help="Images per forward pass.")] = MAGIC.BATCH_SIZE,
    no_cache: Annotated[bool, typer.Option("--no-cache",
        help="Disable embedding cache.")] = False,
    output: Annotated[Path, typer.Option("--output", "-o",
        help="Root output directory.", show_default=True)] = DFLT.THESIS_DIR,
    device: Annotated[str, typer.Option("--device",
        help="Device (cpu, cuda, mps, auto).", show_default=True)] = "auto",
    seed: Annotated[int, typer.Option("--seed",
        help="Random seed.", show_default=True)] = MAGIC.SEED,
    log_level: Annotated[str, typer.Option("--log-level", show_default=True)] = DFLT.LOG_LEVEL,
) -> None:
    """Run the thesis benchmark (k-fold cross-validation).

    Runs stratified cross-validation with the given number of folds,
    computing aggregate metrics (mean ± SD) and generating Typst tables
    suitable for inclusion in the thesis document.

    Example::

        uv run benchmark thesis \
            --dataset-root data/raw/deepfashion \
            --models fashion-clip,resnet-50 \
            --folds 3
    """
    setup_logging(level=log_level, log_file=output / "logs" / LOG.THESIS)

    from benchmark.evaluation.thesis import THESIS_MODEL_KEYS, ThesisRunner
    from benchmark.reporting import write_thesis_tables
    model_keys = THESIS_MODEL_KEYS if models == CLI_STR.ALL else [k.strip() for k in models.split(",")]

    top_k = [int(v) for v in k.split(",")]

    config_table = Table(title="Thesis Benchmark Configuration", show_header=False)
    config_table.add_column("Key", style="bold")
    config_table.add_column("Value")
    config_table.add_row("Models", ", ".join(model_keys))
    config_table.add_row("Folds", str(folds))
    config_table.add_row("K values", str(top_k))
    config_table.add_row("Dataset root", str(dataset_root))
    config_table.add_row("Batch size", str(batch_size))
    config_table.add_row("Cache", "disabled" if no_cache else "enabled")
    config_table.add_row("Seed", str(seed))
    console.print(config_table)

    runner = ThesisRunner(
        dataset_root=dataset_root,
        output_dir=output,
        k_values=top_k,
        folds=folds,
        seed=seed,
        device=device,
        use_cache=not no_cache,
        batch_size=batch_size,
    )
    results = runner.run(model_keys=model_keys)

    # Save results
    results_dir = output / "results"
    results_dir.mkdir(parents=True, exist_ok=True)
    out_path = results_dir / "thesis_results.json"
    out_path.write_text(json.dumps(results, indent=2))
    console.print(f"\n[green]✓ Results written to {out_path}[/green]")

    # Generate Typst tables
    write_thesis_tables(results, output_dir=output / "tables")
    console.print(f"[green]✓ Typst tables written to {output / 'tables'}[/green]")

    # Summary table
    summary = Table(title="Thesis Results (Aggregate)", show_header=True, header_style="bold cyan")
    summary.add_column("Model")
    summary.add_column("mAP (mean ± SD)", justify="right")
    summary.add_column("Latency (ms)", justify="right")
    summary.add_column("Throughput (/s)", justify="right")

    for r in results:
        agg = r.get("aggregate", {})
        map_agg = agg.get("map", {})
        lat_agg = agg.get("latency_mean_ms", {})
        thr_agg = agg.get("throughput_per_sec", {})
        summary.add_row(
            r["model_name"],
            f"{map_agg.get('mean', 0):.4f} ± {map_agg.get('std', 0):.4f}",
            f"{lat_agg.get('mean', 0):.1f}",
            f"{thr_agg.get('mean', 0):.1f}",
        )
    console.print(summary)


# ── pipeline command ──────────────────────────────────────────────────────

@app.command()
def pipeline(
    dataset_root: Annotated[Path, typer.Option("--dataset-root", "-d",
        help="Path to the raw dataset directory.")] = DFLT.DATASET_ROOT,
    models: Annotated[str, typer.Option("--models", "-m",
        help="Comma-separated model keys, or 'all'.")] = "all",
    folds: Annotated[int, typer.Option("--folds",
        help="Number of cross-validation folds.")] = MAGIC.N_FOLDS_DEFAULT,
    k: Annotated[str, typer.Option("--k",
        help="Comma-separated K values for P@K / R@K.")] = "5,10,20",
    batch_size: Annotated[int, typer.Option("--batch-size",
        help="Images per forward pass.")] = MAGIC.BATCH_SIZE,
    no_cache: Annotated[bool, typer.Option("--no-cache",
        help="Disable embedding cache.")] = False,
    output: Annotated[Path, typer.Option("--output", "-o",
        help="Root output directory.", show_default=True)] = DFLT.PIPELINE_DIR,
    device: Annotated[str, typer.Option("--device",
        help="Device (cpu, cuda, mps, auto).", show_default=True)] = "auto",
    seed: Annotated[int, typer.Option("--seed",
        help="Random seed.", show_default=True)] = MAGIC.SEED,
    conn_string: Annotated[str, typer.Option("--conn-string",
        help="PostgreSQL connection string.", show_default=True)] = DFLT.CONN_STRING,
    pg_lists: Annotated[int, typer.Option("--pg-lists",
        help="IVFFlat lists parameter.", show_default=True)] = FAISS_PARAMS.N_LISTS,
    log_level: Annotated[str, typer.Option("--log-level", show_default=True)] = DFLT.LOG_LEVEL,
) -> None:
    """Run the production pipeline benchmark (thesis + pgvector).

    Extends the thesis protocol by also ingesting embeddings into a
    pgvector database, building an IVFFlat index, and measuring query
    latency and recall from the database backend.

    Example::

        uv run benchmark pipeline \\
            --dataset-root data/raw/deepfashion \\
            --models fashion-clip,resnet-50 \\
            --folds 3
    """
    setup_logging(level=log_level, log_file=output / "logs" / LOG.PIPELINE)

    from benchmark.evaluation.pipeline import THESIS_MODEL_KEYS, PipelineRunner
    from benchmark.reporting.pipeline import write_pipeline_typst
    model_keys = THESIS_MODEL_KEYS if models == CLI_STR.ALL else [k.strip() for k in models.split(",")]

    top_k = [int(v) for v in k.split(",")]

    config_table = Table(title="Pipeline Benchmark Configuration", show_header=False)
    config_table.add_column("Key", style="bold")
    config_table.add_column("Value")
    config_table.add_row("Models", ", ".join(model_keys))
    config_table.add_row("Folds", str(folds))
    config_table.add_row("K values", str(top_k))
    config_table.add_row("Dataset root", str(dataset_root))
    config_table.add_row("Batch size", str(batch_size))
    config_table.add_row("Cache", "disabled" if no_cache else "enabled")
    config_table.add_row("PGVector", conn_string.split("@")[-1])
    config_table.add_row("PG lists", str(pg_lists))
    config_table.add_row("Seed", str(seed))
    console.print(config_table)

    runner = PipelineRunner(
        dataset_root=dataset_root,
        output_dir=output,
        k_values=top_k,
        folds=folds,
        seed=seed,
        device=device,
        use_cache=not no_cache,
        batch_size=batch_size,
        conn_string=conn_string,
        pg_lists=pg_lists,
    )
    results = runner.run(model_keys=model_keys)

    results_dir = output / "results"
    results_dir.mkdir(parents=True, exist_ok=True)
    out_path = results_dir / "pipeline_results.json"
    out_path.write_text(json.dumps(results, indent=2))
    console.print(f"\n[green]✓ Results written to {out_path}[/green]")

    write_pipeline_typst(results, output_dir=output / "tables")
    console.print(f"[green]✓ Typst tables written to {output / 'tables'}[/green]")

    summary = Table(title="Pipeline Results", show_header=True, header_style="bold cyan")
    summary.add_column("Model")
    summary.add_column("mAP (mean ± SD)", justify="right")
    summary.add_column("PG Recall@10", justify="right")
    summary.add_column("Query Latency (ms)", justify="right")

    for r in results:
        agg = r.get("aggregate", {})
        pm = r.get("production_metrics", {})
        summary.add_row(
            r["model_name"],
            f"{agg.get('map', {}).get('mean', 0):.4f} ± {agg.get('map', {}).get('std', 0):.4f}",
            f"{pm.get('pgvector_recall@10', {}).get('mean', 0):.4f}",
            f"{pm.get('pgvector_query_latency_ms', {}).get('mean', 0):.1f}",
        )
    console.print(summary)


# ── report command ────────────────────────────────────────────────────────────

@app.command()
def report(
    results_dir: Annotated[Path, typer.Option("--results-dir",
        help="Directory containing per-model .json result files.")] = DFLT.METRICS_DIR,
    format: Annotated[str, typer.Option("--format", "-f",
        help="csv | json | markdown | typst | charts | all")] = "all",
    k: Annotated[str, typer.Option("--k",
        help="Comma-separated K values to include in tables.")] = "1,5,10,20",
    output: Annotated[Path, typer.Option("--output", "-o")] = DFLT.OUTPUTS_ROOT,
    log_level: Annotated[str, typer.Option("--log-level")] = DFLT.LOG_LEVEL,
) -> None:
    """Re-generate reports from stored JSON metric files.

    Loads per-model JSON files from the metrics directory and regenerates
    CSV, JSON comparison, Markdown, Typst tables, and/or charts. Useful
    when you want to change table formatting without re-running inference.

    Example::

        uv run benchmark report --format typst
    """
    setup_logging(level=log_level)

    from benchmark.evaluation.evaluator import ModelMetrics
    from benchmark.reporting import (
        generate_all_charts,
        write_all_tables,
        write_comparison_json,
        write_csv,
        write_markdown,
    )

    # ── load stored metrics ───────────────────────────────────────────────────
    if not results_dir.exists():
        console.print(f"[red]Results directory not found: {results_dir}[/red]")
        raise typer.Exit(code=EXIT.EXIT_FAILURE)

    json_files = sorted(results_dir.glob("*.json"))
    if not json_files:
        console.print(f"[red]No .json files found in {results_dir}[/red]")
        raise typer.Exit(code=EXIT.EXIT_FAILURE)

    all_metrics: list[ModelMetrics] = []
    for jf in json_files:
        raw = json.loads(jf.read_text(encoding=FILE_ENCODING))
        # Reconstruct ModelMetrics from the serialised dict
        m = ModelMetrics(
            model_name=raw["model"],
            dataset=raw.get("dataset", "unknown"),
            k_values=list(raw.get("precision", {}).keys()),
        )
        m.map_score = raw.get("map", 0.0)
        m.precision = {int(k.lstrip("@")): v for k, v in raw.get("precision", {}).items()}
        m.recall    = {int(k.lstrip("@")): v for k, v in raw.get("recall", {}).items()}
        m.ndcg      = {int(k.lstrip("@")): v for k, v in raw.get("ndcg", {}).items()}
        m.latency   = raw.get("latency_ms", {})
        m.throughput_per_sec = raw.get("throughput_per_sec", 0.0)
        all_metrics.append(m)

    top_k = [int(v) for v in k.split(",")]
    fmts = {CLI_STR.CSV, CLI_STR.JSON, CLI_STR.MARKDOWN, CLI_STR.TYPST, CLI_STR.CHARTS} if format == CLI_STR.ALL else {format}

    if "json" in fmts:
        write_comparison_json(all_metrics, output_dir=output / "reports")
    if "csv" in fmts:
        write_csv(all_metrics, k_values=top_k, output_dir=output / "reports")
    if "markdown" in fmts:
        write_markdown(all_metrics, k_values=top_k, output_dir=output / "reports")
    if "typst" in fmts:
        write_all_tables(all_metrics, k_values=top_k, output_dir=output / "tables")
    if "charts" in fmts:
        generate_all_charts(all_metrics, k_values=top_k, output_dir=output / "figures")

    console.print(f"[green]✓ Reports written to {output}/[/green]")


# ── cache command ─────────────────────────────────────────────────────────────

@app.command()
def cache(
    action: Annotated[str, typer.Argument(
        help="list | stats | clear")] = "list",
    cache_dir: Annotated[Path, typer.Option(
        help="Cache directory.")] = Path("data/cache"),
) -> None:
    """Inspect or clear the embedding cache.

    Lists cached npz files with sizes, shows aggregate stats, or
    clears all cache entries from the cache directory.

    Examples::

        uv run benchmark cache list
        uv run benchmark cache stats
        uv run benchmark cache clear
    """
    from benchmark.embeddings.cache import CACHE_DIR

    effective_dir = cache_dir if cache_dir != Path("data/cache") else CACHE_DIR

    if action == CLI_STR.LIST:
        files = sorted(effective_dir.glob("*.npz")) if effective_dir.exists() else []
        if not files:
            console.print("Cache is empty.")
            return
        t = Table(title=f"Embedding Cache ({effective_dir})")
        t.add_column("File")
        t.add_column("Size", justify="right")
        for f in files:
            size_mb = f.stat().st_size / 1_048_576
            t.add_row(f.name, f"{size_mb:.1f} MB")
        console.print(t)

    elif action == CLI_STR.STATS:
        files = sorted(effective_dir.glob("*.npz")) if effective_dir.exists() else []
        total = sum(f.stat().st_size for f in files)
        console.print(f"Cache entries : {len(files)}")
        console.print(f"Total size    : {total / 1_048_576:.1f} MB")

    elif action == CLI_STR.CLEAR:
        import shutil
        if not effective_dir.exists():
            console.print("Cache is already empty.")
            return
        count = len(list(effective_dir.glob("*.npz")))
        shutil.rmtree(effective_dir)
        effective_dir.mkdir(parents=True, exist_ok=True)
        console.print(f"[green]Cleared {count} cache file(s).[/green]")

    else:
        console.print(f"[red]Unknown action '{action}'. Use: list | stats | clear[/red]")
        raise typer.Exit(code=EXIT.EXIT_FAILURE)
