#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/admin/uc-0009-manage-products.png", width: 100%),
  caption: [Use Case Diagram for UC-0009],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0009*], [*Manage Products*],
    [Actor], [Administrator],
    [Description], [Create and manage products, variants, and metadata in the catalog.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Admin is authenticated.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Admin clicks "New Product".],
      [2], [Enters Name, Slug, Description.],
      [3], [Adds Variants (Price, SKU, Attributes).],
      [4], [Sets Status (Draft/Active).],
      [5], [System saves Product and Variants to database.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Product is discoverable (if Active).],
    ),

    [Related Use Cases], [UC-0011 (Taxonomy)],
  ),
  caption: [UC-0009: Manage Products],
)

This use case encompasses the lifecycle management of products within the catalog. It empowers administrators to create, update, and deactivate products, establishing the core data required for the storefront. A critical aspect is the management of SKU-level variants (e.g., sizes, colors) under a single parent product, allowing for complex inventory tracking and accurate pricing models.
