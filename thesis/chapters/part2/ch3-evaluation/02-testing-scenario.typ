== Scenario of Testing

The testing scenarios cover the four functional areas identified in Section 3.1: visual search (7 scenarios), ML embedding pipeline (6 scenarios), shopping cart and checkout (8 scenarios), and admin product management (7 scenarios). Each scenario is validated with step-by-step test cases defined in Section 3.3.

=== Testing Environment

- *Hardware.* Intel Core i7-1165G7 (4 cores, 2.80 GHz), 16 GB DDR4 RAM, 512 GB NVMe SSD.
- *Database.* PostgreSQL 17 with pgvector 0.7.0, provisioned via Testcontainers for integration tests.
- *Cache.* Redis 7 (Alpine) for HybridCache L2 storage and Hangfire job persistence.
- *ML Sidecar.* Python 3.12 FastAPI service with PyTorch 2.13.0, CPU-only inference.
- *Web Browsers.* Google Chrome 131, Firefox 135.
