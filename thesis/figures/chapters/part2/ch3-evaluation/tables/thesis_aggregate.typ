// Manually updated from authoritative benchmark JSON (thesis_results_category_only.json)
// Matches Appendix A.1 — category-only binary relevance ground truth.
// Updated: 2026-08-19

#figure(
  caption: [Thesis Benchmark — Aggregate Retrieval Metrics (3-Fold CV)],
  table(
    columns: 8,
    align: (left,) + (center,) * 7,
    table.header(
      [*Model*], [*mAP (mean ± SD)*], [*P@5*], [*P@10*], [*P@20*], [*R@5*], [*R@10*], [*R@20*],
    ),
    [FashionCLIP], [*0.9309 ± 0.0068*], [*0.9582*], [*0.9493*], [*0.9374*], [*0.0280*], [*0.0483*], [*0.0810*],
    [CLIP-generic], [0.9115 ± 0.0077], [0.9440], [0.9364], [0.9239], [0.0264], [0.0459], [0.0768],
    [EfficientNet-B0], [0.8895 ± 0.0056], [0.9340], [0.9229], [0.9077], [0.0249], [0.0426], [0.0720],
    [ResNet-50], [0.8857 ± 0.0114], [0.9327], [0.9203], [0.9035], [0.0274], [0.0470], [0.0799],
  )
) <tab:thesis-aggregate>
