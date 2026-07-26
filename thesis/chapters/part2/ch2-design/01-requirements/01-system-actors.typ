=== System Actors

The platform serves three categories of actors, defined by their access level, responsibilities, and interaction surface.

==== Customer: Product Discovery and Purchase

The *Customer* accesses the platform through the browser-based *storefront* and is the primary beneficiary of the research contribution.

- *Product Discovery.* Browse the catalog with faceted filters and hierarchical category navigation. Perform *keyword searches* by product name or attribute. Perform *visual searches* by uploading a reference image; the system returns visually similar products ranked by embedding similarity.

- *Cart and Checkout.* Add products to a persistent cart (guest or authenticated). Complete a *multi-step checkout* spanning *address selection*, *delivery method choice*, *payment confirmation*, and *order finalisation*.

- *Account.* Register with email and password or *Google OAuth*. Manage *profile details*, *shipping addresses*, and *wishlists*. View *order history* and track fulfilment status.

- *Guest capability.* Browse, search, and manage a cart without registration. On login or registration, the *guest session is promoted* to the authenticated context, preserving cart contents.

==== Administrator: Data Management and Operational Oversight

The *Administrator* operates the *administration interface*, a separate application surface with independent authentication.

- *Product Management.* Create, update, archive, and delete products. Define *fashion-specific metadata*: style code, season, material composition, department, gender target. Manage *variants* (size and colour combinations) with SKUs, barcodes, dimensions, and independent pricing. Upload product images; each upload triggers the *embedding generation pipeline*.

- *Taxonomy and Classification.* Define *hierarchical taxonomies* (e.g., Clothing → Dresses → Evening Dresses). Assign products to taxon nodes. Manage *option types* (Size, Colour, Material) with ordered values.

- *Order Fulfilment.* Review incoming orders, update *fulfilment status*, process *payment captures* and *refunds*, and manage *shipment tracking*.

- *Inventory Monitoring.* View *real-time stock levels* per variant per location. Review *stock movement history* for audit purposes.

- *User Governance.* Create and manage user accounts. Assign *roles* (Customer, Administrator) and granular *permissions* following a `domain:category:action` format.

==== System: Automated Background Processes

The *System* actor executes without human interaction through *scheduled* and *event-driven* background jobs.

- *Embedding Generation.* Processes uploaded images through the ML sidecar. Uploads return immediately; embeddings become available for search when processing completes.

- *Cart Expiry.* A *daily scheduled job* removes carts inactive for *seven days*, releasing reserved inventory.

- *Inventory Reservation.* Holds stock during active checkout. Releases expired reservations after *fifteen minutes* of inactivity.

- *Index Maintenance.* Periodic rebuilds maintain *search performance* as the catalog grows.

- *Payment Webhooks.* Validates and processes incoming gateway webhooks *asynchronously*, updating payment state without blocking the response.