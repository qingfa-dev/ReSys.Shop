== Vector Databases

Once images are converted to embeddings, those vectors must be stored and searched efficiently. This section explains the challenge of vector similarity search at scale, introduces two indexing algorithms, and describes pgvector, the PostgreSQL extension used in this project.

#include "f5/01-ann-search.typ"
#include "f5/02-hnsw.typ"
#include "f5/03-ivfflat.typ"
#include "f5/04-pgvector.typ"
#include "f5/05-decision.typ"
