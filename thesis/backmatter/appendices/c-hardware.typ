= Hardware Specifications <appendix-c>

All benchmark results reported in Chapter 3 and Appendix A were collected on a single workstation.

== Compute Hardware

#figure(
  caption: [Benchmark Workstation Configuration],
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Component*], [*Specification*],
    [CPU], [Intel (11th Gen Core i7-1165G7, 4 cores / 8 threads, 2.80 GHz base / 4.70 GHz boost, 12 MB L3 cache)],
    [RAM], [16 GB DDR4],
    [Storage], [512 GB NVMe SSD (KIOXIA KBG40ZNS)],
  ),
  kind: table,
) <tbl-appendix-hardware>

All benchmarks were executed on CPU (no GPU). Pretrained model weights from HuggingFace and PyTorch Hub were used without fine-tuning. Inference timing includes preprocessing and postprocessing.

== Software Stack

#figure(
  caption: [Benchmark Software Environment],
  table(
    columns: (auto, 1fr),
    align: (left, left),
    stroke: 0.5pt,
    [*Component*], [*Version*],
    [Operating System], [Linux (Ubuntu 24.04 LTS, kernel 6.8)],
    [Python], [3.12.x (CPython)],
    [PyTorch], [2.13.0],
    [TorchVision], [0.28.0],
    [HuggingFace Transformers], [5.14.1],
    [OpenCLIP], [3.3.0],
    [NumPy], [2.5.1],
  ),
  kind: table,
) <tbl-appendix-software>

All models were loaded from pre-trained weights on HuggingFace or PyTorch Hub; no fine-tuning was performed.

== Precision and Reproducibility

All computations used float32 precision; mixed precision was not used due to numerical instability. Reproducibility settings: random seed 42, `model.eval()` with `torch.no_grad()`. Results may not be bitwise-identical on different hardware due to CPU performance variance, memory capacity differences, and disk I/O speed. All benchmark scripts and raw result files are available in the project's benchmark repository.

#pagebreak()
