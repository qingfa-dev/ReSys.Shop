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
  caption: [ML embedding pipeline: image upload to normalised vector response.],
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

*Goal:* Perform scheduled and event-driven system maintenance tasks. *Trigger:* a scheduled job fires or the payment gateway sends a signed webhook. *Related requirements:* CAT-FR-06, CAT-FR-08, ORD-FR-03, INV-FR-07, PAY-FR-04. The flow monitors ML service health, expires abandoned carts, releases expired inventory reservations, processes payment webhooks with signature and idempotency validation, and maintains the search index; alternatives and exceptions cover empty batches, duplicate or out-of-order webhooks, and database or index failures.
