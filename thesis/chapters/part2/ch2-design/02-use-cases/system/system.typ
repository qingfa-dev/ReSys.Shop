==== Embedding Operations

// Diagram placeholder: Embedding Operations use case diagram

==== UC-SYS-EMB-01 — Generate Image Embeddings

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-SYS-EMB-01],
    [*Use Case Name*], [Generate Image Embeddings],
    [*Primary Actor*], [System],
    [*Supporting Actors*], [ML Service],
    [*Goal*], [Process an uploaded product image asynchronously to produce a visual embedding.],
    [*Trigger*], [An image has been uploaded and is awaiting processing.],
    [*Preconditions*], [
      - Image uploaded and stored.
      - ML service is operational.
    ],
    [*Postconditions*], [
      - Image has an embedding stored in the index for visual search.
    ],
    [*Main Success Scenario*], [
      1. System detects a new unprocessed image in the queue.
      2. System retrieves the image file from storage.
      3. System preprocesses the image (resize, normalise) for model requirements.
      4. System sends preprocessed image to ML service with configured model identifier.
      5. ML Service computes the visual embedding vector.
      6. ML Service returns the embedding with model metadata (name, version, dimension).
      7. System stores embedding in the index with image reference and metadata.
      8. System marks the image as processed.
    ],
    [*Alternative Flows*], [
      A1. Image not accessible (deleted/corrupted): system marks as failed and records reason.
      A2. Multiple images queued: system processes sequentially, throttling to ML service capacity.
    ],
    [*Exception Flows*], [
      E1. ML service returns error: system retries with exponential backoff; after max retries marks as failed.
      E2. ML service unreachable: system requeues image and triggers alert.
    ],
    [*Related Requirements*], [CAT-FR-05],
  ),
  caption: [UC-SYS-EMB-01 -- Generate Image Embeddings.],
)

==== UC-SYS-EMB-02 — Regenerate All Embeddings

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-SYS-EMB-02],
    [*Use Case Name*], [Regenerate All Embeddings],
    [*Primary Actor*], [System],
    [*Supporting Actors*], [ML Service],
    [*Goal*], [Regenerate embeddings for all product images when the model configuration changes.],
    [*Trigger*], [Embedding model configuration has changed.],
    [*Preconditions*], [
      - Embedding model configuration has changed.
      - New model is available on ML service.
    ],
    [*Postconditions*], [
      - All embeddings consistent with active model. Search results reflect current configuration.
    ],
    [*Main Success Scenario*], [
      1. System detects change to embedding model configuration.
      2. System validates new model is available via ML service health check.
      3. System retrieves list of all product images requiring regeneration.
      4. System sends each image to ML service using the new model.
      5. ML Service computes new embedding and returns it.
      6. System stores new embedding, replacing previous.
      7. System updates model metadata on each embedding record.
      8. System reports completion with success/failure count summary.
    ],
    [*Alternative Flows*], [
      A1. Triggered during peak traffic: system throttles to avoid impacting search performance.
      A2. Missing or corrupted files: system skips and continues; reports failures.
      A3. No model change detected: system logs event and skips processing.
    ],
    [*Exception Flows*], [
      E1. New model not available: system aborts regeneration; existing embeddings remain in use.
      E2. Partial failure during batch: system continues; final report lists failures with reasons.
    ],
    [*Related Requirements*], [CAT-FR-15],
  ),
  caption: [UC-SYS-EMB-02 -- Regenerate All Embeddings.],
)

==== Background Maintenance

// Diagram placeholder: Background Maintenance use case diagram

==== UC-SYS-MNT-01 — Monitor Service Health

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-SYS-MNT-01],
    [*Use Case Name*], [Monitor Service Health],
    [*Primary Actor*], [System],
    [*Supporting Actors*], [ML Service],
    [*Goal*], [Continuously check the availability and responsiveness of the ML service.],
    [*Trigger*], [Health check interval elapses on the monitoring schedule.],
    [*Preconditions*], [
      - Monitoring infrastructure is operational.
    ],
    [*Postconditions*], [
      - Service availability monitored. Requests routed only when healthy.
    ],
    [*Main Success Scenario*], [
      1. System invokes health check endpoint on ML service at configured interval.
      2. ML Service responds with current health status: healthy, degraded, or unhealthy.
      3. System records health status and response time.
      4. System updates service availability flag based on response.
      5. If unhealthy, system routes search requests to degraded path (cached results or unavailable message).
    ],
    [*Alternative Flows*], [
      A1. Degraded status: system continues routing but logs warning for operations team.
      A2. Healthy after unavailability: system clears unhealthy flag and resumes normal routing.
    ],
    [*Exception Flows*], [
      E1. Health check endpoint does not respond: system marks unhealthy after timeout and triggers alert.
      E2. Malformed health check response: system treats as unhealthy and logs parsing error.
    ],
    [*Related Requirements*], [CAT-FR-08],
  ),
  caption: [UC-SYS-MNT-01 -- Monitor Service Health.],
)

==== UC-SYS-MNT-02 — Expire Abandoned Carts

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-SYS-MNT-02],
    [*Use Case Name*], [Expire Abandoned Carts],
    [*Primary Actor*], [System],
    [*Supporting Actors*], [None],
    [*Goal*], [Daily: identify and remove carts with no activity in seven days; release reserved inventory.],
    [*Trigger*], [Daily maintenance schedule fires.],
    [*Preconditions*], [
      - Maintenance schedule is active.
    ],
    [*Postconditions*], [
      - Abandoned carts removed. Reserved inventory released.
    ],
    [*Main Success Scenario*], [
      1. System executes scheduled job at configured time.
      2. System queries for carts with no modification in past seven days.
      3. For each abandoned cart, identifies reserved inventory.
      4. System releases reserved inventory back to availability.
      5. System deletes or marks abandoned cart for cleanup.
      6. System logs count of carts expired and inventory released.
    ],
    [*Alternative Flows*], [
      A1. No carts meet abandonment criteria: system logs zero expired carts and completes.
      A2. Reserved inventory references deleted stock item: system skips release and proceeds with expiration.
    ],
    [*Exception Flows*], [
      E1. Database unavailable: system records failure and retries on next execution.
      E2. Partial failure during batch: system continues; report lists failures.
    ],
    [*Related Requirements*], [ORD-FR-03],
  ),
  caption: [UC-SYS-MNT-02 -- Expire Abandoned Carts.],
)

==== UC-SYS-MNT-03 — Release Expired Reservations

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-SYS-MNT-03],
    [*Use Case Name*], [Release Expired Reservations],
    [*Primary Actor*], [System],
    [*Supporting Actors*], [None],
    [*Goal*], [Periodically expire inventory reservations held beyond the configured time window.],
    [*Trigger*], [Maintenance schedule fires at configured interval.],
    [*Preconditions*], [
      - Maintenance schedule is active.
    ],
    [*Postconditions*], [
      - Stale reservations expired. Inventory reflects true availability.
    ],
    [*Main Success Scenario*], [
      1. System executes scheduled job at configured interval.
      2. System queries for reservations with hold time exceeding configured expiry window (default 15 min).
      3. For each expired reservation, releases reserved quantity back to available stock.
      4. System marks reservation record as expired.
      5. System logs count of expired reservations and quantities released.
    ],
    [*Alternative Flows*], [
      A1. Associated with active checkout: system retains reservation (activity timestamp prevents incorrect expiry).
      A2. No expired reservations: system logs zero expirations and completes.
    ],
    [*Exception Flows*], [
      E1. Database unavailable: system records failure and retries on next execution.
    ],
    [*Related Requirements*], [INV-FR-07],
  ),
  caption: [UC-SYS-MNT-03 -- Release Expired Reservations.],
)

==== UC-SYS-MNT-04 — Process Payment Webhooks

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-SYS-MNT-04],
    [*Use Case Name*], [Process Payment Webhooks],
    [*Primary Actor*], [System],
    [*Supporting Actors*], [Payment Gateway],
    [*Goal*], [Receive payment state-change notifications; update payment and order state; discard duplicates.],
    [*Trigger*], [Payment gateway sends a webhook notification to the system endpoint.],
    [*Preconditions*], [
      - Webhook endpoint is reachable by the payment gateway.
    ],
    [*Postconditions*], [
      - Payment state synchronised with gateway. Order state transitions triggered where appropriate.
    ],
    [*Main Success Scenario*], [
      1. Payment Gateway sends signed webhook notification to system endpoint.
      2. System validates cryptographic signature against configured gateway secret.
      3. System extracts payment identifier, event type, and payload.
      4. System looks up local payment record by gateway payment identifier.
      5. System verifies not a duplicate by checking idempotency key (event identifier).
      6. System updates local payment state to match gateway state.
      7. If event requires order state change, triggers corresponding transition.
      8. System returns success response to gateway.
    ],
    [*Alternative Flows*], [
      A1. Duplicate webhook: system returns success without changes; duplicate logged.
      A2. Payment record not found locally: system logs and returns success; event queued for investigation.
      A3. Out-of-order event (e.g. refund before capture): system logs anomaly and applies state change.
    ],
    [*Exception Flows*], [
      E1. Signature validation fails: system rejects with authentication error.
      E2. Persistence of state changes fails: system returns failure; gateway retries delivery.
    ],
    [*Related Requirements*], [PAY-FR-04],
  ),
  caption: [UC-SYS-MNT-04 -- Process Payment Webhooks.],
)

==== UC-SYS-MNT-05 — Maintain Search Index

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    [*Field*], [*Description*],
    [*Use Case ID*], [UC-SYS-MNT-05],
    [*Use Case Name*], [Maintain Search Index],
    [*Primary Actor*], [System],
    [*Supporting Actors*], [None],
    [*Goal*], [Periodically maintain the vector search index to sustain query performance.],
    [*Trigger*], [Maintenance schedule fires at configured interval.],
    [*Preconditions*], [
      - Maintenance schedule is active.
      - Search index exists.
    ],
    [*Postconditions*], [
      - Search index optimised. Visual search latency remains stable.
    ],
    [*Main Success Scenario*], [
      1. System executes scheduled job at configured interval.
      2. System analyses current embedding index state: vector count, index size, recent query performance.
      3. System determines whether maintenance is likely to improve performance.
      4. System performs maintenance (e.g. index rebuild, dead tuple removal, statistics update).
      5. System verifies maintenance completed successfully.
      6. System logs execution with before/after metrics.
    ],
    [*Alternative Flows*], [
      A1. Index below maintenance threshold: system skips and logs decision with current metrics.
      A2. Active search queries during maintenance: system performs lighter, non-blocking operations.
    ],
    [*Exception Flows*], [
      E1. Index maintenance fails: system logs error and triggers alert; search continues with current index.
      E2. Index appears corrupted: system triggers full rebuild from stored embeddings.
    ],
    [*Related Requirements*], [CAT-FR-06],
  ),
  caption: [UC-SYS-MNT-05 -- Maintain Search Index.],
)
