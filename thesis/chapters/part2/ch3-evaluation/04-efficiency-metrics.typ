== Efficiency Metrics

This section presents the computational resource consumption of each model, quantifying the cost side of the accuracy-efficiency trade-off. @tbl-efficiency summarises the efficiency metrics.

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

EfficientNet-B0 dominates every efficiency metric. Its inference time of 23.9 milliseconds is 2.7 times faster than ResNet-50 (64.0 ms) and 3.8 times faster than both CLIP-based models (92.0 ms for Fashion-CLIP, 92.9 ms for CLIP-generic). Its throughput of 33.2 images per second is 1.7 times higher than the next-best model, CLIP-generic (19.9 img/s), and 2.6 times higher than ResNet-50 (12.9 img/s). Its model load time of just 126.3 milliseconds is less than half of ResNet-50 (286.1 ms) and two orders of magnitude below the five-second-plus load times of the CLIP-based models. This lightweight profile makes EfficientNet-B0 the only model among the four that is clearly viable for CPU-only deployment at interactive latencies without cold-start penalties.

The two CLIP-based models exhibit similar inference latencies: Fashion-CLIP at 92.0 ms and CLIP-generic at 92.9 ms, both roughly 3.8 times slower than EfficientNet-B0. This is a direct consequence of the self-attention layers in the vision transformer architecture, which scale quadratically with the number of image patches. Notably, CLIP-generic achieves higher throughput (19.9 img/s) than Fashion-CLIP (18.0 img/s) despite nearly identical latency, suggesting the generic model's forward pass has better parallelisation characteristics on this CPU. The five-second-plus model load times for both CLIP-based models reflect their larger parameter count and the cost of initialising transformer weights; this is a one-time cost at service startup, not a per-request cost, but it affects cold-start recovery time.

ResNet-50 occupies an intermediate position with 64.0 ms inference time and 12.9 img/s throughput. Its load time of 286.1 ms is moderate. However, ResNet-50 has the largest embedding storage footprint at 13.0 MB, 1.6 times larger than EfficientNet-B0 (8.1 MB) and nearly 4 times larger than either CLIP model (3.3 MB each). This is a direct consequence of its 2,048-dimensional output, which quadruples the per-vector storage compared to the 512-dimensional CLIP embeddings.

The storage column now reflects realistic values. The embedding index for the 5,000-item catalogue ranges from 3.3 MB (CLIP-based models at 512 dimensions) through 8.1 MB (EfficientNet-B0 at 1,280 dimensions) to 13.0 MB (ResNet-50 at 2,048 dimensions). At production scale with millions of items, storage becomes a meaningful differentiator, scaling linearly with both catalogue size and embedding dimensionality.

The RAM column reports dashes for all models. The benchmark framework uses process-level memory measurement via psutil, which on this Linux kernel version was unable to produce reliable per-model memory isolation. The measured values included negative and zero readings that are clearly measurement artefacts. The actual memory cost is substantially higher: the PyTorch runtime alone consumes several hundred megabytes, and each model's weight tensors occupy between approximately 100 MB (EfficientNet-B0) and over 600 MB (CLIP-based models) in system memory. The RAM figures in @tbl-efficiency should be interpreted as a measurement limitation rather than actual consumption values; Section 3.5.3 discusses this limitation further.

*Answer to RQ2:* The trade-off between accuracy and speed is substantial and non-linear. The fastest model, EfficientNet-B0 (23.9 ms), achieves 92.8% of the mAP of the most accurate model, Fashion-CLIP (0.8158 vs 0.8788), while operating at 3.8 times lower latency and 1.8 times higher throughput. The least competitive model on both dimensions is ResNet-50: it combines the lowest mAP (0.8120) with middling latency (64.0 ms) and the largest storage footprint (13.0 MB). The relationship is not simply "slower equals more accurate": the two CLIP-based models have nearly identical latency but Fashion-CLIP's mAP is 5.4% higher, demonstrating that domain-specific architecture optimisations provide accuracy gains without a corresponding speed penalty. Practitioners must weigh a 7.7% mAP improvement against a 3.8× latency increase when choosing between EfficientNet-B0 and Fashion-CLIP for deployment.
