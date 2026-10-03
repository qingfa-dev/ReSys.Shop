=== Implementation Insights

1. *Command Validation is not enough:* It was observed that *Domain Invariants* (like Stock Levels) must be checked inside the *Transaction*, not just in the Validator. Issue #2 (Race Condition) highlighted this.
2. *Dataset Balance is critical:* Early tests with random images skewed results towards "Tops". Implementing the controlled seeding pipeline (Methodology 3.2.3) was essential for trustworthy ML metrics.
