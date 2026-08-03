#!/usr/bin/env python
"""Extract variant image entities from styles.csv (single entity: VariantImage).

Master variant carries Default/Search/Gallery images (unchanged IDs); each
child variant gets one Default image. Width/height/file_size are backfilled
by 10_process_images.py.
"""
from __future__ import annotations

import argparse
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
from ids import master_variant_id, variant_id, variant_image_id  # noqa: E402
from json_io import check_overwrite, ensure_output_dir, write_json  # noqa: E402
from metadata import build_image_alt  # noqa: E402
from shared import SCRIPTS_DIR  # noqa: E402
from source import (
    extract_product_metadata,
    group_products,
    load_style_json,
    load_styles_rows,
    sizes_for_colors,
    unique_colors,
)  # noqa: E402
from variants import generate_variants  # noqa: E402


def main() -> None:
    parser = argparse.ArgumentParser(description="Extract variant image seed data")
    parser.add_argument("--dataset", type=Path, required=True)
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--count", type=int, default=1000, help="Target number of product groups")
    parser.add_argument("--display-size", type=int, default=512)
    parser.add_argument("--search-size", type=int, default=224)
    parser.add_argument("--force", action="store_true")
    args = parser.parse_args()

    out = args.output / "007_demo_variant_images.json"
    check_overwrite(out, args.force)

    rows = load_styles_rows(args.dataset)
    images: list[dict] = []

    for display_name, group_rows in group_products(rows)[: args.count]:
        first = group_rows[0]
        benchmark_id = first.get("id", "").strip()

        master_id = master_variant_id(display_name)
        images.append({
            "id": variant_image_id(display_name, "0.default"),
            "variant_id": master_id,
            "content_type": "image/jpeg",
            "file_name": f"{benchmark_id}.jpg",
            "storage_path": str(Path("images") / "medium" / f"{benchmark_id}.jpg"),
            "position": 0,
            "alt": build_image_alt(display_name, "Default"),
            "type": "Default",
        })
        images.append({
            "id": variant_image_id(display_name, "0.search"),
            "variant_id": master_id,
            "content_type": "image/jpeg",
            "file_name": f"{benchmark_id}.jpg",
            "storage_path": str(Path("images") / "search" / str(args.search_size) / f"{benchmark_id}.jpg"),
            "position": 1,
            "alt": build_image_alt(display_name, "Search"),
            "type": "Search",
        })
        meta = extract_product_metadata(load_style_json(args.dataset, benchmark_id))
        gallery_labels = ["back", "front"]
        for gi, gl in enumerate(gallery_labels):
            s_images = meta.get("style_images")
            if s_images and gl in s_images:
                images.append({
                    "id": variant_image_id(display_name, f"0.{gl}"),
                    "variant_id": master_id,
                    "content_type": "image/jpeg",
                    "file_name": f"{benchmark_id}.jpg",
                    "storage_path": f"images/medium/{benchmark_id}.jpg",
                    "position": 2 + gi,
                    "alt": build_image_alt(display_name, "Gallery"),
                    "type": "Gallery",
                })

        colors = unique_colors(group_rows)
        sizes_by_color = sizes_for_colors(args.dataset, group_rows)
        for combo in generate_variants(display_name, colors, sizes_by_color):
            if combo["is_master"]:
                continue
            child_id = variant_id(display_name, combo["color"] or "", combo["size"] or "")
            images.append({
                "id": variant_image_id(display_name, f"{combo['color'] or 'NA'}.{combo['size'] or 'NA'}.default"),
                "variant_id": child_id,
                "content_type": "image/jpeg",
                "file_name": f"{benchmark_id}.jpg",
                "storage_path": str(Path("images") / "medium" / f"{benchmark_id}.jpg"),
                "position": 0,
                "alt": build_image_alt(display_name, "Default"),
                "type": "Default",
            })

    ensure_output_dir(args.output)
    write_json(out, images)
    print(f"Written {len(images)} variant images to {out}")


if __name__ == "__main__":
    main()
