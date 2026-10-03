== Retrieval Performance and Accuracy

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_map.png", width: 75%),
  caption: [mAP comparison across six benchmarked models, with Fashion-CLIP leading at 0.9336.],
) <fig-benchmark-map>

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.5_benchmark_precision.png", width: 75%),
  caption: [Precision at K = 5, 10, and 20, with Fashion-CLIP highest across all six models.],
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
    [Fashion-CLIP], [*0.9336 ± 0.0060*], [*0.9607*], [*0.9527*], [*0.9383*], [*0.0282*], [*0.0488*], [*0.0816*],
    [DINOv2 ViT-S/14], [0.9299 ± 0.0058], [0.9572], [0.9491], [0.9360], [0.0275], [0.0484], [0.0813],
    [CLIP ViT-B/16], [0.9202 ± 0.0043], [0.9515], [0.9423], [0.9297], [0.0275], [0.0474], [0.0790],
    [CLIP ViT-B/32], [0.9184 ± 0.0060], [0.9482], [0.9408], [0.9282], [0.0272], [0.0474], [0.0785],
    [ResNet-50], [0.9132 ± 0.0057], [0.9457], [0.9364], [0.9246], [0.0262], [0.0452], [0.0766],
    [EfficientNet-B0], [0.9077 ± 0.0076], [0.9439], [0.9342], [0.9205], [0.0257], [0.0443], [0.0748],
  ),
  kind: table,
) <tbl-aggregate>

#footnote[† Recall values appear low because the category-only ground truth defines hundreds to thousands of relevant items per query (Appendix B); even a perfect model would show R\@20 < 0.10 under this scheme.]

@tbl-aggregate reports the full aggregate metrics.

*F3.6.1* Fashion-CLIP achieved the highest retrieval accuracy across every metric (mAP 0.9336), but the six-model field is tightly clustered: the full spread from Fashion-CLIP to EfficientNet-B0 (0.9077) is only 2.86%. Fashion-CLIP's lead over the nearest competitor, DINOv2 ViT-S/14 (0.9299), is just 0.40%, and its advantage over the generic CLIP ViT-B/16 (0.9202) is 1.46%.

*F3.6.2* The three transformer-based models (Fashion-CLIP, DINOv2 ViT-S/14, CLIP ViT-B/16, CLIP ViT-B/32) form the top tier, separated from the two CNN models (ResNet-50, EfficientNet-B0) by roughly 0.5--1.5 percentage points in mAP.

With only 3 folds, formal significance testing has limited power. As a descriptive visual check (not a formal significance test), the non-overlapping-bounds heuristic (mean ± two standard deviations) shows that Fashion-CLIP's lower mAP bound (0.9216) overlaps the upper bounds of every other model: DINOv2 (0.9415), CLIP ViT-B/16 (0.9288), CLIP ViT-B/32 (0.9304), ResNet-50 (0.9246), and EfficientNet-B0 (0.9229). No model can therefore be separated from any other at this heuristic level.

#figure(
  caption: [Key Caveats for Retrieval Performance],
  table(
    columns: (auto, 1fr, 1fr),
    align: (left, left, left),
    stroke: 0.5pt,
    table.header([*ID*], [*Caveat*], [*Why it matters*]),
    [C1], [Top-2 model gap (0.40%) within measurement uncertainty at 3-fold CV], [Do not claim the top model is definitively best],
    [C2], [Sporting Goods stratum thin (150 images, about 50 per test fold)], [mAP unstable at finer label granularity],
    [C3], [Personal Care stratum thin (350 images total)], [Same risk as C2 at category+colour+pattern scheme],
  ),
  kind: table,
) <tbl-retrieval-caveats>

=== Ground-Truth Sensitivity

The category-only scheme above is the broadest relevance definition and yields the highest absolute scores. To validate that the model ranking is not an artefact of this single label scheme, all six models were re-measured under two progressively stricter ground-truth definitions: *category + colour* (requiring master category and base colour agreement) and *category + colour + pattern* (additionally requiring pattern-attribute agreement). The three configurations are summarised below and reported in full in Appendix A.

#figure(
  caption: [mAP Under Three Ground-Truth Definitions (6 Models, 3-Fold CV)],
  table(
    columns: (auto,) + (1fr,) * 3,
    align: (left,) + (center,) * 3,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*Category-Only*], [*Category + Colour*], [*Cat. + Colour + Pattern*],
    ),
    [Fashion-CLIP], [0.9336], [0.2439], [0.2071],
    [DINOv2 ViT-S/14], [0.9299], [0.1899], [0.1651],
    [CLIP ViT-B/16], [0.9202], [0.2253], [0.1861],
    [CLIP ViT-B/32], [0.9184], [0.2250], [0.1859],
    [ResNet-50], [0.9132], [0.2028], [0.1629],
    [EfficientNet-B0], [0.9077], [0.2248], [0.1842],
  ),
  kind: table,
) <tbl-groundtruth>

Two patterns emerge. First, absolute mAP degrades monotonically as the relevance criterion tightens (category-only ≈ 0.91--0.93, category + colour ≈ 0.19--0.25, category + colour + pattern ≈ 0.16--0.21), because finer-grained relevance admits fewer correct matches per query. The model ranking is *not* invariant: under the coarse category-only scheme DINOv2 ViT-S/14 is the runner-up to Fashion-CLIP, but under category + colour it collapses to last place (0.1899), while the CLIP family (trained with textual colour and pattern tokens) and EfficientNet-B0 remain comparatively stable. This indicates that DINOv2 captures category-level semantics strongly yet encodes fashion-specific fine attributes (colour, pattern) less precisely than the CLIP-family models. Fashion-CLIP, which combines fashion-domain fine-tuning with the CLIP architecture, retains the lead under every scheme, indicating that its advantage is not an artefact of the label definition. Per-stratum reliability degrades for Sporting Goods and Personal Care, where the combination of small class size and fine-grained relevance labels produces very few relevant matches per query.

*Answer to RQ1.* Fashion-CLIP achieved the highest mean mAP across every accuracy metric and under all three ground-truth definitions, although its advantage over the nearest competitor, DINOv2 ViT-S/14, is small (0.40%) and, with only three folds, within measurement uncertainty. The 1.46% mAP advantage over the generic CLIP ViT-B/16 shows that domain-specific fine-tuning gives measurable retrieval quality improvements beyond general-purpose contrastive pre-training. However, the six-model comparison reveals a tightly packed top tier: DINOv2 ViT-S/14 (self-supervised) closes to within 0.40% of Fashion-CLIP on category-only retrieval, and the CLIP family appears markedly more stable than DINOv2 as the relevance criterion tightens to colour and pattern. The recommendation depends on the task: Fashion-CLIP leads on visual similarity, while DINOv2 is a strong, lighter-weight alternative for coarse category retrieval.
