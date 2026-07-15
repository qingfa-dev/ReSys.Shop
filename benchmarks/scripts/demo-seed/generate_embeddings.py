#!/usr/bin/env python
"""Generate image embeddings using local PyTorch models."""
from __future__ import annotations

import argparse
import json
from pathlib import Path

import numpy as np
import torch
from PIL import Image
from tqdm import tqdm

SCRIPTS_DIR = Path(__file__).resolve().parent
DEFAULT_MODELS = ["fashion_clip", "efficientnet_b0", "clip_vit_b16", "dinov2_vits14"]


def load_model(model_id: str):
    if model_id == "fashion_clip":
        from transformers import CLIPModel, CLIPProcessor
        model = CLIPModel.from_pretrained("patrickjohncyh/fashion-clip")
        processor = CLIPProcessor.from_pretrained("patrickjohncyh/fashion-clip")
        model.eval()
        def embed(img):
            inputs = processor(images=img, return_tensors="pt")
            with torch.no_grad():
                return model.get_image_features(**inputs).pooler_output.squeeze().numpy()
        return embed, 512, "patrickjohncyh/fashion-clip"

    elif model_id == "efficientnet_b0":
        from torchvision.models import efficientnet_b0, EfficientNet_B0_Weights
        weights = EfficientNet_B0_Weights.DEFAULT
        model = efficientnet_b0(weights=weights)
        model.classifier = torch.nn.Identity()
        model.eval()
        preprocess = weights.transforms()
        def embed(img):
            tensor = preprocess(img).unsqueeze(0)
            with torch.no_grad():
                return model(tensor).squeeze().numpy()
        return embed, 1280, "torchvision/efficientnet_b0"

    elif model_id == "clip_vit_b16":
        from transformers import CLIPModel, CLIPProcessor
        model = CLIPModel.from_pretrained("openai/clip-vit-base-patch32")
        processor = CLIPProcessor.from_pretrained("openai/clip-vit-base-patch32")
        model.eval()
        def embed(img):
            inputs = processor(images=img, return_tensors="pt")
            with torch.no_grad():
                return model.get_image_features(**inputs).pooler_output.squeeze().numpy()
        return embed, 512, "openai/clip-vit-base-patch32"

    elif model_id == "dinov2_vits14":
        model = torch.hub.load("facebookresearch/dinov2", "dinov2_vits14")
        model.eval()
        from torchvision import transforms
        preprocess = transforms.Compose([
            transforms.Resize(256, interpolation=transforms.InterpolationMode.BICUBIC),
            transforms.CenterCrop(224),
            transforms.ToTensor(),
            transforms.Normalize(mean=[0.485, 0.456, 0.406], std=[0.229, 0.224, 0.225]),
        ])
        def embed(img):
            tensor = preprocess(img).unsqueeze(0)
            with torch.no_grad():
                return model(tensor).squeeze().numpy()
        return embed, 384, "facebook/dinov2-vits14"

    else:
        raise ValueError(f"Unknown model: {model_id}")


def normalize(vector: np.ndarray) -> list[float]:
    norm = np.linalg.norm(vector)
    if norm == 0:
        return vector.tolist()
    return (vector / norm).tolist()


def main() -> None:
    parser = argparse.ArgumentParser(description="Generate image embeddings locally")
    parser.add_argument("--output", type=Path, default=SCRIPTS_DIR / "output")
    parser.add_argument("--models", nargs="+", default=DEFAULT_MODELS)
    args = parser.parse_args()

    images_json = args.output / "demo_variant_images.json"
    if not images_json.exists():
        print(f"ERROR: {images_json} not found; run extract_products.py first")
        return

    records = json.loads(images_json.read_text())
    search_records = [r for r in records if r.get("type") == "Search"]

    if not search_records:
        print("No search images found. Skipping embedding generation.")
        return

    print(f"Generating embeddings for {len(search_records)} search images")
    print(f"Models: {args.models}")

    all_embeddings: list[dict] = []

    for model_id in args.models:
        print(f"\n--- Loading model: {model_id} ---")
        try:
            embed_fn, dimension, version = load_model(model_id)
        except Exception as e:
            print(f"  WARN: Cannot load {model_id}: {e}. Skipping.")
            continue

        for rec in tqdm(search_records, desc=f"  {model_id}"):
            img_path = args.output / rec["storage_path"]
            if not img_path.exists():
                continue

            try:
                img = Image.open(img_path).convert("RGB")
                vector = embed_fn(img)
                all_embeddings.append({
                    "variant_image_id": rec["id"],
                    "model_name": model_id,
                    "model_version": version,
                    "vector": normalize(vector),
                    "dimensions": dimension,
                })
            except Exception as e:
                print(f"  WARN: {rec['storage_path']}: {e}")
                continue

        del embed_fn
        if torch.cuda.is_available():
            torch.cuda.empty_cache()

    (args.output / "demo_embeddings.json").write_text(json.dumps(all_embeddings, indent=2))
    print(f"\nWritten {len(all_embeddings)} embeddings for {len(search_records)} images × {len(args.models)} models")


if __name__ == "__main__":
    main()
