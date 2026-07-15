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


def run_step(step_num: int, total: int, name: str, args: list[str]) -> int:
    print(f"\n{'='*60}\n  STEP {step_num}/{total}: {name}\n{'='*60}")
    cmd = [sys.executable, str(SCRIPTS_DIR / name)] + args
    result = subprocess.run(cmd)
    return result.returncode


def main() -> None:
    parser = argparse.ArgumentParser(description="Run the full demo seed ETL pipeline")
    parser.add_argument("--count", type=int, default=2000, help="Target number of product groups")
    parser.add_argument("--dataset", type=Path,
                        default=REPO_ROOT / "benchmarks" / "data" / "raw" / "fashion-product-images")
    parser.add_argument("--output", type=Path,
                        default=SCRIPTS_DIR / "output")
    parser.add_argument("--storage", type=Path,
                        default=REPO_ROOT / "infra" / "Storage" / "demo")
    parser.add_argument("--base-url", default="http://localhost:8000")
    parser.add_argument("--skip-embeddings", action="store_true")
    parser.add_argument("--deploy", action="store_true")
    parser.add_argument("--display-size", default="512")
    parser.add_argument("--search-size", default="224")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    force_args = ["--force"] if args.force else []

    steps = [
        ("extract_taxonomies.py", ["--dataset", str(args.dataset), "--output", str(args.output)] + force_args),
        ("extract_products.py", ["--dataset", str(args.dataset), "--output", str(args.output), "--count", str(args.count),
                                 "--display-size", args.display_size, "--search-size", args.search_size] + force_args),
        ("process_images.py", ["--dataset", str(args.dataset), "--output", str(args.output),
                               "--display-size", args.display_size, "--search-size", args.search_size]),
    ]

    if not args.skip_embeddings:
        steps.append(("generate_embeddings.py", ["--output", str(args.output), "--base-url", args.base_url]))

    steps.append(("extract_stock.py", ["--output", str(args.output)] + force_args))

    total_steps = len(steps)
    for i, (script_name, script_args) in enumerate(steps, 1):
        rc = run_step(i, total_steps, script_name, script_args)
        if rc != 0 and script_name != "generate_embeddings.py":
            print(f"\nERROR: {script_name} failed with code {rc}")
            sys.exit(rc)

    if args.deploy:
        print(f"\n{'='*60}\n  DEPLOY: copying images to storage\n{'='*60}")
        shutil.copytree(args.output / "images", args.storage / "images", dirs_exist_ok=True)
        print(f"Deployed images to {args.storage / 'images'}")

    print(f"\n{'='*60}")
    print("  PIPELINE COMPLETE")
    print(f"{'='*60}")
    import json as _json
    summary_files = {
        "Products": "demo_products.json",
        "Variants": "demo_variants.json",
        "Total Images": "demo_variant_images.json",
        "Option Assignments": "demo_option_assignments.json",
        "Taxons": "demo_taxons.json",
        "Embeddings": "demo_embeddings.json",
        "Stock Items": "demo_stock_items.json",
        "Stock Locations": "demo_stock_locations.json",
    }
    for label, fname in summary_files.items():
        fp = args.output / fname
        if fp.exists():
            data = _json.loads(fp.read_text())
            if isinstance(data, dict):
                summary_files[label] = sum(len(v) if isinstance(v, list) else 1 for v in data.values())
                print(f"  {label:.<40} {summary_files[label]:>6}")
            else:
                print(f"  {label:.<40} {len(data):>6}")


if __name__ == "__main__":
    main()
