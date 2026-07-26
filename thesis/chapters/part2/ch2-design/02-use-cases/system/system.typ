==== Embedding Operations

// Diagram placeholder: Embedding Operations use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-SYS-EMB-01], [Generate image embeddings], [System], [Process an uploaded product image asynchronously to produce a visual embedding.], [An image has been uploaded and is awaiting processing. The embedding service is operational.], [The image has an associated embedding available for visual search.],
  [UC-SYS-EMB-02], [Regenerate all embeddings], [System], [Regenerate embeddings for all existing product images when the embedding model configuration changes.], [The embedding model configuration has changed.], [All product images have embeddings consistent with the active model. Search results reflect the current configuration.],
)

==== Background Maintenance

// Diagram placeholder: Background Maintenance use case diagram

#table(
  columns: (auto, 1fr, auto, 3fr, 2fr, 3fr),
  stroke: 0.5pt,
  [*UC-ID*], [*Use Case*], [*Actor*], [*Goal*], [*Preconditions*], [*Postconditions*],
  [UC-SYS-MNT-01], [Monitor service health], [System], [Continuously check the availability and responsiveness of the ML sidecar service.], [Monitoring infrastructure is operational.], [Service availability monitored. Requests are routed only when the service reports healthy.],
  [UC-SYS-MNT-02], [Expire abandoned carts], [System], [On a daily schedule, identify and remove carts with no activity in the past seven days; release any reserved inventory.], [The maintenance schedule is active.], [Abandoned carts removed. Reserved inventory returned to availability.],
  [UC-SYS-MNT-03], [Release expired reservations], [System], [Periodically scan for inventory reservations held beyond the configured time window without checkout completion; expire stale holds.], [The maintenance schedule is active.], [Stale reservations expired. Inventory accurately reflects true availability.],
  [UC-SYS-MNT-04], [Process payment webhooks], [System], [Receive and validate payment state-change notifications from the payment gateway; update payment and order state accordingly; detect and discard duplicates.], [The webhook endpoint is reachable by the payment gateway.], [Payment state synchronised with the gateway. Order state transitions triggered where appropriate.],
  [UC-SYS-MNT-05], [Maintain search index], [System], [Periodically optimise the search index to maintain query performance as the catalog grows.], [The maintenance schedule is active. The search index exists.], [Search index optimised. Visual search latency remains stable.],
)
