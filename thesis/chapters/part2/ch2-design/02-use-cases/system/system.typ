==== Embedding Operations

// Diagram placeholder: Embedding Operations use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-SYS-EMB-01], [Generate image embeddings], [System],
    [When an administrator uploads a product image, the system processes the image asynchronously to produce a visual embedding using the configured model.],
    [The image has an associated embedding available for visual search.],
    [CAT-FR-05, CAT-FR-15],
    [UC-SYS-EMB-02], [Regenerate all embeddings], [System],
    [When the embedding model configuration changes, regenerate embeddings for all existing product images using the new model.],
    [All product images have embeddings consistent with the active model. Search results reflect the current configuration.],
    [CAT-FR-08, CAT-FR-15],
    [UC-SYS-EMB-03], [Monitor ML service health], [System],
    [Continuously check the availability and responsiveness of the ML sidecar service.],
    [ML sidecar availability monitored. Requests are routed only when the service reports healthy.],
    [NFR-04],
  ),
  caption: [System use cases — Embedding Operations.],
)

==== Background Maintenance

// Diagram placeholder: Background Maintenance use case diagram

#figure(
  table(
    columns: (auto, auto, auto, 1fr, 1fr, 1fr),
    stroke: 0.5pt,
    table.header(
      [*UC-ID*],
      [*Use Case*],
      [*Primary Actor*],
      [*Main Flow*],
      [*Postcondition*],
      [*Related FR*],
    ),
    [UC-SYS-JOB-01], [Expire abandoned carts], [System],
    [On a daily schedule, identify and remove carts with no activity in the past seven days. Release any reserved inventory.],
    [Abandoned carts removed. Reserved inventory returned to availability.],
    [ORD-FR-03, NFR-05],
    [UC-SYS-JOB-02], [Release expired reservations], [System],
    [Periodically scan for inventory reservations held beyond the configured time window without checkout completion. Release expired holds.],
    [Stale reservations expired. Inventory accurately reflects true availability.],
    [INV-FR-03, INV-FR-07, NFR-05],
    [UC-SYS-JOB-03], [Process payment webhooks], [System],
    [Receive and validate payment state-change notifications from the payment gateway. Update payment and order state accordingly. Detect and discard duplicate notifications.],
    [Payment state synchronised with the gateway. Order state transitions triggered where appropriate.],
    [PAY-FR-04, NFR-05],
    [UC-SYS-JOB-04], [Maintain search index], [System],
    [Periodically optimise the search index to maintain query performance as the catalog grows.],
    [Search index optimised. Visual search latency remains stable.],
    [CAT-FR-06],
  ),
  caption: [System use cases — Background Maintenance.],
)
