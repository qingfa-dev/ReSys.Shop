#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/customer/uc-0003-keyword-search.png", width: 100%),
  caption: [Use Case Diagram for UC-0003],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0003*], [*Keyword Search*],
    [Actor], [Customer],
    [Description], [Finding products using text-based queries against product metadata.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Customer is on the store page.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Customer enters query.],
      [2], [System executes text search on Name/Description.],
      [3], [System returns paginated list.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Results displayed.],
    ),

    [Related Use Cases], [UC-0001 (Browse)],
  ),
  caption: [UC-0003: Keyword Search],
)

The Keyword Search functionality provides a direct mechanism for users to find specific items by matching query strings against product titles, descriptions, and metadata. This feature utilizes full-text search capabilities within the database, offering immediate feedback and ensuring that users with specific intent can locate products without navigating the category tree.
