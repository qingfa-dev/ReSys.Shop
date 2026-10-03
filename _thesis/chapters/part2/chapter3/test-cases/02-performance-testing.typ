=== Performance Benchmark Definitions

Performance benchmarks primarily focus on system latency and throughput under load. The specific targets (Service Level Objectives) are derived from industry standards for e-commerce interactivity ($< 500$ ms for search) to ensure a frictionless user experience.


#figure(
  table(
    columns: (1fr, 1fr, 1fr, 1fr),
    align: center,
    stroke: 0.5pt,
    [*Metric*], [*Target*], [*Actual*], [*Status*],
    [Search Latency (Hybrid)], [$< 200$ ms], [280 ms], [PARTIAL FAIL (See Analysis)],
    [API Response Time], [$< 200$ ms], [45 ms], [PASS],
    [Max Concurrent Users], [$> 20$], [45], [PASS],
    [Page Load Time (Home)], [$< 1.5$ s], [0.8 s], [PASS],
  ),
  caption: [System Performance Metrics on Laptop Hardware (i7-1165G7 / MX330).],
  kind: table,
)
