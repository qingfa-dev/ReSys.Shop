// Manually updated from authoritative benchmark JSON (outputs/thesis_catonly/results/thesis_results.json)
// Matches Appendix A.4 - efficiency metrics.
// Updated: 2026-08-22

#figure(
  caption: [Thesis Benchmark: Efficiency Metrics (3-Fold CV)],
  table(
    columns: 6,
    align: (left,) + (center,) * 5,
    table.header(
      [*Model*], [*Latency (ms)*], [*Throughput (img/s)*], [*Load (ms)*], [*Storage (MB)*], [*RAM (MB)*],
    ),
    [Fashion-CLIP], [113.6 ± 3.7], [14.2 ± 0.5], [5109.9], [3.3], [N/A],
    [EfficientNet-B0], [42.6 ± 5.6], [21.4 ± 1.0], [118.3], [8.1], [N/A],
    [ResNet-50], [96.6 ± 7.4], [10.2 ± 0.0], [385.6], [13.0], [N/A],
    [DINOv2 ViT-S/14], [126.3 ± 5.1], [10.2 ± 0.2], [1223.4], [2.4], [N/A],
    [CLIP ViT-B/16], [235.5 ± 7.3], [4.0 ± 0.1], [6518.5], [3.3], [N/A],
    [CLIP ViT-B/32], [140.5 ± 7.8], [11.9 ± 0.2], [1868.8], [3.3], [N/A],
  )
) <tab:thesis-efficiency>
