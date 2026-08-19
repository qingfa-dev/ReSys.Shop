=== System Actors

Three categories of actors interact with the platform, distinguished by access level and interaction surface.

==== Customer

The *Customer* accesses the browser-based *storefront*.

- *Discovery.* Browse catalog with faceted filters, keyword search, and visual CBIR (Section 2.3.3). Guests browse and cart without registration; session promoted on login.
- *Purchase.* Persistent cart, multi-step checkout (address, delivery, payment, confirm).
- *Account.* Register or Google OAuth login; manage profile, addresses, wishlists, order history.

==== Administrator

The *Administrator* operates a separate administration interface.

- *Catalog.* CRUD products with fashion metadata; variants and pricing; image uploads triggering embedding pipeline; taxonomies, option types, product classification.
- *Operations.* Order review, payment capture/refund, shipment management; real-time stock monitoring per location with audit history.
- *Governance.* User CRUD; role assignment (Customer, Administrator); permission grants (`domain.category.resource.action`).

==== System

The *System* actor runs automated background jobs via *Hangfire* @hangfire-docs and *Redis* @redis-docs.

- *Embedding Generation.* Process uploaded images via ML sidecar.
- *Cart Expiry.* Daily job: delete carts inactive 7 days, release inventory.
- *Inventory Reservation.* Hold stock during checkout; expire after 15-minute inactivity.
- *Maintenance.* Periodic HNSW index rebuilds; async payment webhook validation.
