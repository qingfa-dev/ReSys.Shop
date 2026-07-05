# Admin SPA Vertical-Slice (Feature-Folder) Architecture

**Date:** 2026-07-06
**Status:** Approved
**Scope:** `app/Admin/` (Vue 3 + TypeScript + Vite + PrimeVue v4 + Tailwind v4)

## Problem

The Admin SPA (`app/Admin/`) is mostly scaffold:

- `App.vue` is the Vue 3 "You did it!" placeholder
- `router/index.ts` has empty routes
- `stores/counter.ts` is a placeholder
- Only a generic `api.ts` HTTP client exists
- `shared/api/envelop/` directory exists but is empty

There is no architectural pattern in place. We need one **before** building real features (Identity, Catalog, Location, Profile CRUD + auth + dashboard) so the codebase stays navigable and code-splits cleanly as features grow.

The Store SPA (`app/Store/`) is also scaffold-like (placeholder views + a couple of stores) and is **out of scope** for this spec.

## Goals

1. Establish a pragmatic feature-sliced architecture for `app/Admin/`
2. Group features by backend module to mirror the C# `Features/{Module}/{Feature}/` pattern
3. Use TanStack Query for server data; Pinia for client-only UI state; no per-feature Pinia stores
4. Centralize cross-cutting concerns (HTTP, UI primitives, composables, config) in `shared/`
5. Auto-import composables from `shared/composables/` to reduce boilerplate
6. Keep PrimeVue + Tailwind as the UI/tooling stack
7. Co-locate tests with code; one-action-per-file in `api/`

## Non-Goals

- Migrating `app/Store/` (separate spec, separate effort)
- Migrating backend C# (already organized)
- Building any actual feature UI in this spec — only the **scaffold + first slice (`auth`) + canonical template (`identity/users`)**
- Adding Tailwind plugins, design tokens, or theming beyond what PrimeVue Aura provides

## Top-Level Tree

```
src/
├── app/                              # Bootstrap & cross-cutting wiring
│   ├── main.ts
│   ├── App.vue
│   ├── providers/AppProviders.vue
│   ├── plugins/{primevue,pinia,vue-query}.ts
│   ├── stores/                       # Pinia — CLIENT-ONLY state
│   │   ├── theme.store.ts
│   │   ├── sidebar.store.ts
│   │   └── tenant.store.ts
│   ├── layout/{AppShell,AppSidebar,AppTopbar,AppFooter}.vue
│   └── router/{index,routes}.ts
│
├── features/                         # 1 folder per backend module
│   ├── auth/                         # special: not a CRUD module
│   ├── identity/{users,roles,permissions}/
│   ├── catalog/{products,variants,option-types,taxonomies}/
│   ├── location/{countries,states}/
│   ├── profile/{profiles,addresses,wishlists,notifications}/
│   ├── dashboard/
│   └── _template/users-template/     # one-time scaffold reference
│
├── shared/                           # cross-slice primitives
│   ├── api/
│   ├── ui/
│   ├── composables/                  # auto-imported
│   ├── lib/
│   ├── types/
│   └── config/
│
├── assets/{main.css, sekai/}
├── env.d.ts
└── __tests__/                        # top-level smoke tests
```

## Canonical Feature Slice (using `features/identity/users/` as template)

```
features/identity/users/
├── api/
│   ├── query-keys.ts                 # usersQueryKeys factory
│   ├── get-list.ts                   # useUsersList(params)
│   ├── get-by-id.ts                  # useUser(id)
│   ├── create.ts                     # useCreateUser()
│   ├── update.ts                     # useUpdateUser()
│   └── delete.ts                     # useDeleteUser()
│
├── model/
│   ├── user.types.ts                 # User, UserListItem, UserCreateRequest, UserUpdateRequest
│   ├── user.schema.ts                # Zod: createSchema, updateSchema
│   └── user.mapper.ts                # mapUserResponse(), mapUserListItem()
│
├── ui/
│   ├── UserList.vue
│   ├── UserFormDialog.vue
│   ├── UserDetailsDrawer.vue
│   ├── UserFilters.vue
│   └── UserStatusBadge.vue
│
├── composables/
│   └── useUserForm.ts                # @primevue/forms + Zod + mutation
│
├── __tests__/
│   ├── api/{get-list,create}.spec.ts
│   ├── model/user.mapper.spec.ts
│   └── ui/UserList.spec.ts
│
└── index.ts                          # public API barrel
```

**Conventions:**

- One action per file in `api/` — easy to find, swap, test
- Query keys centralized in `query-keys.ts` for invalidation
- `index.ts` is the **only** public surface — features import only from barrels
- No `store/` folder; TanStack Query owns server state
- Co-located tests; one spec file per source file

## `shared/` Sub-folders

### `shared/api/`

- `client.ts` — typed HTTP client (get/post/put/delete); was `src/api.ts`
- `errors.ts` — `ApiError`, `isApiError`, error code enum
- `envelope.ts` — `Envelope<T>` matching backend `Result<T>`
- `paged-result.ts` — `PagedResult<T>` matching backend `PagedResult<T>`
- `query-keys.ts` — helpers `withFilters()`, `withId()`
- `fetch-options.ts` — auth header injection, request-id, default headers

### `shared/ui/` (wrapped PrimeVue primitives)

`AppButton`, `AppDataTable`, `AppFormField`, `AppDialog`, `AppDrawer`, `AppConfirmDialog`, `AppToast`, `AppPageHeader`, `AppEmptyState`, `AppErrorState`, `AppLoadingState`, `AppStatusBadge`

### `shared/composables/` (auto-imported)

`usePagination`, `useDebouncedRef`, `useToast`, `useConfirm`, `useQueryString`, `useDisclosure`, `useFormatters`

### `shared/lib/` (pure, testable)

`formatters.ts`, `slug.ts`, `validators.ts`, `strings.ts`, `arrays.ts`

### `shared/types/`

`id.ts` (branded IDs), `timestamp.ts`, `page.ts`, `sort.ts`

### `shared/config/`

`env.ts` (VITE_API_URL validation), `app.ts` (name, version, default page size), `routes.ts` (route-name constants)

**Import rule:** `shared/` never imports from `features/`. Only `vue`, `vue-router`, `@tanstack/vue-query`, `primevue/...`, `zod`.

## Cross-Cutting

### `app/providers/AppProviders.vue`

Composition order: **PrimeVue → Pinia → VueQuery → Toast**. Mounted once in `app/App.vue`.

### `app/stores/` (Pinia — client-only)

- `theme.store.ts` — `light | dark`; syncs to `.p-dark` on `<html>`
- `sidebar.store.ts` — `collapsed: boolean`; persisted to `localStorage`
- `tenant.store.ts` — current tenant stub (flesh out if multi-tenant is confirmed)

Server data lives in TanStack Query. The two never overlap.

### `features/auth/` (special slice)

```
features/auth/
├── api/{login,logout,refresh,current-user}.ts
├── model/{auth.types,auth.schema}.ts
├── ui/{LoginPage,LoginForm,LogoutButton}.vue
├── composables/useAuthGuard.ts        # router beforeEach
└── index.ts
```

The HTTP-layer token injection lives in `shared/api/fetch-options.ts`. The router guard is wired in `app/router/index.ts` via `useAuthGuard()`.

### Routing

- Routes declared in `app/router/routes.ts`; **lazy-imported**: `component: () => import('@/features/users/ui/UserList.vue')`
- Each route declares `meta.authRequired: boolean` and `meta.permission?: string`
- Vite auto-code-splits per slice

### Auto-imports

- **Components:** PrimeVue via `unplugin-vue-components` (already configured)
- **Composables:** `unplugin-auto-import` with `imports: { dirs: ['src/shared/composables'] }` (NEW dep)
- ESLint extends the auto-generated `.eslintrc-auto-import.json` so `useDebouncedRef` resolves without import

## State Management Split

| Concern                         | Tool                              | Location                              |
|---------------------------------|-----------------------------------|---------------------------------------|
| Server data                     | TanStack Query                    | `features/*/api/*.ts`                 |
| Client UI state                 | Pinia                             | `app/stores/`                         |
| Form state                      | `@primevue/forms` + Zod resolver  | `features/*/composables/useXForm.ts`  |
| URL state (filters, page, sort) | `useQueryString` → URL            | `features/*/composables/`             |
| Auth tokens                     | module-scoped memory + refresh    | `features/auth/api/`                  |

## Testing

- **Unit:** Vitest + Vue Test Utils + happy-dom (already configured)
- **Seam:** `shared/api/client.ts` — tests use `vi.mock('@/shared/api/client')` instead of mocking `fetch`
- **Coverage:** opt-in via `vitest run --coverage`; gate at 70% for `shared/lib/`, `shared/composables/`, model mappers
- **Integration:** `app/__tests__/router.spec.ts` walks a logged-in user through `users → roles → permissions` with mocked client

## Migration Plan (22 steps)

| #  | Action                                                                                  | Notes                                      |
|----|-----------------------------------------------------------------------------------------|--------------------------------------------|
| 1  | `pnpm add @tanstack/vue-query zod`                                                      | runtime deps                               |
| 2  | `pnpm add -D unplugin-auto-import eslint-plugin-boundaries`                             | dev deps                                   |
| 3  | Create empty `app/`, `features/`, reorganize `shared/`                                  | folders first                              |
| 4  | Move `src/api.ts` → `src/shared/api/client.ts`; keep `ApiError`                         | behavior-preserving                        |
| 5  | Move `src/main.ts` → `src/app/main.ts`; create `app/providers/AppProviders.vue`         | preserve plugin order                      |
| 6  | Move `src/router/index.ts` → `src/app/router/`; add `routes.ts` with `/login` + `/`     |                                            |
| 7  | Move `src/App.vue` → `src/app/App.vue`; render `<AppShell>`                             | drops "You did it!"                        |
| 8  | Delete `src/stores/counter.ts` (placeholder)                                             |                                            |
| 9  | Fill `shared/api/{envelope,paged-result,errors,query-keys,fetch-options}.ts`             | matches backend types                      |
| 10 | Fill `shared/ui/*` (11 components)                                                      | one PR                                     |
| 11 | Fill `shared/composables/*` (7 composables)                                             | one PR                                     |
| 12 | Fill `shared/lib/*`, `shared/types/*`, `shared/config/*`                                | one PR                                     |
| 13 | Build `features/auth/` end-to-end (login, guard, current-user)                          | **first usable slice**                     |
| 14 | Build `features/dashboard/`                                                             | landing page                               |
| 15 | Build `features/identity/users/` as the CRUD template                                   | most other slices copy this                |
| 16 | Build `features/identity/{roles,permissions}`                                           | mirror users                               |
| 17 | Build `features/catalog/{products,variants,option-types,taxonomies}`                    | products is the largest                    |
| 18 | Build `features/location/{countries,states}`                                            | smallest                                   |
| 19 | Build `features/profile/{profiles,addresses,wishlists,notifications}`                   | profile aggregates smaller entities        |
| 20 | Delete `src/views/`, `src/stores/`, root `src/api.ts`, `src/App.vue`, `src/main.ts`     | tree is now `app/ + features/ + shared/`  |
| 21 | Update `tsconfig.app.json` paths, `vitest.config.ts` aliases                            | ensure `@/` still works                    |
| 22 | Wire `eslint-plugin-boundaries` to enforce "no cross-feature imports" + barrel-only     |                                            |

**Gate per step:** `pnpm test:unit`, `pnpm type-check`, `pnpm lint` all green.

## Open Questions / Future Considerations

- `features/_template/users-template/` is opt-in; can be replaced by copying `features/identity/users/` directly
- `tenant.store.ts` is a stub; if multi-tenant is confirmed, expand it
- `eslint-plugin-boundaries` adds dev-time enforcement; safe to drop if team prefers code review

## Acceptance Criteria

- [ ] `pnpm test:unit`, `pnpm type-check`, `pnpm lint` all green
- [ ] `pnpm dev` boots an admin shell with `/login` and `/` (dashboard) routes
- [ ] First slice `features/auth/` works end-to-end (login → guard → current-user)
- [ ] Canonical slice `features/identity/users/` works end-to-end (list + create + edit + delete)
- [ ] ESLint fails on any cross-feature import
- [ ] `pnpm build` produces a code-split bundle with separate chunks per slice

## Out of Scope (separate specs)

- Store SPA migration
- Real Product/Order UI (just scaffold here; features built in step 17)
- Internationalization
- Storybook
- E2E tests (Playwright)
- Backend API changes
