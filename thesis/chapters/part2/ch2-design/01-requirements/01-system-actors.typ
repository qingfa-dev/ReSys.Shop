=== System Actors

The platform serves three categories of actors, defined by their access level, responsibilities, and interaction surface.

==== Customer: Product Discovery and Purchase

The *Customer* is the primary beneficiary of the research contribution. Customers interact through a browser-based *storefront* (Vue 3 SPA) and their capabilities span the full shopping workflow.

*Product Discovery.* Browse the catalog with faceted filters and hierarchical category navigation. Perform *keyword searches* by product name or attribute. Perform *visual searches* by uploading a reference image; the system returns visually similar products ranked by embedding similarity score.

*Cart and Checkout.* Add products to a persistent cart (guest or authenticated). Complete a *multi-step checkout* spanning address selection, delivery method choice, payment confirmation, and order finalisation. Cart contents survive page navigation and browser restarts.

*Account.* Register with email and password. Log in with credentials or Google OAuth. Manage profile details, shipping addresses, and wishlists. View order history and track fulfilment status.

*Guest capability.* Guest users can browse, search, and manage a cart without registration. Upon account creation, the guest session is promoted to the authenticated context, preserving cart contents.

==== Administrator: Data Management and Operational Oversight

The *Administrator* operates the *administration interface* (Vue 3 Admin SPA), a separate application surface from the storefront with independent authentication requirements.

*Product Management.* Create, update, archive, and delete products. Define *fashion-specific metadata*: style code, season, material composition, department, gender target, care instructions, and fit notes. Manage *variants* (size and colour combinations) with SKUs, barcodes, physical dimensions, and independent pricing per variant. Upload and organise product images; each upload automatically triggers the *embedding generation pipeline*.

*Taxonomy and Classification.* Define *hierarchical taxonomies* (e.g., Clothing → Dresses → Evening Dresses). Assign products to taxon nodes for category-based browsing. Manage *option types* (Size, Colour, Material) with predefined option values.

*Order Fulfilment.* Review incoming orders, update fulfilment status, process payment captures and refunds, and manage shipment tracking.

*Inventory Monitoring.* View real-time stock levels per product variant per warehouse location. Review stock movement history for audit purposes.

*User Governance.* Create and manage user accounts. Assign *roles* (Customer, Administrator) and granular *permissions* (e.g., `catalog:products:create`, `orders:fulfillment:update`). Review user activity and manage account status.

==== System: Automated Background Processes

The *System* actor represents automated processes executing without human interaction. These run as scheduled or event-driven *background jobs* within the .NET application, backed by durable Hangfire storage.

*Embedding Generation.* When an administrator uploads a product image, a background job sends the image to the Python ML sidecar, receives the embedding vector, and stores it in pgvector with model metadata. The upload endpoint returns immediately; the embedding becomes available for search once the job completes.

*Cart Expiry.* A daily scheduled job removes carts with no activity for seven days, releasing reserved inventory and preventing accumulation of abandoned session data.

*Inventory Reservation.* During checkout, stock quantities are temporarily held. If checkout is not completed within a configurable window (fifteen minutes), the reservation expires and stock is returned to availability.

*Index Maintenance.* Periodic HNSW index rebuilds on the embedding column maintain search performance as the catalog grows, preventing query degradation from index fragmentation.

*Payment Webhook Processing.* Incoming Stripe webhooks are validated via signature verification and processed asynchronously, updating payment intent state without blocking the HTTP response to the gateway.