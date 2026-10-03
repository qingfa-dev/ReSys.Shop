#!/usr/bin/env python
"""Generate stock transfer seed data covering all TransferState values."""
from __future__ import annotations

import argparse
import json
import random
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from json_io import check_overwrite, write_json  # noqa: E402
from shared import guid  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate stock transfer seed data")
    parser.add_argument("--output", type=Path, default=Path(__file__).resolve().parent / "output", help="Output directory")
    parser.add_argument("--seed", type=int, default=42, help="Random seed")
    parser.add_argument("--count", type=int, default=6, help="Number of transfers to generate")
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    check_overwrite(args.output / "012_demo_stock_transfers.json", args.force)

    variants_path = args.output / "006_demo_variants.json"
    if not variants_path.exists():
        print(f"ERROR: {variants_path} not found")
        sys.exit(1)

    random.seed(args.seed)
    variants = json.loads(variants_path.read_text())
    # Prefer non-master variants (real sellables)
    sellables = [v for v in variants if not v.get("is_master", False)]
    if len(sellables) < 4:
        sellables = variants

    # Deterministic demo transfers matching seeder expectations
    # Uses real variant ids and valid location codes (MAIN/EAST/EXPRESS)
    states_cycle = ["Draft", "InTransit", "Received", "Canceled"]
    pairs = [("MAIN", "EAST"), ("MAIN", "EXPRESS"), ("EAST", "EXPRESS"), ("EXPRESS", "MAIN")]

    transfers: list[dict] = []
    base_date = "2026-05-20T08:00:00Z"
    dates = [
        "2026-05-20T08:00:00Z",
        "2026-05-20T09:00:00Z",
        "2026-05-18T10:00:00Z",
        "2026-05-19T11:00:00Z",
        "2026-05-21T07:30:00Z",
        "2026-05-17T14:00:00Z",
    ]

    for i in range(args.count):
        state = states_cycle[i % len(states_cycle)]
        src, dst = pairs[i % len(pairs)]
        item_count = random.randint(1, 3)
        picked = random.sample(sellables, k=item_count)
        items = []
        for v in picked:
            qty = random.randint(5, 30)
            recv = qty if state == "Received" else 0
            items.append({"variant_id": v["id"], "quantity": qty, "received_quantity": recv})

        transfers.append({
            "id": guid("stock_transfer", f"transfer_{i+1}"),
            "number": f"T20260522-{1001 + i}",
            "reference": f"PO-2026-{i+1:03d}",
            "source_location_code": src,
            "destination_location_code": dst,
            "state": state,
            "created_at_utc": dates[i % len(dates)],
            "items": items,
        })

    write_json(args.output / "012_demo_stock_transfers.json", transfers)
    print(f"Written {len(transfers)} stock transfers")


if __name__ == "__main__":
    main()
