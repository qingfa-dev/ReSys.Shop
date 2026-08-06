# Gap 14: Visual Search Model Selector Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add model selector dropdown to visual search page. Show similarity scores on results. Backend returns scores + new models list endpoint.

**Architecture:** Backend: modify VectorSearchService to return scores, add SimilarityScore to response DTO, create models list endpoint. Frontend: model dropdown, score badges on results.

**Tech Stack:** .NET 10 EF Core + Npgsql pgvector, Python FastAPI (Embedding sidecar), Vue 3, PrimeVue Select

## Global Constraints

- Warnings-as-errors: `TreatWarningsAsErrors=true` in .csproj
- Vertical slice: each feature in `Features/{Admin|Storefront}/{Feature}/{Action}/`
- Result objects: all operations return `Result<T>`
- SearchByImage endpoint is public (no auth required)
- Model parameter already accepted by backend — just needs frontend wiring
- Embedding service already supports dynamic model loading

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.Response.cs` | MODIFY | Add SimilarityScore |
| `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Services/VectorSearchService.Interface.cs` | MODIFY | Add FindSimilarWithScoresAsync |
| `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Services/VectorSearchService.cs` | MODIFY | Return scores |
| `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.cs` | MODIFY | Use scores in response |
| `service/Api/src/Module/Catalog/Features/Storefront/Products/VisualSearchModels/ListVisualSearchModels.cs` | CREATE | Handler |
| `service/Api/src/Module/Catalog/Features/Storefront/Products/VisualSearchModels/ListVisualSearchModels.Endpoint.cs` | CREATE | Carter endpoint |
| `app/Store/src/features/catalog/services/searchByImageApi.ts` | MODIFY | Add models API + score type |
| `app/Store/src/features/catalog/views/VisualSearchView.vue` | MODIFY | Add model selector |
| Result display component | MODIFY | Show score badges |

---

## Tasks

### Task 1: Add SimilarityScore to SearchByImage response

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.Response.cs`

**Interfaces:**
- Consumes: None
- Produces: Updated response DTO with score field

- [ ] **Step 1: Read current response**

Read `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.Response.cs`.

- [ ] **Step 2: Add SimilarityScore field**

Add to the `SimilarProductResponse` record:

```csharp
public double SimilarityScore { get; init; }
```

- [ ] **Step 3: Build backend**

```bash
cd service/Api && dotnet build
```

Expected: PASS (compiles, handler needs updating next)

### Task 2: Modify VectorSearchService to return scores

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Services/VectorSearchService.Interface.cs`
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Services/VectorSearchService.cs`

**Interfaces:**
- Consumes: pgvector cosine distance SQL
- Produces: `List<(Guid VariantId, double Score)>` tuples

- [ ] **Step 1: Read VectorSearchService files**

Read both the interface and implementation files.

- [ ] **Step 2: Add new method to interface**

```csharp
Task<List<(Guid VariantId, double Score)>> FindSimilarWithScoresAsync(
    byte[] imageBytes,
    string? model,
    int topK,
    CancellationToken ct = default);
```

- [ ] **Step 3: Implement in VectorSearchService**

The SQL already computes `ie.vector <=> {0}::vector` (cosine distance). Add the distance to SELECT and convert to similarity score:

```csharp
// In the SQL query, add distance to SELECT:
// SELECT ie.variant_id, 1.0 - (ie.vector <=> @queryVector) AS score

// Return as tuples:
return results.Select(r => (r.VariantId, r.Score)).ToList();
```

- [ ] **Step 4: Build backend**

```bash
cd service/Api && dotnet build
```

Expected: PASS

### Task 3: Update SearchByImage handler to use scores

**Files:**
- Modify: `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.cs`

**Interfaces:**
- Consumes: `FindSimilarWithScoresAsync` from VectorSearchService
- Produces: Response with `SimilarityScore` per product

- [ ] **Step 1: Read SearchByImage handler**

Read the handler to find where it calls VectorSearchService.

- [ ] **Step 2: Call FindSimilarWithScoresAsync**

Replace the call to `FindSimilarVariantIdsAsync` with `FindSimilarWithScoresAsync`.

- [ ] **Step 3: Map scores to response**

When building the response, include the score:

```csharp
Products = results.Select(r => new SimilarProductResponse
{
    VariantId = r.VariantId,
    ProductId = ...,
    ProductName = ...,
    SimilarityScore = r.Score,
    // ... other fields
}).ToList()
```

- [ ] **Step 4: Build backend**

```bash
cd service/Api && dotnet build
```

Expected: PASS

### Task 4: Create models list endpoint

**Files:**
- Create: `service/Api/src/Module/Catalog/Features/Storefront/Products/VisualSearchModels/` (2 files)

**Interfaces:**
- Consumes: None (hardcoded list)
- Produces: `GET /api/storefront/visual-search/models`

- [ ] **Step 1: Create handler**

Create `ListVisualSearchModels.cs`:

```csharp
namespace Module.Catalog.Features.Storefront.Products.VisualSearchModels;

public static partial class ListVisualSearchModels
{
    public sealed record Query : IRequest<Result<Response>>;

    public sealed record Response
    {
        public IReadOnlyList<ModelItem> Models { get; init; } = [];
    }

    public sealed record ModelItem
    {
        public string Id { get; init; } = "";
        public string Name { get; init; } = "";
        public string Description { get; init; } = "";
    }

    internal sealed class Handler : IRequestHandler<Query, Result<Response>>
    {
        public ValueTask<Result<Response>> Handle(Query query, CancellationToken ct)
        {
            return new Result<Response>(new Response
            {
                Models = new[]
                {
                    new ModelItem { Id = "fashion-clip", Name = "Fashion-CLIP", Description = "Fashion-tuned ViT-B/32" },
                    new ModelItem { Id = "clip-vit-l", Name = "CLIP ViT-L/14", Description = "General-purpose ViT-L/14" },
                    new ModelItem { Id = "efficientnet", Name = "EfficientNet-B0", Description = "CNN-based features" },
                    new ModelItem { Id = "dinov2", Name = "DINOv2 ViT-S/14", Description = "Self-supervised ViT" },
                    new ModelItem { Id = "resnet50", Name = "ResNet-50", Description = "Classic CNN features" },
                }
            });
        }
    }
}
```

- [ ] **Step 2: Create endpoint**

Create `ListVisualSearchModels.Endpoint.cs`:

```csharp
namespace Module.Catalog.Features.Storefront.Products.VisualSearchModels;

public static partial class ListVisualSearchModels
{
    public static void MapEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet(CatalogFeature.Storefront.Products.VisualSearchModels.Route, async (
            ISender sender) =>
        {
            var result = await sender.Send(new Query());
            return result.MatchCreated();
        })
        .WithName(nameof(ListVisualSearchModels))
        .WithTags(CatalogFeature.Tags.Products);
    }
}
```

- [ ] **Step 3: Register endpoint**

Add to `MapCatalogStorefront`.

- [ ] **Step 4: Build backend**

```bash
cd service/Api && dotnet build
```

Expected: PASS

- [ ] **Step 5: Commit backend**

```bash
cd service/Api && git add src/Module/Catalog/Features/Storefront/Products/
git commit -m "feat(catalog): add visual search model selector and similarity scores"
```

### Task 5: Add models API and score type (frontend)

**Files:**
- Modify: `app/Store/src/features/catalog/services/searchByImageApi.ts`

**Interfaces:**
- Consumes: `GET /api/storefront/visual-search/models`, `POST /api/storefront/products/images/search`
- Produces: `getVisualSearchModels()`, updated response type with scores

- [ ] **Step 1: Add model type**

```typescript
export interface VisualSearchModel {
  id: string
  name: string
  description: string
}
```

- [ ] **Step 2: Add models API function**

```typescript
export async function getVisualSearchModels(): Promise<Result<VisualSearchModel[]>> {
  return get('api/storefront/visual-search/models')
}
```

- [ ] **Step 3: Update response type**

Add `similarityScore` to the search result item type:

```typescript
export interface SearchResultItem {
  productId: string
  productName: string
  imageUrl: string
  price: number
  similarityScore: number  // NEW
}
```

- [ ] **Step 4: Update searchByImage function**

Add optional `model` parameter:

```typescript
export async function searchByImage(image: File, model?: string): Promise<Result<SearchResultItem[]>> {
  const formData = new FormData()
  formData.append('image', image)
  if (model) formData.append('model', model)
  return post('api/storefront/products/images/search', formData)
}
```

- [ ] **Step 5: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 6: Add model selector to VisualSearchView

**Files:**
- Modify: `app/Store/src/features/catalog/views/VisualSearchView.vue`

**Interfaces:**
- Consumes: `getVisualSearchModels()` API
- Produces: Model dropdown in UI

- [ ] **Step 1: Read VisualSearchView.vue**

Read the current view to understand the layout.

- [ ] **Step 2: Add models state**

```typescript
import { getVisualSearchModels } from '../services/searchByImageApi'
import type { VisualSearchModel } from '../services/searchByImageApi'

const models = ref<VisualSearchModel[]>([])
const selectedModel = ref<string | null>(null)

onMounted(async () => {
  const res = await getVisualSearchModels()
  if (res.isSuccess) models.value = res.value
})
```

- [ ] **Step 3: Add model selector dropdown**

Above the upload dropzone, add:

```vue
<!-- Section: Model Selector -->
<div v-if="models.length > 0" class="mb-4">
  <label class="block text-sm font-medium text-stone-700 mb-1">Model</label>
  <Select
    v-model="selectedModel"
    :options="models"
    option-label="name"
    option-value="id"
    placeholder="Select model"
    class="w-full"
  />
</div>
```

- [ ] **Step 4: Pass model to search**

In the search function, pass the selected model:

```typescript
const result = await searchByImage(imageFile, selectedModel.value ?? undefined)
```

- [ ] **Step 5: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

### Task 7: Show similarity scores on results

**Files:**
- Modify: Result display component (wherever search results are rendered)

**Interfaces:**
- Consumes: `similarityScore` from response
- Produces: Score badge on each result card

- [ ] **Step 1: Find result rendering**

In `VisualSearchView.vue` or a child component, find where results are rendered.

- [ ] **Step 2: Add score badge**

On each result card, add:

```vue
<span
  class="absolute top-2 right-2 text-xs font-bold px-2 py-0.5 rounded-full"
  :class="result.similarityScore > 0.85 ? 'bg-emerald-100 text-emerald-700' : result.similarityScore > 0.7 ? 'bg-amber-100 text-amber-700' : 'bg-red-100 text-red-700'"
>
  {{ Math.round(result.similarityScore * 100) }}%
</span>
```

- [ ] **Step 3: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 4: Run unit tests**

```bash
cd app/Store && pnpm run test:unit
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
cd app/Store && git add src/features/catalog/services/searchByImageApi.ts src/features/catalog/views/VisualSearchView.vue
git commit -m "feat(catalog): add visual search model selector and similarity scores to UI"
```
