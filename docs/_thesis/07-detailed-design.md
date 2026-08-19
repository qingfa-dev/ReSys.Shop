# Chapter 7 — Detailed Design

## 7.1 Component-Level Design

### 7.1.1 MediatR Pipeline (Decorator Chain)

The pipeline is the backbone of the system's cross-cutting concern handling. It is registered in this order (outermost first):

```
LoggingBehavior<TRequest, TResponse>
  → ValidationBehavior<TRequest, TResponse>
    → ExceptionMappingBehavior<TRequest, TResponse>
      → Handler
```

**Behavior responsibilities**:

| Behavior | Order | Responsibility | Short-circuit? |
|----------|-------|----------------|----------------|
| `LoggingBehavior` | 1 (outer) | Log request entry/exit with CorrelationId, elapsed ms | No |
| `ValidationBehavior` | 2 | Run FluentValidation; return `Result.Validation` on failure | Yes |
| `ExceptionMappingBehavior` | 3 | Catch unhandled exceptions; return `Result.Unexpected` | No (catch-all) |

**Design rationale**: Validation is placed *inside* logging so that validation failures are still logged. Exception mapping is innermost to catch anything that leaks past validation.

**Evidence**: `Shared/Application/Mediators/Mediator.Extension.cs:46-50`

### 7.1.2 Sequence Diagram: Create Product

```
┌─────────┐  ┌─────────────┐  ┌──────────────┐  ┌─────────────┐  ┌──────────┐  ┌─────────┐
│ Client  │  │ Carter      │  │ MediatR      │  │ CreateProduct│  │ Application│  │ Python  │
│         │  │ Endpoint    │  │ Pipeline     │  │ Handler      │  │ DbContext  │  │ Sidecar │
└────┬────┘  └──────┬──────┘  └──────┬───────┘  └──────┬───────┘  └─────┬────┘  └────┬────┘
     │              │                │                 │                │          │
     │ POST /api/...│                │                 │                │          │
     │─────────────>│                │                 │                │          │
     │              │ Send(Command)  │                 │                │          │
     │              │───────────────>│                 │                │          │
     │              │                │ Validate()      │                │          │
     │              │                │───────────────>│ (if fail, return)│         │
     │              │                │ Handle()        │                │          │
     │              │                │────────────────>│                │          │
     │              │                │                 │ Check slug unique│          │
     │              │                │                 │───────────────>│          │
     │              │                │                 │ Query product  │          │
     │              │                │                 │<───────────────│          │
     │              │                │                 │ Create Product   │          │
     │              │                │                 │ (factory method) │          │
     │              │                │                 │ SaveChanges()    │          │
     │              │                │                 │───────────────>│          │
     │              │                │                 │ Send AddVariant  │          │
     │              │                │                 │───────────────>│          │
     │              │                │                 │ (nested command) │         │
     │              │                │                 │ Set MasterVariantId          │
     │              │                │                 │ SaveChanges()    │          │
     │              │                │                 │───────────────>│          │
     │              │                │                 │ Mapster → Response            │
     │              │                │<────────────────│                │          │
     │              │<───────────────│                │                │          │
     │ 201 Created  │                │                 │                │          │
     │<─────────────│                │                 │                │          │
```

**Evidence**: `CreateProduct.cs:36-78`, `CreateProduct.Endpoint.cs:14-32`

### 7.1.3 Sequence Diagram: Checkout

```
┌─────────┐  ┌─────────────┐  ┌─────────────────────┐  ┌─────────────┐  ┌─────────────┐  ┌──────────┐
│ Client  │  │ Carter      │  │ CreateOrderFromCart │  │ Application │  │ Payment     │  │ Stripe   │
│         │  │ Endpoint    │  │ Handler             │  │ DbContext   │  │ Gateway     │  │ API      │
└────┬────┘  └──────┬──────┘  └──────────┬──────────┘  └──────┬──────┘  └──────┬──────┘  └────┬────┘
     │              │                    │                    │                │            │
     │ POST checkout│                    │                    │                │            │
     │─────────────>│                    │                    │                │            │
     │              │ Send(Command)      │                    │                │            │
     │              │───────────────────>│                    │                │            │
     │              │                    │ Validate cart not empty            │            │
     │              │                    │ Validate stock available           │            │
     │              │                    │                    │                │            │
     │              │                    │ BeginTransaction(RepeatableRead) │            │
     │              │                    │───────────────────>│                │            │
     │              │                    │ Generate OrderNumber │                │            │
     │              │                    │ Create Order + LineItems           │            │
     │              │                    │ SaveChanges()      │                │            │
     │              │                    │───────────────────>│                │            │
     │              │                    │ Create PaymentIntent │              │            │
     │              │                    │─────────────────────>│                │            │
     │              │                    │                    │ POST /v1/payment_intents    │
     │              │                    │                    │────────────────────────────>│
     │              │                    │                    │                │            │
     │              │                    │                    │<────────────────────────────│
     │              │                    │<─────────────────────│                │            │
     │              │                    │ Commit Transaction │                │            │
     │              │                    │───────────────────>│                │            │
     │              │                    │ Mapster → Response │                │            │
     │              │<──────────────────│                    │                │            │
     │ 201 Created  │                    │                    │                │            │
     │<─────────────│                    │                    │                │            │
```

**Design decision**: Order number generation and all related inserts happen inside a `RepeatableRead` transaction to prevent phantom reads on inventory during high-concurrency checkout scenarios.

**Evidence**: `git log: commit 887a77c7`, `CreateOrderFromCart.cs`

### 7.1.4 Sequence Diagram: Image Search (CBIR — Model-Agnostic)

```
┌─────────┐  ┌─────────────┐  ┌──────────────────┐  ┌─────────────┐  ┌─────────────┐
│ Client  │  │ Store SPA   │  │ Catalog Backend  │  │ Embedding   │  │ PostgreSQL  │
│         │  │             │  │ (Search Handler) │  │ Sidecar     │  │ + pgvector  │
└────┬────┘  └──────┬──────┘  └────────┬─────────┘  └──────┬──────┘  └──────┬──────┘
     │              │                    │                   │                │
     │ Upload image │                    │                   │                │
     │─────────────>│                    │                   │                │
     │              │ POST /search-by-image (multipart)       │                │
     │              │───────────────────>│                   │                │
     │              │                    │ Forward image bytes│                │
     │              │                    │ + model=fashion-clip│                │
     │              │                    │──────────────────>│                │
     │              │                    │                   │ Load Fashion-  │
     │              │                    │                   │ CLIP model     │
     │              │                    │                   │ (lazy init)    │
     │              │                    │                   │ ↓              │
     │              │                    │                   │ encode_image() │
     │              │                    │<──────────────────│                │
     │              │                    │ vector + dim=512  │                │
     │              │                    │ + model_name      │                │
     │              │                    │                   │                │
     │              │                    │ SELECT ... WHERE model_name=$1      │
     │              │                    │ ORDER BY embedding <=> $2 LIMIT 20 │
     │              │                    │─────────────────────────────────────>│
     │              │                    │                   │                │
     │              │                    │<─────────────────────────────────────│
     │              │                    │ Top-K results     │                │
     │              │<──────────────────│                   │                │
     │ Results      │                    │                   │                │
     │<─────────────│                    │                   │                │
```

**Model abstraction in sidecar**: The sidecar uses a **Strategy pattern** with a `BaseEmbeddingModel` abstract class. Each model (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic) is a concrete implementation loaded at runtime based on the `EMBEDDING_MODEL` environment variable. This enables swapping models without code changes.

**Evidence**: `ImageEmbedding.Inference.cs:21-36`, `ApiTests/Catalog/Storefront/search-by-image.http`

## 7.2 Class Diagrams

### 7.2.1 Result Type Hierarchy

```
┌─────────────────┐
│   Result<T>     │
├─────────────────┤
│ + IsSuccess     │
│ + IsFailure     │
│ + StatusCode    │
│ + Errors[]      │
│ + Value?        │
│ + ToResult()    │
└────────┬────────┘
         │ implicit conversion
         ▼
┌─────────────────┐
│     Error       │
├─────────────────┤
│ + Code          │
│ + Message       │
│ + Type          │
│ + Metadata      │
└─────────────────┘
```

**Evidence**: `Shared/Application/Models/Results/Result.cs`, `Shared/Application/Models/Errors/Error.cs`

### 7.2.2 Pipeline Behavior Hierarchy

```
┌──────────────────────────────────────────┐
│ IPipelineBehavior<TRequest, TResponse>  │
└──────────────────────────────────────────┘
         ▲
         │ implements
    ┌────┴────┐
    │         │
┌───┴───┐ ┌───┴────────────────┐ ┌──────────────┐
│Logging│ │ValidationBehavior  │ │ExceptionMapping│
│Behavior│ │<TRequest,TResponse>│ │Behavior        │
└───────┘ └────────────────────┘ └──────────────┘
```

**Evidence**: `Shared/Application/Mediators/Behaviours/Logging/Logging.Behaviours.cs`, `Validation/Validation.Behavior.cs`, `Exceptions/Exception.Behavior.cs`

## 7.3 ML Service Workflow

The Python sidecar is a stateless FastAPI application that performs one primary function: image embedding generation. It is architected for **pluggability**: multiple pretrained models can be swapped at runtime via environment variable, supporting the comparative evaluation in Chapter 11.

### 7.3.1 Architecture

```
service/Embedding/
├── src/
│   ├── main.py              # FastAPI app, CORS, exception handlers
│   ├── routers/
│   │   ├── embedding_router.py   # POST /embeddings (accepts model param)
│   │   ├── health_router.py      # GET /health
│   │   └── model_router.py       # GET /models (lists available models)
│   ├── services/
│   │   └── embedding_service.py  # Delegates to BaseEmbeddingModel
│   ├── models/
│   │   ├── base_model.py         # Abstract base: encode_image() → np.ndarray
│   │   ├── clip_model.py         # Fashion-CLIP (open_clip) — 512-d
│   │   ├── resnet_model.py       # ResNet-50 (torchvision) — 2048-d
│   │   ├── efficientnet_model.py # EfficientNet-B0 (timm) — 1280-d
│   │   └── clip_generic_model.py # CLIP-generic (transformers) — 512-d
│   └── utils/
│       └── image_preprocessing.py # Resize, normalize, tensor conversion
└── tests/
    ├── unit/
    ├── integration/
    └── e2e/
```

### 7.3.2 Embedding Model Class Hierarchy (Strategy Pattern)

```
┌────────────────────────────────────────────────────────────┐
│                   BaseEmbeddingModel (ABC)                 │
├────────────────────────────────────────────────────────────┤
│ + model_name: str                                          │
│ + vector_dim: int                                          │
│ + __init__()                                               │
│ + encode_image(image: PIL.Image) -> np.ndarray           │
│ + warmup(): optional model preloading                      │
└─────────────────────┬──────────────────────────────────────┘
                      │ inherits
         ┌────────────┼────────────┬────────────────┐
         ▼            ▼            ▼                ▼
┌─────────────┐ ┌──────────┐ ┌──────────────┐ ┌─────────────┐
│ FashionCLIP │ │ ResNet50 │ │ EfficientNet │ │ CLIPGeneric │
│  Model      │ │  Model   │ │    B0 Model  │ │    Model    │
├─────────────┤ ├──────────┤ ├──────────────┤ ├─────────────┤
│ open_clip   │ │ torchvision│ │ timm       │ │ transformers │
│ 512-d       │ │ 2048-d    │ │  1280-d      │ │ 512-d        │
└─────────────┘ └──────────┘ └──────────────┘ └─────────────┘
```

**Model selection at runtime**:
```python
# From embedding_service.py
MODEL_REGISTRY = {
    "fashion-clip": FashionCLIPModel,
    "resnet50": ResNet50Model,
    "efficientnet_b0": EfficientNetB0Model,
    "clip": CLIPGenericModel,
}

def get_model() -> BaseEmbeddingModel:
    model_name = os.getenv("EMBEDDING_MODEL", "fashion-clip")
    return MODEL_REGISTRY[model_name]()
```

### 7.3.3 Embedding Generation Flow

1. **Input**: Multipart image upload (JPEG/PNG/WebP) + optional `model` query parameter
2. **Model resolution**: `embedding_service.py` reads `model` param or `EMBEDDING_MODEL` env var → instantiates correct `BaseEmbeddingModel` subclass
3. **Preprocessing**: Model-specific preprocessing (e.g., Fashion-CLIP: 224×224; ResNet-50: 224×224; EfficientNet-B0: 224×224 with B0-specific normalization)
4. **Inference**: `model.encode_image(image)` → `np.ndarray` (variable dimension: 512, 2048, or 1280)
5. **Output**: JSON `{"embedding": [0.0123, ...], "model_name": "fashion-clip", "vector_dim": 512}`

**Evidence**: `service/Embedding/src/main.py:1-29`, `service/Embedding/pyproject.toml:15-16`

## 7.4 Evidence

- `service/Api/src/Shared/Application/Mediators/Mediator.Extension.cs:1-79` — pipeline registration
- `service/Api/src/Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs:1-67` — validation behavior
- `service/Api/src/Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs:1-42` — exception mapping
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs:36-78` — handler logic
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — checkout handler
- `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.cs` — embedding client
- `service/Embedding/src/main.py:1-29` — Python service entry
- `service/Embedding/src/models/base_model.py` — embedding model abstraction
- `service/Embedding/src/models/clip_model.py` — Fashion-CLIP implementation
- `service/Embedding/src/models/resnet_model.py` — ResNet-50 implementation
- `service/Embedding/src/models/efficientnet_model.py` — EfficientNet-B0 implementation
- `service/Embedding/src/models/clip_generic_model.py` — CLIP-generic implementation
- `service/Api/src/Shared/Application/Models/Results/Result.cs:1-43` — Result type

---

## [ASK USER] Items

13. Should the sequence diagrams be formal UML (PlantUML / Mermaid), or are the ASCII representations acceptable?
14. Is there a need for a class diagram of the entire Module assembly, or are the representative hierarchies sufficient?
