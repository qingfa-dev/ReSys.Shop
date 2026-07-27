===== Recommendations API (Clean Architecture)
The `RecommendationsModule` exposes endpoints via *Carter* modules, delegating logic to the Domain via MediatR. This separates the *Transport Mechanism* (HTTP) from the *Application Logic*.

```csharp
// POST /api/storefront/recommendations/by-product/{id}
app.MapGet("/by-product/{productId:guid}", async (ISender sender, ...) =>
{
    var query = new GetProductRecommendations.Query(productId, "fashion_clip", TopK: 10);
    return await sender.Send(query);
});
```
