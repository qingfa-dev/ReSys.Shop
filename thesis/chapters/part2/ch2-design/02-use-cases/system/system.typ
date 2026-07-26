==== Embedding Operations
// Diagram placeholder for Embedding Operations

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-SYS-EMB-01], [Generate image embeddings], [System],
    [Administrator uploads a product image. Upload endpoint returns immediately. A background job picks up the image, sends it to the Python ML sidecar via HTTP, receives the embedding vector, stores it in pgvector with model metadata (model name, version, dimension, generation timestamp). HNSW index automatically updated.],
    [Embedding stored and indexed. Image becomes searchable via CBIR within seconds of the job completing.],
    [CAT-FR-05, CAT-FR-15],
    [UC-SYS-EMB-02], [Regenerate all embeddings], [System],
    [When the configured embedding model is changed (via environment variable), existing image embeddings must be regenerated. A bulk regeneration job processes all variant images, generating new embeddings from the current model and updating pgvector records.],
    [All embeddings regenerated with the current model. Search results consistent with the active model.],
    [CAT-FR-08, CAT-FR-15],
    [UC-SYS-EMB-03], [Verify model health], [System],
    [The Python ML sidecar exposes a health check endpoint. .NET Aspire periodically probes the endpoint. The sidecar responds with the currently loaded model, its embedding dimension, and the last inference latency. If the sidecar is unreachable or returns an error, Aspire restarts the container.],
    [ML sidecar availability continuously monitored. Automatic restart on failure. .NET API routes embedding requests only when sidecar reports healthy.],
    [NFR-04],
  ),
  caption: [System use cases — Embedding Operations.],
)

==== Background Maintenance
// Diagram placeholder for Background Maintenance

#figure(
  table(
    columns: (auto, auto, auto, 1fr, auto, 1fr),
    stroke: 0.5pt,
    table.header([*UC-ID*], [*Use Case*], [*Actor*], [*Flow*], [*Postcondition*], [*Related FR*]),
    [UC-SYS-JOB-01], [Expire abandoned carts], [System],
    [A daily scheduled Hangfire job queries for carts with no activity in the past seven days. Each expired cart: releases reserved inventory for all line items, voids any associated payment intents, marks the cart as expired.],
    [Abandoned carts removed. Reserved inventory returned to availability. Database storage from stale carts reclaimed.],
    [ORD-FR-03, NFR-05],
    [UC-SYS-JOB-02], [Manage inventory reservations], [System],
    [During checkout, stock quantities are temporarily reserved to prevent overselling. A scheduled job scans for reservations held longer than fifteen minutes without checkout completion. Expired reservations are released and inventory returned to availability.],
    [Stale reservations expired. Inventory accurately reflects true availability. Overselling prevented through the reservation window.],
    [INV-FR-03, INV-FR-07, NFR-05],
    [UC-SYS-JOB-03], [Process payment webhooks], [System],
    [Stripe sends an HTTP POST webhook to the .NET API for each payment state change (payment intent succeeded, charge captured, refund processed). The system validates the webhook signature using the Stripe signing secret. On valid signature, the webhook payload is processed: payment state updated in the database, associated order state updated if applicable. Idempotency key prevents duplicate processing of retried webhooks.],
    [Payment state synchronised between Stripe and the system database. Order state transitions triggered where appropriate. Duplicate webhooks safely discarded.],
    [PAY-FR-04, PAY-FR-07, NFR-05],
    [UC-SYS-JOB-04], [Maintain vector index], [System],
    [A periodic maintenance job rebuilds the HNSW index on the variant images embedding column. This optimises search performance as the catalog grows and prevents query degradation from index fragmentation after many insertions and updates.],
    [HNSW index optimised. CBIR search latency remains stable as catalog size increases.],
    [CAT-FR-06],
  ),
  caption: [System use cases — Background Maintenance.],
)
