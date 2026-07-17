# Phase 9 — Admin SPA Profile & Reports Type/Schema Restructure

## Files Created

### Profile
- `app/Admin/src/features/profile/schemas/Profile.Schema.ts` — Zod schema (firstName, lastName, phoneNumber, dateOfBirth, gender, bio, avatarUrl, acceptsEmailMarketing)
- `app/Admin/src/features/profile/types/Profile.Parameters.Type.ts` — re-exports `ProfileParameters` from schema
- `app/Admin/src/features/profile/types/Profile.Request.Type.ts` — `ProfileUpdateRequest` extends parameters + preferences/notifications
- `app/Admin/src/features/profile/types/Profile.Response.Type.ts` — `Profile`, `ProfilePreferences`, `NotificationPreferences`
- `app/Admin/src/features/profile/types/Profile.Query.Type.ts` — `ProfileQuery = ServerQueryingParameters`

### Reports
- `app/Admin/src/features/reports/schemas/Report.Schema.ts` — `DashboardQuerySchema` with optional from/to
- `app/Admin/src/features/reports/types/Report.Response.Type.ts` — `SalesSummary`, `InventorySummary`, `CatalogSummary`, `ActivityItem`, `RecentActivityResponse`
- `app/Admin/src/features/reports/types/Report.Query.Type.ts` — `DashboardQuery extends ServerQueryingParameters`

## Files Deleted
- `profile/types/profile.domain.types.ts`
- `profile/types/profile.request.types.ts`
- `reports/types/report.domain.types.ts`
- `reports/types/report.request.types.ts`

## Consumers Updated
- **profile.service.ts** — imports `Profile.Response.Type`, `Profile.Request.Type`
- **profile.store.ts** — imports `Profile.Response.Type`, lazy import `Profile.Request.Type`
- **profile.mapper.ts** — imports `Profile.Response.Type`
- **profile.repository.ts** — imports `Profile.Response.Type`, `Profile.Request.Type`
- **report.store.ts** — imports `Report.Response.Type`
- **report.service.ts** — imports `SalesSummary`, `InventorySummary`, `CatalogSummary` from `Report.Response.Type`

## Verification
- `pnpm run type-check` — only pre-existing errors in catalog feature (unrelated)
- `pnpm run test:unit` — 22/27 files pass, 38 pre-existing test failures (none in profile/reports)
