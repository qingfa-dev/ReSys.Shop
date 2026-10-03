#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/customer/uc-0001-browse-products.png", width: 100%),
  caption: [Use Case Diagram for UC-0001],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0001*], [*Browse Products*],
    [Actor], [Customer],
    [Description], [Navigate the product catalog via categories and filters to discover items.],

    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Customer is on the storefront homepage.],
      [-], [Catalog service is operational.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Customer clicks a Category (Taxonomy).],
      [2], [System retrieves products in category.],
      [3], [Customer applies Filters (Price, Brand).],
      [4], [System refreshes product grid.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Products displayed in grid.],
    ),

    [Alternative Sequences],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [A1], [If no products in category: Show empty state.],
    ),

    [Related Use Cases], [UC-0003 (Keyword Search)],
  ),
  caption: [UC-0001: Browse Products],
)

This use case defines the primary discovery capability of the storefront, allowing customers to navigate through a structured taxonomy of categories and apply dynamic filters. It serves as the entry point for product exploration, enabling users to drill down from broad categories (e.g., "Men", "Women") to specific product lists using facets like price range, brand, and size. The system updates the grid view asynchronously to provide a responsive user experience.
