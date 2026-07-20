#!/usr/bin/env python
"""Verify demo seed output JSON integrity — all FKs, counts, and cross-references."""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from shared import SCRIPTS_DIR  # noqa: E402


def load(path: Path, filename: str) -> list[dict]:
    fp = path / filename
    if not fp.exists():
        printfail(f"MISSING: {filename}")
        sys.exit(1)
    return json.loads(fp.read_text())


def passmsg(msg: str) -> None:
    print(f"  PASS  {msg}")


def failmsg(msg: str) -> None:
    print(f"  FAIL  {msg}")


def printfail(msg: str) -> None:
    print(f"  FAIL  {msg}")


def main() -> None:
    parser = argparse.ArgumentParser(description="Verify demo seed output JSON")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=0, help="Expected product count (0=skip check)")
    args = parser.parse_args()

    out = args.output
    print(f"Verifying output in {out}\n")

    errors = 0

    products = load(out, "demo_products.json")
    variants = load(out, "demo_variants.json")
    images = load(out, "demo_variant_images.json")
    assignments = load(out, "demo_option_assignments.json")
    classifications = load(out, "demo_classifications.json")
    taxonomies = load(out, "demo_taxonomies.json")
    taxons = load(out, "demo_taxons.json")
    option_types = load(out, "demo_option_types.json")
    option_values = load(out, "demo_option_values.json")
    stock_locations = load(out, "demo_stock_locations.json")
    stock_items = load(out, "demo_stock_items.json")
    embeddings = load(out, "demo_embeddings.json")

    product_ids = {p["id"] for p in products}
    variant_ids = {v["id"] for v in variants}
    variant_image_ids = {i["id"] for i in images}
    taxon_ids = {t["id"] for t in taxons}
    option_value_names = {v["name"] for v in option_values}
    location_codes = {loc["code"] for loc in stock_locations}

    # --- counts ---
    print("=== Counts ===")
    checks = [
        ("Products", len(products), args.count if args.count else ">=1"),
        ("Variants", len(variants), ">=1"),
        ("VariantImages", len(images), ">=2"),
        ("OptionAssignments", len(assignments), ">=1"),
        ("Classifications", len(classifications), ">=1"),
        ("Taxonomies", len(taxonomies), 3),
        ("Taxons", len(taxons), ">=3"),
        ("OptionTypes", len(option_types), 2),
        ("OptionValues", len(option_values), ">=2"),
        ("StockLocations", len(stock_locations), 3),
        ("StockItems", len(stock_items), ">=1"),
        ("Embeddings", len(embeddings), ">=1"),
    ]
    for label, actual, expected in checks:
        if isinstance(expected, int) and actual != expected:
            failmsg(f"{label}: {actual} (expected {expected})")
            errors += 1
        elif isinstance(expected, str) and actual < 1:
            failmsg(f"{label}: {actual} (expected at least 1)")
            errors += 1
        else:
            passmsg(f"{label}: {actual}")

    # --- FK integrity ---
    print("\n=== FK Integrity ===")

    # variant.product_id -> product.id
    orphan_v = [v for v in variants if v["product_id"] not in product_ids]
    if orphan_v:
        failmsg(f"{len(orphan_v)} variants with missing product FK")
        errors += 1
    else:
        passmsg("variant.product_id -> product.id")

    # image.variant_id -> variant.id
    orphan_i = [i for i in images if i["variant_id"] not in variant_ids]
    if orphan_i:
        failmsg(f"{len(orphan_i)} images with missing variant FK")
        errors += 1
    else:
        passmsg("image.variant_id -> variant.id")

    # assignment.variant_id -> variant.id
    orphan_a = [a for a in assignments if a["variant_id"] not in variant_ids]
    if orphan_a:
        failmsg(f"{len(orphan_a)} assignments with missing variant FK")
        errors += 1
    else:
        passmsg("assignment.variant_id -> variant.id")

    # assignment.option_value_name -> option_value.name (case-insensitive)
    ov_names_lower = {n.lower() for n in option_value_names}
    bad_ov = [a for a in assignments if a["option_value_name"].lower() not in ov_names_lower]
    if bad_ov:
        failmsg(f"{len(bad_ov)} assignments with unknown option_value_name")
        errors += 1
    else:
        passmsg("assignment.option_value_name -> option_value.name")

    # classification.product_id -> product.id
    orphan_c = [c for c in classifications if c["product_id"] not in product_ids]
    if orphan_c:
        failmsg(f"{len(orphan_c)} classifications with missing product FK")
        errors += 1
    else:
        passmsg("classification.product_id -> product.id")

    # classification.taxon_id -> taxon.id
    orphan_ct = [c for c in classifications if c["taxon_id"] not in taxon_ids]
    if orphan_ct:
        failmsg(f"{len(orphan_ct)} classifications with missing taxon FK")
        errors += 1
    else:
        passmsg("classification.taxon_id -> taxon.id")

    # stock_item.variant_id -> variant.id
    orphan_si = [s for s in stock_items if s["variant_id"] not in variant_ids]
    if orphan_si:
        failmsg(f"{len(orphan_si)} stock_items with missing variant FK")
        errors += 1
    else:
        passmsg("stock_item.variant_id -> variant.id")

    # stock_item.stock_location_code -> stock_location.code
    bad_sl = [s for s in stock_items if s["stock_location_code"] not in location_codes]
    if bad_sl:
        failmsg(f"{len(bad_sl)} stock_items with unknown location code")
        errors += 1
    else:
        passmsg("stock_item.stock_location_code -> stock_location.code")

    # embedding.variant_image_id -> variant_image.id
    orphan_emb = [e for e in embeddings if e["variant_image_id"] not in variant_image_ids]
    if orphan_emb:
        failmsg(f"{len(orphan_emb)} embeddings with missing variant_image FK")
        errors += 1
    else:
        passmsg("embedding.variant_image_id -> variant_image.id")

    # product.master_variant_id -> variant.id
    bad_mv = [p for p in products if p.get("master_variant_id") and p["master_variant_id"] not in variant_ids]
    if bad_mv:
        failmsg(f"{len(bad_mv)} products with missing master_variant FK")
        errors += 1
    else:
        passmsg("product.master_variant_id -> variant.id")

    # --- image types ---
    print("\n=== Image Types ===")
    search_imgs = [i for i in images if i.get("type") == "Search"]
    master_variant_ids = {v["id"] for v in variants if v.get("is_master")}
    search_variant_ids = {i["variant_id"] for i in search_imgs}
    non_master_search = search_variant_ids - master_variant_ids
    if non_master_search:
        failmsg(f"{len(non_master_search)} search images on non-master variants")
        errors += 1
    else:
        passmsg(f"Search images ({len(search_imgs)}) all on master variants")

    # --- embeddings match search images ---
    print("\n=== Embedding ↔ Search Image ===")
    emb_img_ids = {e["variant_image_id"] for e in embeddings}
    search_img_ids = {i["id"] for i in search_imgs}
    missing_emb = search_img_ids - emb_img_ids
    if missing_emb:
        failmsg(f"{len(missing_emb)} search images missing embeddings")
        errors += 1
    else:
        passmsg(f"All {len(search_imgs)} search images have embeddings")
    extra_emb = emb_img_ids - search_img_ids
    if extra_emb:
        failmsg(f"{len(extra_emb)} embeddings reference non-search images")
        errors += 1
    else:
        passmsg("All embeddings reference search images only")

    # --- dupe check ---
    print("\n=== Uniqueness ===")
    for label, data, key in [
        ("Products", products, "id"),
        ("Variants", variants, "id"),
        ("VariantImages", images, "id"),
        ("Taxons", taxons, "id"),
        ("StockItems", stock_items, "id"),
    ]:
        ids = [d[key] for d in data]
        dupes = len(ids) - len(set(ids))
        if dupes:
            failmsg(f"{label}: {dupes} duplicate {key}s")
            errors += 1
        else:
            passmsg(f"{label}: no duplicate {key}s")

    print(f"\n{'=' * 60}")
    if errors:
        printfail(f"VERIFICATION FAILED — {errors} error(s)")
        sys.exit(1)
    else:
        print("  VERIFICATION PASSED — all checks OK")
        print(f"{'=' * 60}")


if __name__ == "__main__":
    main()
