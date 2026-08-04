#!/usr/bin/env python
"""Verify demo seed output JSON integrity — FKs, counts, cross-references, domain invariants."""
from __future__ import annotations

import argparse
import json
import sys
from collections import Counter
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from shared import SCRIPTS_DIR  # noqa: E402


def printfail(msg: str) -> None:
    print(f"  FAIL  {msg}")


def passmsg(msg: str) -> None:
    print(f"  PASS  {msg}")


def main() -> None:
    parser = argparse.ArgumentParser(description="Verify demo seed output JSON")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=0, help="Expected product count (0=skip check)")
    args = parser.parse_args()

    out = args.output
    print(f"Verifying output in {out}\n")

    errors = 0
    files = {
        "products": "005_demo_products.json",
        "variants": "006_demo_variants.json",
        "images": "007_demo_variant_images.json",
        "product_taxons": "008_demo_product_taxons.json",
        "taxonomies": "001_demo_taxonomies.json",
        "taxons": "002_demo_taxons.json",
        "option_types": "003_demo_option_types.json",
        "option_values": "004_demo_option_values.json",
        "stock_locations": "009_demo_stock_locations.json",
        "stock_items": "010_demo_stock_items.json",
        "embeddings": "012_demo_embeddings.json",
    }
    data: dict[str, list[dict]] = {}
    for key, name in files.items():
        fp = out / name
        if not fp.exists():
            printfail(f"MISSING: {name}")
            errors += 1
            data[key] = []
        else:
            data[key] = json.loads(fp.read_text())

    products = data["products"]
    variants = data["variants"]
    images = data["images"]
    refs = data["product_taxons"]
    taxons = data["taxons"]
    option_values = data["option_values"]
    stock_locations = data["stock_locations"]
    stock_items = data["stock_items"]
    embeddings = data["embeddings"]

    product_ids = {p["id"] for p in products}
    variant_ids = {v["id"] for v in variants}
    image_ids = {i["id"] for i in images}
    taxon_ids = {t["id"] for t in taxons}
    option_value_names = {v["name"] for v in option_values}
    location_codes = {loc["code"] for loc in stock_locations}
    search_image_ids = {i["id"] for i in images if i.get("type") == "Search"}

    # --- counts ---
    print("=== Counts ===")
    checks = [
        ("Products", len(products), args.count if args.count else ">=1"),
        ("Variants", len(variants), ">=1"),
        ("VariantImages", len(images), ">=2"),
        ("ProductTaxons", len(refs), ">=1"),
        ("Taxonomies", len(data["taxonomies"]), 3),
        ("Taxons", len(taxons), ">=3"),
        ("OptionTypes", len(data["option_types"]), 2),
        ("OptionValues", len(option_values), ">=2"),
        ("StockLocations", len(stock_locations), 3),
        ("StockItems", len(stock_items), ">=1"),
        ("Embeddings", len(embeddings), ">=1"),
    ]
    for label, actual, expected in checks:
        if isinstance(expected, int) and actual != expected:
            printfail(f"{label}: {actual} (expected {expected})")
            errors += 1
        elif isinstance(expected, str) and actual < 1:
            printfail(f"{label}: {actual} (expected at least 1)")
            errors += 1
        else:
            passmsg(f"{label}: {actual}")

    # --- FK integrity ---
    print("\n=== FK Integrity ===")
    checks_fk = [
        ("variant.product_id -> product.id", [v for v in variants if v["product_id"] not in product_ids]),
        ("image.variant_id -> variant.id", [i for i in images if i["variant_id"] not in variant_ids]),
        ("product_taxon.product_id -> product.id", [r for r in refs if r["product_id"] not in product_ids]),
        ("product_taxon.taxon_id -> taxon.id", [r for r in refs if r["taxon_id"] not in taxon_ids]),
        ("stock_item.variant_id -> variant.id", [s for s in stock_items if s["variant_id"] not in variant_ids]),
        ("stock_item.stock_location_code -> location.code", [s for s in stock_items if s["stock_location_code"] not in location_codes]),
        ("embedding.variant_image_id -> image.id", [e for e in embeddings if e["variant_image_id"] not in image_ids]),
        ("product.master_variant_id -> variant.id", [p for p in products if p.get("master_variant_id") and p["master_variant_id"] not in variant_ids]),
        ("assignment option_value_name -> option_value.name", [o for v in variants for o in v.get("option_values", []) if o["option_value_name"].lower() not in {n.lower() for n in option_value_names}]),
        ("embedding -> search image only", [e for e in embeddings if e["variant_image_id"] not in search_image_ids]),
        ("every search image has an embedding", [i for i in images if i.get("type") == "Search" and i["id"] not in {e["variant_image_id"] for e in embeddings}]),
    ]
    for label, bad in checks_fk:
        if bad:
            printfail(f"{len(bad)} {label}")
            errors += 1
        else:
            passmsg(label)

    # --- domain invariants ---
    print("\n=== Domain Invariants ===")

    per_product = Counter(v["product_id"] for v in variants)
    over_cap = {pid: n for pid, n in per_product.items() if n > 10}
    if over_cap:
        printfail(f"{len(over_cap)} products exceed 10 variants")
        errors += 1
    else:
        passmsg("every product has at most 10 variants")

    masters_bad_pos = [v for v in variants if v.get("is_master") and v.get("position") != 0]
    if masters_bad_pos:
        printfail(f"{len(masters_bad_pos)} master variants not at position 0")
        errors += 1
    else:
        passmsg("every master variant is at position 0")

    multi_type = [v for v in variants
                  if len(v.get("option_values", [])) > len({o["option_type_id"] for o in v.get("option_values", [])})]
    if multi_type:
        printfail(f"{len(multi_type)} variants with >1 value per option type")
        errors += 1
    else:
        passmsg("every variant has at most 1 value per option type")

    master_combo_dupes = 0
    for v in variants:
        if not v.get("is_master"):
            continue
        master_types = {o["option_type_id"]: o["option_value_name"] for o in v.get("option_values", [])}
        for other in variants:
            if other.get("is_master") or other["product_id"] != v["product_id"]:
                continue
            other_types = {o["option_type_id"]: o["option_value_name"] for o in other.get("option_values", [])}
            if master_types == other_types:
                master_combo_dupes += 1
    if master_combo_dupes:
        printfail(f"{master_combo_dupes} child variants duplicate the master combo")
        errors += 1
    else:
        passmsg("no child variant duplicates the master combo")

    pid_to_master = {p["id"]: p.get("master_variant_id") for p in products}
    bad_master_id = [v for v in variants if v.get("is_master") and pid_to_master.get(v["product_id"]) != v["id"]]
    if bad_master_id:
        printfail(f"{len(bad_master_id)} master variants mismatch product.master_variant_id")
        errors += 1
    else:
        passmsg("product.master_variant_id matches its master variant id")

    # --- uniqueness ---
    print("\n=== Uniqueness ===")
    for label, key, data_list in [
        ("Products", "id", products),
        ("Variants", "id", variants),
        ("Variants", "sku", variants),
        ("VariantImages", "id", images),
        ("Taxons", "id", taxons),
        ("StockItems", "id", stock_items),
        ("ProductTaxons", ("product_id", "taxon_id"), refs),
    ]:
        if isinstance(key, tuple):
            ids = [tuple(d[k] for k in key) for d in data_list]
        else:
            ids = [d[key] for d in data_list]
        dupes = len(ids) - len(set(ids))
        if dupes:
            printfail(f"{label}: {dupes} duplicate {key}s")
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
