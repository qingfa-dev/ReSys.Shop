"""
Vision model export implementations.
"""
import torch
from PIL import Image
from embedding.core.constants import Constants
from scripts.export.base import get_model_path, verify_export


def export_efficientnet():
    """Exports EfficientNet-B0 to ONNX."""
    print("📦 Exporting EfficientNet-B0...")
    from torchvision import models
    model = models.efficientnet_b0(weights=models.EfficientNet_B0_Weights.IMAGENET1K_V1).eval()
    model.classifier = torch.nn.Identity()
    
    dummy_input = torch.randn(1, 3, 224, 224)
    output_path = get_model_path("efficientnet_b0")
    
    torch.onnx.export(
        model,
        dummy_input,
        str(output_path),
        input_names=["input"],
        output_names=["output"],
        dynamic_axes={"input": {0: "batch"}, "output": {0: "batch"}},
        opset_version=17,
        dynamo=False
    )
    verify_export(output_path)


def export_clip():
    """Exports OpenAI CLIP ViT-B/16 to ONNX."""
    print("📦 Exporting OpenAI CLIP ViT-B/16...")
    from transformers import CLIPModel, CLIPProcessor
    model_id = "openai/clip-vit-base-patch32"
    
    model = CLIPModel.from_pretrained(model_id).eval()
    processor = CLIPProcessor.from_pretrained(model_id)
    vision_model = model.vision_model
    
    dummy_image = Image.new("RGB", (224, 224))
    inputs = processor(images=dummy_image, return_tensors="pt")
    pixel_values = inputs["pixel_values"]
    
    output_path = get_model_path("clip_vit_b16")
    
    torch.onnx.export(
        vision_model,
        (pixel_values,),
        str(output_path),
        input_names=["pixel_values"],
        output_names=["last_hidden_state", "output"],
        dynamic_axes={"pixel_values": {0: "batch"}, "output": {0: "batch"}},
        opset_version=17,
        dynamo=False
    )
    verify_export(output_path)


def export_fashion_clip():
    """Exports the Vision Transformer component of Fashion-CLIP to ONNX."""
    print("📦 Exporting Fashion-CLIP (Vision only)...")
    from transformers import CLIPModel, CLIPProcessor
    model_id = "patrickjohncyh/fashion-clip"
    
    model = CLIPModel.from_pretrained(model_id).eval()
    processor = CLIPProcessor.from_pretrained(model_id)
    vision_model = model.vision_model
    
    dummy_image = Image.new("RGB", (224, 224))
    inputs = processor(images=dummy_image, return_tensors="pt")
    pixel_values = inputs["pixel_values"]
    
    output_path = get_model_path("fashion_clip")
    
    torch.onnx.export(
        vision_model,
        (pixel_values,),
        str(output_path),
        input_names=["pixel_values"],
        output_names=["last_hidden_state", "output"],
        dynamic_axes={"pixel_values": {0: "batch"}, "output": {0: "batch"}},
        opset_version=17,
        dynamo=False
    )
    verify_export(output_path)


def export_dinov2():
    """Exports DINOv2 ViT-S/14 to ONNX."""
    print("📦 Exporting DINOv2 ViT-S/14...")
    
    class DinoV2Wrapper(torch.nn.Module):
        def __init__(self):
            super().__init__()
            self.model = torch.hub.load("facebookresearch/dinov2", "dinov2_vits14", pretrained=True).eval()
        def forward(self, x):
            return self.model(x)

    model = DinoV2Wrapper().eval()
    dummy_input = torch.randn(1, 3, 224, 224)
    output_path = get_model_path("dinov2_vits14")
    
    torch.onnx.export(
        model,
        (dummy_input,),
        str(output_path),
        input_names=["input"],
        output_names=["output"],
        dynamic_axes={"input": {0: "batch"}, "output": {0: "batch"}},
        opset_version=17,
        dynamo=False
    )
    verify_export(output_path)
