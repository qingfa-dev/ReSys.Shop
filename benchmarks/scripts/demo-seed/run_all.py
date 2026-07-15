#!/usr/bin/env python
"""Orchestrate all demo seed ETL steps."""
from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path

SCRIPTS_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPTS_DIR.parent.parent.parent


def run_step(name: str, args: list[str]) -> int:
    print(f"\n{'='*60}\n  STEP: {name}\n{'='*60}")
    cmd = [sys.executable, str(SCRIPTS_DIR / name)] + args
    result = subprocess.run(cmd)
    return result.returncode


def main() -> None:
    parser = argparse.ArgumentParser(description="Run the full demo seed ETL pipeline")
    parser.add_argument("--count", type=int, default=200, help="Target number of product groups")
    parser.add_argument("--dataset", type=Path,
                        default=REPO_ROOT / "benchmarks" / "data" / "raw" / "fashion-product-images")
    parser.add_argument("--output", type=Path,
                        default=REPO_ROOT / "service" / "Api" / "src" / "Module" / "Catalog" / "Persistence" / "Seeders" / "Data")
    parser.add_argument("--storage", type=Path,
                        default=REPO_ROOT / "infra" / "Storage" / "demo")
    parser.add_argument("--base-url", default="http://localhost:8000")
    parser.add_argument("--skip-embeddings", action="store_true")
    args = parser.parse_args()

    steps = [
        ("extract_taxonomies.py", ["--dataset", str(args.dataset), "--output", str(args.output)]),
        ("extract_products.py", ["--dataset", str(args.dataset), "--output", str(args.output), "--count", str(args.count)]),
        ("process_images.py", ["--dataset", str(args.dataset), "--storage", str(args.storage), "--json-dir", str(args.output)]),
    ]

    if not args.skip_embeddings:
        steps.append(("generate_embeddings.py", ["--json-dir", str(args.output), "--storage", str(args.storage), "--base-url", args.base_url]))

    steps.append(("extract_stock.py", ["--json-dir", str(args.output)]))

    for script_name, script_args in steps:
        rc = run_step(script_name, script_args)
        if rc != 0 and script_name != "generate_embeddings.py":
            print(f"\nERROR: {script_name} failed with code {rc}")
            sys.exit(rc)

    print(f"\nDone. JSON data written to {args.output}")
    print(f"Images written to {args.storage}")
    print("Next: run the .NET app to import seed data.")


if __name__ == "__main__":
    main()
