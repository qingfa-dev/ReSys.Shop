== PostgreSQL and pgvector

PostgreSQL 17 hosts both relational business data and vector embeddings in a single database.

- *Relational schema.* Tables for products, variants, orders, users, and supporting entities are organised by bounded context. Foreign keys reference entities within the same context; cross-context references use identifier columns without database-level constraints, preserving logical isolation.

- *pgvector extension.* Adds a `vector(N)` column type and cosine distance operator (`<=>`). An HNSW index on the embedding column enables sub-10ms ANN search on catalog-scale datasets.

- *Transactional consistency.* A product update and its embedding update share the same ACID boundary. New images trigger embedding generation; catalog modifications and index changes are committed together, eliminating stale-index drift.

- *Performance.* Composite indexes on query-critical combinations (user status, session status, product slug) optimise frequent access patterns. Query plans combine vector similarity and relational filtering in a single execution.
