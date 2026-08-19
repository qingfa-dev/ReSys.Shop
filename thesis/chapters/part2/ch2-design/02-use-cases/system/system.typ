==== Embedding Operations
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-embedding-operations.png",
    width: 100%
  ),
  caption: [Use case diagram for Embedding Operations (UC-SYS-EMB).],
) <fig-uc-sys-emb-d>

==== UC-SYS-EMB: Embedding Operations

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-SYS-EMB: Embedding Operations],
    [*Actor*], [System],
    [*Support*], [ML Service],
    [*Goal*], [Generate and regenerate image embeddings for product search.],
    [*Pre/Post*], [
      Pre: images uploaded and stored; ML service is operational.
      Post: embeddings stored in the index for visual search.
    ],
    [*Scenario*], [
      *Generate Image Embeddings*
      + System detects a new unprocessed image in the queue.
      + System retrieves the image file from storage.
      + System preprocesses the image (resize, normalise) for model requirements.
      + System sends preprocessed image to ML service with configured model identifier.
      + ML Service computes the visual embedding vector and returns it with metadata.
      + System stores embedding in the index with image reference and metadata.
      + System marks the image as processed.
      ,
      *Regenerate All Embeddings*
      + System detects change to embedding model configuration.
      + System validates new model is available via ML service health check.
      + System retrieves list of all product images requiring regeneration.
      + System sends each image to ML service using the new model.
      + ML Service computes new embedding and returns it.
      + System stores new embedding replacing previous and updates model metadata.
      + System reports completion with success/failure count summary.
      ,
    ],
    [*Alternatives*], [
      + A1. Image not accessible (deleted/corrupted) → system marks as failed and records reason.
      + A2. Multiple images queued → system processes sequentially, throttling to ML service capacity.
      + A3. Triggered during peak traffic (Regenerate) → system throttles to avoid impacting search performance.
      + A4. No model change detected → system logs event and skips processing.
    ],
    [*Exceptions*], [
      + E1. ML service returns error → system retries with exponential backoff; after max retries marks as failed.
      + E2. ML service unreachable → system requeues image and triggers alert.
      + E3. New model not available (Regenerate) → system aborts regeneration; existing embeddings remain in use.
    ],
    [*Requirements*], [CAT-FR-05, CAT-FR-15],
  ),
    kind: table,
  caption: [Embedding Operations.],
)

The embedding generation process described in the main success scenario follows the pipeline illustrated in @fig-ml-pipeline: an image is authenticated, preprocessed through standardised transforms, passed through the selected model, and the resulting feature vector is serialised alongside performance metadata.

#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/04-implementation/diagrams/P2S2.2.4_ml-pipeline.png",
    width: 100%
  ),
  caption: [ML embedding pipeline: step-by-step flow from image upload to normalised vector response.],
) <fig-ml-pipeline>

==== Background Maintenance
#figure(
  image(
    "../../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_usecase-background-maintenance.png",
    width: 85%
  ),
  caption: [Use case diagram for Background Maintenance (UC-SYS-MNT).],
) <fig-uc-sys-mnt-d>

==== UC-SYS-MNT: System Maintenance

#figure(
  table(
    columns: (auto, 1fr),
    align: (left + horizon, left),
    stroke: 0.5pt,
    [*Use Case*], [UC-SYS-MNT: System Maintenance],
    [*Actor*], [System],
    [*Support*], [ML Service, Payment Gateway],
    [*Goal*], [Perform scheduled and event-driven system maintenance tasks.],
    [*Pre/Post*], [
      Pre: maintenance schedule is active; required services are operational.
      Post: system health, data consistency, and index performance maintained.
    ],
    [*Scenario*], [
      *Monitor Service Health*
      + System invokes health check endpoint on ML service at configured interval.
      + ML Service responds with current health status: healthy, degraded, or unhealthy.
      + System records health status and response time.
      + System updates service availability flag; if unhealthy, routes search to degraded path.
      ,
      *Expire Abandoned Carts*
      + System executes scheduled job at configured time.
      + System queries for carts with no modification in past seven days.
      + For each abandoned cart, releases reserved inventory back to availability.
      + System deletes or marks abandoned cart for cleanup and logs counts.
      ,
      *Release Expired Reservations*
      + System executes scheduled job at configured interval.
      + System queries for reservations with hold time exceeding configured expiry window (default 15 min).
      + For each expired reservation, releases reserved quantity back to available stock.
      + System marks reservation record as expired and logs counts.
      ,
      *Process Payment Webhooks*
      + Payment Gateway sends signed webhook notification to system endpoint.
      + System validates cryptographic signature against configured gateway secret.
      + System extracts payment identifier, event type, and payload.
      + System verifies not a duplicate by checking idempotency key.
      + System updates local payment state and triggers any required order state transitions.
      + System returns success response to gateway.
      ,
      *Maintain Search Index*
      + System executes scheduled job at configured interval.
      + System analyses current embedding index state: vector count, index size, recent query performance.
      + System determines whether maintenance is likely to improve performance.
      + System performs maintenance (e.g. index rebuild, dead tuple removal, statistics update).
      + System verifies maintenance completed and logs execution with before/after metrics.
      ,
    ],
    [*Alternatives*], [
      + A1. No carts/items meet criteria → system logs zero and completes.
      + A2. Duplicate webhook → system returns success without changes; duplicate logged.
      + A3. Out-of-order webhook event → system logs anomaly and applies state change.
      + A4. Index below maintenance threshold → system skips and logs decision with current metrics.
      + A5. Active search queries during index maintenance → system performs lighter, non-blocking operations.
    ],
    [*Exceptions*], [
      + E1. Health check endpoint does not respond → system marks unhealthy after timeout and triggers alert.
      + E2. Database unavailable → system records failure and retries on next execution.
      + E3. Webhook signature validation fails → system rejects with authentication error.
      + E4. Index maintenance fails → system logs error and triggers alert; search continues with current index.
      + E5. Index appears corrupted → system triggers full rebuild from stored embeddings.
    ],
    [*Requirements*], [CAT-FR-06, CAT-FR-08, ORD-FR-03, INV-FR-07, PAY-FR-04],
  ),
    kind: table,
  caption: [System Maintenance.],
)
