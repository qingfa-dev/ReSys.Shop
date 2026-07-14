# =============================================================================
# Inference Service Dockerfile — Production Optimized
#
# Build optimizations:
# - Python 3.12-slim: Latest stable, matching the local dev environment
# - uv binary copied directly: No pip install needed, very fast sync
# - UV_COMPILE_BYTECODE: pre-compiles .pyc at build time → faster cold start
# - UV_LINK_MODE=copy: safe across Docker filesystem boundaries
# - NuGet-style cache mount for uv package cache across builds
# - Non-root user with explicit UID (1001) for security
#
# Runtime optimizations:
# - libgomp1: Essential for ONNX Runtime / PyTorch multi-threading performance
# - tini as PID 1: correct SIGTERM forwarding and zombie reaping
# - curl healthcheck: lighter than spawning a Python subprocess per probe
# =============================================================================

# --- Stage 1: Builder ---
FROM python:3.12-slim AS builder

# Copy uv binary from the official image
COPY --from=ghcr.io/astral-sh/uv:latest /uv /uvx /bin/

# Build-time environment variables
ENV UV_COMPILE_BYTECODE=1 \
    UV_LINK_MODE=copy \
    UV_PYTHON_DOWNLOADS=never \
    UV_PROJECT_ENVIRONMENT=/app/.venv

WORKDIR /app

# Copy dependency manifests first for layer caching
COPY pyproject.toml uv.lock ./

# Install ONLY runtime dependencies into the isolated virtual environment.
# --frozen: Fail if uv.lock is out of date
# --no-dev: Skip development/test dependencies
# --no-install-project: Skip installing the project package (src/ not present yet)
RUN --mount=type=cache,target=/root/.cache/uv \
    uv sync --frozen --no-dev --no-install-project

# --- Stage 2: Production runtime ---
FROM python:3.12-slim AS runtime

# Install runtime system dependencies:
# - libgomp1: OpenMP (multi-threading) for ONNX/PyTorch
# - tini: Minimal init process (PID 1)
# - curl: Lightweight healthcheck probes
RUN apt-get update \
    && apt-get install -y --no-install-recommends \
    libgomp1 \
    tini \
    curl \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/* /var/cache/apt/archives/*

# Non-root user setup
RUN useradd --no-create-home --shell /bin/bash --uid 1001 inference

WORKDIR /app

# Copy the pre-built virtual environment with correct ownership
COPY --from=builder --chown=inference:inference /app/.venv /app/.venv

# Prepend venv to PATH and set runtime-only Python env vars
ENV PATH="/app/.venv/bin:$PATH" \
    PYTHONUNBUFFERED=1 \
    PYTHONDONTWRITEBYTECODE=1 \
    PYTHONHASHSEED=random \
    PYTHONPATH=/app \
    # Align with the project's default port configuration
    PORT=8000 \
    # Ensure models are looked for in the /app/models mount point in Docker
    ONNX_MODEL_DIR=/app/models

# Copy application source
COPY --chown=inference:inference src/ ./src/

# Create the models mount point with correct ownership
RUN mkdir -p /app/models && chown inference:inference /app/models

USER inference

# Expose the configured service port
EXPOSE 8000

# tini handles signal forwarding (SIGTERM) and zombie process reaping
ENTRYPOINT ["/usr/bin/tini", "--"]

# Launch uvicorn directly through the venv's python
# --workers 1: Sidecars scale via pod replicas, not internal uvicorn workers
CMD ["python", "-m", "uvicorn", "embedding.main:app", \
    "--host", "0.0.0.0", \
    "--port", "8000", \
    "--workers", "1", \
    "--no-access-log"]

# Healthcheck via curl to the internal health endpoint
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -sf http://localhost:8000/health || exit 1
