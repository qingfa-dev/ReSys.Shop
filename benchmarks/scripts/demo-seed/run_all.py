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
                        default=SCRIPTS_DIR / "output")
    parser.add_argument("--storage", type=Path,
                        default=REPO_ROOT / "infra" / "Storage" / "demo")
    parser.add_argument("--base-url", default="http://localhost:8000")
    parser.add_argument("--skip-embeddings", action="store_true")
    parser.add_argument("--deploy", action="store_true")
    args = parser.parse_args()

    steps = [
        ("extract_taxonomies.py", ["--dataset", str(args.dataset), "--output", str(args.output)]),
        ("extract_products.py", ["--dataset", str(args.dataset), "--output", str(args.output), "--count", str(args.count)]),
        ("process_images.py", ["--dataset", str(args.dataset), "--output", str(args.output)]),
    ]

    if not args.skip_embeddings:
        steps.append(("generate_embeddings.py", ["--output", str(args.output), "--base-url", args.base_url]))

    steps.append(("extract_stock.py", ["--output", str(args.output)]))

    for script_name, script_args in steps:
        rc = run_step(script_name, script_args)
        if rc != 0 and script_name != "generate_embeddings.py":
            print(f"\nERROR: {script_name} failed with code {rc}")
            sys.exit(rc)

    if args.deploy:
        print(f"\n{'='*60}\n  DEPLOY: copying images to storage\n{'='*60}")
        shutil.copytree(args.output / "images", args.storage / "images", dirs_exist_ok=True)
        print(f"Deployed images to {args.storage / 'images'}")

    print(f"\nDone. JSON data written to {args.output}")
    print(f"Images written to {args.output / 'images'}")
    if args.deploy:
        print(f"Images deployed to {args.storage / 'images'}")


if __name__ == "__main__":
    main()
