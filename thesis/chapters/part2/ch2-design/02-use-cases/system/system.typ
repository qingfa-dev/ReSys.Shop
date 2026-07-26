==== Embedding Operations

// Diagram placeholder: Embedding Operations use case diagram

*UC-SYS-EMB-01 — Generate image embeddings.*
*Primary Actor:* System. \
*Main Flow:* When an administrator uploads a product image, the system processes the image asynchronously to produce a visual embedding using the configured model. \
*Postcondition:* The image has an associated embedding available for visual search. \
*Related FR:* CAT-FR-05, CAT-FR-15.

#v(0.5cm)
*UC-SYS-EMB-02 — Regenerate all embeddings.*
*Primary Actor:* System. \
*Main Flow:* When the embedding model configuration changes, regenerate embeddings for all existing product images using the new model. \
*Postcondition:* All product images have embeddings consistent with the active model. Search results reflect the current configuration. \
*Related FR:* CAT-FR-08, CAT-FR-15.

#v(0.5cm)
*UC-SYS-EMB-03 — Monitor ML service health.*
*Primary Actor:* System. \
*Main Flow:* Continuously check the availability and responsiveness of the ML sidecar service. \
*Postcondition:* ML sidecar availability monitored. Requests are routed only when the service reports healthy. \
*Related FR:* NFR-04.

==== Background Maintenance

// Diagram placeholder: Background Maintenance use case diagram

*UC-SYS-JOB-01 — Expire abandoned carts.*
*Primary Actor:* System. \
*Main Flow:* On a daily schedule, identify and remove carts with no activity in the past seven days. Release any reserved inventory. \
*Postcondition:* Abandoned carts removed. Reserved inventory returned to availability. \
*Related FR:* ORD-FR-03, NFR-05.

#v(0.5cm)
*UC-SYS-JOB-02 — Release expired reservations.*
*Primary Actor:* System. \
*Main Flow:* Periodically scan for inventory reservations held beyond the configured time window without checkout completion. Release expired holds. \
*Postcondition:* Stale reservations expired. Inventory accurately reflects true availability. \
*Related FR:* INV-FR-03, INV-FR-07, NFR-05.

#v(0.5cm)
*UC-SYS-JOB-03 — Process payment webhooks.*
*Primary Actor:* System. \
*Main Flow:* Receive and validate payment state-change notifications from the payment gateway. Update payment and order state accordingly. Detect and discard duplicate notifications. \
*Postcondition:* Payment state synchronised with the gateway. Order state transitions triggered where appropriate. \
*Related FR:* PAY-FR-04, NFR-05.

#v(0.5cm)
*UC-SYS-JOB-04 — Maintain search index.*
*Primary Actor:* System. \
*Main Flow:* Periodically optimise the search index to maintain query performance as the catalog grows. \
*Postcondition:* Search index optimised. Visual search latency remains stable. \
*Related FR:* CAT-FR-06.
