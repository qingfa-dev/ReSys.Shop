=== Environment Specification

All performance benchmarks were conducted on a standardized *System Under Test (SUT)* configuration. Using a constrained *Ultrabook* profile highlights the efficiency of the implementation, demonstrating that the microservices architecture does not require data-center class hardware for inference.

#figure(
  table(
    columns: (1fr, 1fr),
    stroke: 0.5pt,
    [*Component*], [*Specification*],
    [GPU], [NVIDIA GeForce MX330 (2GB VRAM)],
    [CPU], [Intel Core i7-1165G7 (4 Cores, 8 Threads)],
    [RAM], [16 GB LPDDR4x],
    [OS], [Windows 11 (WSL2)],
  ),
  caption: [Hardware Environment for Benchmarks (Standard Laptop).],
)
