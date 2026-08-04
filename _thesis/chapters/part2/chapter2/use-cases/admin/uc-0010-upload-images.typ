#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/admin/uc-0010-upload-images.png", width: 100%),
  caption: [Use Case Diagram for UC-0010],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0010*], [*Upload Product Images*],
    [Actor], [Administrator],
    [Description], [Upload product images which are automatically processed for display and AI vectorization.],
    [Preconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Admin is authenticated.],
      [-], [Product exists (UC-0009).],
    ),

    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Admin selects product and clicks "Upload Image".],
      [2], [Uses file picker to select local image.],
      [3], [System validates file constraints (< 10MB, Type).],
      [4], [System generates thumbnail and saves to storage.],
      [5], [System creates Product Image Record (Status: Pending).],
      [6], [System saves and triggers Background Vectorization (UC-0017).],
    ),

    [Postconditions],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [-], [Image appears in gallery.],
      [-], [Image queued for AI processing.],
    ),

    [Related Use Cases], [UC-0009 (Product Management), UC-0017 (Vector Gen)],
  ),
  caption: [UC-0010: Upload Product Images],
)

Uploading product images is a multi-step process that triggers both storage operations and AI processing. When an admin uploads an image, the system not only saves it to the blob store but also pipelines it to the AI service. This automatic trigger (UC-0017) generates vector embeddings for the image, enabling the visual search and recommendation features without manual intervention.
