import numpy as np
from PIL import Image

from benchmark.models.clip_generic import ClipGenericModel


def test_clip_generic_name_and_dim():
    model = ClipGenericModel(device="cpu")
    assert model.name == "CLIP-generic"
    assert model.embedding_dim == 512


def test_clip_generic_embeds_image():
    model = ClipGenericModel(device="cpu")
    model.load()
    img = Image.new("RGB", (224, 224), color=(128, 64, 32))
    vec = model.embed(img)
    assert isinstance(vec, np.ndarray)
    assert vec.shape == (512,)
    assert vec.dtype == np.float32
    np.testing.assert_allclose(np.linalg.norm(vec), 1.0, rtol=1e-5)


def test_clip_generic_embed_batch():
    model = ClipGenericModel(device="cpu")
    model.load()
    images = [Image.new("RGB", (224, 224), color=c) for c in [(255, 0, 0), (0, 255, 0)]]
    batch = model.embed_batch(images)
    assert batch.shape == (2, 512)
    for row in batch:
        np.testing.assert_allclose(np.linalg.norm(row), 1.0, rtol=1e-5)
