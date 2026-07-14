# External Integrations

## Core Sections (Required)

### 1) Integration Inventory

| System | Type (API/DB/Queue/etc) | Purpose | Auth model | Criticality | Evidence |
|--------|---------------------------|---------|------------|-------------|----------|
| Hugging Face Hub | Model Registry / API | Download pre-trained model weights (FashionCLIP, CLIP, SigLIP) | None (public models) | High | `src/benchmark/models/fashion_clip.py:L26` |
| OpenCLIP model zoo | Model weights | Download EVA-CLIP weights | None (public) | Medium | `src/benchmark/models/eva_clip.py` |
| PyTorch Hub / torchvision | Model weights | Download EfficientNet weights | None (public) | Low | `src/benchmark/models/efficientnet_b0.py` |
| PostgreSQL + PGVector (research only) | Database | Vector search benchmark and HNSW accuracy validation | Connection string with user/password | Low | `src/benchmark/research/db.py`, `src/benchmark/cli/research.py:L73-84` |

### 2) Data Stores

| Store | Role | Access layer | Key risk | Evidence |
|-------|------|--------------|----------|----------|
| NPZ file cache (`data/cache/*.npz`) | Embedding persistence (transient) | `src/benchmark/embeddings/cache.py` | Cache invalidation — no checksum on dataset; stale cache if dataset changes | `src/benchmark/embeddings/cache.py:L24` |
| Durable embeddings (`outputs/embeddings/`) | Human-inspectable embedding storage for benchmark record (differs from cache: persistent, keyed by model slug only) | `src/benchmark/embeddings/storage.py:L17-41` | No invalidation — same slug always overwrites | `src/benchmark/embeddings/storage.py` |
| PostgreSQL (research sandbox) | Vector index for PGVector benchmarks | `src/benchmark/research/db.py` | Requires running PostgreSQL + PGVector extension; connection string hardcoded in CLI defaults | `src/benchmark/research/db.py`, `src/benchmark/cli/research.py:L74` |
| ReSys.Research Docker PostgreSQL | Thesis experiment sandbox | `ReSys.Research/docker-compose.yml`, `ReSys.Research/db/schema.sql` | Separate from main benchmark; needs Docker | `ReSys.Research/docker-compose.yml` |

### 3) Secrets and Credentials Handling

- Credential sources: No secret management. Database connection strings for PGVector appear as CLI argument defaults (visible in `src/benchmark/cli/research.py:L74`). No `.env.example` or `.env.template` files found.
- Hardcoding checks: Connection string `postgresql://research_user:research_password@localhost:5433/research_sandbox` is hardcoded as CLI default. This is credentials in source code.
- Rotation or lifecycle notes: Unknown — no secret rotation mechanism detected.

### 4) Reliability and Failure Behavior

- Retry/backoff behavior: None implemented. Model download failures from Hugging Face are not retried; `transformers` and `open-clip-torch` handle their own download logic.
- Timeout policy: No explicit timeouts configured for model downloads or database connections.
- Circuit-breaker or fallback behavior: None. If Hugging Face is unreachable, benchmark fails at model load time.

### 5) Observability for Integrations

- Logging around external calls: Model loading is logged (e.g., `"Loading FashionCLIP from patrickjohncyh/fashion-clip …"`). PGVector operations log errors only via Python logging.
- Metrics/tracing coverage: No APM, Prometheus, or distributed tracing. Latency tracking exists via `benchmark.utils.timing.timed()` context manager for in-process measurement only.
- Missing visibility gaps: No network call monitoring, no model download speed/status tracking, no PGVector query latency to external metrics.

### 6) Evidence

- `src/benchmark/models/fashion_clip.py:L46-52` — HuggingFace model loading
- `src/benchmark/models/eva_clip.py` — OpenCLIP loading
- `src/benchmark/embeddings/cache.py` — NPZ cache read/write
- `src/benchmark/research/db.py` — PGVector integration
- `src/benchmark/cli/research.py:L74-83` — PGVector benchmark CLI with hardcoded connection string
- `ReSys.Research/docker-compose.yml` — PostgreSQL Docker service
