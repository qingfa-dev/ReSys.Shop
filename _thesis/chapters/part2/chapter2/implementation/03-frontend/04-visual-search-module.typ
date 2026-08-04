===== 1. Visual Search Module
The Visual Search implementation orchestrates a complex interaction converting user intent (an image) into vector queries.

*Input Processing:*
A dedicated *Drag-and-Drop Input Component* handles the creation of the query. It manages the visual state (`isDragging`) to provide affordability and strictly validates file types on the client side before upload.

```typescript
// Visual Search Input Flow
IF User drags file over drop-zone:
    SET state to "Dragging" (Highlight UI)
ON DROP:
    PREVENT default browser behavior
    VALIDATE file type is Image (JPG/PNG/WEBP)
    EMIT "Selection Event" with File Object
```

*Similarity Intelligence:*
Search results are rendered with explicit *Confidence Badges*. The UI interprets the raw cosine distance returned by the backend to categorize results, giving users transparency into the AI's logic.

```typescript
// Similarity Classification Logic
FUNCTION GetSimilarityBadge(score):
    IF score > 0.85: RETURN "Exact Match" (Green)
    IF score > 0.70: RETURN "Highly Similar" (Blue)
    ELSE: RETURN "Conceptual Match" (Yellow)
```

The following sequence diagram illustrates the end-to-end lifecycle of a visual search request.

*Visual Inference Pipeline:*
- *UI Action:* The interaction begins with a Drag-and-Drop event. The frontend immediately validates the MIME type and renders a local *Optimistic Preview* of the image. Crucially, while the heavy inference runs, the results grid displays a *Skeleton Loader* animation to manage perceived latency.
- *System Sequence:*
  1. *Upload:* The image is POSTed to the API Gateway.
  2. *Vectorization:* The backend delegates to the `ML Service` via HTTP (@fig:sq-0004-visual).
  3. *Search:* The resulting 512-dim vector is used in a `pgvector` KNN query.
  4. *Response:* The Skeleton Loader is replaced by the hydrated list of visually similar products.

#figure(
  placement: none,
  image("../../../../../images/diagrams/sequences/customer/sq-0004-visual-search.png", width: 100%),
  caption: [Visual Search Data Flow: End-to-End sequence from user interaction to AI inference and retrieval (UC-0004).],
) <fig:sq-0004-visual>


#figure(
  placement: none,
  image("../../../../../images/ui/store/ui-store-visualsearch-upload.png", width: 100%),
  caption: [Visual Search UI: Interface for uploading images and selecting visual queries.],
)

#figure(
  placement: none,
  image("../../../../../images/ui/store/ui-store-visualsearch-results-mixed.png", width: 100%),
  caption: [Visual Search Results: Hybrid result grid displaying visually similar items with confidence scores.],
)
