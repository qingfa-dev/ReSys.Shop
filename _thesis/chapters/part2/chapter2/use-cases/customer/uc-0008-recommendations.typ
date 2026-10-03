#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/customer/uc-0008-recommendations.png", width: 100%),
  caption: [Use Case Diagram for UC-0008],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0008*], [*Product Recommendations*],
    [Actor], [Customer],
    [Description], [View AI-driven recommendations based on product similarity.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [User viewing a product.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [System detects current product.],
      [2], [Queries ML Service/Vector DB for similar items (UC-0017).],
      [3], [Displays "You May Also Like".],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Recommendations displayed.],
    ),

    [Related Use Cases], [UC-0001 (Browse)],
  ),
  caption: [UC-0008: Product Recommendations],
)

Leveraging the same underlying vector infrastructure as Visual Search, this use case delivers passive recommendations ("You May Also Like") on product detail pages. By analyzing vector similarity between the currently viewed item and the rest of the catalog, the system surfaces relevant alternatives or complementary products without explicit user queries, increasing cross-sell opportunities.
