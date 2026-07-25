== Efficiency Metrics

This section presents the computational resource consumption of each model, quantifying the cost side of the accuracy-efficiency trade-off. Table @tbl-efficiency summarises the efficiency metrics.

#figure(
  caption: [Efficiency Metrics, 3-Fold Cross-Validation],
  table(
    columns: 6,
    align: (left,) + (center,) * 5,
    stroke: 0.5pt,
    table.header(
      [*Model*], [*Latency (ms)*], [*Throughput (img/s)*], [*Load (ms)*], [*Storage (MB)*], [*RAM (MB)*],
    ),
    [EfficientNet-B0], [*21.6 ± 1.6*], [*35.6 ± 2.6*], [*119.8*], [0.5], [*15.3*],
    [ResNet-50], [60.5 ± 2.2], [13.8 ± 0.7], [357.5], [0.8], [0.0],
    [Fashion-CLIP], [84.4 ± 4.0], [20.8 ± 0.6], [5,288.3], [*0.2*], [0.0],
    [CLIP-generic], [105.6 ± 16.2], [13.7 ± 1.1], [5,836.1], [*0.2*], [0.0],
  ),
  kind: table,
) <tbl-efficiency>

EfficientNet-B0 dominates every efficiency metric. Its inference time of 21.6 milliseconds is over 2.8 times faster than ResNet-50 (60.5 ms), 3.9 times faster than Fashion-CLIP (84.4 ms), and 4.9 times faster than CLIP-generic (105.6 ms). Its throughput of 35.6 images per second is 1.7 times higher than the next-best model, Fashion-CLIP (20.8 img/s). Its model load time, the one-time penalty paid at first inference, is just 119.8 milliseconds, compared to 357.5 ms for ResNet-50 and over five seconds for the two CLIP-based models. This lightweight profile makes EfficientNet-B0 the only model among the four that is clearly viable for CPU-only deployment at interactive latencies.

The two CLIP-based models, Fashion-CLIP and CLIP-generic, exhibit the highest inference latencies and the largest load-time penalties. Fashion-CLIP at 84.4 milliseconds and CLIP-generic at 105.6 milliseconds are roughly four to five times slower than EfficientNet-B0, a direct consequence of the self-attention layers in the vision transformer architecture, which scale quadratically with the number of image patches. The elevated standard deviation of CLIP-generic (±16.2 ms) compared to Fashion-CLIP (±4.0 ms) suggests that the generic model's inference time is more variable, possibly due to less predictable batch processing on the available hardware. The five-second-plus model load times for both CLIP-based models reflect the larger parameter count and the cost of initialising the transformer weights; this is a one-time cost at service startup, not a per-request cost, but it affects cold-start recovery time.

ResNet-50 occupies an intermediate position: neither the fastest nor the slowest. Its 60.5 ms inference time and 13.8 img/s throughput place it between the CLIP models and EfficientNet-B0 on both dimensions. Its intermediate character makes it a reasonable choice for deployments where the highest accuracy is desired but the large disk footprint of transformer-based models is prohibitive.

The storage column shows minimal variation: all four embedding indices occupy under one megabyte of disk space for the benchmark catalogue. This reflects the fact that the embedding vectors themselves, even at 2,048 dimensions for ResNet-50, are compact floating-point arrays, and the index metadata overhead is negligible at the 5,000-item catalogue scale. At production scale with millions of items, storage would become a more meaningful differentiator, scaling linearly with both catalogue size and embedding dimensionality.

The RAM column reports near-zero values for three of the four models. The benchmark framework uses process-level memory measurement via the operating system, which on this Linux configuration was unable to isolate the per-model memory footprint for three models. EfficientNet-B0 reports 15.3 MB, which represents the lower bound of measurable memory consumption. The actual memory cost is substantially higher: the PyTorch runtime alone consumes several hundred megabytes, and each model's weight tensors occupy between 100 MB (EfficientNet-B0) and over 600 MB (Fashion-CLIP, CLIP-generic) in GPU VRAM. The RAM figures in Table @tbl-efficiency should be interpreted as a measurement limitation rather than actual consumption values; Section 3.5.3 discusses this limitation further.

*Answer to RQ2:* The trade-off between accuracy and speed is substantial and non-linear. The fastest model, EfficientNet-B0 (21.6 ms), achieves 96.5% of the mAP of the most accurate model, Fashion-CLIP (0.7196 vs 0.7455), while operating at 3.9 times lower latency and 1.7 times higher throughput. The slowest model, CLIP-generic (105.6 ms), achieves the lowest mAP (0.7026), making it the least attractive choice on both dimensions. The relationship is not simply "slower equals more accurate": the middle-tier models demonstrate that architectural differences, CNN efficiency versus transformer expressiveness, produce distinct points on the accuracy-speed plane. Practitioners must weigh a 3.5% mAP improvement against a 3.9× latency increase when choosing between EfficientNet-B0 and Fashion-CLIP for deployment.
