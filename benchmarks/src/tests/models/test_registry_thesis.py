from benchmark.models import get_registry


def test_thesis_models_present():
    reg = get_registry(device="cpu")
    thesis_keys = {"fashion-clip", "resnet-50", "efficientnet-b0", "clip-generic"}
    for key in thesis_keys:
        assert key in reg, f"Missing thesis model: {key}"
