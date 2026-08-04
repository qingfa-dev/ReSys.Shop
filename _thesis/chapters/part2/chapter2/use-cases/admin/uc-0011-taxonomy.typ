#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/admin/uc-0011-taxonomy.png", width: 100%),
  caption: [Use Case Diagram for UC-0011],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0011*], [*Taxonomy Management*],
    [Actor], [Administrator],
    [Description], [Organize categories hierarchy for product navigation.],
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
      [1], [Admin clicks "New Category".],
      [2], [Enters Name and Slug.],
      [3], [Selects Parent Category (optional).],
      [4], [System saves Category.],
      [5], [Admin assigns Products to Category (UC-0009).],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Category hierarchy updated.],
      [-], [Navigation menu reflects changes.],
    ),

    [Related Use Cases], [UC-0009 (Product Management)],
  ),
  caption: [UC-0011: Taxonomy Management],
)

Taxonomy Management allows the administrator to define the hierarchical structure of the catalog. By creating parent and child categories, the admin controls the "Browse" menu structure on the storefront. This organization is essential for efficient user navigation and ensures that products are grouped logically for both display and filtering purposes.
