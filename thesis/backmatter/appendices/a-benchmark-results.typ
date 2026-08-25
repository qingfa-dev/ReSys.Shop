= Full Benchmark Results <appendix-a>

This appendix presents the complete retrieval accuracy and operational efficiency results from the 3-fold cross-validation benchmark described in Chapter 3. All six benchmarked models are reported under three ground-truth label schemes.

== Category-Only Ground Truth (5 Categories)

Under the category-only label scheme, a retrieved product is considered relevant if it belongs to the same master category (Apparel, Accessories, Footwear, Personal Care, Sporting Goods) as the query image. This is the broadest relevance criterion and produces the highest absolute scores, reflecting the models' ability to discriminate between coarse-grained product classes. It is the headline scheme used for the Chapter 3 primary results.

#figure(
  caption: [Retrieval Accuracy, Category-Only Ground Truth (3-Fold CV, Mean ± SD)],
  table(
    columns: (auto,) + (1fr,) * 7,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP*], [*P\@5*], [*P\@10*], [*P\@20*], [*R\@5*], [*R\@10*], [*R\@20*],
    ),
    [FashionCLIP], [0.9336 ± 0.0060], [0.9607 ± 0.0041], [0.9527 ± 0.0043], [0.9383 ± 0.0036], [0.0282 ± 0.0014], [0.0488 ± 0.0021], [0.0816 ± 0.0031],
    [DINOv2 ViT-S/14], [0.9299 ± 0.0058], [0.9572 ± 0.0029], [0.9491 ± 0.0026], [0.9360 ± 0.0028], [0.0275 ± 0.0021], [0.0484 ± 0.0030], [0.0813 ± 0.0032],
    [CLIP ViT-B/16], [0.9202 ± 0.0043], [0.9515 ± 0.0029], [0.9423 ± 0.0023], [0.9297 ± 0.0021], [0.0275 ± 0.0009], [0.0474 ± 0.0018], [0.0790 ± 0.0025],
    [CLIP ViT-B/32], [0.9184 ± 0.0060], [0.9482 ± 0.0028], [0.9408 ± 0.0041], [0.9282 ± 0.0037], [0.0272 ± 0.0011], [0.0474 ± 0.0023], [0.0785 ± 0.0019],
    [ResNet-50], [0.9132 ± 0.0057], [0.9457 ± 0.0024], [0.9364 ± 0.0020], [0.9246 ± 0.0021], [0.0262 ± 0.0020], [0.0452 ± 0.0025], [0.0766 ± 0.0023],
    [EfficientNet-B0], [0.9077 ± 0.0076], [0.9439 ± 0.0041], [0.9342 ± 0.0046], [0.9205 ± 0.0042], [0.0257 ± 0.0016], [0.0443 ± 0.0019], [0.0748 ± 0.0022],
  ),
  kind: table,
)

Under this broadest criterion, all six models achieve high precision (above 0.90 at P\@10). Fashion-CLIP leads with mAP of 0.9336, followed by DINOv2 ViT-S/14 (0.9299) and the two CLIP ViT-B variants (0.9184--0.9202). The low recall values (R\@20 of 0.07--0.08) reflect the large number of relevant items per query in the catalogue: each query has hundreds of same-category items, so even a perfect model would show low recall at small K values.

== Category + Colour Ground Truth

Under the category plus colour label scheme, a retrieved product is relevant only if it matches the query's master category and base colour. This finer-grained scheme better isolates the models' ability to capture visual similarity, as opposed to merely coarse category classification.

#figure(
  caption: [Retrieval Accuracy, Category + Colour Ground Truth (3-Fold CV, Mean ± SD)],
  table(
    columns: (auto,) + (1fr,) * 7,
    align: (left,) + (center,) * 7,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*mAP*], [*P\@5*], [*P\@10*], [*P\@20*], [*R\@5*], [*R\@10*], [*R\@20*],
    ),
    [FashionCLIP], [0.2439 ± 0.0074], [0.4414 ± 0.0106], [0.4041 ± 0.0100], [0.3619 ± 0.0082], [0.0430 ± 0.0017], [0.0717 ± 0.0031], [0.1130 ± 0.0027],
    [CLIP ViT-B/16], [0.2253 ± 0.0046], [0.4068 ± 0.0069], [0.3763 ± 0.0063], [0.3411 ± 0.0061], [0.0392 ± 0.0012], [0.0669 ± 0.0006], [0.1089 ± 0.0018],
    [CLIP ViT-B/32], [0.2250 ± 0.0034], [0.4099 ± 0.0057], [0.3761 ± 0.0049], [0.3414 ± 0.0046], [0.0390 ± 0.0033], [0.0666 ± 0.0019], [0.1066 ± 0.0026],
    [EfficientNet-B0], [0.2248 ± 0.0018], [0.4050 ± 0.0042], [0.3767 ± 0.0021], [0.3435 ± 0.0025], [0.0350 ± 0.0014], [0.0595 ± 0.0019], [0.0976 ± 0.0019],
    [ResNet-50], [0.2028 ± 0.0065], [0.3813 ± 0.0105], [0.3516 ± 0.0069], [0.3208 ± 0.0059], [0.0350 ± 0.0017], [0.0582 ± 0.0030], [0.0955 ± 0.0029],
    [DINOv2 ViT-S/14], [0.1899 ± 0.0034], [0.3478 ± 0.0059], [0.3305 ± 0.0045], [0.3057 ± 0.0041], [0.0336 ± 0.0025], [0.0586 ± 0.0031], [0.0975 ± 0.0044],
  ),
  kind: table,
)

Under this stricter label scheme, absolute mAP drops to the 0.19-0.25 range. Fashion-CLIP maintains the clear lead (0.2439); the CLIP ViT-B/16 and CLIP ViT-B/32 variants cluster at 0.2253 and 0.2250, with EfficientNet-B0 close behind at 0.2248. Notably, DINOv2 ViT-S/14 collapses to 0.1899, the lowest of all six models, indicating that its self-supervised representation, while strong at category level, encodes fashion-specific colour attributes less precisely than the CLIP-family models. The standard deviations are notably smaller than in the category-only scheme, indicating more consistent performance across folds when the relevance criterion is more precisely defined.

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
    [FashionCLIP], [0.2071 ± 0.0074], [0.3762 ± 0.0097], [0.3377 ± 0.0090], [0.2952 ± 0.0065], [0.0628 ± 0.0024], [0.1032 ± 0.0044], [0.1586 ± 0.0042],
    [CLIP ViT-B/16], [0.1861 ± 0.0057], [0.3350 ± 0.0069], [0.3025 ± 0.0058], [0.2668 ± 0.0052], [0.0562 ± 0.0024], [0.0923 ± 0.0015], [0.1460 ± 0.0037],
    [CLIP ViT-B/32], [0.1859 ± 0.0052], [0.3399 ± 0.0061], [0.3053 ± 0.0052], [0.2685 ± 0.0052], [0.0564 ± 0.0037], [0.0928 ± 0.0026], [0.1437 ± 0.0025],
    [EfficientNet-B0], [0.1842 ± 0.0022], [0.3369 ± 0.0027], [0.3071 ± 0.0040], [0.2748 ± 0.0038], [0.0519 ± 0.0010], [0.0861 ± 0.0027], [0.1370 ± 0.0032],
    [DINOv2 ViT-S/14], [0.1651 ± 0.0040], [0.2984 ± 0.0045], [0.2771 ± 0.0041], [0.2511 ± 0.0037], [0.0478 ± 0.0020], [0.0810 ± 0.0032], [0.1320 ± 0.0045],
    [ResNet-50], [0.1629 ± 0.0065], [0.3105 ± 0.0112], [0.2805 ± 0.0074], [0.2517 ± 0.0063], [0.0512 ± 0.0025], [0.0834 ± 0.0033], [0.1335 ± 0.0046],
  ),
  kind: table,
)

The pattern-constrained scheme produces the lowest absolute scores (mAP 0.16--0.21), which is expected given the narrowest definition of relevance. Fashion-CLIP again leads (0.2071), followed by the CLIP ViT-B/16 (0.1861) and CLIP ViT-B/32 (0.1859) variants and EfficientNet-B0 (0.1842); DINOv2 ViT-S/14 (0.1651) and ResNet-50 (0.1629) occupy the lower tier. The model ranking is therefore highly dependent on the relevance definition: DINOv2's strong category-level showing does not extend to fine-grained colour and pattern attributes, whereas the CLIP family remains consistently robust.

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
    [EfficientNet-B0], [42.6 ± 5.6], [21.4 ± 1.0], [118.3], [8.1], [~100], [1280],
    [ResNet-50], [96.6 ± 7.4], [10.2 ± 0.0], [385.6], [13.0], [~150], [2048],
    [DINOv2 ViT-S/14], [126.3 ± 5.1], [10.2 ± 0.2], [1 223.4], [2.4], [~250], [384],
    [CLIP ViT-B/16], [235.5 ± 7.3], [4.0 ± 0.1], [6 518.5], [3.3], [~600], [512],
    [CLIP ViT-B/32], [140.5 ± 7.8], [11.9 ± 0.2], [1 868.8], [3.3], [~600], [512],
    [FashionCLIP], [113.6 ± 3.7], [14.2 ± 0.5], [5 109.9], [3.3], [~600], [512],
  ),
  kind: table,
)

EfficientNet-B0 achieves the lowest inference latency (42.6 ms) and highest throughput (21.4 images per second), making it the most computationally efficient model. CLIP-based models (FashionCLIP, CLIP ViT-B/16, CLIP ViT-B/32) and DINOv2 incur the highest model load times (1.2--6.5 seconds) due to their transformer architectures and larger weight files. ResNet-50 requires the most storage (13.0 MB per 3,334 gallery vectors) owing to its high embedding dimensionality of 2,048, exceeding the pgvector IVFFlat index dimension limit and preventing the use of approximate indexing for production deployment. RAM values are estimates derived from each model's parameter count plus PyTorch runtime overhead, because direct process-level measurement via psutil proved unreliable on the Linux host used for these experiments. These figures are indicative rather than instrumented; actual values vary with batch size and runtime overhead, scaling from approximately 100 MB (EfficientNet-B0) to over 600 MB (CLIP-based).

== Per-Fold Variability

The per-fold results for the primary label scheme (category only) are presented below to provide transparency on the cross-validation procedure and to permit independent verification of aggregate statistics.

#figure(
  caption: [Per-Fold Breakdown, Category-Only Ground Truth],
  table(
    columns: (auto,) + (1fr,) * 4,
    align: (left,) + (center,) * 4,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*Fold 0 mAP*], [*Fold 1 mAP*], [*Fold 2 mAP*], [*Mean ± SD*],
    ),
    [FashionCLIP], [0.9274], [0.9340], [0.9394], [0.9336 ± 0.0060],
    [DINOv2 ViT-S/14], [0.9254], [0.9278], [0.9365], [0.9299 ± 0.0058],
    [CLIP ViT-B/16], [0.9180], [0.9174], [0.9251], [0.9202 ± 0.0043],
    [CLIP ViT-B/32], [0.9131], [0.9171], [0.9249], [0.9184 ± 0.0060],
    [ResNet-50], [0.9099], [0.9099], [0.9198], [0.9132 ± 0.0057],
    [EfficientNet-B0], [0.9031], [0.9036], [0.9165], [0.9077 ± 0.0076],
  ),
  kind: table,
)

All six models exhibit low fold-to-fold variability (standard deviation 0.0043--0.0076); the 3-fold stratified split therefore preserves the dataset's category distribution effectively, and model performance stays stable across different partitionings of the data.
