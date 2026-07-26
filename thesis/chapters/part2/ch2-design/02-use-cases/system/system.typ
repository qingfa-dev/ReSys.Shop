==== Embedding Operations

// Diagram placeholder: Embedding Operations use case diagram

==== UC-SYS-EMB — Embedding Operations

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-SYS-EMB],
    [*Use Case Name*], [Embedding Operations],
    [*Primary Actor*], [System],
    [*Supporting Actors*], [ML Service],
    [*Goal*], [Generate and regenerate image embeddings for product search.],
    [*Trigger*], [An image has been uploaded or embedding model configuration has changed.],
    [*Preconditions*], [
      - Images uploaded and stored.
      - ML service is operational.
    ],
    [*Postconditions*], [
      - Embeddings stored in the index for visual search.
    ],
    [*Main Success Scenario*], [
      *Generate Image Embeddings*
      1. System detects a new unprocessed image in the queue.
      2. System retrieves the image file from storage.
      3. System preprocesses the image (resize, normalise) for model requirements.
      4. System sends preprocessed image to ML service with configured model identifier.
      5. ML Service computes the visual embedding vector and returns it with metadata.
      6. System stores embedding in the index with image reference and metadata.
      7. System marks the image as processed.
      ,
      *Regenerate All Embeddings*
      1. System detects change to embedding model configuration.
      2. System validates new model is available via ML service health check.
      3. System retrieves list of all product images requiring regeneration.
      4. System sends each image to ML service using the new model.
      5. ML Service computes new embedding and returns it.
      6. System stores new embedding replacing previous and updates model metadata.
      7. System reports completion with success/failure count summary.
    ],
    [*Alternative Flows*], [
      A1. Image not accessible (deleted/corrupted): system marks as failed and records reason.
      A2. Multiple images queued: system processes sequentially, throttling to ML service capacity.
      A3. Triggered during peak traffic (Regenerate): system throttles to avoid impacting search performance.
      A4. No model change detected: system logs event and skips processing.
    ],
    [*Exception Flows*], [
      E1. ML service returns error: system retries with exponential backoff; after max retries marks as failed.
      E2. ML service unreachable: system requeues image and triggers alert.
      E3. New model not available (Regenerate): system aborts regeneration; existing embeddings remain in use.
    ],
    [*Related Requirements*], [CAT-FR-05, CAT-FR-15],
  ),
  caption: [UC-SYS-EMB -- Embedding Operations.],
)

==== Background Maintenance

// Diagram placeholder: Background Maintenance use case diagram

==== UC-SYS-MNT — System Maintenance

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-SYS-MNT],
    [*Use Case Name*], [System Maintenance],
    [*Primary Actor*], [System],
    [*Supporting Actors*], [ML Service, Payment Gateway],
    [*Goal*], [Perform scheduled and event-driven system maintenance tasks.],
    [*Trigger*], [Maintenance schedule fires or event-driven trigger occurs.],
    [*Preconditions*], [
      - Maintenance schedule is active.
      - Required services are operational.
    ],
    [*Postconditions*], [
      - System health, data consistency, and index performance maintained.
    ],
    [*Main Success Scenario*], [
      *Monitor Service Health*
      1. System invokes health check endpoint on ML service at configured interval.
      2. ML Service responds with current health status: healthy, degraded, or unhealthy.
      3. System records health status and response time.
      4. System updates service availability flag. If unhealthy, routes search to degraded path.
      ,
      *Expire Abandoned Carts*
      1. System executes scheduled job at configured time.
      2. System queries for carts with no modification in past seven days.
      3. For each abandoned cart, releases reserved inventory back to availability.
      4. System deletes or marks abandoned cart for cleanup and logs counts.
      ,
      *Release Expired Reservations*
      1. System executes scheduled job at configured interval.
      2. System queries for reservations with hold time exceeding configured expiry window (default 15 min).
      3. For each expired reservation, releases reserved quantity back to available stock.
      4. System marks reservation record as expired and logs counts.
      ,
      *Process Payment Webhooks*
      1. Payment Gateway sends signed webhook notification to system endpoint.
      2. System validates cryptographic signature against configured gateway secret.
      3. System extracts payment identifier, event type, and payload.
      4. System verifies not a duplicate by checking idempotency key.
      5. System updates local payment state and triggers any required order state transitions.
      6. System returns success response to gateway.
      ,
      *Maintain Search Index*
      1. System executes scheduled job at configured interval.
      2. System analyses current embedding index state: vector count, index size, recent query performance.
      3. System determines whether maintenance is likely to improve performance.
      4. System performs maintenance (e.g. index rebuild, dead tuple removal, statistics update).
      5. System verifies maintenance completed and logs execution with before/after metrics.
    ],
    [*Alternative Flows*], [
      A1. No carts/items meet criteria: system logs zero and completes.
      A2. Duplicate webhook: system returns success without changes; duplicate logged.
      A3. Out-of-order webhook event: system logs anomaly and applies state change.
      A4. Index below maintenance threshold: system skips and logs decision with current metrics.
      A5. Active search queries during index maintenance: system performs lighter, non-blocking operations.
    ],
    [*Exception Flows*], [
      E1. Health check endpoint does not respond: system marks unhealthy after timeout and triggers alert.
      E2. Database unavailable: system records failure and retries on next execution.
      E3. Webhook signature validation fails: system rejects with authentication error.
      E4. Index maintenance fails: system logs error and triggers alert; search continues with current index.
      E5. Index appears corrupted: system triggers full rebuild from stored embeddings.
    ],
    [*Related Requirements*], [CAT-FR-06, CAT-FR-08, ORD-FR-03, INV-FR-07, PAY-FR-04],
  ),
  caption: [UC-SYS-MNT -- System Maintenance.],
)
