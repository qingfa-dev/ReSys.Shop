=== Use Cases

This section presents three use cases that represent the system's core functional scenarios: visual search (the primary research capability), checkout (the primary e-commerce transaction), and model benchmark evaluation (the research methodology for Chapter 3). Each use case is described in a compact tabular format comprising the actor, preconditions, main flow as numbered sequential steps, and postconditions. Figure @fig-use-case-diagram provides a visual summary of actor-system interactions.

==== Use Case 1: Visual Search (CBIR)

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

==== Use Case 2: Multi-Step Checkout

#figure(
  table(
    columns: (1fr, 3fr),
    stroke: 0.5pt,
    align: (left + horizon, left),

    [*Actor*], [Customer (Guest or Authenticated)],
    [*Precondition*], [
      The customer's cart contains at least one valid, in-stock item. The customer has a shipping address on file or is prepared to enter one.
    ],
    [*Main Flow*], [
      1. Customer clicks "Proceed to Checkout" from the cart page. \
      2. System presents the checkout interface, showing the current cart contents with item totals. \
      3. Customer selects or enters a shipping address. \
      4. Customer selects a delivery method from available shipping options. \
      5. Customer selects a payment method and provides payment details. \
      6. Customer reviews the order summary (items, shipping cost, tax, total) and clicks "Place Order". \
      7. System begins an atomic transaction: creates the order record, reserves inventory quantities for each line item, processes the payment through the configured gateway, and clears the cart. \
      8. System displays the order confirmation page with the order number and summary.
    ],
    [*Postcondition*], [
      An order record is created with status "Placed". Inventory quantities for each ordered variant are reserved. A payment intent is linked to the order. The customer's cart is emptied. A confirmation is displayed with the order reference number.
    ],
  ),
  caption: [UC-2: Multi-Step Checkout, the primary e-commerce transaction use case.],
) <tbl-uc-checkout>

==== Use Case 3: Model Benchmark Evaluation

#figure(
  table(
    columns: (1fr, 3fr),
    stroke: 0.5pt,
    align: (left + horizon, left),

    [*Actor*], [Researcher / System],
    [*Precondition*], [
      The benchmark dataset is available on disk, consisting of query images and catalog images organised into human-labelled similarity groups. The Python ML sidecar is running. All candidate embedding model weights are downloaded and accessible.
    ],
    [*Main Flow*], [
      1. Researcher selects a model from the candidate set (e.g., Fashion-CLIP, ResNet-50, DINOv2-S) and configures the ML sidecar via environment variable. \
      2. System generates embedding vectors for all query images and all catalog images using the selected model. \
      3. For each query image, the system executes a top-K (K = 20) similarity search against the catalog embeddings. \
      4. System computes retrieval metrics: Mean Average Precision (mAP), Precision at K, and Recall at K, using the human-labelled groups as ground truth. \
      5. System records operational metrics: average inference time per image, throughput (images per second), disk storage for the embedding index, and RAM consumption. \
      6. Steps 1 to 5 are repeated for each of the 11 candidate models. \
      7. System aggregates all results into comparison tables, ranking models by retrieval accuracy and operational efficiency.
    ],
    [*Postcondition*], [
      A complete benchmark report is produced containing accuracy metrics (mAP, P\@20, R\@20) and efficiency metrics (latency, throughput, storage, RAM) for every evaluated model. The report identifies the optimal model for each deployment scenario (GPU production, CPU-only, maximum accuracy, resource-constrained).
    ],
  ),
  caption: [UC-3: Model Benchmark Evaluation, the research methodology use case.],
) <tbl-uc-benchmark>

Figure @fig-use-case-diagram positions these three use cases alongside the broader system functionality within a single visual summary.

#figure(
  image("../../../../images/diagrams/02-use-case.png", width: 85%),
  caption: [
    System use case diagram showing the three actors, Customer, Administrator, and System background services, and their primary interactions with the ReSys.Shop platform.
  ],
) <fig-use-case-diagram>

The three use cases serve distinct purposes within the thesis. The visual search use case defines the functional behaviour of the system's primary research capability; the checkout use case establishes the realistic e-commerce context in which search success can be measured through downstream conversion events; and the benchmark use case defines the systematic methodology used in Chapter 3 to evaluate and compare embedding models. The breadth of the system, nine background actors and use cases in the diagram, encompassing catalog browsing, account management, product administration, and order processing, reflects the full operational scope of the platform, while the three detailed use cases focus on the scenarios most relevant to the research questions.
