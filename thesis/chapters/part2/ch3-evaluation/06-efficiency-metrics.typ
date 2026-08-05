== Computational Efficiency and Resource Trade-offs

#figure(
  image("../../../figures/chapters/part2/ch3-evaluation/diagrams/P2S3.6_benchmark_latency.png", width: 100%),
  caption: [Inference latency comparison across four evaluated models. EfficientNet-B0 leads at 23.9 ms, followed by ResNet-50 (64.0 ms), Fashion-CLIP (92.0 ms), and CLIP-generic (92.9 ms).],
) <fig-benchmark-latency>

#figure(
  caption: [Efficiency Metrics, 3-Fold Cross-Validation],
  table(
    columns: (auto,) + (1fr,) * 5,
    align: (left,) + (center,) * 5,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*Latency (ms)*], [*Throughput (img/s)*], [*Load (ms)*], [*Storage (MB)*], [*RAM (MB)*],
    ),
    [EfficientNet-B0], [*23.9 ± 2.5*], [*33.2 ± 2.2*], [*126.3*], [8.1], [---],
    [ResNet-50], [64.0 ± 3.1], [12.9 ± 0.5], [286.1], [13.0], [---],
    [Fashion-CLIP], [92.0 ± 5.8], [18.0 ± 0.7], [5,441.8], [*3.3*], [---],
    [CLIP-generic], [92.9 ± 2.9], [19.9 ± 0.5], [6,514.0], [*3.3*], [---],
  ),
  kind: table,
) <tbl-efficiency>

EfficientNet-B0 dominates every efficiency metric. Its inference time of 23.9 ms is 2.7× faster than ResNet-50 (64.0 ms) and 3.8× faster than both CLIP models (92.0--92.9 ms). Its throughput of 33.2 img/s is 1.7× higher than CLIP-generic (19.9) and 2.6× higher than ResNet-50 (12.9). Load time of 126.3 ms is less than half of ResNet-50 (286.1 ms) and two orders of magnitude below the five-second-plus CLIP load times. This profile makes EfficientNet-B0 the only model clearly viable for CPU-only deployment at interactive latencies without cold-start penalties.

The two CLIP-based models exhibit similar inference latencies (Fashion-CLIP 92.0 ms, CLIP-generic 92.9 ms), a consequence of self-attention layers scaling quadratically with image patches. CLIP-generic achieves slightly higher throughput (19.9 vs 18.0 img/s) despite near-identical latency, suggesting better parallelisation on this CPU. Five-second-plus load times for both CLIP models reflect transformer weight initialisation cost -- a one-time startup penalty, not per-request.

ResNet-50 occupies an intermediate position (64.0 ms latency, 12.9 img/s throughput) but has the largest storage footprint: 13.0 MB, 1.6× larger than EfficientNet-B0 (8.1 MB) and 3.9× larger than either CLIP model (3.3 MB each). Storage scales linearly with embedding dimensionality: 512-dim (3.3 MB), 1,280-dim (8.1 MB), and 2,048-dim (13.0 MB) for 5,000 images. At production scale with millions of items, this becomes a meaningful differentiator.

RAM values are reported as dashes: process-level measurement via psutil proved unreliable on this Linux kernel, producing negative and zero artefacts. Actual memory consumption ranges from approximately 100 MB (EfficientNet-B0) to over 600 MB (CLIP-based) for model weights alone, plus PyTorch runtime overhead.

*Answer to RQ2.* The accuracy-speed trade-off is substantial and non-linear. EfficientNet-B0 (23.9 ms) achieves 92.8% of Fashion-CLIP's mAP (0.8158 vs 0.8788) at 26.0% of the latency. ResNet-50 combines the lowest mAP (0.8120) with middling latency (64.0 ms) and the largest storage footprint (13.0 MB). The relationship is not simply "slower equals more accurate": the two CLIP models have near-identical latency yet Fashion-CLIP's mAP is 5.4% higher, demonstrating that domain-specific optimisation provides accuracy gains without a speed penalty. Practitioners choosing between EfficientNet-B0 and Fashion-CLIP weigh a 7.7% mAP improvement against a 3.8× latency increase.
