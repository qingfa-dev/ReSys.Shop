=== Execution Summary

The comprehensive testing phase executed *63 Verification Scenarios* encompassing functional, performance, and security requirements.

/*
  DATA SOURCES FOR RESULTS:
  - Functional: tests\services\ReSys.Api.IntegrationTests
  - Performance: src\services\ReSys.ML\results\thesis_validation\performance_benchmarks.csv
  - Accuracy: src\services\ReSys.ML\results\thesis_validation\recommendation_metrics.csv
  - Search Metrics: src\services\ReSys.ML\results\thesis_validation\search_metrics.csv
*/

#figure(
  table(
    columns: (1fr, 1fr, 1fr, 1fr, 1fr),
    align: center,
    stroke: 0.5pt,
    [*Testing Type*], [*Total*], [*Passed*], [*Failed*], [*Pass Rate*],
    [Functional], [42], [42], [0], [100%],
    [ML Accuracy], [5], [5], [0], [100%],
    [Performance], [4], [3], [1], [75%],
    [Security], [12], [12], [0], [100%],
    [Data Validation], [3], [3], [0], [100%],
    [*TOTAL*], [*66*], [*65*], [*1*], [*98%*],
  ),
  caption: [Overall Test Execution Summary.],
  kind: table,
)
