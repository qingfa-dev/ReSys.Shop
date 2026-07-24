== Component-Level Design

=== MediatR Pipeline (Decorator Chain)

The pipeline is the backbone of the system's cross-cutting concern handling. It is registered in this order (outermost first):

```
LoggingBehavior<TRequest, TResponse>
  → ValidationBehavior<TRequest, TResponse>
    → ExceptionMappingBehavior<TRequest, TResponse>
      → Handler
```

*Behavior responsibilities*:

#figure(
  table(
    columns: (auto, auto, auto, auto),
    align: (start, start, start, start),
    [*Behavior*], [*Order*], [*Responsibility*], [*Short-circuit?*],
    [`LoggingBehavior`], [1 (outer)], [Log request entry/exit with CorrelationId, elapsed ms], [No],
    [`ValidationBehavior`], [2], [Run FluentValidation; return `Result.Validation` on failure], [Yes],
    [`ExceptionMappingBehavior`], [3], [Catch unhandled exceptions; return `Result.Unexpected`], [No (catch-all)],
  ),
  caption: [MediatR pipeline behavior responsibilities],
)

*Design rationale*: Validation is placed *inside* logging so that validation failures are still logged. Exception mapping is innermost to catch anything that leaks past validation.

*Evidence*: `Shared/Application/Mediators/Mediator.Extension.cs:46-50`

=== Create Product Flow

The Create Product flow demonstrates the vertical slice pattern in action. The following describes the interaction between system components:

1. The *Client* sends a `POST /api/catalog/admin/products` request to the *Carter Endpoint*
2. The *Carter Endpoint* dispatches a `CreateProduct` command via `ISender.Send()` to the *MediatR Pipeline*
3. The pipeline runs the *ValidationBehavior* which invokes the FluentValidation validator; on failure, returns early
4. On success, the *CreateProduct Handler* is invoked
5. The handler queries *Application DbContext* to verify slug uniqueness
6. The handler invokes the Product factory method to create the domain entity
7. The handler calls `SaveChanges()` to persist the Product
8. The handler dispatches an `AddVariant` nested command to create the master variant
9. The handler sets `MasterVariantId` on the Product and saves again
10. The handler maps the result to `CreateProduct.Response` via Mapster and returns 201 Created

*Evidence*: `CreateProduct.cs:36-78`, `CreateProduct.Endpoint.cs:14-32`

=== Checkout Flow

The Checkout flow orchestrates order creation, inventory reservation, and payment initiation:

1. The *Client* sends a `POST /api/ordering/storefront/cart/checkout` request to the *Carter Endpoint*
2. The *Carter Endpoint* dispatches a `CreateOrderFromCart` command via the MediatR pipeline
3. The *CreateOrderFromCart Handler* validates the cart is not empty and validates stock availability
4. The handler begins a `RepeatableRead` transaction on *Application DbContext*
5. The handler generates an order number, creates the Order and LineItems entities
6. The handler calls `SaveChanges()` to persist the order within the transaction
7. The handler invokes the *Payment Gateway* to create a PaymentIntent via the *Stripe API*
8. On success, the handler commits the transaction
9. The handler maps the result to the checkout response via Mapster and returns 201 Created

*Design decision*: Order number generation and all related inserts happen inside a `RepeatableRead` transaction to prevent phantom reads on inventory during high-concurrency checkout scenarios.

*Evidence*: `git log: commit 887a77c7`, `CreateOrderFromCart.cs`

=== Image Search Flow (CBIR — Model-Agnostic)

The Content-Based Image Retrieval flow connects the frontend, backend, Python sidecar, and database:

1. The *Client* uploads an image to the *Store SPA*
2. The *Store SPA* sends a `POST /search-by-image` multipart request to the *Catalog Backend (Search Handler)*
3. The *Catalog Backend* forwards the image bytes and `model=fashion-clip` parameter to the *Embedding Sidecar*
4. The *Embedding Sidecar* lazily initializes the requested model (Fashion-CLIP) and calls `encode_image()`
5. The sidecar returns a 512-dimensional embedding vector and model name to the *Catalog Backend*
6. The *Catalog Backend* executes a vector similarity query against *PostgreSQL + pgvector* using the cosine distance operator (`<=>`), filtering by model name and ordering by similarity
7. The top-K results are returned to the *Store SPA* and displayed to the client

*Model abstraction in sidecar*: The sidecar uses a *Strategy pattern* with a `BaseEmbeddingModel` abstract class. Each model (Fashion-CLIP, ResNet-50, EfficientNet-B0, CLIP-generic) is a concrete implementation loaded at runtime based on the `EMBEDDING_MODEL` environment variable. This enables swapping models without code changes.

*Evidence*: `ImageEmbedding.Inference.cs:21-36`, `ApiTests/Catalog/Storefront/search-by-image.http`

== Class Diagrams

=== Result Type Hierarchy

The `Result<T>` type is the universal return type for all domain operations. It encapsulates either a success value or a collection of errors:

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

*Evidence*: `Shared/Application/Models/Results/Result.cs`, `Shared/Application/Models/Errors/Error.cs`

=== Pipeline Behavior Hierarchy

The MediatR pipeline behaviors all implement `IPipelineBehavior<TRequest, TResponse>`:

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

*Evidence*: `Shared/Application/Mediators/Behaviours/Logging/Logging.Behaviours.cs`, `Validation/Validation.Behavior.cs`, `Exceptions/Exception.Behavior.cs`

== ML Service Workflow

The Python sidecar is a stateless FastAPI application that performs one primary function: image embedding generation. It is architected for *pluggability*: multiple pretrained models can be swapped at runtime via environment variable, supporting the comparative evaluation in Chapter 11.

=== Architecture

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

=== Embedding Model Class Hierarchy (Strategy Pattern)

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

*Model selection at runtime*:

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

=== Embedding Generation Flow

+ *Input*: Multipart image upload (JPEG/PNG/WebP) + optional `model` query parameter
+ *Model resolution*: `embedding_service.py` reads `model` param or `EMBEDDING_MODEL` env var → instantiates correct `BaseEmbeddingModel` subclass
+ *Preprocessing*: Model-specific preprocessing (e.g., Fashion-CLIP: 224×224; ResNet-50: 224×224; EfficientNet-B0: 224×224 with B0-specific normalization)
+ *Inference*: `model.encode_image(image)` → `np.ndarray` (variable dimension: 512, 2048, or 1280)
+ *Output*: JSON `{"embedding": [0.0123, ...], "model_name": "fashion-clip", "vector_dim": 512}`

*Evidence*: `service/Embedding/src/main.py:1-29`, `service/Embedding/pyproject.toml:15-16`

== Evidence

- `service/Api/src/Shared/Application/Mediators/Mediator.Extension.cs:1-79` --- pipeline registration
- `service/Api/src/Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs:1-67` --- validation behavior
- `service/Api/src/Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs:1-42` --- exception mapping
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs:36-78` --- handler logic
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` --- checkout handler
- `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.cs` --- embedding client
- `service/Embedding/src/main.py:1-29` --- Python service entry
- `service/Embedding/src/models/base_model.py` --- embedding model abstraction
- `service/Embedding/src/models/clip_model.py` --- Fashion-CLIP implementation
- `service/Embedding/src/models/resnet_model.py` --- ResNet-50 implementation
- `service/Embedding/src/models/efficientnet_model.py` --- EfficientNet-B0 implementation
- `service/Embedding/src/models/clip_generic_model.py` --- CLIP-generic implementation
- `service/Api/src/Shared/Application/Models/Results/Result.cs:1-43` --- Result type
