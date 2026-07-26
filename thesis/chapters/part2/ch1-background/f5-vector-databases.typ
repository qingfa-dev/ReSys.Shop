== Vector Databases

Once images are converted to embeddings, those vectors must be stored and searched efficiently. For catalog-scale data, brute-force distance computation is impractical. This section introduces approximate nearest neighbour search, two index algorithms (HNSW and IVFFlat), and the pgvector extension that provides vector search within a standard PostgreSQL database.

#include "f5/01-ann-search.typ"
#include "f5/02-hnsw.typ"
#include "f5/03-ivfflat.typ"
#include "f5/04-pgvector.typ"
