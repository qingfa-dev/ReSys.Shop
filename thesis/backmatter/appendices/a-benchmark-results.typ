= Full Benchmark Results <appendix-a>

This appendix presents the complete retrieval accuracy and operational efficiency results from the 3-fold cross-validation benchmark described in Chapter 3.

== Category-Only Ground Truth (5 Categories)

Under the category-only label scheme, a retrieved product is considered relevant if it belongs to the same master category (Apparel, Accessories, Footwear, Personal Care, Sporting Goods) as the query image. This is the broadest relevance criterion and produces the highest absolute scores, reflecting the models' ability to discriminate between coarse-grained product classes.

#figure(
  caption: [Retrieval Accuracy, Category-Only Ground Truth (3-Fold CV, Mean ± SD)],
  table(
    columns: (auto,) + (1fr,) * 7,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP*], [*P\@5*], [*P\@10*], [*P\@20*], [*R\@5*], [*R\@10*], [*R\@20*],
    ),
    [FashionCLIP], [0.9309 ± 0.0068], [0.9582 ± 0.0055], [0.9493 ± 0.0045], [0.9374 ± 0.0036], [0.0280 ± 0.0027], [0.0483 ± 0.0031], [0.0810 ± 0.0033],
    [CLIP-generic], [0.9115 ± 0.0077], [0.9440 ± 0.0060], [0.9364 ± 0.0046], [0.9239 ± 0.0036], [0.0264 ± 0.0025], [0.0459 ± 0.0027], [0.0768 ± 0.0027],
    [EfficientNet-B0], [0.8895 ± 0.0056], [0.9340 ± 0.0053], [0.9229 ± 0.0032], [0.9077 ± 0.0013], [0.0249 ± 0.0020], [0.0426 ± 0.0014], [0.0720 ± 0.0018],
    [ResNet-50], [0.8857 ± 0.0114], [0.9327 ± 0.0031], [0.9203 ± 0.0075], [0.9035 ± 0.0110], [0.0274 ± 0.0067], [0.0470 ± 0.0101], [0.0799 ± 0.0174],
  ),
  kind: table,
)

Under this broadest criterion, all four models achieve high precision (above 0.90 at P\@10). Fashion-CLIP leads with mAP of 0.9309, followed by CLIP-generic (0.9115). The low recall values (R\@20 of 0.07--0.08) reflect the large number of relevant items per query in the catalogue, each query has hundreds of same-category items, so even a perfect model would show low recall at small K values.

== Category + Colour Ground Truth

Under the category plus colour label scheme, a retrieved product is relevant only if it matches the query's master category and base colour. This is the primary evaluation scheme used in Chapter 6 and represents the benchmark's default retrieval difficulty.

#figure(
  caption: [Retrieval Accuracy, Category + Colour Ground Truth (3-Fold CV, Mean ± SD)],
  table(
    columns: (auto,) + (1fr,) * 7,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP*], [*P\@5*], [*P\@10*], [*P\@20*], [*R\@5*], [*R\@10*], [*R\@20*],
    ),
    [FashionCLIP], [0.2454 ± 0.0039], [0.4288 ± 0.0034], [0.3904 ± 0.0018], [0.3510 ± 0.0023], [0.0645 ± 0.0045], [0.1036 ± 0.0062], [0.1667 ± 0.0053],
    [CLIP-generic], [0.2308 ± 0.0059], [0.4118 ± 0.0045], [0.3744 ± 0.0035], [0.3330 ± 0.0031], [0.0571 ± 0.0053], [0.0952 ± 0.0066], [0.1523 ± 0.0083],
    [EfficientNet-B0], [0.2203 ± 0.0058], [0.3933 ± 0.0059], [0.3630 ± 0.0033], [0.3273 ± 0.0040], [0.0520 ± 0.0035], [0.0876 ± 0.0048], [0.1412 ± 0.0046],
    [ResNet-50], [0.2091 ± 0.0038], [0.3817 ± 0.0021], [0.3491 ± 0.0043], [0.3153 ± 0.0028], [0.0503 ± 0.0020], [0.0839 ± 0.0023], [0.1361 ± 0.0050],
  ),
  kind: table,
)

Under this stricter label scheme, absolute mAP drops to the 0.20--0.25 range. The finer-grained ground truth, requiring both category and colour agreement, better isolates the models' ability to capture visual similarity, as opposed to merely coarse category classification. Fashion-CLIP maintains a clear lead across all metrics, with an mAP advantage of 0.0146 over CLIP-generic and 0.0363 over ResNet-50. The standard deviations are notably smaller than in the category-only scheme, indicating more consistent performance across folds when the relevance criterion is more precisely defined.

== Category + Colour + Pattern Ground Truth

Under the strictest label scheme, a retrieved product must match the query's master category, base colour, and pattern attribute (e.g., Solid, Striped, Checked, Floral). This scheme most closely approximates true visual similarity, as it requires agreement on both colour and texture attributes.

#figure(
  caption: [Retrieval Accuracy, Category + Colour + Pattern Ground Truth (3-Fold CV, Mean ± SD)],
  table(
    columns: (auto,) + (1fr,) * 7,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP*], [*P\@5*], [*P\@10*], [*P\@20*], [*R\@5*], [*R\@10*], [*R\@20*],
    ),
    [FashionCLIP], [0.2146 ± 0.0076], [0.3790 ± 0.0103], [0.3417 ± 0.0095], [0.2997 ± 0.0093], [0.0616 ± 0.0023], [0.1037 ± 0.0027], [0.1608 ± 0.0050],
    [CLIP-generic], [0.2007 ± 0.0070], [0.3609 ± 0.0108], [0.3251 ± 0.0068], [0.2836 ± 0.0062], [0.0584 ± 0.0036], [0.0947 ± 0.0039], [0.1475 ± 0.0038],
    [EfficientNet-B0], [0.1923 ± 0.0040], [0.3474 ± 0.0069], [0.3150 ± 0.0064], [0.2806 ± 0.0021], [0.0530 ± 0.0027], [0.0886 ± 0.0025], [0.1398 ± 0.0023],
    [ResNet-50], [0.1859 ± 0.0073], [0.3332 ± 0.0079], [0.3002 ± 0.0054], [0.2655 ± 0.0038], [0.0583 ± 0.0134], [0.0950 ± 0.0195], [0.1482 ± 0.0276],
  ),
  kind: table,
)

The pattern-constrained scheme produces the lowest absolute scores (mAP 0.19--0.22), which is expected given the narrowest definition of relevance. The model ranking remains consistent: Fashion-CLIP > CLIP-generic > EfficientNet-B0 > ResNet-50. ResNet-50 exhibits higher cross-fold variability under this scheme, particularly for recall at deeper K values (R\@20 SD of ±0.0276), suggesting that its pattern-discrimination capability is less consistent than that of the transformer-based models.

== Operational Efficiency (3-Fold CV)

#figure(
  caption: [Operational Efficiency, All Models (3-Fold CV, Mean ± SD)],
  table(
    columns: (auto,) + (1fr,) * 6,
    align: (left,) + (center,) * 6,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*Latency (ms)*], [*Throughput*], [*Load (ms)*], [*Storage (MB)*], [*RAM (MB)*], [*Dim*],
    ),
    [EfficientNet-B0], [37.8 ± 26.6], [30.2 ± 13.5], [110.2 ± 0.0], [8.1 ± 0.0], [2.6 ± 22.3], [1280],
    [ResNet-50], [61.9 ± 5.8], [13.5 ± 0.7], [374.1 ± 0.0], [13.0 ± 0.0], [6.1 ± 10.6], [2048],
    [CLIP-generic], [86.6 ± 8.4], [21.4 ± 0.3], [6848.5 ± 0.0], [3.3 ± 0.0], [0.0 ± 0.0], [512],
    [FashionCLIP], [96.8 ± 6.8], [18.5 ± 1.3], [5255.4 ± 0.0], [3.3 ± 0.0], [0.0 ± 0.0], [512],
  ),
  kind: table,
)

EfficientNet-B0 achieves the lowest inference latency (37.8 ms) and highest throughput (30.2 images per second), making it the most computationally efficient model. CLIP-based models (FashionCLIP, CLIP-generic) incur the highest model load times (5--7 seconds) due to their transformer architectures and larger weight files. ResNet-50 requires the most storage (13.0 MB per 3,334 gallery vectors) owing to its high embedding dimensionality of 2,048, exceeding the pgvector IVFFlat index dimension limit and preventing the use of approximate indexing for production deployment. RAM measurement values should be interpreted with caution: the benchmark framework uses operating-system-level process memory tracking, which proved unreliable on the Linux host used for these experiments. Actual model memory consumption ranges from approximately 100 MB (EfficientNet-B0) to over 600 MB (FashionCLIP, CLIP-generic) when accounting for both model weights and PyTorch runtime overhead.

== Per-Fold Variability

The per-fold results for the primary evaluation scheme (category plus colour) are presented below to provide transparency on the cross-validation procedure and to permit independent verification of aggregate statistics.

#figure(
  caption: [Per-Fold Breakdown, Category + Colour Ground Truth],
  table(
    columns: (auto,) + (1fr,) * 4,
    align: (left,) + (center,) * 4,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*Fold 0 mAP*], [*Fold 1 mAP*], [*Fold 2 mAP*], [*Mean ± SD*],
    ),
    [FashionCLIP], [0.2473], [0.2480], [0.2410], [0.2454 ± 0.0039],
    [CLIP-generic], [0.2358], [0.2324], [0.2243], [0.2308 ± 0.0059],
    [EfficientNet-B0], [0.2242], [0.2230], [0.2136], [0.2203 ± 0.0058],
    [ResNet-50], [0.2084], [0.2132], [0.2056], [0.2091 ± 0.0038],
  ),
  kind: table,
)

All models exhibit low fold-to-fold variability (standard deviation 0.0039--0.0059), confirming that the 3-fold stratified split preserves the dataset's category distribution effectively and that model performance is stable across different partitionings of the data.
