== Computational Efficiency and Resource Trade-offs

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.6_benchmark_latency.png", width: 75%),
  caption: [Inference latency across the six benchmarked models.],
) <fig-benchmark-latency>

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.6_benchmark_throughput.png", width: 75%),
  caption: [Throughput across the six benchmarked models.],
) <fig-benchmark-throughput>

#figure(
  caption: [Efficiency Metrics, 3-Fold Cross-Validation],
  table(
    columns: (auto,) + (1fr,) * 5,
    align: (left,) + (center,) * 5,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*Latency (ms)*], [*Throughput (img/s)*], [*Load (ms)*], [*Storage (MB)*], [*RAM (MB)*],
    ),
    [EfficientNet-B0], [*42.6 ± 5.6*], [*21.4 ± 1.0*], [118.3], [8.1], [~100],
    [ResNet-50], [96.6 ± 7.4], [10.2 ± 0.0], [385.6], [13.0], [~150],
    [FashionCLIP], [113.6 ± 3.7], [14.2 ± 0.5], [5 109.9], [*3.3*], [~600],
    [DINOv2 ViT-S/14], [126.3 ± 5.1], [10.2 ± 0.2], [1 223.4], [*2.4*], [~250],
    [CLIP ViT-B/32], [140.5 ± 7.8], [11.9 ± 0.2], [1 868.8], [*3.3*], [~600],
    [CLIP ViT-B/16], [235.5 ± 7.3], [4.0 ± 0.1], [6 518.5], [*3.3*], [~600],
  ),
  kind: table,
) <tbl-efficiency>

EfficientNet-B0 leads every efficiency metric. Its inference time of 42.6 ms is 3.0× faster than DINOv2 ViT-S/14 (126.3 ms) and 5.5× faster than CLIP ViT-B/16 (235.5 ms). Its throughput of 21.4 img/s is 1.5× higher than FashionCLIP (14.2) and 2.1× higher than DINOv2 and ResNet-50 (10.2). Load time of 118.3 ms is less than a third of ResNet-50 (385.6 ms) and two orders of magnitude below the multi-second CLIP load times. This profile makes EfficientNet-B0 the only model clearly viable for CPU-only deployment at interactive latencies without cold-start penalties.

The CLIP-family and DINOv2 models exhibit different inference latencies driven by self-attention scaling with image patches. FashionCLIP (113.6 ms) and CLIP ViT-B/32 (140.5 ms) are markedly faster than CLIP ViT-B/16 (235.5 ms), a consequence of the larger ViT-B/16 patch configuration on this CPU; CLIP ViT-B/16 also has the lowest throughput (4.0 img/s). DINOv2 ViT-S/14 (126.3 ms) sits between the CLIP models. Load times of 1.2-6.5 seconds for the transformer models reflect weight initialisation cost, a one-time startup penalty, not per-request.

ResNet-50 holds an intermediate position (96.6 ms latency, 10.2 img/s throughput) and has the largest storage footprint: 13.0 MB, 1.6× larger than EfficientNet-B0 (8.1 MB) and 4.0-5.4× larger than the transformer models (2.4-3.3 MB). Storage scales linearly with embedding dimensionality: 384-dim DINOv2 (2.4 MB), 512-dim CLIP family (3.3 MB), 1,280-dim EfficientNet-B0 (8.1 MB), and 2,048-dim ResNet-50 (13.0 MB) for 5,000 images. At production scale with millions of items, this becomes a meaningful differentiator.

RAM values are reported as approximate ranges derived from each model's documented parameter count plus PyTorch runtime overhead, because direct process-level measurement via psutil proved unreliable on this Linux kernel, producing negative and zero artefacts. Memory consumption scales with model size: the lightweight EfficientNet-B0 (5.3 M parameters) sits at roughly 100 MB, ResNet-50 (25.6 M) and DINOv2 ViT-S/14 (21 M) at roughly 150-250 MB, and the CLIP-family models (~150 M parameters) at roughly 600 MB. These figures are indicative rather than instrumented; actual values vary with batch size and PyTorch overhead.

*Answer to RQ2.* The accuracy-speed trade-off is large and non-linear. EfficientNet-B0 (42.6 ms) achieves 97.2% of Fashion-CLIP's mAP (0.9077 vs 0.9336) at 37.5% of the latency. ResNet-50 combines a middling mAP (0.9132) with middling latency (96.6 ms) and the largest storage footprint (13.0 MB). The relationship is not simply "slower equals more accurate": CLIP ViT-B/16 has the highest latency (235.5 ms) yet ranks below the faster FashionCLIP, DINOv2, and CLIP ViT-B/32, which shows that architecture and patch configuration, not raw inference time, govern accuracy. Practitioners choosing between EfficientNet-B0 and Fashion-CLIP weigh a 2.86% mAP improvement against a 2.67× latency increase.
