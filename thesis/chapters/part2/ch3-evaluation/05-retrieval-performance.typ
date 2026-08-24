== Retrieval Performance and Accuracy

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_map.png", width: 75%),
  caption: [mAP comparison across six evaluated models. Fashion-CLIP leads at 0.9336, followed by DINOv2 ViT-S/14 (0.9299), CLIP ViT-B/16 (0.9202), CLIP ViT-B/32 (0.9184), ResNet-50 (0.9132), and EfficientNet-B0 (0.9077).],
) <fig-benchmark-map>

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_precision.png", width: 75%),
  caption: [Precision at K (K = 5, 10, 20) across six evaluated models. Fashion-CLIP maintains the highest precision at every retrieval depth.],
) <fig-benchmark-precision>

#figure(
  caption: [Aggregate Retrieval Metrics, 3-Fold Cross-Validation (Category-Only Ground Truth)],
  table(
    columns: (auto,) + (1fr,) * 7,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP (mean ± SD)*], [*P\@5*], [*P\@10*], [*P\@20*], [*R\@5*], [*R\@10*], [*R\@20*],
    ),
    [FashionCLIP], [*0.9336 ± 0.0060*], [*0.9607*], [*0.9527*], [*0.9383*], [*0.0282*], [*0.0488*], [*0.0816*],
    [DINOv2 ViT-S/14], [0.9299 ± 0.0058], [0.9572], [0.9491], [0.9360], [0.0275], [0.0484], [0.0813],
    [CLIP ViT-B/16], [0.9202 ± 0.0043], [0.9515], [0.9423], [0.9297], [0.0275], [0.0474], [0.0790],
    [CLIP ViT-B/32], [0.9184 ± 0.0060], [0.9482], [0.9408], [0.9282], [0.0272], [0.0474], [0.0785],
    [ResNet-50], [0.9132 ± 0.0057], [0.9457], [0.9364], [0.9246], [0.0262], [0.0452], [0.0766],
    [EfficientNet-B0], [0.9077 ± 0.0076], [0.9439], [0.9342], [0.9205], [0.0257], [0.0443], [0.0748],
  ),
  kind: table,
) <tbl-aggregate>

Fashion-CLIP achieved the highest retrieval accuracy across every metric, but the six-model field is tightly clustered: the full spread from Fashion-CLIP (0.9336) to EfficientNet-B0 (0.9077) is only 2.86%. Fashion-CLIP's lead over the nearest competitor, DINOv2 ViT-S/14 (0.9299), is just 0.40%, and its advantage over the generic CLIP ViT-B/16 (0.9202) is 1.46%. The advantage holds at all K values: P\@5 (0.9607 vs 0.9515 for CLIP ViT-B/16), P\@10 (0.9527 vs 0.9423), and P\@20 (0.9383 vs 0.9297). Fashion-CLIP's standard deviation (±0.0060) is comparable to the other transformer models, indicating both highest average quality and competitive cross-fold consistency.

The three transformer-based models (Fashion-CLIP, DINOv2 ViT-S/14, and the two CLIP ViT-B variants) form the top tier, separated from the two CNN models (ResNet-50, EfficientNet-B0) by roughly 0.5--1.5 percentage points in mAP. ResNet-50's higher embedding dimensionality (2,048 vs 1,280 for EfficientNet-B0, 512 for the CLIP family) does not improve category-level retrieval, consistent with higher dimensionality benefiting finer-grained distinctions rather than coarse category classification.

With only 3 folds, formal significance testing has limited power. Using the non-overlapping-bounds heuristic, Fashion-CLIP's lower mAP bound (mean minus two standard deviations: 0.9216) exceeds the upper bounds of the two CNN models (ResNet-50 0.9246, EfficientNet-B0 0.9229) only marginally, and overlaps the upper bounds of DINOv2 (0.9415) and the CLIP ViT-B variants. Four of the six models therefore form a statistically indistinguishable cluster on category-only retrieval; the two CNNs sit at its lower edge.

=== Ground-Truth Sensitivity

The category-only scheme above is the broadest relevance definition and yields the highest absolute scores. To validate that the model ranking is not an artefact of this single label scheme, all six models were re-evaluated under two progressively stricter ground-truth definitions: *category + colour* (requiring master category and base colour agreement) and *category + colour + pattern* (additionally requiring pattern-attribute agreement). The three configurations are summarised below and reported in full in Appendix A.

#figure(
  caption: [mAP Under Three Ground-Truth Definitions (6 Models, 3-Fold CV)],
  table(
    columns: (auto,) + (1fr,) * 3,
    align: (left,) + (center,) * 3,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*Category-Only*], [*Category + Colour*], [*Cat. + Colour + Pattern*],
    ),
    [FashionCLIP], [0.9336], [0.2439], [0.2071],
    [DINOv2 ViT-S/14], [0.9299], [0.1899], [0.1651],
    [CLIP ViT-B/16], [0.9202], [0.2253], [0.1861],
    [CLIP ViT-B/32], [0.9184], [0.2250], [0.1859],
    [ResNet-50], [0.9132], [0.2028], [0.1629],
    [EfficientNet-B0], [0.9077], [0.2248], [0.1842],
  ),
  kind: table,
) <tbl-groundtruth>

Two patterns emerge. First, absolute mAP degrades monotonically as the relevance criterion tightens (category-only ≈ 0.91--0.93, category + colour ≈ 0.19--0.25, category + colour + pattern ≈ 0.16--0.21), because finer-grained relevance admits fewer correct matches per query. Second, and more importantly, the model ranking is *not* invariant: under the coarse category-only scheme DINOv2 ViT-S/14 is the runner-up to Fashion-CLIP, but under category + colour it collapses to last place (0.1899), while the CLIP family (trained with textual colour and pattern tokens) and EfficientNet-B0 remain comparatively robust. This indicates that DINOv2 captures category-level semantics strongly yet encodes fashion-specific fine attributes (colour, pattern) less precisely than the CLIP-family models. Fashion-CLIP, which combines fashion-domain fine-tuning with the CLIP architecture, retains the lead under every scheme, indicating that its advantage is not an artefact of the label definition.

*Answer to RQ1.* Fashion-CLIP achieved the highest mean mAP across every accuracy metric and under all three ground-truth definitions, although its advantage over the nearest competitor, DINOv2 ViT-S/14, is small (0.40%) and, with only three folds, within measurement uncertainty. The 1.46% mAP advantage over the generic CLIP ViT-B/16 shows that domain-specific fine-tuning gives measurable retrieval quality improvements beyond general-purpose contrastive pre-training. However, the six-model comparison reveals a tightly packed top tier: DINOv2 ViT-S/14 (self-supervised) closes to within 0.40% of Fashion-CLIP on category-only retrieval, and the CLIP family proves markedly more robust than DINOv2 as the relevance criterion tightens to colour and pattern. The recommendation is therefore not absolute but task-dependent: Fashion-CLIP leads on visual similarity, while DINOv2 is a strong, lighter-weight alternative for coarse category retrieval.
