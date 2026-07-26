=== System Actors

The platform serves three categories of actors, each with a distinct role, set of responsibilities, and interaction surface. Table @tbl-system-actors summarises these actors.

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left, left),
    table.header([*Actor*], [*Role and Responsibilities*], [*Interaction Surface*]),
    [Customer
    (Guest and
    Authenticated)], [
      *Product Discovery.* Browse catalog with faceted filters and category navigation. Perform keyword searches. Perform *visual searches* by uploading a reference image to find similar products. \
      *Cart and Checkout.* Add products to a persistent cart. Complete *multi-step checkout*: address selection, delivery method, payment confirmation, order finalisation. \
      *Account.* Register with email and password or Google OAuth. Manage profile, addresses, and wishlists. View order history and track fulfilment. \
      *Guest capability.* Browse, search, and manage a cart without registration. Guest session promoted to authenticated context on login.
    ], [
      Vue 3 Storefront SPA\
      (Web browser)
    ],
    [Administrator], [
      *Product Management.* Create, update, archive, and delete products. Define fashion-specific metadata (style code, season, material, department, gender target). Manage variants with SKUs, barcodes, dimensions, and pricing. Upload product images; uploads trigger the embedding generation pipeline. \
      *Taxonomy and Classification.* Define hierarchical taxonomies. Assign products to taxon nodes. Manage option types (Size, Colour, Material) with ordered values. \
      *Order Fulfilment.* Review orders, update fulfilment status, process payment captures and refunds, manage shipment tracking. \
      *Inventory Monitoring.* View real-time stock levels per variant per location. Review stock movement audit history. \
      *User Governance.* Create and manage user accounts. Assign roles and granular permissions (`domain:category:action`).
    ], [
      Vue 3 Admin SPA\
      (Web browser, separate surface)
    ],
    [System
    (Background
    Services)], [
      *Embedding Generation.* Processes uploaded images through the ML sidecar, stores resulting embeddings with model metadata. Runs asynchronously; uploads complete immediately, embeddings become available when processing finishes. \
      *Cart Expiry.* Daily scheduled job removes carts inactive for seven days, releasing reserved inventory. \
      *Inventory Reservation.* Holds stock during active checkout. Releases expired reservations after fifteen minutes of inactivity. \
      *Index Maintenance.* Periodic rebuilds maintain search performance as the catalog grows. \
      *Payment Webhooks.* Validates and processes incoming gateway webhooks asynchronously, updating payment state.
    ], [
      Internal services\
      (No direct user interface)
    ],
  ),
  caption: [System actors and their roles within the ReSys.Shop platform. The Customer and Administrator represent human users; the System represents automated background processes.],
) <tbl-system-actors>
