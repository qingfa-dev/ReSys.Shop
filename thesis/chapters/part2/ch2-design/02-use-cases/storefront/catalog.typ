==== Catalog Browsing

// Diagram placeholder: Catalog Browsing use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-STR-BRW-01], [Browse catalog], [Customer], [Navigate the taxonomy tree to browse products by category with faceted filtering and paginated product grids.], [Catalog and taxonomy data are published.], [Filtered product listing displayed with thumbnails, prices, and availability indicators.],
  [UC-STR-BRW-02], [View product detail], [Customer], [View complete product information: description, fashion metadata, variant options with availability, images, pricing, and taxonomy path.], [The product exists and is published.], [Product detail page displayed with all variant configurations and real-time availability.],
  [UC-STR-BRW-03], [View similar products], [Customer], [On a product detail page, view products visually similar to the current product.], [The product has image embeddings available.], [Similar products displayed for passive discovery without requiring an upload.],
)

==== Search

// Diagram placeholder: Search use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-STR-SRC-01], [Search by image], [Customer], [Upload a reference image and find visually similar products ranked by similarity.], [Catalog contains products with available image embeddings.], [Visually similar products displayed with similarity scores, filtered above the configured minimum threshold.],
  [UC-STR-SRC-02], [Keyword search], [Customer], [Enter a text query to search products by name, description, or fashion attributes.], [Catalog data is indexed and searchable.], [Matching products displayed ranked by relevance.],
)

=== UC-STR-SRC-01 — Search by Image

#table(
  columns: (auto, 1fr),
  stroke: 0.5pt,
  [*Field*], [*Description*],
  [Use Case ID], [UC-STR-SRC-01],
  [Use Case Name], [Search by Image],
  [Primary Actor], [Customer],
  [Goal], [Find visually similar products by uploading a reference image and receiving ranked results.],
  [Trigger], [The customer navigates to the visual search interface and uploads an image.],
  [Preconditions], [
    - Catalog contains products with available image embeddings.
    - The search service is operational and healthy.
  ],
  [Postconditions], [
    - Visually similar products are displayed with similarity scores.
    - Results are filtered above the configured minimum similarity threshold.
  ],
  [Related FR], [CAT-FR-06, CAT-FR-07, CAT-FR-08],
)

*Main Success Scenario*

#table(
  columns: (auto, 1fr, 2fr),
  stroke: 0.5pt,
  [*Step*], [*Actor*], [*System Response*],
  [1], [Customer], [Navigates to the visual search interface.],
  [2], [Customer], [Selects or drags an image file from the local file system.],
  [3], [System], [Validates the image format and size constraints.],
  [4], [System], [Generates a visual embedding for the uploaded image using the configured model.],
  [5], [System], [Performs a similarity search against the image embeddings index.],
  [6], [System], [Retrieves matching products and ranks them by similarity score.],
  [7], [System], [Displays results as a grid of product thumbnails with similarity scores, linking each result to its product detail page.],
)

*Alternative and Exception Flows*

#table(
  columns: (auto, 1fr, 2fr),
  stroke: 0.5pt,
  [*ID*], [*Condition*], [*System Response*],
  [A1], [Invalid image format], [Rejects the upload and displays a message listing the accepted formats.],
  [A2], [No visually similar products found above threshold], [Displays an empty result message suggesting the customer try a different image or use keyword search.],
  [E1], [Search service is unavailable], [Displays an error message indicating the service is temporarily unavailable and suggests retrying later.],
)
