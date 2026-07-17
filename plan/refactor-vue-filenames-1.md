---
goal: Standardize Vue file names to PascalCase.NameType.vue convention
version: 1.0
date_created: 2026-07-17
status: 'Completed'
tags: refactor, naming, convention
---

# Introduction

![Status: Completed](https://img.shields.io/badge/status-Completed-brightgreen)

Standardize all Vue component file names to `<PascalCase>.<ComponentType>.vue` convention. Three patterns exist currently:
1. Mixed-case with suffix (`login.view.vue`, `Profile.view.vue`)
2. PascalCase without suffix (`CatalogDashboard.vue`)
3. kebab-case with suffix (`topbar.layout.vue`)

All will become `PascalCase.ComponentType.vue`.

## 1. Requirements & Constraints

- **REQ-001**: Name part must be PascalCase (e.g., `Login`, `ProductForm`, `StockItemList`)
- **REQ-002**: Type suffix must be PascalCase: `.View.vue`, `.Layout.vue`, `.Component.vue`
- **REQ-003**: `App.vue` (root) excluded — no type suffix needed
- **REQ-004**: All internal import paths must be updated after rename
- **CON-001**: Lazy route imports use dynamic path strings — must be updated

## 2. Implementation Steps

### Phase 1 — Rename files

- GOAL-001: Rename all Vue files to PascalCase.NameType.vue

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Rename .view.vue files (40 files) | ✅ | 2026-07-17 |
| TASK-002 | Rename .layout.vue files (7 files) | ✅ | 2026-07-17 |
| TASK-003 | Rename .component.vue files (4 files) | ✅ | 2026-07-17 |
| TASK-004 | Add .View.vue suffix to view files missing it (7 files) | ✅ | 2026-07-17 |
| TASK-005 | Add .Component.vue suffix to component files missing it (20 files) | ✅ | 2026-07-17 |
| TASK-006 | Update all import paths across src/ | ✅ | 2026-07-17 |

## 3. Dependencies

- **DEP-001**: None — self-contained file rename

## 4. Files

### File Name Mapping (old → new)

**Views** — `.View.vue`:
- `login.view.vue` → `Login.View.vue`
- `Profile.view.vue` → `Profile.View.vue`
- `NotFound.view.vue` → `NotFound.View.vue`
- `ErrorPage.view.vue` → `ErrorPage.View.vue`
- `EmptyPage.view.vue` → `EmptyPage.View.vue`
- `AccessDenied.view.vue` → `AccessDenied.View.vue`
- `CatalogDashboard.vue` → `CatalogDashboard.View.vue`
- `DashboardPage.vue` → `DashboardPage.View.vue`
- `product-form.view.vue` → `ProductForm.View.vue`
- `product-list.view.vue` → `ProductList.View.vue`
- `product-classification-manager.view.vue` → `ProductClassificationManager.View.vue`
- `option-type-form.view.vue` → `OptionTypeForm.View.vue`
- `option-type-list.view.vue` → `OptionTypeList.View.vue`
- `option-type-manager.view.vue` → `OptionTypeManager.View.vue`
- `option-value-list.view.vue` → `OptionValueList.View.vue`
- `property-type-form.view.vue` → `PropertyTypeForm.View.vue`
- `property-type-list.view.vue` → `PropertyTypeList.View.vue`
- `taxonomy-form.view.vue` → `TaxonomyForm.View.vue`
- `taxonomy-list.view.vue` → `TaxonomyList.View.vue`
- `taxonomy-manager.view.vue` → `TaxonomyManager.View.vue`
- `taxon-form.view.vue` → `TaxonForm.View.vue`
- `taxon-list.view.vue` → `TaxonList.View.vue`
- `taxon-tree-manager.view.vue` → `TaxonTreeManager.View.vue`
- `StockLocationForm.view.vue` → `StockLocationForm.View.vue`
- `StockLocationList.view.vue` → `StockLocationList.View.vue`
- `StockLocationManager.view.vue` → `StockLocationManager.View.vue`
- `StockItemList.view.vue` → `StockItemList.View.vue`
- `StockTransferForm.view.vue` → `StockTransferForm.View.vue`
- `StockTransferList.view.vue` → `StockTransferList.View.vue`
- `StockTransferDetail.view.vue` → `StockTransferDetail.View.vue`
- `InventoryUnitList.view.vue` → `InventoryUnitList.View.vue`
- `CountryForm.view.vue` → `CountryForm.View.vue`
- `CountryList.view.vue` → `CountryList.View.vue`
- `StateForm.view.vue` → `StateForm.View.vue`
- `StateList.view.vue` → `StateList.View.vue`
- `order-form.view.vue` → `OrderForm.View.vue`
- `order-list.view.vue` → `OrderList.View.vue`
- `order-detail.view.vue` → `OrderDetail.View.vue`
- `fulfillment-queue.view.vue` → `FulfillmentQueue.View.vue`
- `dashboard.view.vue` → `Dashboard.View.vue`
- `admin-user-list.view.vue` → `AdminUserList.View.vue`
- `customer-list.view.vue` → `CustomerList.View.vue`
- `customer-detail.view.vue` → `CustomerDetail.View.vue`
- `staff-detail.view.vue` → `StaffDetail.View.vue`
- `staff-form.view.vue` → `StaffForm.View.vue`
- `permission-list.view.vue` → `PermissionList.View.vue`
- `role-form.view.vue` → `RoleForm.View.vue`
- `role-list.view.vue` → `RoleList.View.vue`
- `role-permissions-manager.view.vue` → `RolePermissionsManager.View.vue`

**Layouts** — `.Layout.vue`:
- `main.layout.vue` → `Main.Layout.vue`
- `topbar.layout.vue` → `Topbar.Layout.vue`
- `sidebar.layout.vue` → `Sidebar.Layout.vue`
- `menu.layout.vue` → `Menu.Layout.vue`
- `menu-item.layout.vue` → `MenuItem.Layout.vue`
- `footer.layout.vue` → `Footer.Layout.vue`
- `configurator.layout.vue` → `Configurator.Layout.vue`

**Components** — `.Component.vue`:
- `taxon-form.component.vue` → `TaxonForm.Component.vue`
- `taxon-rules-manager.component.vue` → `TaxonRulesManager.Component.vue`
- `taxon-products-preview.component.vue` → `TaxonProductsPreview.Component.vue`
- `metadata-manager.component.vue` → `MetadataManager.Component.vue`
- `breadcrumb.component.vue` → `Breadcrumb.Component.vue`
- `FloatingConfigurator.vue` → `FloatingConfigurator.Component.vue`
- `GlobalSearch.vue` → `GlobalSearch.Component.vue`
- `AppProviders.vue` → `AppProviders.Component.vue`
- `LocationSelector.vue` → `LocationSelector.Component.vue`
- `StockAdjustmentDialog.vue` → `StockAdjustmentDialog.Component.vue`
- `StockMovementTimeline.vue` → `StockMovementTimeline.Component.vue`
- `AddressDialog.vue` → `AddressDialog.Component.vue`
- `ItemDialog.vue` → `ItemDialog.Component.vue`
- `RefundDialog.vue` → `RefundDialog.Component.vue`
- `ShipmentDialog.vue` → `ShipmentDialog.Component.vue`
- `UserPermissionManager.vue` → `UserPermissionManager.Component.vue`
- `UserRoleManager.vue` → `UserRoleManager.Component.vue`
- `UserSecurityManager.vue` → `UserSecurityManager.Component.vue`
- `AppBrandMark.vue` → `AppBrandMark.Component.vue`
- `ManagerWelcome.vue` → `ManagerWelcome.Component.vue`
- `ProductClassificationManager.vue` → `ProductClassificationManager.Component.vue`
- `ProductImageManager.vue` → `ProductImageManager.Component.vue`
- `ProductImageUploader.vue` → `ProductImageUploader.Component.vue`
- `ProductImageList.vue` → `ProductImageList.Component.vue`
- `ProductInventoryManager.vue` → `ProductInventoryManager.Component.vue`
- `ProductOptionTypeManager.vue` → `ProductOptionTypeManager.Component.vue`
- `ProductPropertyManager.vue` → `ProductPropertyManager.Component.vue`
- `ProductVariantManager.vue` → `ProductVariantManager.Component.vue`
- `VariantFormDialog.vue` → `VariantFormDialog.Component.vue`
- `VariantGenerationDialog.vue` → `VariantGenerationDialog.Component.vue`
- `HelloWorld.vue` → `HelloWorld.Component.vue`

## 5. Testing

- **TEST-001**: `npx vite build` — only pre-existing 4 Vue template errors
- **TEST-002**: `vue-tsc --build` — no new TS errors from import resolution

## 6. Risks & Assumptions

- **RISK-001**: Lazy route imports in router use string paths — must update all dynamic imports
- **ASSUMPTION-001**: No external consumers reference these files by path
