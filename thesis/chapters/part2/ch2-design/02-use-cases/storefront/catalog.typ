==== Catalog Browsing
// Diagram placeholder for Catalog Browsing

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-STR-CAT-01], [Browse catalog], [Customer],
    [Navigate the hierarchical taxonomy tree to browse products by category. Apply faceted filters (price range, size, colour, brand) to narrow results. Scroll through paginated product grids.],
    [Filtered product listing displayed with thumbnails, prices, and availability indicators.],
    [CAT-FR-09, CAT-FR-10],
    [UC-STR-CAT-02], [View product detail], [Customer],
    [Select a product to view full detail: description, fashion metadata, variant options with size and colour availability, multiple images, pricing per variant, and taxonomy path.],
    [Product detail page displayed with all variant configurations and real-time availability per variant.],
    [CAT-FR-01, CAT-FR-16],
    [UC-STR-CAT-05], [Keyword search], [Customer],
    [Enter a text query to search products by name, description, or fashion attributes. Results ranked by relevance.],
    [Matching products displayed. Text search complements the visual search capability.],
    [CAT-FR-01],
  ),
  caption: [Customer use cases — Catalog Browsing.],
)

==== Visual Search
// Diagram placeholder for Visual Search

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-STR-CAT-03], [Search by image (CBIR)], [Customer],
    [Upload a reference image (JPEG, PNG, WebP, max 10 MB). System: validates image format and size, sends to .NET API, which forwards to Python ML sidecar for embedding generation, queries pgvector with cosine distance HNSW search, returns top-K results ranked by similarity score. Results display image thumbnail, product name, variant, price, and similarity score.],
    [Visually similar products displayed ranked by cosine similarity. Results filtered to items above the configured minimum similarity threshold.],
    [CAT-FR-06, CAT-FR-07, CAT-FR-08, NFR-01],
    [UC-STR-CAT-04], [View similar products], [Customer],
    [On a product detail page, view products visually similar to the current product based on embedding similarity.],
    [Similar products displayed for passive discovery without requiring an upload.],
    [CAT-FR-17],
  ),
  caption: [Customer use cases — Visual Search. CBIR visual search (UC-STR-CAT-03) is the primary research use case.],
)
