from benchmark.models import get_registry


def test_thesis_models_present():
    reg = get_registry(device="cpu")
    thesis_keys = {"fashion_clip", "resnet50", "efficientnet_b0", "clip_generic"}
    for key in thesis_keys:
        assert key in reg, f"Missing thesis model: {key}"
