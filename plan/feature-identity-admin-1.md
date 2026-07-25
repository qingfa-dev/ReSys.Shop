---
goal: Implement admin Identity/Users module frontend API services
version: 1.0
date_created: 2026-07-25
status: Planned
tags: feature, api, identity, users, admin
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

Implement the admin Identity module in `app/Admin/src/features/users/`. 23 backend endpoints: Users CRUD + status toggle, User Roles/Permissions assign/revoke/sync, Roles CRUD + Role Permissions assign/revoke/sync, and Permissions listing. All pages are placeholder shells.

Backend route prefix: `api/identity`

## 1. Requirements & Constraints

- **REQ-001**: Every backend endpoint must have a frontend API method
- **REQ-002**: All API methods use shared `apiClient`
- **REQ-003**: Response types as camelCase interfaces matching backend C# records
- **REQ-004**: Zod validation for entities with create/update forms (User, Role)
- **REQ-005**: Assign/revoke/sync pattern for role/permission assignments (no forms, just ID lists)
- **REQ-006**: Roles and Permissions list pages get Pinia stores
- **REQ-007**: Replace PlaceholderPage with real components
- **CON-001**: Follow catalog module patterns exactly
- **CON-002**: Zero TypeScript errors
- **CON-003**: Follow assign/revoke/sync pattern from ProductOptionTypes (sends `{ items: [{ id, ... }] }`)
- **PAT-001** to **PAT-009**: Same as catalog patterns

## 2. Implementation Steps

### Phase 1: Users (Staff + Customers) CRUD + status toggle

- GOAL-001: Implement Users CRUD: types, schemas, mappers, API, store, composable, pages, components

Backend endpoints:
- GET `/users` — GetPaged
- GET `/users/{id:guid}` — GetById
- POST `/users` — Create
- PUT `/users/{id:guid}` — Update
- DELETE `/users/{id:guid}` — Delete
- PATCH `/users/{id:guid}/status` — ToggleStatus

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Create `types/user.response.ts` — `UserResponse`: id, email, userName, firstName, lastName, phone?, isActive, roles: UserRoleInfo[], createdAt, updatedAt; `UserRoleInfo`: id, name | | |
| TASK-002 | Create `types/user.request.ts` — `CreateUserRequest`: email, userName, password, firstName, lastName, phone?, isActive?; `UpdateUserRequest`: email, firstName, lastName, phone?, isActive?; `ToggleUserStatusRequest`: isActive: boolean | | |
| TASK-003 | Create `schemas/user.fields.ts` — fields: email (required email), userName (required), password (required for create), firstName (required), lastName (required), phone (optional), isActive (boolean) | | |
| TASK-004 | Create `schemas/user.forms.ts` — `UserForms` with create()/update() schemas | | |
| TASK-005 | Create `mappers/user.mapper.ts` — `UserFormMapper` with toCreate/toUpdate | | |
| TASK-006 | Create `api/user.api.ts` — `UserApi`: getMany(query), get(id), create(data), update(id, data), delete(id), toggleStatus(id, data) | | |
| TASK-007 | Create `store/user.store.ts` — `useUserStore` for staff list page | | |
| TASK-008 | Create `composables/useUser.ts` — returns { id, mode, route, router, toast, api: UserApi } | | |
| TASK-009 | Create `components/UserForm.vue` — fields: email, userName, password (only for create), firstName, lastName, phone, isActive checkbox | | |
| TASK-010 | Create `components/UserListTable.vue` — columns: email, userName, firstName + lastName, isActive (badge), roles (tags), ActionMenu | | |
| TASK-011 | Replace `pages/StaffListPage.vue` and `pages/CustomerListPage.vue` — both use UserListTable with different title/filters | | |
| TASK-012 | Replace `pages/StaffDetailPage.vue` and `pages/CustomerDetailPage.vue` — both use UserForm | | |
| TASK-013 | Update routes, barrels | | |
| TASK-014 | Verify: type-check passes | | |

### Phase 2: User Roles + User Permissions (assign/revoke/sync pattern)

- GOAL-002: Implement User Roles and User Permissions assignment managers

Backend endpoints:
- GET `/users/{id:guid}/roles` — GetUserRoles
- POST `/users/{id:guid}/roles/assign` — AssignUserRoles
- POST `/users/{id:guid}/roles/revoke` — RevokeUserRoles
- PATCH `/users/{id:guid}/roles/sync` — SyncUserRoles
- GET `/users/{id:guid}/permissions` — GetUserPermissions
- POST `/users/{id:guid}/permissions/assign` — AssignUserPermissions
- DELETE `/users/{id:guid}/permissions/revoke` — RevokeUserPermissions
- PUT `/users/{id:guid}/permissions/sync` — SyncUserPermissions

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Create `types/user-role.response.ts` — `UserRoleItem`: roleId, name, isAssigned; `UserRoleListResponse`: items: UserRoleItem[] | | |
| TASK-016 | Create `types/user-role.request.ts` — `UserRoleAssignmentItem`: roleId: string; `UserRoleIdsRequest`: items: { roleId }[] | | |
| TASK-017 | Create `api/user-role.api.ts` — `UserRoleApi`: get(userId), assign(userId, data), revoke(userId, data), sync(userId, data) | | |
| TASK-018 | Create `types/user-permission.response.ts` — `UserPermissionItem`: permissionId, name, isAssigned; `UserPermissionListResponse`: items: UserPermissionItem[] | | |
| TASK-019 | Create `api/user-permission.api.ts` — `UserPermissionApi`: get(userId), assign(userId, data), revoke(userId, data), sync(userId, data) | | |
| TASK-020 | Create `components/UserRoleManager.vue` — checkbox list of all roles, toggle assign/revoke, sync on save | | |
| TASK-021 | Create `components/UserPermissionManager.vue` — checkbox list grouped by category, toggle, sync on save | | |
| TASK-022 | Integrate managers into UserForm.vue, update barrels | | |
| TASK-023 | Verify: type-check passes | | |

### Phase 3: Roles CRUD + Role Permissions

- GOAL-003: Implement Roles CRUD + Role Permissions assign/revoke/sync

Backend endpoints:
- GET `/roles` — GetPaged
- GET `/roles/{id:guid}` — GetById
- POST `/roles` — Create
- PUT `/roles/{id:guid}` — Update
- DELETE `/roles/{id:guid}` — Delete
- GET `/roles/{id:guid}/permissions` — GetRolePermissions
- PUT `/roles/{id:guid}/permissions/assign` — AssignRolePermissions
- DELETE `/roles/{id:guid}/permissions/revoke` — RevokeRolePermissions
- PATCH `/roles/{id:guid}/permissions/sync` — SyncRolePermissions

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-024 | Create `types/role.response.ts` — `RoleResponse`: id, name, description?, isSystem, permissionCount?, createdAt, updatedAt | | |
| TASK-025 | Create `types/role.request.ts` — `CreateRoleRequest`/`UpdateRoleRequest`: name, description? | | |
| TASK-026 | Create `schemas/role.fields.ts` — name (required), description (optional) | | |
| TASK-027 | Create `schemas/role.forms.ts` — `RoleForms` with create()/update() | | |
| TASK-028 | Create `mappers/role.mapper.ts` — `RoleFormMapper` with toCreate/toUpdate | | |
| TASK-029 | Create `api/role.api.ts` — `RoleApi`: getMany(query), get(id), create(data), update(id, data), delete(id) | | |
| TASK-030 | Create `store/role.store.ts` — `useRoleStore` | | |
| TASK-031 | Create `composables/useRole.ts` — returns { id, mode, route, router, toast, api: RoleApi } | | |
| TASK-032 | Create `components/RoleForm.vue` — fields: name, description | | |
| TASK-033 | Create `components/RoleListTable.vue` — columns: name, description, isSystem (badge), permissionCount, ActionMenu | | |
| TASK-034 | Create `api/role-permission.api.ts` — `RolePermissionApi`: get(roleId), assign(roleId, data), revoke(roleId, data), sync(roleId, data) | | |
| TASK-035 | Create `components/RolePermissionManager.vue` — checkbox list, sync on save | | |
| TASK-036 | Replace pages: RoleListPage, RoleDetailPage; update UserForm.vue with role/permission managers | | |
| TASK-037 | Update routes, barrels | | |
| TASK-038 | Verify: type-check passes | | |

### Phase 4: Permissions listing

- GOAL-004: Implement Permissions list page

Backend endpoint: GET `/permissions` — GetPermissions (full list, no paging)

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-039 | Create `types/permission.response.ts` — `PermissionResponse`: id, name, description?, module, isAssignedToCurrentUser? | | |
| TASK-040 | Create `api/permission.api.ts` — `PermissionApi`: getMany(): Promise<Result<PermissionResponse[]>> — GET `/identity/permissions` | | |
| TASK-041 | Replace `pages/PermissionListPage.vue` — DataTable: name, description, module, ActionMenu (view details) | | |
| TASK-042 | Replace `pages/PermissionDetailPage.vue` — read-only detail view | | |
| TASK-043 | Update barrels, verify type-check passes | | |

## 3. Alternatives

- **ALT-001**: Separate Staff and Customers into different API classes — rejected: same UserResponse type, just different filters

## 4. Dependencies

- **DEP-001**: Shared apiClient, Result/PagedResult/ListQuery from `@/shared/`

## 5. Files

- **FILE-001** to **FILE-043**: One per task

## 6. Testing

- **TEST-001**: `api/__tests__/users.spec.ts` — verify all 6 methods
- **TEST-002**: `api/__tests__/user-roles.spec.ts` — verify get/assign/revoke/sync
- **TEST-003**: `api/__tests__/user-permissions.spec.ts` — verify get/assign/revoke/sync
- **TEST-004**: `api/__tests__/roles.spec.ts` — verify all 5 methods
- **TEST-005**: `api/__tests__/role-permissions.spec.ts` — verify get/assign/revoke/sync
- **TEST-006**: `api/__tests__/permissions.spec.ts` — verify getMany

## 7. Risks & Assumptions

- **RISK-001**: Identity API route prefix is `api/identity` but the frontend uses feature directory `users/` — the baseURL is `/api` so API calls use `/api/identity/...` while routes use `/users/...`. This is fine as they are different layers (API calls vs SPA routes).
- **ASSUMPTION-001**: Backend `GetUsersPagedOrAll` endpoint serves both staff and customers with a filter parameter

## 8. Related Specifications / Further Reading

Backend: `service/Api/src/Module/Identity/Features/Admin/`
Route constants: `service/Api/src/Module/Identity/Features/Identity.Feature.cs`
