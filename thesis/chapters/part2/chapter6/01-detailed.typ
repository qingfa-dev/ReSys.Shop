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

The Create Product flow demonstrates the vertical slice pattern in action. The following sequence diagram shows the interactions between system components:

#figure(
  block(width: 100%, inset: 0.8em, stroke: 0.5pt + black, radius: 4pt)[
    ```text
    Admin          Admin SPA        Carter Endpoint    MediatR Pipeline    Handler           DbContext
     │                │                   │                   │                │                 │
     │  Fill form     │                   │                   │                │                 │
     │  + "Create"    │                   │                   │                │                 │
     ├───────────────>│                   │                   │                │                 │
     │                │  POST /api/catalog/admin/products      │                │                 │
     │                │  + JWT + X-CSRF   │                   │                │                 │
     │                ├──────────────────>│                   │                │                 │
     │                │                   │  sender.Send()    │                │                 │
     │                │                   ├──────────────────>│                │                 │
     │                │                   │                   │  Validate(req) │                 │
     │                │                   │                   ├───────────────>│                 │
     │                │                   │                   │                │                 │
     │           ┌────┴──── Validation fails ────┐            │                │                 │
     │           │ Result.Validation(errors)     │            │                │                 │
     │           │ 400 Bad Request               │            │                │                 │
     │           └───────────────────────────────┘            │                │                 │
     │                │                   │                   │                │                 │
     │           ┌────┴──── Validation passes ────┐           │                │                 │
     │                │                   │         │          │                │                 │
     │                │                   │         │  Handle(Command)          │                 │
     │                │                   │         ├─────────────────────────>│                 │
     │                │                   │         │          │   Query: slug exists?            │
     │                │                   │         │          ├────────────────────────────────>│
     │                │                   │         │          │            false  <──────────────┤
     │                │                   │         │          │   Product.Create()               │
     │                │                   │         │          ├────────────────────────────────>│
     │                │                   │         │          │   db.Products.Add(product)       │
     │                │                   │         │          │   SaveChanges()                  │
     │                │                   │         │          ├────────────────────────────────>│
     │                │                   │         │          │                                  │
     │                │                   │         │          │   Send(AddVariant.Command)       │
     │                │                   │         │          ├────────────────────────────────>│
     │                │                   │         │          │   Add variant + set IsMaster     │
     │                │                   │         │          ├────────────────────────────────>│
     │                │                   │         │          │                                  │
     │                │                   │         │          │   product.MasterVariantId = v.Id │
     │                │                   │         │          │   SaveChanges()                  │
     │                │                   │         │          ├────────────────────────────────>│
     │                │                   │         │          │                                  │
     │                │                   │         │  Result.Created(...)  │                     │
     │                │                   │         │<─────────────────────┤                      │
     │                │                   │  result  │                     │                      │
     │                │                   │<─────────┤                     │                      │
     │                │  201 Created      │         │                     │                      │
     │                │  {id,name,varId}  │         │                     │                      │
     │                │<──────────────────┤         │                     │                      │
     │           └──────────────────────────────────┘                     │                      │
    ```
  ],
  caption: [Sequence diagram: Create Product (Admin)],
)

*Design decisions*:

1. *FluentValidation short-circuit* -- `ValidationBehavior` stops the pipeline before the handler if rules fail.
2. *Nested command dispatch* -- `CreateProduct` dispatches `AddVariant` via `ISender`, demonstrating module-internal CQRS and the MediatR dispatch pattern.
3. *Two SaveChanges calls* -- product is saved first (to get its ID), then variant is added and saved. This is a deliberate trade-off for ID generation vs. batching.
4. *Exception mapping* -- any unhandled exception in the handler is caught by `ExceptionMappingBehavior` and converted to `Result.Unexpected`.

*Evidence*: `CreateProduct.cs:36-78`, `CreateProduct.Endpoint.cs:14-32`, `Validation.Behavior.cs:1-67`, `Exception.Behavior.cs:1-42`

=== Checkout Flow

The Checkout flow is the *highest-stakes business flow* in the system. The following sequence diagram shows the interactions between system components, including the critical transaction boundary:

#figure(
  block(width: 100%, inset: 0.8em, stroke: 0.5pt + black, radius: 4pt)[
    ```text
    Customer       Store SPA         Carter Endpoint    MediatR Pipeline    Handler           DbContext       Payment        Hangfire
     │                │                   │                   │                │                 │               │               │
     │  Review cart   │                   │                   │                │                 │               │               │
     │  + "Checkout"  │                   │                   │                │                 │               │               │
     ├───────────────>│                   │                   │                │                 │               │               │
     │                │  POST /api/ordering/storefront/cart/checkout           │                │               │               │
     │                │  + address + shipping + payment method                │                │               │               │
     │                ├──────────────────>│                   │                │                 │               │               │
     │                │                   │  sender.Send()    │                │                 │               │               │
     │                │                   ├──────────────────>│                │                 │               │               │
     │                │                   │                   │  Handle(Command)                 │               │               │
     │                │                   │                   ├─────────────────────────────────>│               │               │
     │                │                   │                   │                │  BEGIN TRANSACTION             │               │
     │                │                   │                   │                │  IsolationLevel.RepeatableRead  │               │
     │                │                   │                   │                ├────────────────────────────────>│               │
     │                │                   │                   │                │                                  │               │
     │                │                   │                   │                │  Query cart + line items         │               │
     │                │                   │                   │                ├────────────────────────────────>│               │
     │                │                   │                   │                │  Query stock for each variant    │               │
     │                │                   │                   │                ├────────────────────────────────>│               │
     │                │                   │                   │                │                                  │               │
     │           ┌────┴──── Stock insufficient ────┐          │                │               │               │               │
     │           │ Result.Conflict("Stock insufficient")       │                │               │               │               │
     │           │ 409 Conflict                  │          │                │               │               │               │
     │           └───────────────────────────────┘          │                │               │               │               │
     │                │                   │                   │                │                                  │               │
     │           ┌────┴──── Stock OK ──────────────┐         │                │               │               │               │
     │                │                   │         │         │                │               │               │               │
     │                │                   │         │         │  Generate OrderNumber           │               │               │
     │                │                   │         │         │  (inside transaction)           │               │               │
     │                │                   │         │         │                │                 │               │               │
     │                │                   │         │         │  INSERT Order + LineItems       │               │               │
     │                │                   │         │         │  SaveChanges()                  │               │               │
     │                │                   │         │         ├────────────────────────────────>│               │               │
     │                │                   │         │         │                │                 │               │               │
     │                │                   │         │         │  CreatePaymentIntent(total, currency)           │               │
     │                │                   │         │         ├─────────────────────────────────────────────────>│               │
     │                │                   │         │         │                │                 │  clientSecret  │               │
     │                │                   │         │         │                │                 │  + intentId   │               │
     │                │                   │         │         │<──────────────────────────────────────────────────┤               │
     │                │                   │         │         │                │                 │               │               │
     │                │                   │         │         │  INSERT PaymentIntent record    │               │               │
     │                │                   │         │         │  SaveChanges()                  │               │               │
     │                │                   │         │         ├────────────────────────────────>│               │               │
     │                │                   │         │         │                │                 │               │               │
     │                │                   │         │         │  COMMIT TRANSACTION             │               │               │
     │                │                   │         │         ├────────────────────────────────>│               │               │
     │                │                   │         │         │                │                 │               │               │
     │                │                   │         │         │  Enqueue order confirmation email              │               │
     │                │                   │         │         ├─────────────────────────────────────────────────────────────>│
     │                │                   │         │         │                │                 │               │               │
     │                │                   │         │  Result.Created(orderDto)                │               │               │
     │                │                   │         │<─────────────────────────────────────────┤               │               │
     │                │                   │  result  │         │                │               │               │               │
     │                │                   │<─────────┤         │                │               │               │               │
     │                │  201 Created       │         │         │                │               │               │               │
     │                │  {orderId,orderNo, │         │         │                │               │               │               │
     │                │   clientSecret}    │         │         │                │               │               │               │
     │                │<──────────────────┤         │         │                │               │               │               │
     │           └──────────────────────────────────┘         │                │               │               │               │
    ```
  ],
  caption: [Sequence diagram: Checkout (Critical Path)],
)

*Design decisions*:

1. *RepeatableRead isolation* -- prevents phantom reads on inventory during high-concurrency checkout.
2. *Order number generation inside transaction* -- ensures uniqueness even with concurrent checkouts.
3. *Payment intent creation inside transaction* -- order and payment are ACID-consistent.
4. *Hangfire for async work* -- email confirmation is offloaded to a background job to reduce response latency.
5. *Stock check before order creation* -- prevents overselling.

*Evidence*: `git log: commit 887a77c7`, `CreateOrderFromCart.cs`, `PaymentIntent.cs`

=== Image Search Flow (CBIR -- Model-Agnostic)

The Content-Based Image Retrieval (CBIR) flow connects the frontend, backend, Python sidecar, and database:

#figure(
  block(width: 100%, inset: 0.8em, stroke: 0.5pt + black, radius: 4pt)[
    ```text
    Customer       Store SPA         Carter Endpoint    Handler           Embedding Sidecar   DbContext        HybridCache
     │                │                   │                │                    │                 │                │
     │  Upload        │                   │                │                    │                 │                │
     │  fashion image │                   │                │                    │                 │                │
     ├───────────────>│                   │                │                    │                 │                │
     │                │  POST /search-by-image             │                    │                 │                │
     │                │  multipart/form-data               │                    │                 │                │
     │                ├──────────────────>│                │                    │                 │                │
     │                │                   │  sender.Send() │                    │                 │                │
     │                │                   ├───────────────>│                    │                 │                │
     │                │                   │                │                    │                 │                │
     │           ┌────┴──── Cache hit ──────────────┐     │                    │                 │                │
     │                │                   │         │     │  TryGet(cacheKey)  │                 │                │
     │                │                   │         │     ├────────────────────────────────────────────────────>│
     │                │                   │         │     │                    │                 │  Cached results │
     │                │                   │         │     │<───────────────────────────────────────────────────┤
     │                │                   │         │     │                    │                 │                │
     │           ┌────┴──── Cache miss ─────────────┐     │                    │                 │                │
     │                │                   │         │     │                    │                 │                │
     │                │                   │         │     │  POST /embeddings  │                 │                │
     │                │                   │         │     │  {image, model}    │                 │                │
     │                │                   │         │     ├────────────────────>│                 │                │
     │                │                   │         │     │                    │                 │                │
     │                │                   │         │     │                    │  Model Registry  │                │
     │                │                   │         │     │                    │  resolves model  │                │
     │                │                   │         │     │                    │  (FashionCLIP)   │                │
     │                │                   │         │     │                    │                 │                │
     │                │                   │         │     │                    │  Lazy-load       │                │
     │                │                   │         │     │                    │  (if cold start) │                │
     │                │                   │         │     │                    │                 │                │
     │                │                   │         │     │                    │  Model-specific  │                │
     │                │                   │         │     │                    │  preprocessing   │                │
     │                │                   │         │     │                    │  (resize+norm)   │                │
     │                │                   │         │     │                    │                 │                │
     │                │                   │         │     │                    │  encode_image()  │                │
     │                │                   │         │     │                    │  (torch)         │                │
     │                │                   │         │     │                    │                 │                │
     │                │                   │         │     │  {embedding:[...], │                 │                │
     │                │                   │         │     │   model, vector_dim}                  │                │
     │                │                   │         │     │<────────────────────┤                 │                │
     │                │                   │         │     │                    │                 │                │
     │                │                   │         │     │  SELECT vi.*, v.sku, p.name           │                │
     │                │                   │         │     │  FROM variant_images vi               │                │
     │                │                   │         │     │  JOIN variants v ON ...               │                │
     │                │                   │         │     │  JOIN products p ON ...               │                │
     │                │                   │         │     │  WHERE model_name = 'fashion-clip'    │                │
     │                │                   │         │     │  ORDER BY embedding <=> @embedding    │                │
     │                │                   │         │     │  LIMIT 20                            │                │
     │                │                   │         │     ├──────────────────────────────────────>│                │
     │                │                   │         │     │                    │                 │  Top-K images   │
     │                │                   │         │     │<──────────────────────────────────────┤                │
     │                │                   │         │     │                    │                 │                │
     │                │                   │         │     │  Set(cacheKey, results, 5min)         │                │
     │                │                   │         │     ├────────────────────────────────────────────────────>│
     │                │                   │         │     │                    │                 │                │
     │           └──────────────────────────────────┘     │                    │                 │                │
     │                │                   │                │                    │                 │                │
     │                │                   │  Result<PagedResult<SearchResult>> │                 │                │
     │                │                   │<───────────────┤                    │                 │                │
     │                │  200 OK           │                │                    │                 │                │
     │                │  {products,       │                │                    │                 │                │
     │                │   similarityScores}│                │                    │                 │                │
     │                │<──────────────────┤                │                    │                 │                │
    ```
  ],
  caption: [Sequence diagram: Image Search (CBIR -- Model-Agnostic)],
)

*Design decisions*:

1. *Strategy Pattern in Sidecar* -- The sidecar maintains a registry of `BaseEmbeddingModel` subclasses. The active model is selected at runtime via `EMBEDDING_MODEL` env var or query parameter, enabling the comparative evaluation in Chapter 11.
2. *Model-specific preprocessing* -- Each model class encapsulates its own resize/normalize logic (e.g., Fashion-CLIP vs ResNet-50 may differ in interpolation or mean/std).
3. *Per-model database filtering* -- The SQL query includes `WHERE model_name = 'fashion-clip'` to ensure only embeddings from the same model are compared. This prevents comparing a Fashion-CLIP query vector against ResNet-50 catalog vectors (different semantic spaces).
4. *HybridCache* -- Repeated searches for the same image avoid re-computing the embedding.
5. *Cosine similarity (`<=>`)* -- pgvector operator optimized for normalized vectors from any model.

*Evidence*: `ImageEmbedding.Inference.cs:21-36`, `Vector.Configuration.cs`, `service/Embedding/src/services/embedding_service.py`, `ApiTests/Catalog/Storefront/search-by-image.http`

== Class Diagrams

=== Result Type Hierarchy

The `Result<T>` type is the universal return type for all domain operations. It encapsulates either a success value or a collection of errors:

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    align: (start, start, start),
    [*Class*], [*Properties*], [*Methods*],
    [`Result<T>`], [`IsSuccess: bool`, `IsFailure: bool`, `StatusCode: int`, `Errors: List<Error>`, `Value: T?`], [`ToResult() → IResult`],
    [`Result` `(static)`], [`IsSuccess: bool`, `IsFailure: bool`, `StatusCode: int`, `Errors: List<Error>`], [`Created<T>(T) → Result<T>`, `Ok<T>(T) → Result<T>`, `NotFound(code)`, `Conflict(code)`, `Validation(code)`, `Unexpected(code)`],
    [`Error`], [`Code: string`, `Message: string`, `Type: ErrorType`, `Metadata: Dictionary?`], [`implicit operator Result(Error)`],
    [`PagedResult<T>`], [`Data: List<T>`, `PageNumber: int`, `PageSize: int`, `TotalCount: int`, `TotalPages: int`, `HasPreviousPage: bool`, `HasNextPage: bool`], [---],
  ),
  caption: [Result type hierarchy],
)

*Relationships*: `Result<T>` uses factory methods on the static `Result` class. `Error` has an implicit conversion to `Result`. `PagedResult<T>` is a specialization for paginated responses.

*Evidence*: `Shared/Application/Models/Results/Result.cs`, `Shared/Application/Models/Errors/Error.cs`

=== Pipeline Behavior Hierarchy

The MediatR pipeline behaviors all implement `IPipelineBehavior<TRequest, TResponse>`:

#figure(
  table(
    columns: (auto, auto, 1fr),
    align: (start, start, start),
    [*Class*], [*Interface*], [*Key Details*],
    [`IPipelineBehavior<TRequest, TResponse>`], [`<<interface>>`], [`Handle(TRequest, RequestHandlerDelegate<TResponse>) → Task<TResponse>`],
    [`LoggingBehavior<TReq, TRes>`], [implements `IPipelineBehavior`], [Logs request entry/exit with CorrelationId, elapsed ms; outermost behavior],
    [`ValidationBehavior<TReq, TRes>`], [implements `IPipelineBehavior`], [Runs FluentValidation rules; short-circuits with `Result.Validation` on failure; holds `IValidator[]`],
    [`ExceptionMappingBehavior<TReq, TRes>`], [implements `IPipelineBehavior`], [Catches unhandled exceptions; returns `Result.Unexpected`; innermost behavior (catch-all)],
    [`ICommand<TResponse>`], [`<<interface>>`], [Marker interface for commands],
    [`IQuery<TResponse>`], [`<<interface>>`], [Marker interface for queries],
    [`ICommandHandler<TCmd, TRes>`], [`<<interface>>`], [`Handle(TCommand, CancellationToken) → Task<TResponse>`],
    [`IQueryHandler<TQuery, TRes>`], [`<<interface>>`], [`Handle(TQuery, CancellationToken) → Task<TResponse>`],
  ),
  caption: [MediatR pipeline behavior class hierarchy],
)

*Relationships*: The three concrete behaviors implement `IPipelineBehavior` (Decorator chain). `ICommandHandler` processes `ICommand` instances; `IQueryHandler` processes `IQuery` instances.

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

#figure(
  table(
    columns: (auto, auto, 1fr),
    align: (start, start, start),
    [*Class*], [*Key Properties*], [*Methods*],
    [*BaseEmbeddingModel* `(abstract)`], [`model_name: str`, `vector_dim: int`, `_model: Any`, `_preprocess: Any`, `_device: str`], [`__init__()`, `warmup()`, `is_loaded()`, `encode_image(PIL.Image) → np.ndarray`, `_load()` `(abstract)`, `_ensure_loaded()`, `_to_numpy()`, `_l2_normalize()`],
    [*FashionCLIPModel*], [`model_name = "fashion-clip"`, `vector_dim = 512`], [`_load()` — open_clip, `encode_image()`],
    [*ResNet50Model*], [`model_name = "resnet50"`, `vector_dim = 2048`], [`_load()` — torchvision, `encode_image()`],
    [*EfficientNetB0Model*], [`model_name = "efficientnet_b0"`, `vector_dim = 1280`], [`_load()` — timm, `encode_image()`],
    [*CLIPGenericModel*], [`model_name = "clip"`, `vector_dim = 512`], [`_load()` — transformers, `encode_image()`],
    [*EmbeddingService*], [`MODEL_REGISTRY: dict`, `_loaded_models: dict`], [`get_model(name)`, `list_available_models()`, `encode_image(image, model)`, `warmup_default_model()`],
  ),
  caption: [Embedding model class hierarchy — Strategy pattern],
)

*Relationships*: `FashionCLIPModel`, `ResNet50Model`, `EfficientNetB0Model`, and `CLIPGenericModel` all inherit from `BaseEmbeddingModel`. `EmbeddingService` uses the `MODEL_REGISTRY` to resolve model classes by string identifier at runtime.

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
