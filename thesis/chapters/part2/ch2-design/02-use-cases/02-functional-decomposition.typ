=== Functional Decomposition

The platform decomposes into three functional areas, whose work breakdown is illustrated by @fig-uc-overview in Section 2.2.3 (with each area's use cases enumerated in the summary matrix @tbl-uc-summary):

- *Administration.* Seven modules: Catalog, Ordering, Payment, Inventory, Identity, Shipping, Location.
- *Storefront.* Three modules: Product Discovery (browse, search, visual search), Purchase Flow (cart, checkout, payment, order history), Account Management (authentication, session, profile).
- *Background Services.* Five processes: Embedding Generation, Cart Expiry, Inventory Reservation, Payment Webhook Processing, Index Optimisation.
