# Implementation Plan: Gap 14 — Visual Search Model Selector

**Spec:** `docs/superpowers/specs/2026-08-07-gap14-visual-search-model-selector-design.md`
**Estimated effort:** Large (4-6 hours)
**Dependencies:** None

## Tasks

### Backend

#### T1: Add SimilarityScore to SearchByImage response
- [ ] Edit `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.Response.cs`
- [ ] Add `public double SimilarityScore { get; init; }` to `SimilarProductResponse`

#### T2: Modify VectorSearchService to return scores
- [ ] Edit `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Services/VectorSearchService.Interface.cs`
- [ ] Add `Task<List<(Guid VariantId, double Score)>> FindSimilarWithScoresAsync(...)` method
- [ ] Edit `service/Api/src/Module/Catalog/Features/Storefront/Products/Shared/Services/VectorSearchService.cs`
- [ ] Implement `FindSimilarWithScoresAsync`: SELECT cosine distance in SQL, convert to similarity score (1 - distance)
- [ ] Return `List<(Guid, double)>` tuples

#### T3: Update SearchByImage handler to use scores
- [ ] Edit `service/Api/src/Module/Catalog/Features/Storefront/Products/Images/Search/SearchByImage.cs`
- [ ] Call `FindSimilarWithScoresAsync` instead of `FindSimilarVariantIdsAsync`
- [ ] Map scores to response DTO

#### T4: Create models list endpoint
- [ ] Create `Module/Catalog/Features/Storefront/Products/VisualSearchModels/ListVisualSearchModels.cs`
- [ ] Create `Module/Catalog/Features/Storefront/Products/VisualSearchModels/ListVisualSearchModels.Endpoint.cs`
- [ ] Route: `GET api/storefront/visual-search/models`
- [ ] Return hardcoded list of available models with id, name, description

### Frontend

#### T5: Add models API call
- [ ] Edit `app/Store/src/features/catalog/services/searchByImageApi.ts`
- [ ] Add `getVisualSearchModels()` function
- [ ] Update `SearchByImageResponse` type to include `similarityScore`

#### T6: Add model selector to VisualSearchView
- [ ] Edit `app/Store/src/features/catalog/views/VisualSearchView.vue`
- [ ] Add PrimeVue Select dropdown above upload area
- [ ] Populate from `getVisualSearchModels()` API
- [ ] Pass selected model to `searchByImage()` call

#### T7: Show similarity scores on results
- [ ] Edit result display component
- [ ] Add colored badge: green (>85%), yellow (>70%), red (<70%)
- [ ] Show "92% match" text on each result card

### T8: Verify
- [ ] Model dropdown shows available models
- [ ] Selected model passed to backend
- [ ] Results include similarity scores
- [ ] Score badges displayed with correct colors
- [ ] Default model is Fashion-CLIP

## Verification

```bash
cd service/Api && dotnet build && dotnet test
cd app/Store && pnpm run lint && pnpm run test:unit
```
