---
goal: Configure Explicit Ports, Vite Proxy, and Aspire AppHost Integration for Admin + Store Frontends
version: 1.0
date_created: 2026-07-01
status: 'Completed'
tags: infrastructure, aspire, frontend, integration, ports
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Configure explicit dev ports for both frontend SPAs (Admin: 5173, Store: 5174), add Vite proxy rules for `/api` requests to the backend API (localhost:5035), and wire both apps into the .NET Aspire AppHost orchestrator using `AddViteApp()` with service-discovery references to the API project. This enables unified `aspire run` that starts Postgres, Redis, the API, and both frontends simultaneously.

## 1. Requirements & Constraints

- **REQ-001**: Admin Vite dev server must listen on port 5173 (explicit, not Vite default).
- **REQ-002**: Store Vite dev server must listen on port 5174 (explicit, different from Admin).
- **REQ-003**: Both Vite dev servers must proxy `/api/*` requests to `http://localhost:5035` (the API standalone port) when running outside Aspire.
- **REQ-004**: Aspire AppHost must launch both Admin and Store as `AddViteApp()` resources alongside the existing Postgres, Redis, and API project.
- **REQ-005**: Both frontend resources must receive the API endpoint via `.WithReference(api)` for Aspire-managed service discovery.
- **REQ-006**: Both frontend resources must run `.WithNpmPackageInstallation()` to install dependencies before dev.
- **CON-001**: No breaking changes to existing `AppHost.cs` orchestration (Postgres, Redis, API must remain unchanged).
- **CON-002**: Vite proxy must only apply in dev mode (`server.proxy` is ignored during `vite build`).
- **CON-003**: The `packageManager` field in `package.json` (pnpm) must be respected by Aspire's `AddViteApp()` (auto-detected from `pnpm-lock.yaml`).
- **PAT-001**: Use the Constants defined in `ReSys.ServiceDefaults.Constants.Application` for the resource names (`Application.Admin`, `Application.Store`).

## 2. Implementation Steps

### Implementation Phase 1: Configure Vite Ports and Proxy

- GOAL-001: Add explicit `server.port` and `server.proxy` to both Vite configs.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Update `app/Admin/vite.config.ts`: add `server: { port: 5173, proxy: { '/api': { target: 'http://localhost:5035', changeOrigin: true } } }` to the `defineConfig()` object | | |
| TASK-002 | Update `app/Store/vite.config.ts`: add `server: { port: 5174, proxy: { '/api': { target: 'http://localhost:5035', changeOrigin: true } } }` to the `defineConfig()` object | | |

### Implementation Phase 2: Wire Frontends into Aspire AppHost

- GOAL-002: Add `AddViteApp()` calls for Admin and Store to `AppHost.cs` and wire them to the API project.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-003 | Update `infra/Aspire/src/ReSys.AppHost/AppHost.cs`: add `IResourceBuilder<ViteAppResource> admin = builder.AddViteApp(Application.Admin, "../../../app/Admin").WithNpmPackageInstallation().WithHttpEndpoint(targetPort: 5173).WithReference(api).WaitFor(api);` | | |
| TASK-004 | Add `IResourceBuilder<ViteAppResource> store = builder.AddViteApp(Application.Store, "../../../app/Store").WithNpmPackageInstallation().WithHttpEndpoint(targetPort: 5174).WithReference(api).WaitFor(api);` | | |

## 3. Alternatives

- **ALT-001**: Use `AddJavaScriptApp` instead of `AddViteApp`. Rejected — `AddViteApp` specifically configures the Vite dev server with the correct `--port` and `--host` flags.
- **ALT-002**: Skip Vite proxy and require the frontend code to always read the API URL from `process.env`. Rejected — the proxy provides a simpler dev experience when running `pnpm dev` standalone.
- **ALT-003**: Hardcode `localhost:5035` in the frontend API client code. Rejected — coupling frontend code to a specific port prevents Aspire from managing the API URL dynamically.

## 4. Dependencies

- **DEP-001**: `Aspire.Hosting.JavaScript` (version 13.4.6) — already referenced in AppHost csproj; provides `AddViteApp()`.
- **DEP-002**: `pnpm` (>= 9) — required on the dev machine for both Admin and Store projects; Aspire detects it from `pnpm-lock.yaml`.
- **DEP-003**: Node.js (>= 20) — required for Vite dev servers.

## 5. Files

- **FILE-001**: `app/Admin/vite.config.ts` — add `server.port` and `server.proxy`.
- **FILE-002**: `app/Store/vite.config.ts` — add `server.port` and `server.proxy`.
- **FILE-003**: `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — add `AddViteApp()` resources for Admin and Store.

## 6. Testing

- **TEST-001**: Run `pnpm dev` in `app/Admin` — Vite starts on port 5173; `curl http://localhost:5173/api/health` proxies to API.
- **TEST-002**: Run `pnpm dev` in `app/Store` — Vite starts on port 5174; `curl http://localhost:5174/api/health` proxies to API.
- **TEST-003**: Run `dotnet run --project infra/Aspire/src/ReSys.AppHost` — Aspire dashboard shows 5 resources (Postgres, Redis, API, Admin, Store) all healthy.
- **TEST-004**: Verify that `using ReSys.ServiceDefaults.Constants;` is present in `AppHost.cs` (already is from existing code).

## 7. Risks & Assumptions

- **RISK-001**: `AddViteApp` may not detect pnpm correctly if it only checks for `package-lock.json`. Mitigation: test with `aspire run`; if pnpm is not auto-detected, the Aspire.Hosting.JavaScript package may need a workaround (fallback to `AddJavaScriptApp` with a custom script command).
- **RISK-002**: The relative path `../../../app/Admin` from the AppHost project directory may be incorrect if the AppHost is run from a different working directory. Mitigation: Aspire resolves paths relative to the AppHost project directory (`ReSys.AppHost.csproj` location), not the current working directory.
- **ASSUMPTION-001**: Ports 5173 and 5174 are available on the dev machine.
- **ASSUMPTION-002**: The API project at `service/Api/src/Api/` exposes an OpenAPI-compatible endpoint at `/api/health` or similar for proxy verification.

## 8. Related Specifications / Further Reading

- `plan/refactor-scss-architecture-3.md` — Previous plan for sakai-assets integration.
- `infra/Aspire/src/ReSys.AppHost/AppHost.cs` — Current orchestration code (to be extended).
- `infra/Aspire/src/ReSys.ServiceDefaults/Constants/Apps.cs` — Defines `Application.Admin` and `Application.Store`.
- https://learn.microsoft.com/en-us/dotnet/aspire/frontend-node/overview — Aspire JavaScript/Node.js integration docs.
- https://vite.dev/config/server-options.html — Vite server options (port, proxy).
