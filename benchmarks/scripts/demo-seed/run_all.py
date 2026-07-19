#!/usr/bin/env python
"""Orchestrate all demo seed ETL steps."""
from __future__ import annotations

import argparse
import shutil
import subprocess
import sys
from pathlib import Path

SCRIPTS_DIR = Path(__file__).resolve().parent
REPO_ROOT = SCRIPTS_DIR.parent.parent.parent


def run_step(name: str, args: list[str]) -> int:
    print(f"\n{'=' * 60}\n  {name}\n{'=' * 60}")
    cmd = ["uv", "run", "python", str(SCRIPTS_DIR / name)] + args
    result = subprocess.run(cmd)
    return result.returncode


def main() -> None:
    parser = argparse.ArgumentParser(description="Run the full demo seed ETL pipeline")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--dataset", type=Path,
                        default=REPO_ROOT / "benchmarks" / "data" / "raw" / "fashion-product-images")
    parser.add_argument("--output", type=Path,
                        default=SCRIPTS_DIR / "output")
    parser.add_argument("--storage", type=Path,
                        default=REPO_ROOT / "infra" / "Storage" / "demo")
    parser.add_argument("--deploy", action="store_true")
    parser.add_argument("--display-size", default="512")
    parser.add_argument("--search-size", default="224")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    force_args = ["--force"] if args.force else []

    steps = [
        ("01_extract_taxonomies.py", ["--dataset", str(args.dataset), "--output", str(args.output)] + force_args),
        ("02_extract_products.py", ["--dataset", str(args.dataset), "--output", str(args.output), "--count", str(args.count),
                                    "--display-size", args.display_size, "--search-size", args.search_size] + force_args),
        ("03_process_images.py", ["--dataset", str(args.dataset), "--output", str(args.output),
                                  "--display-size", args.display_size, "--search-size", args.search_size]),
        ("04_generate_embeddings.py", ["--output", str(args.output)]),
        ("05_extract_stock.py", ["--output", str(args.output)] + force_args),
        ("06_verify_output.py", ["--output", str(args.output), "--count", str(args.count)]),
    ]

    for script_name, script_args in steps:
        rc = run_step(script_name, script_args)
        if rc != 0:
            print(f"\nERROR: {script_name} failed with code {rc}")
            sys.exit(rc)

    if args.deploy:
        print(f"\n{'=' * 60}\n  DEPLOY: copying images to storage\n{'=' * 60}")
        shutil.copytree(args.output / "images", args.storage / "images", dirs_exist_ok=True)
        print(f"Deployed images to {args.storage / 'images'}")

    print(f"\n{'=' * 60}")
    print("  PIPELINE COMPLETE")
    print(f"{'=' * 60}")


if __name__ == "__main__":
    main()
