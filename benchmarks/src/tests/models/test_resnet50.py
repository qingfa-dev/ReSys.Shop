import numpy as np
from PIL import Image

from benchmark.models.resnet50 import ResNet50Model


def test_resnet50_name_and_dim():
    model = ResNet50Model(device="cpu")
    assert model.name == "ResNet-50"
    assert model.embedding_dim == 2048


def test_resnet50_embeds_image():
    model = ResNet50Model(device="cpu")
    model.load()
    img = Image.new("RGB", (224, 224), color=(128, 64, 32))
    vec = model.embed(img)
    assert isinstance(vec, np.ndarray)
    assert vec.shape == (2048,)
    assert vec.dtype == np.float32
    np.testing.assert_allclose(np.linalg.norm(vec), 1.0, rtol=1e-5)


def test_resnet50_embed_batch():
    model = ResNet50Model(device="cpu")
    model.load()
    images = [Image.new("RGB", (224, 224), color=c) for c in [(255, 0, 0), (0, 255, 0)]]
    batch = model.embed_batch(images)
    assert batch.shape == (2, 2048)
    for row in batch:
        np.testing.assert_allclose(np.linalg.norm(row), 1.0, rtol=1e-5)
