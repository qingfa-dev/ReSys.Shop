=== Speed and Performance Results

Besides accuracy, it was important to measure how fast the system responds. Slow search results would frustrate users, even if they are accurate.

==== Test Setup

All speed tests were run on a standard development laptop:

- *CPU:* Intel Core i5-11400H (6 cores)
- *RAM:* 16 GB
- *GPU:* NVIDIA RTX 3050 Laptop (4GB)
- *Database:* PostgreSQL 16 with pgvector (running in Docker)
- *OS:* Windows 11 with WSL2

Note: The AI models were run on CPU rather than GPU for most tests. This simulates what a budget deployment might look like (GPU servers are expensive). GPU acceleration would make things faster but costs more.

==== Speed Comparison

#figure(
  table(
    columns: (auto, auto, auto, auto),
    stroke: 0.5pt,
    align: center,
    [*Model*], [*Avg Time*], [*95th Percentile*], [*Searches/sec*],
    [EfficientNet-B0], [46ms], [78ms], [21.8],
    [DINOv2], [106ms], [131ms], [9.4],
    [Fashion-CLIP], [114ms], [141ms], [8.8],
  ),
  caption: [Speed comparison between models],
  kind: table,
) <tab-efficiency>

==== Performance Interpretation and Latency Analysis

- *EfficientNet is fastest* at about 46ms per image. This is expected because CNNs are generally more efficient than Transformers.

- *DINOv2 and Fashion-CLIP are similar* at around 100-115ms. Both are Vision Transformers, which require more computation.

- *All models are fast enough* for a reasonable user experience. Even Fashion-CLIP at 114ms is well under one second.

The trade-off is clear: EfficientNet is about 2.5x faster than Fashion-CLIP, but Fashion-CLIP is about 10% more accurate. For this project, the accuracy improvement was considered worth the extra processing time.

In a production system with high traffic, caching strategies or GPU acceleration could help reduce latency further. These optimizations were not implemented in this prototype but would be important for a real deployment.

