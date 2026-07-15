#!/usr/bin/env python
"""Generate image embeddings via the Embedding sidecar for search-type images."""
from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from urllib.parse import urljoin

import httpx
from tqdm import tqdm

API_KEY = "dev-key-must-be-long-enough"

SCRIPTS_DIR = Path(__file__).resolve().parent


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate image embeddings")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output", help="Output directory")
    parser.add_argument("--base-url", default="http://localhost:8000", help="Embedding service URL")
    args = parser.parse_args()

    images_json = args.output / "demo_variant_images.json"
    if not images_json.exists():
        print(f"ERROR: {images_json} not found"); sys.exit(1)

    records = json.loads(images_json.read_text())
    search_records = [r for r in records if r.get("type") == "Search"]

    headers = {"X-API-Key": API_KEY}

    embeddings: list[dict] = []
    for rec in tqdm(search_records, desc="Generating embeddings"):
        storage_path = rec["storage_path"]
        image_path = args.output / storage_path
        if not image_path.exists():
            print(f"  WARN: {image_path} not found, skipping")
            continue

        model_name = "fashion_clip"
        try:
            with open(image_path, "rb") as f:
                files = {"image": (image_path.name, f, "image/jpeg")}
                data = {"model": model_name}
                resp = httpx.post(
                    urljoin(args.base_url, "/embeddings/bytes"),
                    headers=headers, files=files, data=data, timeout=30,
                )
            if resp.status_code != 200:
                print(f"  WARN: Embedding API returned {resp.status_code} for {storage_path}")
                continue
            result = resp.json()
            if not result.get("isSuccess"):
                print(f"  WARN: Embedding failed for {storage_path}: {result.get('errors')}")
                continue
            value = result["value"]
            embeddings.append({
                "variant_image_id": rec["id"],
                "model_name": model_name,
                "model_version": value["model_version"],
                "vector": value["vector"],
                "dimensions": value["dimension"],
            })
        except httpx.ConnectError:
            print("ERROR: Cannot connect to embedding service. Is it running?")
            print("  Start with: cd service/Embedding && uv run python src/main.py")
            sys.exit(1)
        except Exception as e:
            print(f"  WARN: {storage_path}: {e}")
            continue

    (args.output / "demo_embeddings.json").write_text(json.dumps(embeddings, indent=2))
    print(f"Written {len(embeddings)} embeddings")


if __name__ == "__main__":
    main()
