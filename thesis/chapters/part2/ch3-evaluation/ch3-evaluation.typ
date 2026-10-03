= Testing and Assessment

This chapter verifies the platform against the requirements of Section 2.1 and assesses the six selected embedding models, answering the three research questions of Chapter 1. Sections 3.1--3.4 cover functional software testing, and Sections 3.5--3.8 present the benchmark analysis on the 5,000-image Fashion Product Images Dataset @kaggle-fashion-dataset with 3-fold cross-validation.

- *Goal of Testing.* Objectives for verifying the .NET backend and Python ML sidecar against system requirements.
- *Testing Strategy.* Unit, integration, and end-to-end testing layers.
- *Scenario of Testing.* Testing environment and hardware specifications.
- *Result of Testing.* Functional test cases across visual search, ML embedding, cart/checkout, and admin management.
- *Benchmark Protocol and Experimental Setup.* Dataset, models, metrics, cross-validation methodology, hardware environment.
- *Retrieval Performance and Accuracy.* Aggregate accuracy metrics (mAP, P\@K, R\@K) and RQ1 answer.
- *Computational Efficiency and Resource Trade-offs.* Latency, throughput, storage, RAM trade-offs and RQ2 answer.
- *Synthesis, Deployment Strategy, and Limitations.* Accuracy-efficiency comparison, deployment recommendations, limitations, and RQ3 answer.

#include "01-testing-goal.typ"
#include "01-testing-strategy.typ"
#include "02-testing-scenario.typ"
#include "03-testing-result.typ"
#include "04-benchmark-protocol.typ"
#include "05-retrieval-performance.typ"
#include "06-efficiency-metrics.typ"
#include "07-model-comparison.typ"
