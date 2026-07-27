== SYSTEM OVERVIEW

This section describes the high-level architecture of ReSys.Shop and how different components work together.

=== System Architecture

ReSys.Shop follows a *microservices-inspired architecture* with three distinct services:

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left),
    [*Service*], [*Technology Stack*], [*Responsibilities*],
    [Vue Frontend],
    [Vue 3 + TypeScript + Vite],
    [
      - Customer storefront (ReSys.Shop)
      - Admin panel (ReSys.Admin)
      - PrimeVue 4 component library
      - Pinia state management
    ],

    [.NET Backend],
    [.NET 10 + EF Core + Carter],
    [
      - REST API endpoints
      - Business logic (MediatR CQRS)
      - PostgreSQL data persistence
      - pgvector similarity search
    ],

    [Python ML],
    [Python 3.12 + FastAPI + PyTorch],
    [
      - AI model inference
      - Vector embedding generation
      - Multi-model support
    ],
  ),
  caption: [System services and technology stacks],
)

=== Bounded Contexts (Domain-Driven Design)

The backend is organized into seven bounded contexts:

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, center, left),
    [*Context*], [*Aggregate Root*], [*Domain Entities*],
    [*Catalog*],
    [Product],
    [
      Variant, ProductImage, ImageEmbedding, OptionType, OptionValue,
      Classification, ProductProperty, PropertyType, Taxonomy, Taxon
    ],

    [*Ordering*],
    [Order],
    [
      LineItem, Payment, Shipment, InventoryUnit, OrderHistory
    ],

    [*Inventories*],
    [StockItem],
    [
      StockLocation, StockMovement, StockTransfer, StockSummary
    ],

    [*Identity*],
    [User],
    [
      UserAddress, Role, UserClaim
    ],

    [*Location*],
    [Address],
    [
      Country, State, Zone
    ],

    [*Common*],
    [None],
    [
      Entity, Aggregate, DomainEvent (abstractions)
    ],

    [*Testing*],
    [None],
    [
      TestData, TestProduct (development fixtures)
    ],
  ),
  caption: [Bounded contexts and their aggregate roots],
)

=== CQRS Feature Structure

The application uses *Vertical Slice Architecture* with CQRS (Command Query Responsibility Segregation):

#figure(
  table(
    columns: (auto, auto, auto),
    stroke: 0.5pt,
    align: (left, center, center),
    [*Feature Area*], [*Admin Features*], [*Storefront Features*],
    [Catalog / Products], [67], [3],
    [Ordering / Cart], [3], [5],
    [Recommendations], [None], [1],
    [Search], [None], [2],
    [Checkout], [None], [3],
    [Identity], [9], [None],
    [Inventories], [5], [None],
    [*Total*], [*84+*], [*21*],
  ),
  caption: [Feature count by area (CQRS handlers)],
)

Each feature follows this structure:
```
Features/Admin/Catalog/Products/CreateProduct/
├── CreateProduct.cs      // Command + Handler + Validator
└── (imports from Common/)
```

=== API Layer Architecture

The API uses *Carter* modules for clean endpoint registration:

#figure(
  table(
    columns: (auto, 1fr, auto),
    stroke: 0.5pt,
    align: (left, left, center),
    [*API Module*], [*Endpoints*], [*Methods*],
    [SearchModule],
    [
      `/api/storefront/search/by-image` \
      `/api/storefront/search/by-image-upload`
    ],
    [POST \ POST],

    [RecommendationsModule],
    [
      `/api/storefront/recommendations/by-product/{id}`
    ],
    [GET],

    [CartModule],
    [
      `/api/storefront/cart` \
      `/api/storefront/cart/items`
    ],
    [GET, POST \ DELETE, PUT],

    [CatalogModule],
    [
      `/api/storefront/products` \
      `/api/storefront/products/{slug}`
    ],
    [GET \ GET],

    [CheckoutModule],
    [
      `/api/storefront/checkout/addresses` \
      `/api/storefront/checkout/place-order`
    ],
    [POST \ POST],

    [ProductsModule (Admin)],
    [
      `/api/catalog/products` \
      `/api/catalog/products/{id}/images`
    ],
    [CRUD \ CRUD],
  ),
  caption: [Key API modules and endpoints],
)

=== ML Service Architecture

The Python ML service provides a FastAPI-based inference API:

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (left, center, left),
    [*Endpoint*], [*Method*], [*Description*],
    [`/api/inference/embed`], [POST], [Generate embedding from image URL],
    [`/api/inference/models`], [GET], [List available/loaded models],
    [`/api/inference/health`], [GET], [Liveness probe with inference test],
  ),
  caption: [ML service API endpoints],
)

==== Embedder Architecture

```
ml/embedders/
├── base.py           # BaseEmbedder abstract class
├── transformers.py   # CLIP, Fashion-CLIP, DINOv2
├── cnn.py            # ConvNeXt, EfficientNet
└── manager.py        # ModelManager (lazy loading, caching)
```

The `ModelManager` handles:
- *Lazy Loading:* Models load on first request, not at startup
- *Caching:* Loaded models stay in GPU memory for fast reuse
- *Device Selection:* Automatic CUDA/CPU detection


=== Visual Search Data Flow

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, left, left),
    [*Step*], [*Component*], [*Action*],
    [1], [Vue Frontend], [User uploads image via FileUpload component],
    [2], [SearchModule], [Receives multipart/form-data, validates file],
    [3], [MediatR], [Dispatches `SearchProductsByImageUpload.Command`],
    [4], [Handler], [Calls `IMlService.GetEmbeddingFromBytesAsync()`],
    [5], [HttpMlService], [Sends POST to Python ML service `/embed`],
    [6], [ModelManager], [Loads Fashion-CLIP if not cached],
    [7], [Embedder], [Processes image to 512-dim normalized vector],
    [8], [Handler], [Queries pgvector: `ORDER BY vector <=> @query`],
    [9], [EF Core], [Returns matching ProductImages with Products],
    [10], [Handler], [Deduplicates by ProductId, filters by threshold],
    [11], [API], [Returns JSON with products and similarity scores],
    [12], [Frontend], [Displays results in grid with "92% match" labels],
  ),
  caption: [Complete visual search data flow],
)

=== Architectural Decision: Separate ML Service

*Rationale for Python-based ML microservice:*

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left, left),
    [*Factor*], [*Justification*],
    [*Technology Fit*],
    [Python has native PyTorch, Transformers, Hugging Face support. Running ML in .NET would require ONNX conversion or complex interop.],

    [*Independent Scaling*],
    [ML service requires GPU memory. Separating allows main API to run on standard servers while ML runs on GPU machines.],

    [*Model Updates*], [When better AI models emerge, only the ML service changes. Backend and database remain stable.],
    [*Fault Isolation*],
    [If ML crashes, storefront continues working. In this case, only visual search is affected, and customers can still browse and purchase.],

    [*Development Speed*],
    [80% of ML ecosystem documentation is Python-first. Development and debugging is faster in native Python.],
  ),
  caption: [Rationale for separate ML microservice],
)

=== Development Deployment

#figure(
  table(
    columns: (auto, auto, 1fr),
    stroke: 0.5pt,
    align: (center, center, left),
    [*Service*], [*Port*], [*Configuration*],
    [Vue Storefront], [5173], [Vite dev server with HMR],
    [Vue Admin], [5174], [Vite dev server with HMR],
    [.NET Backend], [5000/5001], [Kestrel (HTTP/HTTPS)],
    [ML Service], [8000], [Uvicorn with auto-reload],
    [PostgreSQL], [5432], [Docker container with pgvector],
  ),
  caption: [Development deployment topology],
)


