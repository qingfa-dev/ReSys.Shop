-- pgvector schema init for benchmark pipeline
-- Run once on first container start (mounted into /docker-entrypoint-initdb.d/)

CREATE EXTENSION IF NOT EXISTS vector;

-- Core product embedding tables for each supported dimension
CREATE TABLE IF NOT EXISTS products_512  (id TEXT PRIMARY KEY, label TEXT NOT NULL, embedding vector(512));
CREATE TABLE IF NOT EXISTS products_768  (id TEXT PRIMARY KEY, label TEXT NOT NULL, embedding vector(768));
CREATE TABLE IF NOT EXISTS products_1280 (id TEXT PRIMARY KEY, label TEXT NOT NULL, embedding vector(1280));
CREATE TABLE IF NOT EXISTS products_2048 (id TEXT PRIMARY KEY, label TEXT NOT NULL, embedding vector(2048));

-- Benchmark run tracking (optional)
CREATE TABLE IF NOT EXISTS benchmark_runs (
    run_id      TEXT PRIMARY KEY DEFAULT gen_random_uuid()::text,
    model_slug  TEXT NOT NULL,
    fold_idx    INT  NOT NULL,
    started_at  TIMESTAMPTZ DEFAULT now(),
    finished_at TIMESTAMPTZ,
    metrics     JSONB
);
