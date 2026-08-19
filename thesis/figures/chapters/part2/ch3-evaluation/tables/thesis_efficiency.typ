// Manually updated from authoritative benchmark JSON (thesis_results.json)
// Matches Appendix A.4 — efficiency metrics.
// Updated: 2026-08-19

#figure(
  caption: [Thesis Benchmark — Efficiency Metrics (3-Fold CV)],
  table(
    columns: 6,
    align: (left,) + (center,) * 5,
    table.header(
      [*Model*], [*Latency (ms)*], [*Throughput (img/s)*], [*Load (ms)*], [*Storage (MB)*], [*RAM (MB)*],
    ),
    [FashionCLIP], [96.8 ± 6.8], [18.5 ± 1.3], [5255.4], [3.3], [---],
    [EfficientNet-B0], [37.8 ± 26.6], [30.2 ± 13.5], [110.2], [8.1], [---],
    [ResNet-50], [61.9 ± 5.8], [13.5 ± 0.7], [374.1], [13.0], [---],
    [CLIP-generic], [86.6 ± 8.4], [21.4 ± 0.3], [6848.5], [3.3], [---],
  )
) <tab:thesis-efficiency>
