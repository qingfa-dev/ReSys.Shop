== Goal of Testing

The goal of testing is to verify that the platform functions correctly against its system requirements, with emphasis on the core research components: visual search (CBIR), the ML embedding pipeline, and multi-model benchmarking.

#figure(
  table(
    columns: (auto, 1fr),
    stroke: 0.5pt,
    align: (left + horizon, left + horizon),
    table.header([*Testing Type*], [*Objectives for Core Research Features*]),

    [Functional],
    [
      - Visual search returns ranked product results with similarity scores.
      - ML sidecar generates embeddings of correct dimensionality per model.
      - Shopping cart supports add, update, remove, and guest-to-user merge.
      - Checkout enforces forward-only state transitions.
      - Admin CRUD for products, variants, images, and taxonomies.
    ],

    [API Integration],
    [
      - .NET backend dispatches image bytes to Python sidecar and receives vectors.
      - pgvector queries return results filtered by model_name.
      - Error responses conform to RFC 7807 Problem Details.
      - Sidecar health endpoint reports correct status for container orchestration.
    ],

    [Database],
    [
      - Product, variant, and embedding records are persisted atomically.
      - Inventory reservations prevent overselling under concurrent checkout.
      - Soft-deletion preserves referential integrity.
    ],

    [Security],
    [
      - JWT tokens expire after 15 minutes.
      - Endpoints reject requests with insufficient permission claims.
      - Upload validation rejects mismatched magic bytes.
    ],
  ),
  kind: table,
  caption: [Testing objectives focused on core research features.],
) <tbl-testing-goals>
