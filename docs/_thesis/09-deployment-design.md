# Chapter 9 — Deployment Design

## 9.1 Deployment Architecture

### 9.1.1 Local Development (Aspire Orchestration)

Aspire provides a single-command local development environment:

```
┌─────────────────────────────────────────────────────────────┐
│                     Aspire AppHost                           │
│  ┌──────────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐│
│  │ PostgreSQL   │  │  Redis   │  │  API     │  │ Embedding││
│  │ pgvector:17  │  │  7-alpine│  │  (.NET)  │  │ (Python) ││
│  │ Port: 5432   │  │ Port:6379│  │ Port:5035│  │ Port:8000││
│  └──────────────┘  └──────────┘  └────┬─────┘  └────┬─────┘│
│                                       │              │      │
│  ┌──────────────┐  ┌──────────┐      │              │      │
│  │ Store SPA    │  │ Admin SPA│      │              │      │
│  │ Vite 5174    │  │ Vite 5173│◄─────┴──────────────┘      │
│  └──────────────┘  └──────────┘                             │
└─────────────────────────────────────────────────────────────┘
```

**Evidence**: `infra/Aspire/src/ReSys.AppHost/AppHost.cs:9-49`

### 9.1.2 Service Defaults

All Aspire-managed services share `ReSys.ServiceDefaults` which registers:
- OpenTelemetry (traces, metrics, logs)
- Health checks (`/health`, `/alive`)
- Service discovery
- HTTP client resilience (Polly pipeline)

**Evidence**: `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:19-132`

### 9.1.3 Production Deployment (Conceptual)

Since Dockerfiles and CI/CD are explicitly out of scope (deferred), the production deployment design is conceptual:

```
┌──────────────────────────────────────────────────────────────┐
│                        Cloud Environment                      │
│                                                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────┐│
│  │ Load Balancer    │  │ API Container(s)│  │ SPA Static   ││
│  │ (YARP — deferred)│  │ (.NET 10)       │  │ Hosting      ││
│  └────────┬────────┘  └────────┬────────┘  │ (CDN)        ││
│           │                    │            └─────────────┘│
│           └────────────────────┘                           │
│                              │                              │
│  ┌─────────────────┐  ┌─────┴──────┐  ┌─────────────────┐  │
│  │ PostgreSQL 17   │  │ Redis 7    │  │ Python Embedding ││
│  │ (managed/cloud) │  │ (cache/jobs)│  │ Container        ││
│  └─────────────────┘  └────────────┘  └─────────────────┘  │
│                                                              │
│  External: Stripe API, SendGrid API, S3 Bucket               │
└──────────────────────────────────────────────────────────────┘
```

**Design rationale**: A modular monolith is naturally containerizable as a single API container. The frontends are static SPA bundles served from a CDN or object storage. The embedding sidecar is a separate container because Python/.NET runtimes don't share a process. Redis and PostgreSQL are best managed by cloud providers in production.

## 9.2 Configuration per Environment

| Source | Development | Testing | Production |
|--------|-------------|---------|------------|
| `appsettings.json` | Base | Base | Base |
| `appsettings.Development.json` | Override | Ignored | Ignored |
| `appsettings.Testing.json` | Ignored | Override | Ignored |
| `dotnet user-secrets` | Secrets | Ignored | Ignored |
| Environment variables | Optional | Injected by test factory | Primary secret source |

**Evidence**: `service/Api/src/Api/appsettings.json`, `appsettings.Development.json`, `appsettings.Testing.json`, `.env.template`

## 9.3 Health Checks

Three health checks are registered:

| Check | Endpoint | Purpose |
|-------|----------|---------|
| `self` | `/alive` | Liveness — process is running |
| `npgsql` | `/health` | Readiness — database connectivity |
| `redis` | `/health` | Readiness — cache connectivity |
| `database_initialization` | `/health` | Readiness — migrations have run |

**Note**: `MapDefaultEndpoints` only exposes these in non-production environments (`Extensions.cs:114-131`). In production, health checks should be exposed on a separate port or via a sidecar.

**Evidence**: `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:114-131`

## 9.4 Evidence

- `infra/Aspire/src/ReSys.AppHost/AppHost.cs:1-49` — Aspire orchestration
- `infra/Aspire/src/ReSys.ServiceDefaults/Extensions.cs:1-132` — service defaults
- `service/Api/src/Api/appsettings.json:1-237` — environment config hierarchy
- `service/Api/src/Api/.env.template:1-33` — environment variable documentation

---

## [ASK USER] Items

17. Should this chapter include a formal deployment diagram (e.g., using cloud vendor icons), or is the conceptual block diagram sufficient?
18. Is there a specific cloud platform (AWS, Azure, GCP) the examiner expects the deployment design to target?
