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
    [*Goal*], [Process an uploaded product image asynchronously to produce a visual embedding for similarity search.],
    [*Trigger*], [An image has been uploaded for a product variant and is awaiting processing.],
    [*Preconditions*], [
      - An image has been uploaded and stored.
      - The ML service is operational.
    ],
    [*Postconditions*], [
      - The image has an associated embedding vector stored in the index.
      - The embedding is available for visual search and similar product retrieval.
    ],
    [*Main Success Scenario*], [
      1. System -- Detects a new unprocessed image in the processing queue.
      2. System -- Retrieves the image file from storage.
      3. System -- Preprocesses the image (resize, normalise) to match the model's input requirements.
      4. System -- Sends the preprocessed image to the ML service with the configured model identifier.
      5. ML Service -- Computes the visual embedding vector for the image.
      6. ML Service -- Returns the embedding vector with model metadata (name, version, dimension).
      7. System -- Stores the embedding vector in the image embeddings index with the image reference and model metadata.
      8. System -- Marks the image as processed.
    ],
    [*Alternative Flows*], [
      A1. Image file is not accessible (deleted or corrupted) -- System marks the image as failed and records the failure reason; the administrator can re-upload the image.
      A2. Multiple images are queued simultaneously -- System processes them sequentially, throttling requests to respect the ML service's capacity.
    ],
    [*Exception Flows*], [
      E1. ML service returns an error -- System retries with exponential backoff up to a configured maximum; after all retries, the image is marked as failed.
      E2. ML service is unreachable -- System queues the image for retry and triggers an alert for the operations team.
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
    [*Goal*], [Regenerate embeddings for all existing product images when the embedding model configuration changes.],
    [*Trigger*], [The embedding model configuration has been changed (e.g. model identifier or version updated).],
    [*Preconditions*], [
      - The embedding model configuration has changed.
      - The new model is available and operational on the ML service.
    ],
    [*Postconditions*], [
      - All product images have embeddings consistent with the active model.
      - Search results reflect the current model configuration.
    ],
    [*Main Success Scenario*], [
      1. System -- Detects a change to the embedding model configuration.
      2. System -- Validates that the new model is available on the ML service by requesting a health check.
      3. System -- Retrieves the list of all product images that require regeneration.
      4. System -- For each image, sends it to the ML service using the new model.
      5. ML Service -- Computes the new embedding and returns it.
      6. System -- Stores the new embedding, replacing the previous one for that image.
      7. System -- Updates the model metadata on each embedding record.
      8. System -- Reports completion with a summary of success and failure counts.
    ],
    [*Alternative Flows*], [
      A1. Regeneration is triggered during peak traffic -- System throttles processing to avoid impacting search query performance.
      A2. Some images have missing or corrupted files -- System skips those images and records the failure; processing continues for remaining images.
      A3. Regeneration is triggered but no model change is detected -- System logs the event and skips processing.
    ],
    [*Exception Flows*], [
      E1. New model is not available on the ML service -- System aborts regeneration and reports the failure; existing embeddings remain in use.
      E2. Partial failure during batch regeneration -- System continues processing remaining images; a final report lists all failures with reasons.
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
      - Service availability monitored.
      - Requests are routed to the ML service only when it reports healthy.
    ],
    [*Main Success Scenario*], [
      1. System -- Invokes the health check endpoint on the ML service at the configured interval.
      2. ML Service -- Responds with its current health status: healthy, degraded, or unhealthy.
      3. System -- Records the health status and response time.
      4. System -- Updates the service availability flag based on the response.
      5. System -- If the status transitions to unhealthy, routes search requests to a degraded path that returns cached results or a service-unavailable message.
    ],
    [*Alternative Flows*], [
      A1. ML service reports degraded status -- System continues to route requests but logs a warning for the operations team.
      A2. ML service reports healthy after a period of unavailability -- System clears the unhealthy flag and resumes normal request routing.
    ],
    [*Exception Flows*], [
      E1. Health check endpoint does not respond -- System marks the service as unhealthy after the configured timeout and triggers an alert for the operations team.
      E2. Health check response is malformed -- System treats the response as unhealthy and logs the parsing error for investigation.
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
    [*Goal*], [On a daily schedule, identify and remove carts with no activity in the past seven days; release any reserved inventory.],
    [*Trigger*], [The daily maintenance schedule fires.],
    [*Preconditions*], [
      - The maintenance schedule is active.
    ],
    [*Postconditions*], [
      - Abandoned carts removed.
      - Reserved inventory returned to availability.
    ],
    [*Main Success Scenario*], [
      1. System -- Executes the scheduled job at the configured time.
      2. System -- Queries for all carts with no modification activity in the past seven days.
      3. System -- For each abandoned cart, identifies any reserved inventory quantities.
      4. System -- Releases the reserved inventory back to available stock.
      5. System -- Deletes or marks the abandoned cart for cleanup.
      6. System -- Logs the number of carts expired and inventory released.
    ],
    [*Alternative Flows*], [
      A1. No carts meet the abandonment criteria -- System logs the job execution with zero expired carts and completes.
      A2. A cart has reserved inventory but the associated stock item was deleted -- System skips the inventory release for that item and proceeds with cart expiration.
    ],
    [*Exception Flows*], [
      E1. Database is temporarily unavailable -- System records the failure and retries on the next scheduled execution.
      E2. Partial failure during batch processing -- System continues processing remaining carts; a report lists the failures.
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
    [*Goal*], [Periodically scan for inventory reservations held beyond the configured time window without checkout completion; expire stale holds.],
    [*Trigger*], [The maintenance schedule fires at the configured interval.],
    [*Preconditions*], [
      - The maintenance schedule is active.
    ],
    [*Postconditions*], [
      - Stale reservations expired.
      - Inventory accurately reflects true availability.
    ],
    [*Main Success Scenario*], [
      1. System -- Executes the scheduled job at the configured interval.
      2. System -- Queries for all inventory reservations with a hold time exceeding the configured expiry window (default 15 minutes).
      3. System -- For each expired reservation, releases the reserved quantity back to available stock.
      4. System -- Marks the reservation record as expired.
      5. System -- Logs the number of expired reservations and quantities released.
    ],
    [*Alternative Flows*], [
      A1. Reservation is associated with an active checkout session that is still progressing -- System retains the reservation and does not expire it (activity timestamp prevents incorrect expiry).
      A2. No expired reservations are found -- System logs the job execution with zero expirations and completes.
    ],
    [*Exception Flows*], [
      E1. Database is temporarily unavailable -- System records the failure and retries on the next scheduled execution.
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
    [*Goal*], [Receive and validate payment state-change notifications from the payment gateway; update payment and order state accordingly; detect and discard duplicates.],
    [*Trigger*], [The payment gateway sends a webhook notification to the system's webhook endpoint.],
    [*Preconditions*], [
      - The webhook endpoint is reachable by the payment gateway.
    ],
    [*Postconditions*], [
      - Payment state synchronised with the gateway.
      - Order state transitions triggered where appropriate.
    ],
    [*Main Success Scenario*], [
      1. Payment Gateway -- Sends a signed webhook notification to the system endpoint.
      2. System -- Validates the cryptographic signature of the webhook payload against the configured gateway secret.
      3. System -- Extracts the payment identifier, event type, and payload.
      4. System -- Looks up the local payment record by the gateway payment identifier.
      5. System -- Verifies this is not a duplicate by checking the idempotency key (event identifier).
      6. System -- Updates the local payment state to match the gateway state.
      7. System -- If the event type indicates a state transition that requires an order state change (e.g. payment captured, payment refunded), triggers the corresponding order state transition.
      8. System -- Returns a success response to the gateway.
    ],
    [*Alternative Flows*], [
      A1. Duplicate webhook detected (same event identifier already processed) -- System returns a success response without making any changes; the duplicate is logged.
      A2. Payment record not found locally -- System logs the unknown payment event and returns a success response (to acknowledge receipt); the event is queued for investigation.
      A3. Webhook event represents an out-of-order state (e.g. refund before capture) -- System logs the anomaly and applies the state change; the current state always reflects the latest gateway truth.
    ],
    [*Exception Flows*], [
      E1. Signature validation fails -- System rejects the webhook with an authentication error; the event is not processed.
      E2. System fails to persist state changes -- System returns a failure response to the gateway, which will retry the webhook delivery.
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
    [*Goal*], [Periodically maintain the vector search index to sustain query performance as the embedding volume grows.],
    [*Trigger*], [The maintenance schedule fires at the configured interval.],
    [*Preconditions*], [
      - The maintenance schedule is active.
      - The search index exists.
    ],
    [*Postconditions*], [
      - Search index optimised.
      - Visual search latency remains stable.
    ],
    [*Main Success Scenario*], [
      1. System -- Executes the scheduled job at the configured interval.
      2. System -- Analyses the current state of the embedding index: total vector count, index size, and recent query performance metrics.
      3. System -- Determines whether index maintenance is likely to improve performance.
      4. System -- Performs maintenance operations (e.g. index rebuilding, dead tuple removal, statistics update).
      5. System -- Verifies the maintenance completed successfully.
      6. System -- Logs the maintenance execution with before/after metrics.
    ],
    [*Alternative Flows*], [
      A1. Index size is below the threshold where maintenance provides benefit -- System skips maintenance and logs the decision with the current metrics.
      A2. Maintenance runs during a period of active search queries -- System performs lighter, non-blocking maintenance operations to avoid impacting concurrent queries.
    ],
    [*Exception Flows*], [
      E1. Index maintenance fails -- System logs the error and triggers an alert; existing search functionality continues using the current index state.
      E2. Index appears corrupted -- System triggers a full index rebuild from stored embeddings.
    ],
    [*Related Requirements*], [CAT-FR-06],
  ),
  caption: [UC-SYS-MNT-05 -- Maintain Search Index.],
)
