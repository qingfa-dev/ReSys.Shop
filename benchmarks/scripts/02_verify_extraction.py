#!/usr/bin/env python
"""Verify that the Kaggle Fashion Product Images (Small) dataset was extracted
correctly and is ready for benchmarking.

This script goes beyond the basic preflight_check() in validators.py:
  1. Checks directory structure and required files
  2. Validates every CSV row against expected types / ranges
  3. Counts how many images are present vs referenced in the CSV
  4. Samples 50 images at random and confirms they are valid JPEGs
  5. Reports class distribution statistics for every label column
  6. Prints a readiness verdict with a clear go / no-go decision

Usage
-----
    uv run python scripts/02_verify_extraction.py \\
        --dataset-root data/raw/fashion-dataset

    # Verbose mode (prints per-column stats)
    uv run python scripts/02_verify_extraction.py --verbose

    # Output results to JSON
    uv run python scripts/02_verify_extraction.py --json-out outputs/verify.json
"""
from __future__ import annotations

import argparse
import csv
import json
import random
import sys
from collections import Counter
from pathlib import Path
from typing import Any

# ─────────────────────────────────────────────────────────────────────────────

REQUIRED_COLUMNS = {
    "id", "gender", "masterCategory", "subCategory",
    "articleType", "baseColour", "season", "year",
    "usage", "productDisplayName",
}

LABEL_COLUMNS = ("masterCategory", "subCategory", "articleType")

# Expected value counts (approximate, used to flag suspicious extractions)
MIN_PRODUCTS     = 40_000   # real dataset has ~44 k
MIN_IMAGES       = 40_000
MIN_ARTICLE_TYPES = 100


# ─────────────────────────────────────────────────────────────────────────────

def _step(label: str, ok: bool, detail: str = "") -> dict:
    icon = "✓" if ok else "✗"
    msg  = f"  {icon}  {label}"
    if detail:
        msg += f" — {detail}"
    print(msg)
    return {"label": label, "ok": ok, "detail": detail}


def check_structure(root: Path) -> list[dict]:
    results = []
    print("\n[1] Directory structure")
    results.append(_step("Dataset root exists", root.exists(), str(root)))
    results.append(_step("styles.csv present", (root / "styles.csv").exists()))
    results.append(_step("images/ directory present", (root / "images").exists()))
    return results


def check_csv(root: Path, verbose: bool = False) -> tuple[list[dict], list[dict]]:
    """Validate styles.csv; return (check_results, parsed_rows)."""
    results = []
    print("\n[2] styles.csv validation")
    csv_path = root / "styles.csv"
    if not csv_path.exists():
        results.append(_step("styles.csv readable", False))
        return results, []

    rows: list[dict] = []
    bad_rows: list[int] = []

    with csv_path.open(encoding="utf-8", errors="replace") as fh:
        reader = csv.DictReader(fh)
        actual_cols = set(f.strip() for f in (reader.fieldnames or []))
        missing = REQUIRED_COLUMNS - actual_cols
        results.append(
            _step("All required columns present", not missing,
                  f"missing: {sorted(missing)}" if missing else "")
        )

        for lineno, row in enumerate(reader, start=2):
            # Strip whitespace from all values
            row = {k: v.strip() for k, v in row.items()}
            pid = row.get("id", "").strip()
            if not pid or not pid.isdigit():
                bad_rows.append(lineno)
                continue
            rows.append(row)

    results.append(_step(f"Row count ≥ {MIN_PRODUCTS:,}", len(rows) >= MIN_PRODUCTS,
                          f"{len(rows):,} valid rows"))
    results.append(_step("No unparseable rows", not bad_rows,
                          f"{len(bad_rows)} bad rows" if bad_rows else ""))

    if verbose:
        for col in LABEL_COLUMNS:
            counts = Counter(r.get(col, "").strip() for r in rows if r.get(col, "").strip())
            print(f"     {col}: {len(counts)} unique values")
            for val, cnt in counts.most_common(5):
                print(f"       {val:<25s} {cnt:>6,}")

    results.append(_step(
        f"articleType diversity ≥ {MIN_ARTICLE_TYPES}",
        len(Counter(r.get("articleType", "") for r in rows)) >= MIN_ARTICLE_TYPES,
        f"{len(Counter(r.get('articleType', '') for r in rows))} types found",
    ))

    return results, rows


def check_images(root: Path, rows: list[dict], sample_n: int = 50) -> list[dict]:
    results = []
    print("\n[3] Image files")
    img_dir = root / "images"
    if not img_dir.exists():
        results.append(_step("images/ readable", False))
        return results

    on_disk = set(p.stem for p in img_dir.glob("*.jpg"))
    in_csv  = set(r["id"] for r in rows)

    results.append(_step(f"Image count ≥ {MIN_IMAGES:,}", len(on_disk) >= MIN_IMAGES,
                          f"{len(on_disk):,} images on disk"))
    missing_imgs  = in_csv - on_disk
    orphan_imgs   = on_disk - in_csv
    results.append(_step("No CSV rows missing images",  not missing_imgs,
                          f"{len(missing_imgs):,} missing" if missing_imgs else ""))
    results.append(_step("No orphaned images (no CSV row)", not orphan_imgs,
                          f"{len(orphan_imgs):,} orphans" if orphan_imgs else ""))

    # Sample verification
    sample_ids = random.sample(sorted(on_disk), min(sample_n, len(on_disk)))
    bad_images: list[str] = []
    try:
        from PIL import Image, UnidentifiedImageError
        for pid in sample_ids:
            try:
                with Image.open(img_dir / f"{pid}.jpg") as img:
                    img.verify()  # detects truncated files
            except (UnidentifiedImageError, OSError, SyntaxError):
                bad_images.append(pid)
        results.append(_step(
            f"Random sample of {len(sample_ids)} images are valid JPEGs",
            not bad_images,
            f"{len(bad_images)} corrupted: {bad_images[:5]}" if bad_images else "",
        ))
    except ImportError:
        results.append(_step("Pillow available for image validation", False,
                              "pip install Pillow"))

    return results


def check_class_distribution(rows: list[dict]) -> list[dict]:
    results = []
    print("\n[4] Class distribution (articleType)")
    counts = Counter(r.get("articleType", "").strip() for r in rows)
    counts.pop("", None)

    singletons  = sum(1 for c in counts.values() if c == 1)
    small_cls   = sum(1 for c in counts.values() if c < 10)
    large_cls   = sum(1 for c in counts.values() if c >= 50)

    results.append(_step("Top-5 article types",  True,
                          ", ".join(f"{k}({v})" for k, v in counts.most_common(5))))
    results.append(_step("Singleton classes",     singletons == 0,
                          f"{singletons} classes with only 1 image"))
    results.append(_step("Small classes (< 10)",  small_cls < 10,
                          f"{small_cls} classes"))
    results.append(_step(f"Large classes (≥ 50)", large_cls > 50,
                          f"{large_cls} classes"))
    return results


def verdict(all_results: list[dict]) -> bool:
    failed = [r for r in all_results if not r["ok"]]
    print(f"\n{'─'*60}")
    if not failed:
        print("  ✅  Dataset extraction verified — ready for benchmarking.")
        print(f"\n  Run the benchmark with:")
        print("    uv run benchmark benchmark --dataset-root data/raw/fashion-dataset")
    else:
        print(f"  ❌  {len(failed)} check(s) failed:")
        for r in failed:
            msg = f"    • {r['label']}"
            if r["detail"]:
                msg += f": {r['detail']}"
            print(msg)
        print("\n  Re-download or re-extract the dataset and try again.")
        print("  Download: https://www.kaggle.com/datasets/paramaggarwal/fashion-product-images-small")
    print(f"{'─'*60}")
    return len(failed) == 0


def main() -> None:
    parser = argparse.ArgumentParser(
        description="Verify the Kaggle Fashion Product Images (Small) extraction",
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=__doc__,
    )
    parser.add_argument("--dataset-root", "-d", type=Path,
                        default=Path("data/raw/fashion-dataset"),
                        help="Path to the extracted dataset root.")
    parser.add_argument("--sample-images", type=int, default=50,
                        help="Number of random images to validate (default 50).")
    parser.add_argument("--verbose", "-v", action="store_true",
                        help="Print per-column value distributions.")
    parser.add_argument("--json-out", type=Path,
                        help="Write full verification report to JSON.")
    parser.add_argument("--seed", type=int, default=42)
    args = parser.parse_args()

    random.seed(args.seed)
    root = args.dataset_root.expanduser().resolve()
    print(f"Verifying: {root}")

    all_results: list[dict] = []
    all_results += check_structure(root)
    csv_results, rows = check_csv(root, verbose=args.verbose)
    all_results += csv_results
    all_results += check_images(root, rows, sample_n=args.sample_images)
    all_results += check_class_distribution(rows)

    ok = verdict(all_results)

    if args.json_out:
        args.json_out.parent.mkdir(parents=True, exist_ok=True)
        report: dict[str, Any] = {
            "dataset_root": str(root),
            "ok": ok,
            "checks": all_results,
            "summary": {
                "total": len(all_results),
                "passed": sum(1 for r in all_results if r["ok"]),
                "failed": sum(1 for r in all_results if not r["ok"]),
            },
        }
        args.json_out.write_text(json.dumps(report, indent=2), encoding="utf-8")
        print(f"\n  Report written → {args.json_out}")

    sys.exit(0 if ok else 1)


if __name__ == "__main__":
    main()
