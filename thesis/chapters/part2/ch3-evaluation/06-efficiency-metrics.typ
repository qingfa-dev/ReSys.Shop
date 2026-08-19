== Computational Efficiency and Resource Trade-offs

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.6_benchmark_latency.png", width: 100%),
  caption: [Inference latency comparison across four evaluated models. EfficientNet-B0 leads at 37.8 ms, followed by ResNet-50 (61.9 ms), CLIP-generic (86.6 ms), and Fashion-CLIP (96.8 ms).],
) <fig-benchmark-latency>

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.6_benchmark_throughput.png", width: 100%),
  caption: [Throughput comparison across four evaluated models. EfficientNet-B0 leads at 30.2 img/s, followed by CLIP-generic (21.4), Fashion-CLIP (18.5), and ResNet-50 (13.5).],
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
    [EfficientNet-B0], [*37.8 ± 26.6*], [*30.2 ± 13.5*], [*110.2*], [8.1], [---],
    [ResNet-50], [61.9 ± 5.8], [13.5 ± 0.7], [374.1], [13.0], [---],
    [Fashion-CLIP], [96.8 ± 6.8], [18.5 ± 1.3], [5 255.4], [*3.3*], [---],
    [CLIP-generic], [86.6 ± 8.4], [21.4 ± 0.3], [6 848.5], [*3.3*], [---],
  ),
  kind: table,
) <tbl-efficiency>

EfficientNet-B0 dominates every efficiency metric. Its inference time of 37.8 ms is 1.6× faster than ResNet-50 (61.9 ms) and 2.6× faster than both CLIP models (86.6--96.8 ms). Its throughput of 30.2 img/s is 1.4× higher than CLIP-generic (21.4) and 2.2× higher than ResNet-50 (13.5). Load time of 110.2 ms is less than half of ResNet-50 (374.1 ms) and two orders of magnitude below the five-second-plus CLIP load times. This profile makes EfficientNet-B0 the only model clearly viable for CPU-only deployment at interactive latencies without cold-start penalties.

The two CLIP-based models exhibit different inference latencies (Fashion-CLIP 96.8 ms, CLIP-generic 86.6 ms), a consequence of self-attention layers scaling quadratically with image patches. CLIP-generic achieves higher throughput (21.4 vs 18.5 img/s) with lower latency, suggesting better parallelisation on this CPU. Five-second-plus load times for both CLIP models reflect transformer weight initialisation cost -- a one-time startup penalty, not per-request.

ResNet-50 occupies an intermediate position (61.9 ms latency, 13.5 img/s throughput) but has the largest storage footprint: 13.0 MB, 1.6× larger than EfficientNet-B0 (8.1 MB) and 4.0× larger than either CLIP model (3.3 MB each). Storage scales linearly with embedding dimensionality: 512-dim (3.3 MB), 1,280-dim (8.1 MB), and 2,048-dim (13.0 MB) for 5,000 images. At production scale with millions of items, this becomes a meaningful differentiator.

RAM values are reported as dashes: process-level measurement via psutil proved unreliable on this Linux kernel, producing negative and zero artefacts. Actual memory consumption ranges from approximately 100 MB (EfficientNet-B0) to over 600 MB (CLIP-based) for model weights alone, plus PyTorch runtime overhead.

*Answer to RQ2.* The accuracy-speed trade-off is substantial and non-linear. EfficientNet-B0 (37.8 ms) achieves 95.55% of Fashion-CLIP's mAP (0.8895 vs 0.9309) at 39.1% of the latency. ResNet-50 combines the lowest mAP (0.8857) with middling latency (61.9 ms) and the largest storage footprint (13.0 MB). The relationship is not simply "slower equals more accurate": the two CLIP models have near-identical latency yet Fashion-CLIP's mAP is 2.13% higher, demonstrating that domain-specific optimisation provides accuracy gains without a speed penalty. Practitioners choosing between EfficientNet-B0 and Fashion-CLIP weigh a 4.65% mAP improvement against a 2.56× latency increase.
