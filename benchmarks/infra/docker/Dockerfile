# ─────────────────────────────────────────────────────────────────────────────
# Resys.FashionShop — Benchmark image
#
# Multi-stage build:
#   builder  — installs Python deps with uv
#   runtime  — lean image with compiled packages copied in
#
# Build:
#   docker build -f infra/docker/Dockerfile -t resys-benchmark .
#
# Run (all models, real dataset):
#   docker run --rm -v $(pwd)/data:/app/data -v $(pwd)/outputs:/app/outputs \
#     resys-benchmark benchmark --dataset-root data/raw/fashion-dataset
# ─────────────────────────────────────────────────────────────────────────────

# ── Stage 1: builder ──────────────────────────────────────────────────────────
FROM python:3.12-slim AS builder

# uv is the fastest Python package manager; install it from the official image
COPY --from=ghcr.io/astral-sh/uv:0.4 /uv /usr/local/bin/uv

WORKDIR /app

# Copy only dependency manifests first (maximises layer cache hits)
COPY pyproject.toml uv.lock ./

# Install all deps into /app/.venv; no-cache keeps the layer small
RUN uv sync --frozen --no-install-project --no-cache

# Copy the source package and install it
COPY src/ ./src/
RUN uv sync --frozen --no-cache


# ── Stage 2: runtime ──────────────────────────────────────────────────────────
FROM python:3.12-slim AS runtime

# System-level runtime dependencies
# libgomp1   — OpenMP (needed by faiss-cpu)
# libglib2.0 — required by some torch operations
RUN apt-get update && apt-get install -y --no-install-recommends \
        libgomp1 \
        libglib2.0-0 \
        curl \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /app

# Copy the complete virtualenv from the builder
COPY --from=builder /app/.venv /app/.venv

# Copy source (needed for editable install resolution)
COPY --from=builder /app/src ./src
COPY --from=builder /app/pyproject.toml ./pyproject.toml

# Copy configs and scripts so CLI tools can find them
COPY configs/        ./configs/
COPY scripts/        ./scripts/

# Activate the venv for every subsequent command
ENV PATH="/app/.venv/bin:$PATH" \
    PYTHONPATH="/app/src" \
    PYTHONDONTWRITEBYTECODE=1 \
    PYTHONUNBUFFERED=1 \
    # Suppress tokenizer parallelism warnings in containers
    TOKENIZERS_PARALLELISM=false \
    # Use /app/data and /app/outputs as default mount points
    BENCHMARK_DATASET_ROOT=/app/data/raw/fashion-dataset \
    BENCHMARK_OUTPUT_DIR=/app/outputs \
    BENCHMARK_CACHE_DIR=/app/data/cache

# Create mount-point directories so Docker volumes bind cleanly
RUN mkdir -p \
    /app/data/raw \
    /app/data/cache \
    /app/data/splits \
    /app/outputs/embeddings \
    /app/outputs/metrics \
    /app/outputs/reports \
    /app/outputs/tables \
    /app/outputs/figures \
    /app/outputs/logs

# Health-check: verify the CLI loads without error
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD benchmark --help > /dev/null || exit 1

ENTRYPOINT ["benchmark"]
CMD ["--help"]
