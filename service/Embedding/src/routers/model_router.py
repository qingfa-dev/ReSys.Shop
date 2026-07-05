from fastapi import APIRouter

router = APIRouter()


@router.get("/models")
async def list_models():
    return {
        "value": [
            {
                "id": "openclip-vit-b-32",
                "name": "OpenCLIP ViT-B/32",
                "dimension": 512,
                "description": "Open-source CLIP model with ViT-B/32 backbone",
                "is_onnx": True,
                "tags": ["multimodal", "clip", "vit"],
            },
            {
                "id": "efficientnet_b0",
                "name": "EfficientNet B0",
                "dimension": 1280,
                "description": "Lightweight CNN backbone for image embeddings",
                "is_onnx": True,
                "tags": ["cnn", "edge", "fast"],
            },
            {
                "id": "siglip-vit-b-16",
                "name": "SigLIP ViT-B/16",
                "dimension": 768,
                "description": "Google SigLIP vision transformer model",
                "is_onnx": True,
                "tags": ["multimodal", "siglip", "vit"],
            },
            {
                "id": "fashion-clip-v1",
                "name": "Fashion CLIP v1",
                "dimension": 512,
                "description": "Fine-tuned CLIP model for fashion-domain embeddings",
                "is_onnx": True,
                "tags": ["fashion", "clip", "domain-specific"],
            },
            {
                "id": "dinov2-vit-small",
                "name": "DINOv2 ViT-S/14",
                "dimension": 384,
                "description": "Self-supervised vision transformer for visual similarity",
                "is_onnx": False,
                "tags": ["visual-similarity", "vit", "self-supervised"],
            },
            {
                "id": "convnext-v2-tiny",
                "name": "ConvNeXt V2 Tiny",
                "dimension": 768,
                "description": "Modern CNN architecture for edge deployment",
                "is_onnx": True,
                "tags": ["cnn", "edge", "cpu"],
            },
        ],
        "statusCode": 200,
        "isSuccess": True,
        "errors": [],
        "successMessage": None,
        "metadata": None,
    }
