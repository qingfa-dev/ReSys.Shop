#!/usr/bin/env python
"""Generate stock location, stock item, and stock movement seed data."""
from __future__ import annotations

import argparse
import json
import random
import sys
from pathlib import Path
from uuid import uuid5, NAMESPACE_DNS

SEED_NAMESPACE = uuid5(NAMESPACE_DNS, "resys.shop.demo-seed")

SCRIPTS_DIR = Path(__file__).resolve().parent


def guid(entity_type: str, name: str) -> str:
    return str(uuid5(SEED_NAMESPACE, f"{entity_type}.{name}"))


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract stock seed data")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output", help="Output directory")
    parser.add_argument("--seed", type=int, default=42, help="Random seed")
    args = parser.parse_args()

    variants_json = args.output / "demo_variants.json"
    if not variants_json.exists():
        print(f"ERROR: {variants_json} not found"); sys.exit(1)

    random.seed(args.seed)
    variants = json.loads(variants_json.read_text())

    locations = [
        {
            "id": guid("stock_location", "main_warehouse"),
            "name": "Main Warehouse",
            "presentation": "Main Warehouse",
            "code": "MAIN",
            "is_default": True, "active": True,
            "address1": "123 Commerce Blvd", "city": "New York",
            "postal_code": "10001", "phone": "+12025550100",
            "backorderable_default": True, "propagate_all_variants": True,
            "position": 0, "country_iso": "US",
        },
        {
            "id": guid("stock_location", "east_distribution"),
            "name": "East Distribution",
            "presentation": "East Distribution",
            "code": "EAST",
            "is_default": False, "active": True,
            "address1": "456 Peachtree St", "city": "Atlanta",
            "postal_code": "30301", "phone": "+14045550100",
            "backorderable_default": True, "propagate_all_variants": False,
            "position": 1, "country_iso": "US",
        },
        {
            "id": guid("stock_location", "express_hub"),
            "name": "Express Hub",
            "presentation": "Express Hub",
            "code": "EXPRESS",
            "is_default": False, "active": True,
            "address1": "789 Sunset Blvd", "city": "Los Angeles",
            "postal_code": "90001", "phone": "+13105550100",
            "backorderable_default": False, "propagate_all_variants": False,
            "position": 2, "country_iso": "US",
        },
    ]

    stock_items: list[dict] = []
    stock_movements: list[dict] = []

    for variant in variants:
        is_master = variant.get("is_master", False)
        base_qty = random.randint(0, 5) if is_master else random.randint(10, 200)
        ratios = {"MAIN": 1.0, "EAST": 0.4, "EXPRESS": 0.25}

        for loc in locations:
            qty = int(base_qty * ratios[loc["code"]])
            if qty <= 0:
                continue
            si_id = guid("stock_item", f"{variant['sku']}.{loc['code']}")
            stock_items.append({
                "id": si_id,
                "variant_id": variant["id"],
                "stock_location_code": loc["code"],
                "count_on_hand": qty,
                "backorderable": qty > 0,
            })
            stock_movements.append({
                "variant_id": variant["id"],
                "stock_location_code": loc["code"],
                "quantity": qty,
                "previous_count_on_hand": 0,
                "originator_type": "Adjustment",
                "reason": "Initial stock seeding",
                "action": "restock",
            })

    (args.output / "demo_stock_locations.json").write_text(json.dumps(locations, indent=2))
    (args.output / "demo_stock_items.json").write_text(json.dumps(stock_items, indent=2))
    (args.output / "demo_stock_movements.json").write_text(json.dumps(stock_movements, indent=2))

    print(f"Written {len(locations)} locations, {len(stock_items)} items, {len(stock_movements)} movements")


if __name__ == "__main__":
    main()
