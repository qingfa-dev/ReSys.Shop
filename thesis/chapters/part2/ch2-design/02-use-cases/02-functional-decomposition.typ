=== Functional Decomposition

The functional decomposition, shown in @fig-func-decomp, organises the platform into three top-level functional areas: Administration, Storefront, and Background Services. Each functional area decomposes into constituent modules, reflecting the responsibilities defined in the use case summary matrix (Section 2.2.4).

The Administration area encompasses seven modules accessible to the Administrator actor: Catalog Management, Order Management, Payment Management, Inventory Management, Identity Management, Shipping Management, and Location Management. The Storefront area covers Customer-facing functionality across three modules: Product Discovery (browse, search, visual search), Purchase Flow (cart, checkout, payment processing, order history), and Account Management (authentication, session management, profile management). Background Services include five automated processes performed by the System actor: Embedding Generation, Cart Expiry, Inventory Reservation, Payment Webhook Processing, and Index Optimisation.

#figure(
  image("../../../../figures/chapters/part2/ch2-design/02-use-cases/diagrams/P2S2.2.2_functional-decomposition.png", width: 100%),
  caption: [Functional decomposition of ReSys.Shop using a Work Breakdown Structure (WBS), showing the hierarchical breakdown into three functional areas and their constituent modules and sub-functions.]
) <fig-func-decomp>
