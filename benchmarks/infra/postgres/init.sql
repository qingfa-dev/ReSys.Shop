-- ─────────────────────────────────────────────────────────────────────────────
-- Resys.FashionShop Benchmark — PostgreSQL initialisation
--
-- Runs automatically on first container start via docker-entrypoint-initdb.d
--
-- Schema
-- ──────
-- benchmark_products  core product table with pgvector embedding column
-- embedding_runs      audit log: which model produced which embeddings
--
-- Supported embedding dimensions
-- ─────────────────────────────
--   512   FashionCLIP, CLIP ViT-B/32, EVA-CLIP EVA02-B/16
--   768   CLIP ViT-L/14, SigLIP ViT-B/16
--
-- We store all models in a single table with a SMALLINT model_id FK so that
-- pgvector IVFFlat indexes can be created per-model-dimension.
-- ─────────────────────────────────────────────────────────────────────────────

-- Enable pgvector
CREATE EXTENSION IF NOT EXISTS vector;

-- ── Model registry ────────────────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS embedding_models (
    id            SERIAL      PRIMARY KEY,
    slug          TEXT        NOT NULL UNIQUE,   -- e.g. "fashionclip", "clip-vit-b-32"
    name          TEXT        NOT NULL,           -- e.g. "FashionCLIP"
    embedding_dim SMALLINT    NOT NULL,           -- 512 or 768
    hf_model_id   TEXT,                          -- HuggingFace repo id
    registered_at TIMESTAMPTZ NOT NULL DEFAULT now()
);

INSERT INTO embedding_models (slug, name, embedding_dim, hf_model_id) VALUES
    ('fashion-clip',    'FashionCLIP',        512, 'patrickjohncyh/fashion-clip'),
    ('clip-b32',        'CLIP ViT-B/32',      512, NULL),
    ('clip-l14',        'CLIP ViT-L/14',      768, NULL),
    ('siglip',          'SigLIP ViT-B/16',    768, 'google/siglip-base-patch16-224'),
    ('eva-clip',        'EVA-CLIP EVA02-B/16', 512, NULL)
ON CONFLICT (slug) DO NOTHING;

-- ── Product embeddings (one row per product × model) ─────────────────────────

-- 512-D table — FashionCLIP, CLIP ViT-B/32, EVA-CLIP
CREATE TABLE IF NOT EXISTS product_embeddings_512 (
    product_id    TEXT        NOT NULL,
    model_id      INTEGER     NOT NULL REFERENCES embedding_models(id) ON DELETE CASCADE,
    label         TEXT        NOT NULL,           -- articleType from styles.csv
    embedding     VECTOR(512) NOT NULL,
    indexed_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (product_id, model_id)
);

-- 768-D table — CLIP ViT-L/14, SigLIP
CREATE TABLE IF NOT EXISTS product_embeddings_768 (
    product_id    TEXT        NOT NULL,
    model_id      INTEGER     NOT NULL REFERENCES embedding_models(id) ON DELETE CASCADE,
    label         TEXT        NOT NULL,
    embedding     VECTOR(768) NOT NULL,
    indexed_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
    PRIMARY KEY (product_id, model_id)
);

-- ── IVFFlat indexes (approximate nearest-neighbour, cosine distance) ──────────
--
-- Rule of thumb: lists ≈ sqrt(N).
-- For ~44 k products, sqrt(44000) ≈ 210 → use 200 lists.
-- Rebuild after bulk inserts with VACUUM ANALYZE.
--
-- These are created AFTER the tables so init.sql runs quickly even on
-- first start without data.

CREATE INDEX IF NOT EXISTS idx_emb512_cosine
    ON product_embeddings_512
    USING ivfflat (embedding vector_cosine_ops)
    WITH (lists = 200);

CREATE INDEX IF NOT EXISTS idx_emb768_cosine
    ON product_embeddings_768
    USING ivfflat (embedding vector_cosine_ops)
    WITH (lists = 200);

-- ── Benchmark run audit log ───────────────────────────────────────────────────

CREATE TABLE IF NOT EXISTS benchmark_runs (
    id            SERIAL      PRIMARY KEY,
    model_slug    TEXT        NOT NULL,
    dataset_name  TEXT        NOT NULL,
    label_col     TEXT        NOT NULL DEFAULT 'articleType',
    n_gallery     INTEGER,
    n_query       INTEGER,
    map_score     NUMERIC(6, 4),
    precision_at  JSONB,                          -- {"1": 0.91, "5": 0.78, ...}
    recall_at     JSONB,
    ndcg_at       JSONB,
    latency_ms    JSONB,                          -- {"p50_ms": 12.3, ...}
    throughput_per_sec NUMERIC(10, 2),
    ran_at        TIMESTAMPTZ NOT NULL DEFAULT now(),
    notes         TEXT
);

-- ── Helper views ──────────────────────────────────────────────────────────────

-- Most recent run per model
CREATE OR REPLACE VIEW latest_benchmark_runs AS
SELECT DISTINCT ON (model_slug)
    model_slug,
    dataset_name,
    map_score,
    precision_at,
    recall_at,
    ndcg_at,
    latency_ms,
    throughput_per_sec,
    ran_at
FROM  benchmark_runs
ORDER BY model_slug, ran_at DESC;

-- Model ranking by mAP
CREATE OR REPLACE VIEW model_ranking AS
SELECT
    row_number() OVER (ORDER BY map_score DESC) AS rank,
    model_slug,
    map_score,
    (precision_at->>'10')::NUMERIC  AS precision_at_10,
    (recall_at->>'10')::NUMERIC     AS recall_at_10,
    (ndcg_at->>'10')::NUMERIC       AS ndcg_at_10,
    (latency_ms->>'p50_ms')::NUMERIC AS latency_p50_ms,
    ran_at
FROM  latest_benchmark_runs
WHERE map_score IS NOT NULL
ORDER BY map_score DESC;

-- ── Convenience functions ─────────────────────────────────────────────────────

-- Search for the top-K nearest products for a given 512-D embedding
CREATE OR REPLACE FUNCTION search_products_512(
    query_embedding VECTOR(512),
    target_model_id INTEGER,
    top_k           INTEGER DEFAULT 20
)
RETURNS TABLE (
    product_id TEXT,
    label      TEXT,
    score      NUMERIC
)
LANGUAGE SQL STABLE AS $$
    SELECT
        product_id,
        label,
        (1 - (embedding <=> query_embedding))::NUMERIC(6, 4) AS score
    FROM  product_embeddings_512
    WHERE model_id = target_model_id
    ORDER BY embedding <=> query_embedding
    LIMIT top_k;
$$;

-- Search for the top-K nearest products for a given 768-D embedding
CREATE OR REPLACE FUNCTION search_products_768(
    query_embedding VECTOR(768),
    target_model_id INTEGER,
    top_k           INTEGER DEFAULT 20
)
RETURNS TABLE (
    product_id TEXT,
    label      TEXT,
    score      NUMERIC
)
LANGUAGE SQL STABLE AS $$
    SELECT
        product_id,
        label,
        (1 - (embedding <=> query_embedding))::NUMERIC(6, 4) AS score
    FROM  product_embeddings_768
    WHERE model_id = target_model_id
    ORDER BY embedding <=> query_embedding
    LIMIT top_k;
$$;
