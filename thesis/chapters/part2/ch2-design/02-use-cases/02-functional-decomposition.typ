=== Functional Decomposition

@fig-func-decomp presents the work breakdown structure (WBS) of the platform across three functional areas:

- *Administration.* Seven modules: Catalog, Ordering, Payment, Inventory, Identity, Shipping, Location.
- *Storefront.* Three modules: Product Discovery (browse, search, visual search), Purchase Flow (cart, checkout, payment, order history), Account Management (authentication, session, profile).
- *Background Services.* Five processes: Embedding Generation, Cart Expiry, Inventory Reservation, Payment Webhook Processing, Index Optimisation.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_functional-decomposition.png", width: 100%),
  caption: [Functional decomposition of ReSys.Shop using a Work Breakdown Structure (WBS), showing the hierarchical breakdown into three functional areas and their constituent modules and sub-functions.]
) <fig-func-decomp>
