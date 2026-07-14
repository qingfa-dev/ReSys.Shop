# External Integrations

## Core Sections (Required)

### 1) Integration Inventory

| System | Type | Purpose | Auth model | Criticality | Evidence |
|---|---|---|---|---|---|
| Hugging Face Hub | Model Registry / API | Download pre-trained model weights (FashionCLIP, CLIP, SigLIP, CLIP-generic, DINOv2) | None (public) | High | `src/benchmark/models/fashion_clip.py`, `src/benchmark/models/clip_generic.py` |
| OpenCLIP model zoo | Model weights | Download EVA-CLIP weights | None (public) | Medium | `src/benchmark/models/eva_clip.py` |
| PyTorch Hub / torchvision | Model weights | Download ResNet-50, EfficientNet-B0, ConvNeXt weights | None (public) | Medium | `src/benchmark/models/resnet50.py`, `src/benchmark/models/efficientnet_b0.py` |
| PostgreSQL + PGVector | Database | Vector indexing, approximate search, recall validation (pipeline mode) | Connection string (user/password) | Medium | `src/benchmark/retrieval/pgvector.py`, `src/benchmark/evaluation/pipeline.py` |

### 2) Data Stores

| Store | Role | Access layer | Key risk | Evidence |
|---|---|---|---|---|
| NPZ file cache (`data/cache/*.npz`) | Embedding persistence — per model × per fold × per split (e.g., `fashionclip__fold_0_test.npz`) | `src/benchmark/embeddings/cache.py` | Stale cache if dataset changes but name stays same | `src/benchmark/embeddings/cache.py:L24` |
| Durable embeddings (`outputs/embeddings/`) | Human-inspectable embedding storage for benchmark record | `src/benchmark/embeddings/storage.py` | No invalidation — same slug always overwrites | `src/benchmark/embeddings/storage.py` |
| PostgreSQL (pipeline mode) | Vector index for production benchmark: batch ingestion, IVFFlat index, query latency, recall comparison | `src/benchmark/retrieval/pgvector.py` via `PipelineRunner` | pgvector IVFFlat dimension limit (2000). ResNet-50 (2048-d) cannot use IVFFlat; pipeline gracefully degrades | `src/benchmark/retrieval/pgvector.py:L170-211`, `infra/postgres/init.sql` |
| Styles CSV (`styles.csv`) | Product metadata: id, masterCategory, subCategory for ground-truth relevance | `src/benchmark/datasets/ground_truth.py` via pandas | Must have `id`, `masterCategory`, `subCategory` columns; missing columns cause fatal error | `src/benchmark/datasets/ground_truth.py:L29-31` |

### 3) Secrets and Credentials Handling

- Credential sources: pgvector connection string is a CLI argument default (`--conn-string "postgresql://benchmark:benchmark@localhost:5432/benchmark"` in `pipeline` and `thesis` commands). These are local dev credentials — not production secrets.
- Hardcoding: Connection string appears in CLI defaults (`src/benchmark/cli/benchmark.py`) and in `PipelineRunner` init default. Acceptable for local dev; should use env vars for any shared environment.
- Rotation or lifecycle: N/A — local dev only.

### 4) Reliability and Failure Behavior

- **PostgreSQL unavailable**: `PipelineRunner._run_pgvector_pipeline()` catches all exceptions, logs warning, returns zero-filled metrics. Exact cosine evaluation continues unaffected.
- **Model weight download fails**: `transformers`/`open-clip-torch` raise exceptions at model load time. No retry logic built in.
- **Missing images in dataset**: `FashionDataset.load()` and `EmbeddingGenerator` skip missing/corrupt images with warnings. Run continues.
- **IVFFlat index build fails** (ResNet-50, 2048-d): Exception caught, pgvector metrics zeroed, logged. Exact cosine metrics still valid.
- **No timeouts or circuit breakers**: Database queries and model downloads have no explicit timeout.

### 5) Observability for Integrations

- Logging: Model loading logged (`"Loading FashionCLIP from ..."`). PGVector operations logged (`"Connected to pgvector at ..."`, `"Built IVFFlat index ... in X.XX s"`, `"Cleared table ..."`). All failures logged as warnings.
- Metrics: Latency tracked via `time.perf_counter()` for embedding generation, pgvector queries, index building, and ingestion. Query latency distributions recorded per-fold.
- Missing: No Prometheus/OpenTelemetry. No model download speed tracking. No database connection pool monitoring.

### 6) Evidence

- `src/benchmark/retrieval/pgvector.py:L54-67` — pgvector connection with deferred import
- `src/benchmark/retrieval/pgvector.py:L170-211` — index build with timing
- `src/benchmark/evaluation/pipeline.py:L248-333` — pgvector pipeline with graceful degradation
- `src/benchmark/datasets/ground_truth.py` — styles.csv parsing
- `src/benchmark/embeddings/cache.py` — npz cache
- `infra/postgres/init.sql` — PostgreSQL schema with pgvector
