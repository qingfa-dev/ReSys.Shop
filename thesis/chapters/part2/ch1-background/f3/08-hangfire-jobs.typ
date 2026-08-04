=== Hangfire Background Jobs

*Hangfire* processes operations that should not block HTTP requests @hangfire-docs. Jobs are persisted in Redis for resilience across application restarts.

- *Cart expiry.* A recurring job runs daily, removing carts with no activity for seven days. This prevents abandoned carts from accumulating indefinitely.

- *Embedding queue.* When an admin uploads new product images, embedding generation tasks are enqueued for asynchronous processing. The upload endpoint returns immediately; the embedding appears in search results once the job completes.

- *Index maintenance.* A periodic job rebuilds the HNSW index on the embedding column, optimising search performance as the catalog grows.
