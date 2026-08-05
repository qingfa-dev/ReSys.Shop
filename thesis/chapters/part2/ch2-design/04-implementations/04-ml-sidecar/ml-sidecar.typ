=== ML Sidecar and CBIR Search

The Python ML sidecar generates vector embeddings from product images over HTTP, managing multiple pre-trained models through a strategy pattern switchable at runtime via `EMBEDDING_MODEL`.

==== Model Management

Six models span four architectures, selected from a decorator-based registry on first inference:

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

All models conform to `BaseEmbedder` via the Template Method pattern. The base class orchestrates loading, forwarding, and L2-normalisation; subclasses provide only the forward pass. Models load lazily on first request within `torch.no_grad()`. The `device` property resolves CUDA, MPS, or CPU at runtime.

```python
class BaseEmbedder(ABC):
    @abstractmethod
    def _forward(self, image: torch.Tensor) -> torch.Tensor:
        """Model-specific forward pass."""

    async def extract(self, image_input) -> list[float]:
        image = self._load_image(image_input)
        features = self._forward(image)
        return self._normalize(features)
```

`_load_image` handles URLs, paths, raw bytes, and PIL Images, converting to RGB tensors. `_normalize` applies L2-normalisation and handles CLIP `image_embeds`, ViT `pooler_output`, and CNN feature maps via global average pooling.

==== Embedding Generation Pipeline

Six pipeline stages:

#figure(
  table(
    columns: (auto, auto, 2.5fr),
    stroke: 0.5pt,
    align: (center + horizon, left, left),
    inset: 5pt,
    table.header([*Stage*], [*Component*], [*Description*]),
    [1], [Authentication], [Validates `X-API-Key` header; rejects with 401.],
    [2], [Model Selection], [Retrieves active model from registry; initialises from disk if unallocated.],
    [3], [Preprocessing], [Resizes to $224 times 224$ px, converts to tensor, applies ImageNet normalisation.],
    [4], [Forward Pass], [Propagates through convolution or self-attention within `torch.no_grad()`.],
    [5], [Pooling], [Global average pooling to fixed-length vector (512-dim for CLIP, up to 2048 for ResNet-50).],
    [6], [Serialization], [L2-normalisation, JSON packaging with model metadata and inference latency.],
  ),
  kind: table,
  caption: [Embedding generation pipeline stages.],
) <tbl-embedding-pipeline>

API endpoints:

#figure(
  table(
    columns: (auto, auto, 2.8fr),
    stroke: 0.5pt,
    align: (left + horizon, left, left),
    inset: 5pt,
    table.header([*Method*], [*Path*], [*Purpose*]),
    [POST], [`/embeddings/bytes`], [Multipart image upload with optional `model` query parameter.],
    [POST], [`/embeddings`], [Image URL for batch embedding during catalogue indexing.],
    [GET], [`/health`], [Readiness probe: validates CUDA/MPS, active model, and synthetic forward pass.],
  ),
  kind: table,
  caption: [ML sidecar API endpoints. All inference endpoints require `X-API-Key` authentication.],
) <tbl-api-endpoints>

Response shape:

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

// [SCREENSHOT: fastapi-swagger-embeddings.png] FastAPI Swagger UI showing POST /embeddings/bytes with file upload, model dropdown, and response schema.

==== CBIR Search Flow

#figure(
  image("../../../../../figures/chapters/part2/ch2-design/04-implementations/diagrams/P2S2.2.4_cbir-search-sequence.png", width: 100%),
  caption: [CBIR search sequence: end-to-end flow spanning customer upload, embedding extraction, pgvector search, and ranked results rendering.],
) <fig-cbir-sequence>

Six stages across four architectural layers:

1. *Client Validation (Vue 3).* Validates format (JPEG, PNG, WebP) and size ($<= 10$ MB). Dispatches multipart form to `POST /api/catalog/storefront/search-by-image`.
2. *Server Validation (.NET API).* Verifies magic bytes, reapplies payload ceiling.
3. *Vector Extraction (ML Sidecar).* Forwards image bytes to `/embeddings`; sidecar executes preprocessing and inference.
4. *Vector Search (pgvector).* Queries via cosine distance `<=>` filtered by `model_name`; HNSW enables sub-10 ms lookup.
5. *Post-Processing.* Converts distance to similarity ($1 - text("distance")$), filters below $0.7$, deduplicates by product.
6. *UI Rendering (Vue 3).* Renders product grid with thumbnails, prices, and similarity scores within the sub-second budget.

The pluggable architecture supports automated benchmarking (update `EMBEDDING_MODEL`, restart, sequential evaluation of all models without code changes) and production A/B testing via dual sidecar instances with distinct models.

// [SCREENSHOT: postman-embedding-request.png] API client showing POST /embeddings/bytes with fashion image upload and 512-dim float vector response.
