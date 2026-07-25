=== Use Case 1: Visual Search (CBIR)

#figure(
  table(
    columns: (1fr, 3fr),
    stroke: 0.5pt,
    align: (left + horizon, left),

    [*Actor*], [Customer (Guest or Authenticated)],
    [*Precondition*], [
      The Python ML sidecar service is running and has loaded the configured embedding model into memory. The product catalog contains at least one variant with a stored embedding vector for the active model.
    ],
    [*Main Flow*], [
      1. Customer uploads a reference image (JPEG, PNG, or WebP; maximum ten megabytes) via the storefront visual search interface. \
      2. The Vue frontend sends the image as a multipart form data request to the .NET API endpoint. \
      3. The API validates the image, magic-byte verification, extension check, size limit, then forwards the raw image bytes to the Python ML sidecar. \
      4. The ML sidecar preprocesses the image (resize, normalise) and executes a forward pass through the configured embedding model, producing a floating-point vector. \
      5. The API queries PostgreSQL pgvector using cosine similarity against all stored variant embeddings filtered by the active model name, retrieving the top 20 most similar results. \
      6. The API joins variant data with product metadata, computes similarity scores, filters by a minimum similarity threshold (default 0.7), and returns the ordered results as JSON. \
      7. The Vue storefront renders the results as a grid of product thumbnails with similarity scores and prices.
    ],
    [*Postcondition*], [
      A ranked list of visually similar products is displayed to the customer, ordered by decreasing cosine similarity. Each result includes the product thumbnail, name, price, and similarity score.
    ],
  ),
  caption: [UC-1: Visual Search (CBIR), the primary research use case.],
) <tbl-uc-visual-search>
