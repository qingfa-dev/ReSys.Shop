=== CBIR Search Flow

Content-based image retrieval (CBIR) enables real-time visual product search across four distinct architectural layers: the Vue 3 storefront, .NET API backend, Python ML sidecar, and PostgreSQL with pgvector. The end-to-end pipeline is engineered to complete within a strict 1-second total latency budget. @fig-cbir-sequence details the cross-service sequence.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_cbir-search-sequence.png", width: 100%),
  caption: [CBIR search sequence: end-to-end flow spanning customer upload, embedding extraction, pgvector search, and ranked results rendering.],
) <fig-cbir-sequence>

==== End-to-End Execution Pipeline

1. *Client Validation (Vue 3):* The storefront verifies file format (JPEG, PNG, WebP) and size restrictions ($<= 10$ MB), providing immediate UI validation before dispatching a multipart form request to `POST /api/storefront/catalog/products/images/search`.
2. *Server Validation (.NET API):* The backend verifies binary header magic bytes (preventing file extension spoofing), validates the MIME type, and re-enforces the 10 MB payload ceiling.
3. *Vector Extraction (ML Sidecar):* The backend proxies image bytes to the `/embeddings` endpoint. The sidecar executes preprocessing and model inference ($50 thin - thin 100$ ms latency), returning a 512-dimensional float vector with model metadata.
4. *Vector Index Search (pgvector):* The backend queries the `variant_images` table using pgvector's `<=>` cosine distance operator:
   - *HNSW Acceleration:* Leverages HNSW indexing for sub-10 ms logarithmic lookup times across thousands of stored embeddings.
   - *Model Isolation Filter:* Enforces a `model_name` predicate to prevent invalid cross-model vector comparisons.
5. *Post-Processing & Deduplication:* The backend joins vector matches with parent product records:
   - *Similarity Score:* Computes score values via $ text("Similarity") = 1 - text("Distance") $.
   - *Thresholding:* Filters out matches falling below the configurable cut-off (default: $0.7$).
   - *Product Deduplication:* Groups multiple matching variant images by parent product, selecting only the top-scoring variant.
6. *UI Rendering (Vue 3):* The client receives a structured JSON payload containing titles, prices, thumbnails, and similarity percentages, rendering a product grid to complete the sub-second search experience.