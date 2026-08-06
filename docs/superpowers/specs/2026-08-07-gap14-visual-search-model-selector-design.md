# Gap 14: Visual Search Model Selector

## Summary

Add model selector dropdown to visual search page. Show similarity scores on results. Backend adds model selection parameter and score response.

## Current State

- `VisualSearchView.vue`: image upload → `POST /api/storefront/products/images/search`
- Backend uses single configured model (no selection)
- Results show products without similarity scores
- Embedding service has 11 model adapters

## Design

### Backend: Model Selection

**Already implemented.** The `SearchByImage.Request` already has a `Model` parameter (string?, optional). The handler passes it to the Embedding service which supports dynamic model loading.

No backend changes needed for model selection — only the new models list endpoint and similarity scores.

### Backend: Score Response

**File:** `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.Response.cs`

**Add `SimilarityScore` field:**
```csharp
public sealed record Response
{
    public IReadOnlyList<SimilarProductResponse> Products { get; init; } = [];
}

public sealed record SimilarProductResponse
{
    public Guid VariantId { get; init; }
    public Guid ProductId { get; init; }
    public string ProductName { get; init; } = "";
    public string Sku { get; init; } = "";
    public decimal Price { get; init; }
    public string ImageUrl { get; init; } = "";
    public double SimilarityScore { get; init; }  // NEW: 0.0 - 1.0
}
```

**File:** `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Services/VectorSearchService.Interface.cs`

**Change return type:**
```csharp
// Before
Task<List<Guid>> FindSimilarVariantIdsAsync(...)

// After
Task<List<(Guid VariantId, double Score)>> FindSimilarWithScoresAsync(...)
```

**File:** `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Services/VectorSearchService.cs`

**Modify:** Return `(VariantId, Score)` tuples instead of just IDs. The SQL already computes cosine distance — just SELECT it and convert to similarity score (`1 - distance`).

### Backend: Models Endpoint

**New endpoint:** `GET /api/storefront/visual-search/models`

**Response:**
```json
{
  "models": [
    { "id": "fashion-clip", "name": "Fashion-CLIP", "description": "Fashion-tuned ViT-B/32" },
    { "id": "clip-vit-l", "name": "CLIP ViT-L/14", "description": "General-purpose ViT-L/14" },
    { "id": "efficientnet", "name": "EfficientNet-B0", "description": "CNN-based features" },
    { "id": "dinov2", "name": "DINOv2 ViT-S/14", "description": "Self-supervised ViT" },
    { "id": "resnet50", "name": "ResNet-50", "description": "Classic CNN features" }
  ]
}
```

### Frontend: Model Selector

**File:** `app/Store/src/features/catalog/views/VisualSearchView.vue`

**Add above upload area:**
```
Model: [Fashion-CLIP ▾]
```

PrimeVue `Select` dropdown populated from `GET /api/storefront/visual-search/models`.

### Frontend: Score Display

**File:** `app/Store/src/features/catalog/components/VisualSearchResult.vue` (or results grid)

**Add similarity badge on each result card:**
```
┌──────────┐
│ [image]  │
│ 92%      │  ← similarity score badge
│ Classic  │
│ Tee      │
└──────────┘
```

Badge: colored pill (green for >85%, yellow for >70%, red for <70%).

### Frontend: API Changes

**File:** `app/Store/src/features/catalog/services/searchByImageApi.ts`

```ts
// Before
searchByImage(image: File): Promise<SearchByImageResponse>

// After
searchByImage(image: File, model?: string): Promise<SearchByImageResponse>
```

**Response type update:**
```ts
interface SimilarProductResponse {
  productId: string
  name: string
  slug: string
  imageUrl: string
  price: number
  similarityScore: number  // NEW
}
```

## Files to Create/Modify

| File | Action |
|------|--------|
| `Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.Response.cs` | MODIFY — add SimilarityScore field |
| `Module/Catalog/Features/Storefront/Products/Shared/Services/VectorSearchService.Interface.cs` | MODIFY — add FindSimilarWithScoresAsync |
| `Module/Catalog/Features/Storefront/Products/Shared/Services/VectorSearchService.cs` | MODIFY — return scores |
| `Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.cs` | MODIFY — use scores in response |
| `Module/Catalog/Features/Storefront/Products/VisualSearchModels/` | CREATE — new endpoint (2 files) |
| `features/catalog/views/VisualSearchView.vue` | MODIFY — add model selector |
| `features/catalog/services/searchByImageApi.ts` | MODIFY — add model param + score type |

## Acceptance Criteria

- [ ] Model dropdown shows available models
- [ ] Selected model passed to backend
- [ ] Results include similarity scores
- [ ] Score badge displayed on each result card
- [ ] Score colors reflect quality (green/yellow/red)
- [ ] Default model is Fashion-CLIP
- [ ] Works when no model selected (uses default)
