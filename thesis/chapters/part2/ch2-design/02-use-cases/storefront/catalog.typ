==== Catalog Browsing

// Diagram placeholder: Catalog Browsing use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-STR-CAT-01], [Browse catalog], [Customer],
    [Navigate the taxonomy tree to browse products by category. Apply faceted filters to narrow results. Scroll through paginated product grids.],
    [Filtered product listing displayed with thumbnails, prices, and availability indicators.],
    [CAT-FR-09, CAT-FR-10],
    [UC-STR-CAT-02], [View product detail], [Customer],
    [Select a product to view its complete information: description, fashion metadata, variant options with availability, images, pricing, and taxonomy path.],
    [Product detail page displayed with all variant configurations and real-time availability.],
    [CAT-FR-01, CAT-FR-16],
    [UC-STR-CAT-03], [Keyword search], [Customer],
    [Enter a text query to search products by name, description, or fashion attributes.],
    [Matching products displayed ranked by relevance.],
    [CAT-FR-01],
  ),
  caption: [Customer use cases — Catalog Browsing.],
)

==== Visual Search

// Diagram placeholder: Visual Search use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-STR-CAT-04], [Search by image], [Customer],
    [Upload a reference image. The system finds and returns visually similar products ranked by similarity.],
    [Visually similar products displayed with similarity scores. Results filtered to items above the configured minimum threshold.],
    [CAT-FR-06, CAT-FR-07, CAT-FR-08, NFR-01],
    [UC-STR-CAT-05], [View similar products], [Customer],
    [On a product detail page, view products visually similar to the current product.],
    [Similar products displayed for passive discovery without requiring an upload.],
    [CAT-FR-17],
  ),
  caption: [Customer use cases — Visual Search.],
)
