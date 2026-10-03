#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/customer/uc-0004-visual-search.png", width: 100%),
  caption: [Use Case Diagram for UC-0004],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0004*], [*Visual Search*],
    [Actor], [Customer],
    [Description], [Upload an image to find visually similar products using AI embeddings.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [ML Service is online.],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Customer uploads image.],
      [2], [System generates embedding (UC-0017).],
      [3], [System queries vector database.],
      [4], [Displays similar items.],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Similar products displayed.],
    ),

    [Related Use Cases], [UC-0003 (Keyword Search)],
  ),
  caption: [UC-0004: Visual Search],
)

Visual Search represents a core AI capability of the platform, enabling users to upload an image to find visually similar products. Upon upload, the system generates a vector embedding of the image and utilizes the PostgreSQL `pgvector` extension to perform a k-Nearest Neighbors (k-NN) search against the product catalog. This bypasses textual limitations, matching items based on style, color, and pattern.
