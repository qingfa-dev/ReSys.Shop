=== System Actors

The platform serves three categories of actors, defined by their access level, responsibilities, and interaction surface.

*Customer.* The primary beneficiary of the research contribution. Customers interact with the platform through a browser-based storefront. Their capabilities span the full shopping workflow:

- Browse the product catalog with faceted filters and category navigation
- Perform keyword searches and, critically, *visual searches* by uploading images to find similar products
- Manage a shopping cart that persists across sessions (guest or authenticated)
- Complete a multi-step checkout: address selection, delivery choice, payment, and confirmation
- Track order status and view purchase history (authenticated users only)
- Manage profile details, addresses, and wishlists (authenticated users only)

Guest customers can browse, search, and add items to a cart without registration. Upon account creation, the guest session is promoted to the authenticated context, preserving the cart contents.

*Administrator.* Administrators operate the back-office interface, also a browser-based application, with full access to management functions across all business modules:

- Create, update, and archive products with fashion-specific metadata (style code, season, material, department, gender target)
- Upload and organise product images, triggering the embedding generation pipeline
- Define and manage hierarchical product taxonomies
- Monitor real-time inventory levels and process order fulfilment
- Manage user accounts, roles, and fine-grained permissions

Unlike the Customer actor, whose primary interaction is *discovery and purchase*, the Administrator actor is concerned with *data management and operational oversight*. The administrator interface is a distinct application surface from the storefront, with separate authentication requirements.

*System (Background Services).* The System actor represents automated processes that execute without human interaction. These processes run as scheduled or event-driven background jobs within the .NET application:

- *Embedding generation.* When an administrator uploads a new product image, a background job sends the image to the Python ML sidecar, receives the embedding vector, and stores it in pgvector. The upload completes immediately; the embedding appears in search results once the job finishes.

- *Cart management.* A daily scheduled job removes carts with no activity for seven days, releasing reserved inventory and preventing accumulation of abandoned data.

- *Inventory reservation.* During checkout, inventory quantities are temporarily reserved. If the checkout is not completed within a configurable window, the reservation expires and stock is returned to availability.

- *Index maintenance.* Periodic HNSW index rebuilds on the embedding column maintain search performance as the catalog grows.

The three actors together define the complete set of interactions supported by the platform. The Customer and Administrator represent human users; the System represents automated infrastructure that maintains data consistency and performance without direct human interaction.