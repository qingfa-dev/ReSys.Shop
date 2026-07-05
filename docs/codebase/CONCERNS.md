# Codebase Concerns

## Core Sections (Required)

### 1) Top Risks (Prioritized)

| Severity | Concern | Evidence | Impact | Suggested action |
|----------|---------|----------|--------|------------------|
| High | Embedding service cannot start: `src/main.py` imports modules that don't exist (`config.settings`, `routers/embedding_router`) | `service/Embedding/src/main.py:4-6` — imports of non-existent modules | ML sidecar is non-functional despite spec being complete | Create missing modules (`config/`, `routers/embedding_router.py`) or fix imports |
| High | No Dockerfiles / containerization anywhere | `glob Dockerfile*` returns empty; scan shows no container configs | Cannot deploy to production without investing in container pipeline | Create Dockerfiles for API, Embedding, Admin, Store |
| Medium | Admin SPA is a scaffold with no real features | `app/Admin/src/router/index.ts` — empty routes; `App.vue` is placeholder; only `counter.ts` store | Admin panel does not exist | Build admin feature parity with Store SPA |
| Medium | Monolith scaling risk | Single `Api/` process with all 4 modules | Traffic spikes in Catalog affect Identity/Profile/Location | Consider module extraction to separate services when load demands |
| Medium | `.env` files tracked for frontend but not for backend | `app/Admin/.env.development` + `app/Store/.env.development` tracked; no backend `.env.example` | Missing documented connection string / JWT secret contract for new devs | Create `appsettings.Template.json` or `.env.example` for the API |

### 2) Technical Debt

| Debt item | Why it exists | Where | Risk if ignored | Suggested fix |
|-----------|---------------|-------|-----------------|---------------|
| Embedding service `build/lib/` duplicates source | `setuptools` build artifact committed to repo | `service/Embedding/build/lib/` (72 files) | Stale build artifacts confuse developers | Add `build/` to `.gitignore` and remove tracked files |
| Embedding missing packages vs pyproject.toml list | `pyproject.toml` declares 16 packages but only 10 directories exist on disk | `service/Embedding/src/` vs `pyproject.toml:23-41` | Import errors at runtime | Sync package declarations to actual directory structure |
| Aspire Python resource uses outdated API | `AddUvicornApp` requires `WithHttpHealthCheck` which may be deprecated in Aspire 13.4 | `infra/Aspire/src/ReSys.AppHost/AppHost.cs:20-26` | AppHost may fail to start the Embedding service | Verify API compatibility with Aspire 13.4.6 |
| Guide code-commenting conventions are aspirational | Full XML-based commenting rules exist but are not enforced by CI | `guide/code-commenting/CommentingRules.xml` (63KB) | Conventions may drift from actual code | Integrate rules into `.editorconfig` or a Roslyn analyzer |
| .http test files require manual data seeding | Test environment setup is not automated | `ApiTests/README.md` steps 2-4 | Tests fail on first run without manual setup | Create seed-data init script or seed via ApiTests |

### 3) Security Concerns

| Risk | OWASP category (if applicable) | Evidence | Current mitigation | Gap |
|------|--------------------------------|----------|--------------------|-----|
| Development JWT secret committed | A02:2021 (Cryptographic Failures) | `appsettings.Development.json:13` — `"Secret": "ThisIsADevelopmentJwtSecretKey..."` | Marked as development-only config | No warnings or guards preventing accidental production use |
| No environment variable file for production secrets | A05:2021 (Security Misconfiguration) | No `.env.production` or secure config template found | Aspire User Secrets used for local dev | Production deployment config path is undefined |
| Storage encryption key is empty in config | A02:2021 (Cryptographic Failures) | `appsettings.json:108` — `"EncryptionKey": ""` | Encryption is disabled by default (empty = no encryption) | No validation that encryption is enabled in production |
| Malware scanner disabled by default | A08:2021 (Software and Data Integrity Failures) | `appsettings.json:114-118` — ClamAV disabled, no fallback scanner | Scanner can be enabled in config | No runtime warning when scanner is off |
| Anti-forgery enabled with no frontend integration | A01:2021 (Broken Access Control) | `appsettings.json:42-50` — AntiForgery enabled but Admin/Store apps have no CSRF token handling | Middleware will reject state-changing requests | Frontend apps need CSRF token fetch + header injection |

### 4) Performance and Scaling Concerns

| Concern | Evidence | Current symptom | Scaling risk | Suggested improvement |
|---------|----------|-----------------|-------------|-----------------------|
| Image storage via local filesystem | `appsettings.json:121-124` — Local provider at `./uploads` | Works for single-instance dev | Not shareable across instances; data loss on restart | Enable S3 or Azure Blob for multi-instance deployments |
| EF Core N+1 queries possible in complex queries | Specification pattern uses expression trees that may not include `.Include()` | `Shared/Operational/Persistence/Specifications/` | Page load latency at scale | Audit feature queries for include patterns |
| Hangfire in-memory by default | `Directory.Packages.props:78` — `Hangfire.InMemory` included | Jobs not persisted across restarts | Job loss on restarts; no visibility | Default to Redis-backed Hangfire in production |
| Embedding service loads model once but is stateless | `README.yaml` — P1 "Single Model, No Cold Start" | Model loaded at startup, OK for single instance | Not horizontally scalable without sharing model state | Consider model-serving architecture (e.g., ONNX, Triton) |

### 5) Fragile/High-Churn Areas

| Area | Why fragile | Churn signal | Safe change strategy |
|------|-------------|-------------|----------------------|
| `infra/Aspire/src/ReSys.AppHost/AppHost.cs` | Orchestrates all services; changes in any port/endpoint/resource cascade | 7 commits in last 90 days (highest-churn file) | Test AppHost startup via `Aspire.Hosting.Testing`; coordinate changes with service owners |
| `service/Api/src/Api/Program.cs` | DI composition root; every new module/feature adds registration | 6 commits in 90 days | Keep extension methods modular (per-module `AddXxxModule()`); minimize changes to Program.cs itself |
| `service/Api/src/Shared/Operational/Storages/Storage.Extensions.cs` | Storage abstraction registration touches multiple providers | 4 commits in 90 days | Test each provider variant independently; use integration tests |
| `service/Api/src/Migrations/` | Schema changes affect all modules; migrations are irreversible | Snapshot + 4 migration files in ~2 weeks | Use non-destructive migrations; test rollback scripts |
| `ReSys.Shop.slnx` | Solution file changes when projects are added/removed | 5 commits in 90 days | Ensure CI validates solution build after changes |

### 6) `[ASK USER]` Questions

1. [ASK USER] What is the intended deployment target (bare-metal VMs, Kubernetes, cloud PaaS)? This affects whether Dockerfiles or Helm charts should be created.
2. [ASK USER] Is the Embedding service intended to be functional now, or is it still in design phase despite README.yaml claiming "stable" status?
3. [ASK USER] What is the team's coverage threshold/expectation? coverlet is configured but opt-in; no enforcement is configured.
4. [ASK USER] Should the Admin SPA be prioritized to match the Store SPA's feature set? The Admin app is currently a scaffold with no real routes or views.
5. [ASK USER] Are there plans to add CI/CD pipelines (GitHub Actions, GitLab CI)? Scan detected none.
6. [ASK USER] Is the Stripe integration actively being used, or is this a planned dependency? It's in `Directory.Packages.props` but no integration code was detected.

### 7) Evidence

- Scan output: `docs/codebase/.codebase-scan.txt` — churn data, CI/CD absence, TODO/FIXME presence
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — orchestration fragility
- `service/Embedding/src/main.py` — broken imports
- `service/Api/src/Api/appsettings.Development.json` — hardcoded dev secrets
- `service/Api/src/Api/appsettings.json` — empty encryption key, disabled malware scanner
- `app/Admin/src/router/index.ts` — empty routes (scaffold state)
- Git log: `docs/codebase/.codebase-scan.txt:263-283`
