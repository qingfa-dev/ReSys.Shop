---
goal: Refactor Embedding service app structure and simplify pyproject.toml
version: 1.0
date_created: 2026-07-02
owner: Platform Team
status: Planned
tags: refactor, python, fastapi, embedding
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Refactor the Embedding service's application creation into a factory pattern and simplify the `pyproject.toml` package discovery. Currently, `main.py` creates the FastAPI app at module level with inline CORS and router setup, and `pyproject.toml` manually lists every subpackage. This change extracts app creation into `app.py`, uses `find:` for automatic package discovery, and updates tests accordingly.

## 1. Requirements & Constraints

- **REQ-001**: Extract FastAPI app creation into a `create_app()` factory function in `src/app.py`
- **REQ-002**: `src/main.py` must remain the entrypoint and delegate to `create_app()`
- **REQ-003**: `pyproject.toml` must use `find:` directive instead of manual package enumeration
- **REQ-004**: All existing tests must continue to pass without modification to test logic
- **REQ-005**: The `embedding` package namespace must remain unchanged
- **CON-001**: The module `embedding.main` is currently imported in tests (`conftest.py`) — must update import target
- **CON-002**: Uvicorn entrypoint must remain `embedding.main:app` (Apsire AppHost convention) — configurable via `AppConfig` in AppHost

## 2. Implementation Steps

### Implementation Phase 1

- GOAL-001: Create `app.py` with factory function and simplify `main.py`

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `src/app.py` with `create_app(settings: Settings \| None = None) -> FastAPI` factory, moving app creation, CORS, exception handlers, and router registration out of `main.py` | | |
| TASK-002 | Simplify `src/main.py` to import and call `create_app()` at module level | | |
| TASK-003 | Update `pyproject.toml` to replace manual `packages` list with `[tool.setuptools.packages.find] where = ["src"]` and remove `[tool.setuptools.package-dir]` | | |

### Implementation Phase 2

- GOAL-002: Update tests to use the new factory and verify correctness

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | Update `tests/conftest.py` to import `app` from `embedding.app` instead of `embedding.main` | | |
| TASK-005 | Run full test suite with `uv run pytest` to verify all tests pass | | |
| TASK-006 | Run `uv sync` to verify the build completes successfully | | |

## 3. Alternatives

- **ALT-001**: Keep `main.py` as-is and inline all logic there — rejected because it prevents factory-based test patterns and cleaner DI
- **ALT-002**: Use a `lazy_app()` pattern instead of module-level `app` — rejected because it would break Uvicorn's `module:app` convention

## 4. Dependencies

- **DEP-001**: FastAPI `APIRouter` and `FastAPI` class — already present
- **DEP-002**: `pydantic-settings` for `Settings` — already present
- **DEP-003**: `setuptools` `find:` directive — built-in, no additional dependency

## 5. Files

- **FILE-001**: `service/Embedding/src/main.py` — simplify to factory delegation
- **FILE-002**: `service/Embedding/src/app.py` — new file with `create_app()` factory
- **FILE-003**: `service/Embedding/pyproject.toml` — switch to `find:` directive
- **FILE-004**: `service/Embedding/tests/conftest.py` — update import path

## 6. Testing

- **TEST-001**: Run `uv run pytest` in `service/Embedding/` — all existing tests must pass
- **TEST-002**: Run `uv sync` in `service/Embedding/` — build must succeed
- **TEST-003**: Verify `uv run uvicorn embedding.main:app` starts without error

## 7. Risks & Assumptions

- **ASSUMPTION-001**: Aspire AppHost references `embedding.main:app` as the Uvicorn entrypoint — moving app creation to `app.py` but keeping `app` in `main.py` preserves this
- **ASSUMPTION-002**: All subpackages under `src/` follow the `embedding.*` naming convention — `find:` will discover them automatically
- **RISK-001**: If any non-package directory exists under `src/`, `find:` might pick it up — mitigated by checking `src/` structure (all entries are valid Python packages)

## 8. Related Specifications / Further Reading

- [FastAPI Application Factory Pattern](https://fastapi.tiangolo.com/advanced/testing/#using-the-dependency-override-function-in-tests)
- [setuptools find directive documentation](https://setuptools.pypa.io/en/latest/userguide/package_discovery.html#using-find-find_packages)
