=== ML Sidecar and CBIR Search

The Python machine learning sidecar is the core research contribution of this thesis. It generates high-dimensional vector embeddings from product images and serves them to the .NET backend via HTTP, managing multiple pre-trained deep learning models through a strategy pattern that enables runtime model switching.

==== Model Management and Strategy Pattern

===== Environment-Driven Selection

The active embedding model is selected through a single `EMBEDDING_MODEL` environment variable. The Model Manager reads this identifier on first inference, instantiates the target class, loads weight snapshots from disk, and caches the instance in memory.

*Pluggable model architecture:* swapping the active inference model requires updating one environment variable and restarting the container; a pure configuration change with no code modification. This mechanism enables the automated benchmarking and production A/B testing workflows described in Section 3.2.

===== Model Registry

Six concrete models spanning four architectural families are registered through a decorator-based registry:

```python
class ModelRegistry:
    _registry: dict[str, type[BaseEmbedder]] = {}

    @classmethod
    def register(cls, name: str, metadata: dict | None = None):
        def decorator(model_cls: type[BaseEmbedder]):
            cls._registry[name] = model_cls
            return model_cls
        return decorator

@ModelRegistry.register("fashion_clip", {
    "description": "CLIP fine-tuned on 700K+ fashion images",
    "dimensions": 512
})
class FashionCLIPEmbedder(CLIPEmbedder):
    ...
```

#figure(
  table(
    columns: (auto, auto, auto, 2fr),
    stroke: 0.5pt,
    align: (left, center, center, left),
    table.header([*Model ID*], [*Dim*], [*Architecture*], [*Source*]),
    [fashion_clip], [512], [ViT-B/32 + CLIP], [patrickjohncyh/fashion-clip (HuggingFace)],
    [clip_vit_b16], [512], [ViT-B/16 + CLIP], [OpenAI CLIP (torchvision)],
    [openclip-vit-b-32], [512], [ViT-B/32 + CLIP], [OpenCLIP (HuggingFace)],
    [efficientnet_b0], [1280], [EfficientNet-B0], [torchvision ImageNet1K_V1],
    [resnet50], [2048], [ResNet-50], [torchvision ResNet50_Weights.DEFAULT],
    [dinov2_vits14], [384], [ViT-S/14], [facebookresearch/dinov2 (torch.hub)],
  ),
  kind: table,
  caption: [Registered embedding models with output dimensionality and architectural family.],
) <tbl-registered-models>

===== Strategy Pattern and Template Method

All models conform to `BaseEmbedder`, an abstract class employing the Template Method pattern. The base class orchestrates the embedding pipeline; subclasses provide only the model-specific forward pass:

```python
class BaseEmbedder(ABC):
    @abstractmethod
    def _forward(self, image: torch.Tensor) -> torch.Tensor:
        """Model-specific forward pass."""

    async def extract(self, image_input) -> list[float]:
        image = self._load_image(image_input)    # 1. Load
        features = self._forward(image)           # 2. Forward
        return self._normalize(features)          # 3. Normalize
```

The `_load_image` method handles URLs, file paths, raw bytes, and PIL Images uniformly, converting all inputs to RGB tensors. The `_normalize` method L2-normalises the raw features to unit vectors and handles multiple output shapes: `image_embeds` for CLIP models, `pooler_output` for ViT models, and feature maps for CNN classifiers (via global average pooling).

Models load lazily on first request rather than at startup, preventing unnecessary GPU memory consumption. The first request after a cold start incurs the model load time (approx 125 ms for EfficientNet-B0, 5-6 seconds for CLIP-based models); subsequent requests serve from the cache at full inference speed.

===== Hardware Acceleration

The `device` property resolves the execution target at runtime with a priority chain: CUDA GPU, Apple MPS, CPU. On the benchmark workstation, only CPU was available; all reported inference times reflect CPU-only execution. The forward pass executes within `torch.no_grad()` to disable gradient computation, reducing memory usage by approximately 50 percent compared to training mode.

#line(length: 100%, stroke: 0.3pt + luma(200))

==== Embedding Generation Pipeline

#figure(
  table(
    columns: (auto, auto, 2.5fr),
    stroke: 0.5pt,
    align: (center + horizon, left, left),
    inset: 5pt,
    table.header([*Stage*], [*Component*], [*Description*]),
    [1], [Authentication], [Validates the `X-API-Key` header; rejects unauthorised calls with 401.],
    [2], [Model Selection], [Retrieves the active model from the cached registry; initialises from disk if unallocated.],
    [3], [Preprocessing], [Resizes to $224 times 224$ pixels, converts to tensor, applies ImageNet normalisation ($text("mean") = [0.485, 0.456, 0.406]$, $text("std") = [0.229, 0.224, 0.225]$).],
    [4], [Forward Pass], [Propagates the tensor through convolution or self-attention layers within `torch.no_grad()`.],
    [5], [Pooling], [Global average pooling collapses spatial dimensions to a fixed-length vector (512-dim for CLIP variants, 1280 for EfficientNet-B0, 2048 for ResNet-50).],
    [6], [Serialization], [L2-normalises the vector; packs it with model metadata and inference latency into a JSON response.],
  ),
  kind: table,
  caption: [Embedding generation pipeline stages.],
) <tbl-embedding-pipeline>

===== API Endpoints

#figure(
  table(
    columns: (auto, auto, 2.8fr),
    stroke: 0.5pt,
    align: (left + horizon, left, left),
    inset: 5pt,
    table.header([*Method*], [*Path*], [*Purpose*]),
    [POST], [`/embeddings/bytes`], [Accepts multipart image upload with optional `model` query parameter. Validates MIME type and file size; returns embedding vector with metadata.],
    [POST], [`/embeddings`], [Accepts an image URL in the request body for batch embedding during catalogue indexing.],
    [GET], [`/health`], [Readiness probe for the Aspire orchestrator. Validates CUDA/MPS availability, active model status, and synthetic forward pass.],
  ),
  kind: table,
  caption: [ML sidecar API endpoints. All inference endpoints require `X-API-Key` header authentication.],
) <tbl-api-endpoints>

The response shape mirrors the .NET Result pattern:

```json
{
  "value": {
    "vector": [0.023, -0.154, ..., 0.042],
    "model": "fashion_clip",
    "dimensions": 512,
    "inference_ms": 92.0
  },
  "isSuccess": true,
  "statusCode": 200
}
```

// [SCREENSHOT: fastapi-swagger-embeddings.png] FastAPI Swagger UI showing the POST /embeddings/bytes endpoint with file upload field, model dropdown, and response schema.

#line(length: 100%, stroke: 0.3pt + luma(200))

==== CBIR Search Flow

The end-to-end visual search pipeline spans four architectural layers: the Vue 3 storefront, .NET API backend, Python ML sidecar, and PostgreSQL with pgvector. @fig-cbir-sequence illustrates the cross-service sequence, engineered to complete within a 1-second total latency budget.

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/diagrams/P2S2.2.4_cbir-search-sequence.png", width: 100%),
  caption: [CBIR search sequence: end-to-end flow spanning customer upload, embedding extraction, pgvector search, and ranked results rendering.],
) <fig-cbir-sequence>

The sequence crosses four layers: the Vue 3 storefront (client validation and result rendering), the .NET API (server validation and orchestration), the Python ML sidecar (embedding generation), and PostgreSQL with pgvector (vector similarity search). Each layer's role is detailed in the execution pipeline below.

===== Execution Pipeline

1. *Client Validation (Vue 3).* Validates file format (JPEG, PNG, WebP) and size ($<= 10$ MB). Dispatches a multipart form request to `POST /api/catalog/storefront/search-by-image`.
2. *Server Validation (.NET API).* Verifies binary header magic bytes to prevent file extension spoofing, validates MIME type, and reapplies the payload ceiling.
3. *Vector Extraction (ML Sidecar).* Forwards image bytes to `/embeddings`. The sidecar executes preprocessing and model inference (50-100 ms for CLIP-based models, 24 ms for EfficientNet-B0), returning a 512-dimensional float vector.
4. *Vector Index Search (pgvector).* Queries `product_image_embeddings` using the `<=>` cosine distance operator, filtered by `model_name`. HNSW indexing enables sub-10-millisecond logarithmic lookup.
5. *Post-Processing.* Converts cosine distances to similarity scores ($text("similarity") = 1 - text("distance")$), discards results below the configurable threshold (default $0.7$), and deduplicates by parent product.
6. *UI Rendering (Vue 3).* Receives a JSON payload with titles, prices, thumbnails, and similarity percentages; renders a product grid to complete the sub-second search.

===== Model Configuration and A/B Testing

The pluggable architecture supports two operational workflows:

- *Automated benchmarking.* Test scripts update the `EMBEDDING_MODEL` variable and restart the container, enabling sequential evaluation of all 11 candidate models without code modifications.
- *Production A/B testing.* Dual ML sidecar instances run in parallel with distinct model configurations. A traffic router directs user cohorts to each instance, enabling direct comparison of downstream business metrics (click-through rates, conversion rates, session duration).

// [SCREENSHOT: postman-embedding-request.png] API client showing a successful POST /embeddings/bytes request with an uploaded fashion image, receiving a 512-dimensional float vector and 92.0 ms inference time.

// [SCREENSHOT: aspire-dashboard-config.png] Aspire dashboard showing environment variable configuration with EMBEDDING_MODEL=fashion_clip and alternative values listed as comments.
