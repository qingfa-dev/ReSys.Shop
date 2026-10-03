# Code Review — benchmarks

Overall clean. Three real bugs, one performance risk, rest are nits and duplication.

## Biology

🔴 `loader.py:L99-101`: `samples` property raises RuntimeError on empty list, masking "empty dataset" vs "not loaded". Guard with `self._samples is not None and hasattr(self, '_loaded')`.

🔴 `pgvector.py:L276`: 768-dim maps to `product_embeddings_768` but 512-dim maps to `products_512`. Inconsistent naming suggests the 768 table was created by a different init script. Standardize to `products_{dim}`.

🔴 `evaluator.py:L101-103`: `label_set_per_query` computed in O(N²) double-loop. Cached by label with a pre-built `{label: set_of_indices}` dict — O(N) single pass.

## Performance

🟡 `ground_truth.py:L215`: `iterrows()` Python loop builds `meta_by_id` dict. Use `df.set_index("id").to_dict(orient="index")` for vectorized construction.

🟡 `thesis.py:L103-110`: `_run_with_label_field` re-runs full embedding+eval for secondary label. Embeddings are label-agnostic — reuse cached NPZ files, only swap labels for the evaluator.

🟡 `pgvector.py:L310-317`: Per-query retrieval in Python for-loop. Batch all queries in a single `execute_values` SQL call for 10-50× speedup.

🟡 `cache.py:L27`: Cache key is `model_slug + dataset_name` without content hash. Dataset changes under same name silently return stale embeddings. Add a CSV checksum or `--no-cache` warning in docs.

## Maintenance

🟡 `thesis.py:L114-158` vs `L254-297`: `_evaluate_model` and `_evaluate_model_with_field` duplicate 90% of logic. Merge into single method with `label_field: str | None = None` parameter.

🟡 `thesis.py:L160-233` vs `L299-353`: Same for `_evaluate_fold` / `_evaluate_fold_with_field`. Refactor identically.

🟡 `pipeline.py:L277-286`: Dimension-to-table mapping is a hardcoded if/elif chain. Replace with dict lookup: `TABLE_BY_DIM = {512: "products_512", 768: "products_768", ...}`. Add default `products_{dim}` fallback.

🔵 `registry.py:29-35`: `MODELS` dict lists 5 models (fashion-clip, clip-b32, clip-l14, siglip, eva-clip). `__init__.py` `_LazyRegistry` lists 11. Stale registry — delete or sync.

🔵 `_constants.py:212-221`: `ResultCodes` includes HTTP codes (200, 400, 401, 403, 404, 409, 500) — unused by the benchmark CLI. Remove.

🔵 `pipeline.py:L228-238` + `thesis.py:L372-390`: `_measure_peak_ram` logic duplicated identically. Extract to `utils/timing.py` as benchmark function.

## Edge Cases

🔵 `ndcg.py:L11-27`: `dcg_at_k` uses `math.log2(rank + 1)` — a relevance gain of 1.0 always has weight 1/log2(2)=1 at rank 1. This is correct but non-standard; some IR literature uses `log2(rank)` directly. Document the `rank+1` choice.

🔵 `recall_comparison.py:L47-51`: Empty `exact_set` treated as perfect recall (1.0). Should produce NaN or 0.0 — perfect recall for an empty exact set inflates aggregate.

🔵 `cache.py:L99-126`: Legacy wrapper functions `save_embeddings`, `load_embeddings`, `is_cached` are thin aliases. Remove or add deprecation warnings.

## Tests Missing

🔴 `test_ground_truth.py:L53-74`: `test_generate_splits` doesn't verify `label_pattern` output when `pattern` column exists. Add test with `df["pattern"] = ["Solid", "Check", ...]`.

🔴 No tests for `retrieve_batch` or `top_k_indices` in `retrieval/cosine.py`.

## Docs

🔴 `infra/postgres/init.sql` and `infra/postgres/wait-for-pg.sh` referenced in docs but do not exist under benchmarks/. Create them or point to monorepo `infra/`.

🔴 CONCERNS.md [ASK USER] items — resolved ✅ (archive old/, keep outputs, skip checksums, verify RAM, model unloading GPU-only).

🔵 31 Docker references across docs — all should say Podman (primary) / Docker (fallback), not Docker (primary).
