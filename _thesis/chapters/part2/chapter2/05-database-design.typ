== DATABASE DESIGN

#import "../../../template/ctu-styles.typ": figure-placeholder

This section provides the complete database schema for all bounded contexts. The system uses *PostgreSQL 16* as the primary relational store, leveraging its advanced extensions for AI vector operations.

=== Design Principles

The database schema adheres to the following core principles:

- *Polymorphic Precision:* Monetary values are stored differently depending on the use case: `DECIMAL` is used for flexible catalog pricing, while `BIGINT` (cents) is reserved for rigid transactional accounting.
- *Immutable Ledgers:* Inventory is tracked using an append-only ledger pattern (`StockMovements`), ensuring that every change in physical stock is auditable and reversible.
- *Optimistic Concurrency:* High-contention tables (`StockItems`, `Products`) utilize `RowVersion` (mapping to PostgreSQL's `xmin` system column) to detect and prevent overwrites during concurrent edits.
- *Vector-Native:* High-dimensional AI embeddings are treated as first-class citizens, stored directly alongside relational data in `image_embeddings` tables.

#rotate(-90deg, reflow: true)[
  #figure(
    image(
      "../../../images/diagrams/03-data-architecture/data-01-backend-erd.png",
      width: 100%,
      height: 90%,
      fit: "contain",
    ),
    caption: [Backend Entity-Relationship Diagram (ERD): Complete database schema showing tables, keys, and relationships across all bounded contexts.],
  ) <fig:data-01-erd>
]

#include "database/01-identity.typ"
#include "database/02-catalog.typ"
#include "database/03-ordering.typ"
#include "database/04-inventory.typ"

=== Indexing Strategy

To support the specific latency requirements defined in the functional requirements, specialized indexing strategies are applied:

1. *HNSW Index for Vectors:*
  The core visual search relies on Hierarchical Navigable Small World (HNSW) graphs.
  ```sql
  CREATE INDEX idx_embeddings_fashion_clip ON image_embeddings
  USING hnsw (vector vector_cosine_ops)
  WHERE model_name = 'fashion_clip';
  ```

2. *Partial Indexes:*
  To reduce index size and improve insert performance, business-critical queries are optimized with partial constraints.
  `CREATE INDEX idx_products_active ON products (status) WHERE status = 'Active';`

3. *Unique Constraints:*
  Data integrity is enforced at the database level to prevent duplicate logic errors.
  `UNIQUE (slug)` on Taxonomies, Taxons, Products.


