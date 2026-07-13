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

### 7.1.4 Sequence Diagram: Image Search (CBIR)

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
     │              │                    │──────────────────>│                │
     │              │                    │                   │ Fashion-CLIP   │
     │              │                    │                   │ inference      │
     │              │                    │                   │ (torch)        │
     │              │                    │<──────────────────│                │
     │              │                    │ 512-d vector      │                │
     │              │                    │                   │                │
     │              │                    │ SELECT ... ORDER BY embedding <=> $1 │
     │              │                    │─────────────────────────────────────>│
     │              │                    │                   │                │
     │              │                    │<─────────────────────────────────────│
     │              │                    │ Top-K results     │                │
     │              │<──────────────────│                   │                │
     │ Results      │                    │                   │                │
     │<─────────────│                    │                   │                │
```

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

The Python sidecar is a stateless FastAPI application that performs one primary function: image embedding generation.

### 7.3.1 Architecture

```
service/Embedding/
├── src/
│   ├── main.py              # FastAPI app, CORS, exception handlers
│   ├── routers/
│   │   ├── embedding_router.py   # POST /embeddings
│   │   ├── health_router.py      # GET /health
│   │   └── model_router.py       # GET /models
│   ├── services/
│   │   └── embedding_service.py  # Fashion-CLIP inference
│   ├── models/
│   │   └── clip_model.py         # Model loading, caching
│   └── utils/
│       └── image_preprocessing.py # Resize, normalize, tensor conversion
└── tests/
    ├── unit/
    ├── integration/
    └── e2e/
```

### 7.3.2 Embedding Generation Flow

1. **Input**: Multipart image upload (JPEG/PNG/WebP)
2. **Preprocessing**: Resize to 224×224, normalize with ImageNet stats, convert to PyTorch tensor
3. **Inference**: `open_clip.encode_image()` → 512-dimensional float vector
4. **Output**: JSON array `[0.0123, -0.0456, ...]` (512 elements)

**Model**: Fashion-CLIP (pre-trained on fashion-specific image-text pairs) via `open-clip-torch` library.

**Evidence**: `service/Embedding/src/main.py:1-29`, `service/Embedding/pyproject.toml:15-16`

## 7.4 Evidence

- `service/Api/src/Shared/Application/Mediators/Mediator.Extension.cs:1-79` — pipeline registration
- `service/Api/src/Shared/Application/Mediators/Behaviours/Validation/Validation.Behavior.cs:1-67` — validation behavior
- `service/Api/src/Shared/Application/Mediators/Behaviours/Exceptions/Exception.Behavior.cs:1-42` — exception mapping
- `service/Api/src/Module/Catalog/Features/Admin/Products/Create/CreateProduct.cs:36-78` — handler logic
- `service/Api/src/Module/Ordering/Features/Storefront/Cart/Checkout/CreateOrderFromCart.cs` — checkout handler
- `service/Api/src/Module/Catalog/Features/Admin/Products/Variants/Images/Embeddings/Shared/Clients/ImageEmbedding.Inference.cs` — embedding client
- `service/Embedding/src/main.py:1-29` — Python service entry
- `service/Api/src/Shared/Application/Models/Results/Result.cs:1-43` — Result type

---

## [ASK USER] Items

13. Should the sequence diagrams be formal UML (PlantUML / Mermaid), or are the ASCII representations acceptable?
14. Is there a need for a class diagram of the entire Module assembly, or are the representative hierarchies sufficient?
