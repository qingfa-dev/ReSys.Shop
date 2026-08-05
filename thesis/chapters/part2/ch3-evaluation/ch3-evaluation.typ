= Testing and Evaluation

This chapter verifies the platform through functional software testing (Sections 3.1--3.3) and systematic benchmark evaluation of the embedding models (Section 3.4 onward).

- *Goal of Testing.* Objectives for verifying the .NET backend and Python ML sidecar satisfy system requirements.
- *Scenario of Testing.* Testing environment and hardware specifications.
- *Result of Testing.* Functional test cases across visual search, ML embedding, cart/checkout, and admin management.
- *Benchmark Protocol.* Dataset, models, metrics, cross-validation methodology, hardware environment.
- *Retrieval Performance.* Aggregate accuracy metrics (mAP, P\@K, R\@K) and RQ1 answer.
- *Efficiency Metrics.* Latency, throughput, storage, RAM trade-offs and RQ2 answer.
- *Synthesis and Deployment.* Accuracy-efficiency comparison, deployment recommendations, limitations, and RQ3 answer.

#include "01-testing-goal.typ"
#include "02-testing-scenario.typ"
#include "03-testing-result.typ"
#include "04-benchmark-protocol.typ"
#include "05-retrieval-performance.typ"
#include "06-efficiency-metrics.typ"
#include "07-model-comparison.typ"
