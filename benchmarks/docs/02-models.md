# 02 — Models

All models supported by the benchmark, explained simply, with clear recommendations.

## The Models

### 1. Fashion-CLIP

**What it is:** CLIP (a vision-language model) that was fine-tuned specifically on 700,000+ fashion image-text pairs.

**How it works:** It learned to associate fashion images with text descriptions like "red floral summer dress." This means it "understands" fashion concepts — colors, patterns, styles, occasions.

**Vector size:** 512 dimensions

**Why use it:**
- Expected best performer on fashion data (it's literally trained for this)
- Understands semantic concepts, not just pixel patterns
- Can potentially be extended to text-to-image search

**When to avoid:**
- If you need the absolute fastest inference (it's a Transformer, slower than CNNs)

**Thesis role:** The **hypothesized winner** (H1). Fashion-specific fine-tuning should align embeddings with human fashion similarity judgments.

---

### 2. CLIP-generic (OpenAI CLIP ViT-B/32)

**What it is:** The original CLIP model from OpenAI, trained on 400 million general internet image-text pairs (cats, cars, food, landscapes, everything).

**How it works:** Same architecture as Fashion-CLIP, but trained on general images. It knows about "red" and "dress" but never saw fashion catalogs specifically.

**Vector size:** 512 dimensions

**Why use it:**
- Tests whether generic vision-language pretraining is "good enough"
- Much smaller download than Fashion-CLIP (same architecture, different weights)
- Widely studied baseline

**When to avoid:**
- If Fashion-CLIP is available (Fashion-CLIP is strictly better for this domain)

**Thesis role:** Tests H4 — "Does generic CLIP suffice without fashion fine-tuning?" Expected to underperform Fashion-CLIP but possibly outperform CNNs.

---

### 3. EfficientNet-B0

**What it is:** A convolutional neural network (CNN) designed by Google in 2019. It uses "compound scaling" — carefully balancing network depth, width, and resolution.

**How it works:** Classic computer vision. Learns edge detectors → textures → shapes → object parts. No text understanding, just pixels.

**Vector size:** 1280 dimensions

**Why use it:**
- State-of-the-art efficiency-accuracy trade-off for CNNs
- Much faster than Transformer models (CLIP, etc.)
- Smaller memory footprint
- Good baseline for "can we get decent results cheaply?"

**When to avoid:**
- If you need semantic understanding (it only sees pixels, not concepts)

**Thesis role:** Tests H2 — "Does EfficientNet achieve the best efficiency metric (mAP / ms)?" Compound scaling optimizes FLOPs-to-accuracy ratio.

---

### 4. ResNet-50

**What it is:** The most famous CNN architecture, introduced by Microsoft in 2015. Uses "residual connections" that skip layers, making very deep networks trainable.

**How it works:** Same CNN approach as EfficientNet, but older and simpler. 50 layers deep.

**Vector size:** 2048 dimensions

**Why use it:**
- Most widely cited baseline in computer vision literature
- Every paper compares against ResNet-50
- Simple, well-understood, reliable

**When to avoid:**
- It's slower and less accurate than EfficientNet-B0 on most tasks
- Large vector size (2048) wastes storage

**Thesis role:** Classic baseline (H3). ResNet-50 has the highest storage cost per embedding (2048-d vectors = 4× storage of 512-d vectors). Included because it's the standard CNN baseline in fashion retrieval literature.

---

### 5. Other Models (Not in Thesis)

These are available in the general benchmark but not used in the thesis protocol:

| Model | Why It's Interesting | Why Not in Thesis |
|-------|---------------------|-------------------|
| **SigLIP** | Google model with better training than CLIP | Too new, not in original thesis plan |
| **EVA-CLIP** | Large model with excellent general performance | Too computationally expensive |
| **DINOv2** | Self-supervised, learns structure without labels | Not a standard baseline |
| **ConvNeXt** | Modern CNN that rivals Transformers | Not a classic baseline |
| **CLIP ViT-B/16** | Higher-resolution CLIP variant | Thesis uses ViT-B/32 as "generic CLIP" |

---

## Model Comparison Summary

| Model | Type | Vector Size | Speed | Fashion-Specific? | Thesis? |
|-------|------|-------------|-------|-------------------|---------|
| Fashion-CLIP | Vision-Language | 512 | Medium | ✅ Yes | ✅ |
| CLIP-generic | Vision-Language | 512 | Medium | ❌ No | ✅ |
| EfficientNet-B0 | CNN | 1280 | Fast | ❌ No | ✅ |
| ResNet-50 | CNN | 2048 | Slow | ❌ No | ✅ |
| SigLIP | Vision-Language | 768 | Medium | ❌ No | ❌ |
| EVA-CLIP | Vision-Language | 768 | Slow | ❌ No | ❌ |

---

## Recommendations

### For the Thesis
Use exactly the 4 thesis models. No substitutions. The thesis hypothesis tests are designed for these specific models.

### For Production (Real Shop)
1. **Start with Fashion-CLIP** — best accuracy for fashion
2. **If latency is critical, benchmark EfficientNet-B0** — may be "good enough" at 2-3× speed
3. **Measure actual user behavior** — high mAP doesn't always mean better UX

### For Experimentation
```bash
# Quick comparison of all models
uv run benchmark benchmark --models all --dataset deepfashion --k 10

# Just the fast CNNs
uv run benchmark benchmark --models efficientnet-b0,resnet50 --dataset deepfashion

# Just the CLIP variants
uv run benchmark benchmark --models fashion-clip,clip-b32,siglip --dataset deepfashion
```

---

## How Models Are Implemented

Each model has an **adapter** — a Python class that wraps the underlying PyTorch model:

```python
class FashionClipModel(EmbeddingModel):
    @property
    def name(self) -> str:
        return "FashionCLIP"

    @property
    def embedding_dim(self) -> int:
        return 512

    def load(self) -> None:
        # Download weights from HuggingFace
        self._model = CLIPModel.from_pretrained("patrickjohncyh/fashion-clip")

    def embed(self, image: Image.Image) -> np.ndarray:
        # Turn one PIL image into a normalized vector
        ...
```

All adapters live in `src/benchmark/models/`. Adding a new model requires only one new file.

---

## References

- Fashion-CLIP: Chia et al., "Contrastive Language-Image Pre-Training for the Open-World Fashion Challenge", SIGIR 2022
- CLIP: Radford et al., "Learning Transferable Visual Models From Natural Language Supervision", ICML 2021
- EfficientNet: Tan & Le, "EfficientNet: Rethinking Model Scaling for Convolutional Neural Networks", ICML 2019
- ResNet: He et al., "Deep Residual Learning for Image Recognition", CVPR 2016
