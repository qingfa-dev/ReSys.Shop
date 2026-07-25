=== System Actors

The platform serves three categories of actors, each with a distinct role, set of permissions, and interaction surface. Table @tbl-system-actors summarises these actors and their primary responsibilities.

#figure(
  table(
    columns: (auto, 1fr, 1fr),
    stroke: 0.5pt,
    align: (center + horizon, left, left),

    table.header([*Actor*], [*Role Description*], [*Interaction Surface*]),

    [Customer
    (Guest +
    Authenticated)], [
      Browses the product catalog, performs keyword and visual searches, manages a shopping cart, completes multi-step checkout, and tracks order history. Guest users can browse and add items to a cart; authenticated users access profile management, wishlists, and personalised features.
    ], [
      Vue 3 Storefront SPA\
      (Web browser)
    ],

    [Administrator], [
      Manages the full product lifecycle: creating and updating products with fashion-specific metadata, uploading and organising product images, defining taxonomies, monitoring inventory levels, processing order fulfilment, and managing user accounts and permissions.
    ], [
      Vue 3 Admin SPA\
      (Web browser)
    ],

    [System
    (Background
    Services)], [
      Automated background processes that maintain data consistency and system performance: generating and indexing vector embeddings for newly uploaded images, expiring abandoned carts after a configurable time window, reserving and releasing inventory during checkout, and performing periodic index maintenance.
    ], [
      Internal services\
      (No direct UI)
    ],
  ),
  caption: [System actors and their roles within the ReSys.Shop platform.],
) <tbl-system-actors>

The Customer and Administrator actors represent human users interacting through browser-based single-page applications. The System actor represents background processes that operate without direct human interaction, executing scheduled and event-driven tasks through Hangfire job workers within the .NET application process. The three actors together define the complete set of interactions supported by the platform.
