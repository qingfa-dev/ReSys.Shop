= Testing and Evaluation

This chapter presents the empirical evaluation of the ReSys.Shop platform, covering functional testing of the e-commerce system and the systematic benchmark of the visual search pipeline that constitutes the core research contribution.

The chapter is organized into the following key sections:

- *Testing Strategy.* Summarises unit, integration, and end-to-end testing approaches applied across the .NET backend, Python sidecar, and Vue.js frontend.
- *Benchmark Protocol.* Defines the cross-validation setup, dataset partitioning, evaluation metrics, and model configurations used for systematic comparison.
- *Retrieval Performance.* Reports accuracy metrics (mAP, Precision\@K, Recall\@K, nDCG) across 11 embedding models on the fashion product retrieval task.
- *Efficiency Metrics.* Presents inference latency, throughput, model load time, storage footprint, and memory consumption for each model.
- *Model Comparison.* Synthesises the accuracy-efficiency data, analyses trade-offs, and answers the three research questions posed in Chapter 1.

#include "01-testing-strategy.typ"
#include "02-benchmark-protocol.typ"
#include "03-retrieval-performance.typ"
#include "04-efficiency-metrics.typ"
#include "05-model-comparison.typ"
