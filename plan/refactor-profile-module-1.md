---
goal: Refactor Profile Module — Unify Admin/Store Contracts, Handlers Under Store, Shared Under Admin
version: 1.0
date_created: 2026-07-19
status: Planned
tags: refactor, module, profile, consolidation, admin, store
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Correct structural drift in the Profile module: eliminate duplicated Request/Response/Models/Validators/Mappings between Admin and Store, fix broken namespace references (`Store.Profiles.Shared` non-existent but referenced by 8 files), move all shared contracts under `Features/Admin/`, and make Admin endpoints reuse Store handlers via MediatR forwarding.

## 1. Requirements & Constraints

- **REQ-001**: Every shared model type (Request, Response, Parameters) must be defined once under `Features/Admin/<Entity>/Shared/Models/` and used by both Admin and Store features
- **REQ-002**: Every shared validator extension must be defined once under `Features/Admin/<Entity>/Shared/Validators/` and used by both Admin and Store features
- **REQ-003**: Every shared mapping extension must be defined once under `Features/Admin/<Entity>/Shared/Mappings/` and used by both Admin and Store features
- **REQ-004**: Admin Profile CRUD endpoints (Create/Update/Delete) must delegate to Store handler commands via MediatR `ISender` instead of maintaining separate handlers
- **REQ-005**: Admin Address CRUD endpoints (Create/Update/Delete) must delegate to Store handler commands via MediatR `ISender` instead of maintaining separate handlers
- **REQ-006**: Admin-only features (GetUserProfilesPagedOrAll, Admin Get Detail) keep Admin-specific handlers; they have no Store equivalent
- **REQ-007**: Store-only features (Wishlists, NotificationPreferences) are untouched — no Admin equivalent exists
- **CON-001**: `Features/Shared/ProfileFeature.cs` route/tag constants must not be edited (shared route definitions)
- **CON-002**: Domain entities, persistence, and seeders must not be altered
- **CON-003**: Namespace must match physical directory location per project convention
- **GUD-001**: Follow existing vertical-slice pattern (`static partial class`, split files per action)
- **PAT-001**: Apply the existing `IApplicationDbContext` + MediatR handler pattern
- **BUG-001**: `Store/Profiles/Shared/` directory does not exist on disk but 8 files reference `Module.Profile.Features.Store.Profiles.Shared.Models` and `Module.Profile.Features.Store.Profiles.Shared.Mappings` — these using statements are unresolvable and must be redirected to the Admin shared equivalents
- **BUG-002**: `GetUserProfile.cs`, `GetProfile.Response.cs`, `GetProfile.Endpoint.cs` are physically located under `Admin/Profiles/Get/Detail/` but declare namespace `Module.Profile.Features.Store.Profiles.Get.Detail` — directory/namespace mismatch

## 2. Implementation Steps

### Implementation Phase 1: Fix Broken Store/Profiles/Shared References

- GOAL-001: Redirect all 8 files that reference non-existent `Store.Profiles.Shared.*` to the existing `Admin.Profiles.Shared.*` equivalents, fixing build errors

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | `Features/Store/Profiles/Update/UpdateProfile.Request.cs` — change `using Module.Profile.Features.Store.Profiles.Shared.Models;` to `using Module.Profile.Features.Admin.Profiles.Shared.Models;` | | |
| TASK-002 | `Features/Store/Profiles/Update/UpdateProfile.Response.cs` — change `using Module.Profile.Features.Store.Profiles.Shared.Models;` to `using Module.Profile.Features.Admin.Profiles.Shared.Models;` | | |
| TASK-003 | `Features/Store/Profiles/Update/UpdateProfile.cs` — change `using Module.Profile.Features.Store.Profiles.Shared.Mappings;` to `using Module.Profile.Features.Admin.Profiles.Shared.Mappings;` | | |
| TASK-004 | `Features/Store/NotificationPreferences/Get/GetNotificationPreferences.Response.cs` — change using to `Admin.Profiles.Shared.Models;` (was `Store.Profiles.Shared.Models`) | | |
| TASK-005 | `Features/Store/NotificationPreferences/Update/UpdateNotificationPreferences.Request.cs` — change using to `Admin.Profiles.Shared.Models` | | |
| TASK-006 | `Features/Store/NotificationPreferences/Update/UpdateNotificationPreferences.Response.cs` — change using to `Admin.Profiles.Shared.Models` | | |
| TASK-007 | `Features/Admin/Profiles/Get/Detail/GetUserProfile.cs` — change using to `Admin.Profiles.Shared.Mappings` (was `Store.Profiles.Shared.Mappings`) | | |
| TASK-008 | `Features/Admin/Profiles/Get/Detail/GetProfile.Response.cs` — change using to `Admin.Profiles.Shared.Models` (was `Store.Profiles.Shared.Models`) | | |

Validation: Run `dotnet build service/Api/` — must succeed with zero warnings.

### Implementation Phase 2: Consolidate Address Shared Artifacts

- GOAL-002: Eliminate `Store/Addresses/Shared/` duplicate models/validators/mappings. Admin shared becomes canonical. Store features redirect to Admin shared.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Audit diff: compare `Store/Addresses/Shared/Models/Address.Parameters.cs` (no `UserId`) vs `Admin/Addresses/Shared/Models/Address.Parameters.cs` (has `UserId`). Keep Admin version as canonical — Store handler extracts `UserId` from `ICurrentUser` instead of body. | | |
| TASK-010 | Update all Store Address handlers to inject `ICurrentUser` and extract `UserId` from auth context (currently they may take `UserId` from command params). Changes needed in: `CreateAddress.cs`, `UpdateAddress.cs`, `DeleteAddress.cs`, `GetAddressById.cs` — verify each. | | |
| TASK-011 | Update `Store/Addresses/` feature files (`Create/Update/Delete/GetById/GetPaged`) — change all `using Module.Profile.Features.Store.Addresses.Shared.*` to `using Module.Profile.Features.Admin.Addresses.Shared.*` | | |
| TASK-012 | Audit Store Address handler `Request`/`Response` types — ensure they extend Admin shared types (e.g., `Admin.Addresses.Shared.Models.AddressRequest`/`AddressResponse`) | | |
| TASK-013 | Delete directory `Features/Store/Addresses/Shared/` (Models, Validators, Mappings subdirs) after confirming no remaining references | | |

Validation: `dotnet build service/Api/` succeeds. All Store Address endpoints still compile and use Admin shared types.

### Implementation Phase 3: Fix Namespace/Location Mismatch (GetProfile)

- GOAL-003: Move `GetProfile` files physically under `Store/Profiles/Get/Detail/` to match their declared namespace, completing the Store-side Profile detail feature

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Move `Features/Admin/Profiles/Get/Detail/GetUserProfile.cs` → `Features/Store/Profiles/Get/Detail/GetUserProfile.cs` (this is the handler, declared in Store namespace) | | |
| TASK-015 | Move `Features/Admin/Profiles/Get/Detail/GetProfile.Response.cs` → `Features/Store/Profiles/Get/Detail/GetProfile.Response.cs` | | |
| TASK-016 | Move `Features/Admin/Profiles/Get/Detail/GetProfile.Endpoint.cs` → `Features/Store/Profiles/Get/Detail/GetProfile.Endpoint.cs` | | |
| TASK-017 | Remove empty `Features/Admin/Profiles/Get/Detail/` directory | | |
| TASK-018 | Verify `ProfileFeature.cs` route for `Store.Profiles.Get.Route` is `api/store/profiles/profiles` — note duplicate `/profiles/profiles` in route path (document as pre-existing issue, do not change) | | |

Validation: `dotnet build service/Api/` succeeds. The `GetProfile` endpoint now lives at the physical path matching its namespace.

### Implementation Phase 4: Admin Profile Create — Reuse Store Handler

- GOAL-004: Admin `CreateUserProfile` endpoint sends `CreateProfile.Command` (Store handler) instead of `CreateUserProfile.Command` (Admin handler). Remove Admin's handler class.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | `Features/Admin/Profiles/Create/CreateUserProfile.Request.cs` — change to extend `Admin.Profiles.Shared.Models.ProfileRequest` (already does) — no-op, verify | | |
| TASK-020 | `Features/Admin/Profiles/Create/CreateUserProfile.Response.cs` — change to extend `Admin.Profiles.Shared.Models.ProfileDetailResponse` (already does) — no-op, verify | | |
| TASK-021 | `Features/Admin/Profiles/Create/CreateUserProfile.Endpoint.cs` — rewrite to receive `CreateProfile.Request` from body (Store namespace), construct `CreateProfile.Command(Guid userId, Request)` from route/query param `userId`, send via `ISender`, return `Result<CreateProfile.Response>` | | |
| TASK-022 | `Features/Admin/Profiles/Create/CreateUserProfile.Validator.cs` — rewrite to validate `CreateProfile.Request` via shared `ApplyProfileRules` or remove if Store validator covers it | | |
| TASK-023 | Delete `Features/Admin/Profiles/Create/CreateUserProfile.cs` (handler) — logic now lives in Store handler | | |

Validation: Admin POST `api/profiles/profiles` creates a profile via Store's `CreateProfile` handler. `dotnet build` succeeds.

### Implementation Phase 5: Admin Profile Update — Reuse Store Handler

- GOAL-005: Admin `UpdateUserProfile` endpoint sends `UpdateProfile.Command` (Store handler) instead of `UpdateUserProfile.Command`. Remove Admin's handler class.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | `Features/Admin/Profiles/Update/UpdateUserProfile.Endpoint.cs` — rewrite to receive `UpdateProfile.Request` from body, construct `UpdateProfile.Command(Guid userId, Request)` with userId from query param `userId`, send via `ISender` | | |
| TASK-025 | `Features/Admin/Profiles/Update/UpdateUserProfile.Validator.cs` — rewrite or remove; Store validator covers field rules | | |
| TASK-026 | Delete `Features/Admin/Profiles/Update/UpdateUserProfile.cs` (handler) | | |
| TASK-027 | `Features/Admin/Profiles/Update/UpdateUserProfile.Request.cs` — remove file if Admin no longer has its own Request type (store's is used directly) | | |
| TASK-028 | `Features/Admin/Profiles/Update/UpdateUserProfile.Response.cs` — remove file if Admin no longer has its own Response type | | |

Validation: Admin PUT `api/profiles/profiles` updates a profile via Store's `UpdateProfile` handler. `dotnet build` succeeds.

### Implementation Phase 6: Admin Profile Delete — Reuse Store Handler

- GOAL-006: Admin `DeleteUserProfile` endpoint sends `DeleteProfile.Command` (Store handler) instead of `DeleteUserProfile.Command`. Remove Admin's handler class.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | `Features/Admin/Profiles/Delete/DeleteUserProfile.Endpoint.cs` — rewrite to construct `DeleteProfile.Command(userId)` from query param, send via `ISender` | | |
| TASK-030 | Delete `Features/Admin/Profiles/Delete/DeleteUserProfile.cs` (handler) | | |

Validation: Admin DELETE `api/profiles/profiles?userId=X` deactivates profile via Store's `DeleteProfile` handler. `dotnet build` succeeds.

### Implementation Phase 7: Admin Address CRUD — Reuse Store Handlers

- GOAL-007: Admin Address endpoints delegate to Store address handlers. Remove Admin-specific address handler files.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-031 | `Features/Admin/Addresses/Create/CreateUserAddress.Endpoint.cs` — rewrite to receive Store's `CreateAddress.Request` (or Admin shared model), add `userId` from query/body, create `CreateAddress.Command(Guid userId, Request)`, send via `ISender` | | |
| TASK-032 | Delete `Features/Admin/Addresses/Create/CreateUserAddress.cs` (handler) | | |
| TASK-033 | `Features/Admin/Addresses/Create/CreateUserAddress.Validator.cs` — remove or redirect | | |
| TASK-034 | `Features/Admin/Addresses/Update/UpdateUserAddress.Endpoint.cs` — rewrite to delegate to Store `UpdateAddress.Command` | | |
| TASK-035 | Delete `Features/Admin/Addresses/Update/UpdateUserAddress.cs` (handler) | | |
| TASK-036 | `Features/Admin/Addresses/Update/UpdateUserAddress.Validator.cs` — remove or redirect | | |
| TASK-037 | `Features/Admin/Addresses/Delete/DeleteUserAddress.Endpoint.cs` — rewrite to delegate to Store `DeleteAddress.Command` | | |
| TASK-038 | Delete `Features/Admin/Addresses/Delete/DeleteUserAddress.cs` (handler) | | |
| TASK-039 | `Features/Admin/Addresses/Delete/DeleteUserAddress.Response.cs` — remove if not needed by delegated endpoint | | |
| TASK-040 | `Features/Admin/Addresses/Get/ById/GetUserAddressById.Endpoint.cs` — rewrite to delegate to Store `GetAddressById.Query` | | |
| TASK-041 | Delete `Features/Admin/Addresses/Get/ById/GetUserAddressById.cs` (handler) | | |
| TASK-042 | `Features/Admin/Addresses/Get/ById/GetUserAddressById.Validator.cs` — remove or redirect | | |
| TASK-043 | `Features/Admin/Addresses/Get/All/GetAllAddresses.Endpoint.cs` — rewrite to delegate to Store `GetAddresses.Query` (paged) with admin-level access (no ICurrentUser filter) | | |
| TASK-044 | Delete `Features/Admin/Addresses/Get/All/GetAllAddresses.cs` (handler) | | |

Validation: All Admin Address endpoints work via Store handlers. `dotnet build` succeeds.

### Implementation Phase 8: Clean Up Redundant Admin Request/Response/Validator Files

- GOAL-008: Remove Admin feature files that are no longer needed after handler delegation (their Request/Response/Validator are replaced by Store equivalents or Admin shared models)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-045 | Delete `Features/Admin/Profiles/Create/CreateUserProfile.Request.cs` (Admin endpoint now uses `CreateProfile.Request` from Store) | | |
| TASK-046 | Delete `Features/Admin/Profiles/Create/CreateUserProfile.Response.cs` (uses `CreateProfile.Response` from Store) | | |
| TASK-047 | Delete `Features/Admin/Profiles/Create/CreateUserProfile.Validator.cs` (validation handled by Store's validator) | | |
| TASK-048 | Delete `Features/Admin/Profiles/Update/UpdateUserProfile.Request.cs` (Admin endpoint uses `UpdateProfile.Request` from Store) | | |
| TASK-049 | Delete `Features/Admin/Profiles/Update/UpdateUserProfile.Response.cs` (Admin endpoint uses `UpdateProfile.Response` from Store) | | |
| TASK-050 | Delete `Features/Admin/Profiles/Update/UpdateUserProfile.Validator.cs` (validation handled by Store) | | |
| TASK-051 | Delete `Features/Admin/Addresses/Create/CreateUserAddress.Request.cs` | | |
| TASK-052 | Delete `Features/Admin/Addresses/Create/CreateUserAddress.Response.cs` | | |
| TASK-053 | Delete `Features/Admin/Addresses/Create/CreateUserAddress.Validator.cs` | | |
| TASK-054 | Delete `Features/Admin/Addresses/Update/UpdateUserAddress.Request.cs` | | |
| TASK-055 | Delete `Features/Admin/Addresses/Update/UpdateUserAddress.Response.cs` | | |
| TASK-056 | Delete `Features/Admin/Addresses/Update/UpdateUserAddress.Validator.cs` | | |
| TASK-057 | Delete `Features/Admin/Addresses/Get/ById/GetUserAddressById.Request.cs` (if exists — check) | | |
| TASK-058 | Delete `Features/Admin/Addresses/Get/ById/GetUserAddressById.Response.cs` | | |
| TASK-059 | Delete `Features/Admin/Addresses/Get/ById/GetUserAddressById.Validator.cs` | | |
| TASK-060 | Delete `Features/Admin/Addresses/Get/All/GetAllAddresses.Response.cs` | | |
| TASK-061 | Delete `Features/Admin/Addresses/Delete/DeleteUserAddress.Response.cs` | | |

Validation: `dotnet build` succeeds. No "unused" file warnings.

### Implementation Phase 9: Update README.yaml and Verify Build

- GOAL-009: Remove stale README.yaml references to non-existent files, run full build, verify zero warnings.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-062 | `README.yaml` — remove references to `Store/Profiles/Shared/Mappings/Profile.Mapping.Domain.cs`, `Profile.Mapping.Model.cs`, `Store/Profiles/Shared/Validators/Profile.Validator.cs` (lines 231–255) | | |
| TASK-063 | Run `dotnet build service/Api/` — verify zero warnings (TreatWarningsAsErrors) | | |
| TASK-064 | Run `dotnet test service/Api/tests/Module.UnitTests` — verify all Profile tests pass | | |
| TASK-065 | Run `dotnet test --filter "FullyQualifiedName~Profile"` — verify integration tests pass | | |

Validation: `dotnet build` zero warnings. `dotnet test` all Profile-related tests pass.

## 3. Alternatives

- **ALT-001** (Move shared under `Features/Shared/`): Central shared directory at `Features/Shared/Models/`, `Features/Shared/Validators/`, etc. Rejected because user explicitly requested shared artifacts stay under Admin side for organizational clarity and because Admin already has the canonical copy.
- **ALT-002** (Keep Admin handlers, have them call Store handlers internally): Would keep redundant handler files. Rejected because it adds indirection without benefit — Admin endpoints can directly dispatch Store commands via MediatR.
- **ALT-003** (Create Store/Profiles/Shared/ with redirect types): Would add empty wrapper types that extend Admin types. Rejected because it adds unnecessary files and indirection.

## 4. Dependencies

- **DEP-001**: `Module.Profile` assembly — all changes are within this single module
- **DEP-002**: No external NuGet or framework dependencies changed

## 5. Files

- **FILE-001** to **FILE-030**: All files under `Features/Admin/Profiles/`, `Features/Admin/Addresses/`, `Features/Store/Profiles/`, `Features/Store/Addresses/`, `Features/Store/NotificationPreferences/` as enumerated in tasks above
- **FILE-031**: `Features/Shared/ProfileFeature.cs` — read-only (no changes)
- **FILE-032**: `README.yaml` — update stale path references
- **FILE-033**: All domain/persistence files — untouched

## 6. Testing

- **TEST-001**: Build succeeds with zero warnings (`dotnet build service/Api/`)
- **TEST-002**: Existing Profile unit tests pass (`dotnet test service/Api/tests/Module.UnitTests --filter "FullyQualifiedName~Profile"`)
- **TEST-003**: Existing Profile integration tests pass (`dotnet test --filter "FullyQualifiedName~Profile"`)
- **TEST-004**: Manual HTTP test: Admin Create → reuses Store handler → profile created
- **TEST-005**: Manual HTTP test: Admin Update → reuses Store handler → profile updated
- **TEST-006**: Manual HTTP test: Admin Delete → reuses Store handler → profile deactivated
- **TEST-007**: Manual HTTP test: Admin Get Paged → works via admin-specific handler
- **TEST-008**: Manual HTTP test: Store Create → unchanged behavior
- **TEST-009**: Manual HTTP test: Store Update → unchanged behavior
- **TEST-010**: Manual HTTP test: Store Delete → unchanged behavior

## 7. Risks & Assumptions

- **RISK-001**: Admin endpoints currently accept `userId` in request body; Store handlers expect `userId` as command parameter. Endpoint rewrites must correctly extract `userId` from query/route params and pass to Store's `Command(Guid UserId, ...)`.
- **RISK-002**: If Store handlers check `ICurrentUser` and reject non-own-user access, Admin delegation will fail for admin-on-behalf-of operations. This may require adding an `isAdmin` bypass flag to Store commands.
- **ASSUMPTION-001**: Admin feature directory structure remains (empty feature directories like `Create/` may be removed if all files within are deleted).
- **ASSUMPTION-002**: Store handlers are complete and working (Create, Update, Delete for Profiles and Addresses).
- **ASSUMPTION-003**: The `Store/Profiles/Shared/` directory truly does not exist and was never created — its referenced types never existed, and the current codebase fails to build.

## 8. Related Specifications / Further Reading

- `docs/codebase/CONVENTIONS.md` — coding conventions
- `docs/codebase/ARCHITECTURE.md` — architecture reference
- `.harness/domains.yml` — Profile domain boundary
- `service/Api/src/Module/Profile/README.yaml` — current module documentation
