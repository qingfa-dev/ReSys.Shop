#figure(
  placement: none,
  image("../../../../../images/diagrams/usecases/system/uc-0016-embeddings.png", width: 100%),
  caption: [Use Case Diagram for UC-0016],
)

#figure(
  placement: none,
  table(
    columns: (1fr, 3fr),
    align: left,
    stroke: 0.5pt,
    [*UC-0016*], [*Generate AI Search Vectors*],
    [Actor], [System (Background Job)],
    [Description], [Autonomous processing of product images to generate 512-dimensional embeddings for visual search.],
    [Trigger], [Image Upload (Status: Pending) (UC-0009).],
    [Ordinary Sequence],
    table(
      columns: (auto, 1fr),
      stroke: none,
      [1], [Background Job detects Pending Image.],
      [2], [Job sends image URL to ML Service Endpoint.],
      [3], [ML Service computes normalized vector (Fashion-CLIP).],
      [4], [Job saves vector to Embedding Store.],
      [5], [Job marks Image Status as "Processed".],
    ),

    [Related Use Cases], [UC-0001 (Visual Search), UC-0004 (Recommendations)],
  ),
  caption: [UC-0016: Generate AI Search Vectors],
)

This background system process is responsible for the asynchronous generation of vector embeddings from product images. Triggered whenever a new image is uploaded (UC-0010), it invokes the Python ML Service to compute a 512-dimensional vector using the Fashion-CLIP model. This pre-computed vector is essential for powering high-performance visual search and recommendation features.
