---
goal: Delete every .vue file in the Store SPA and rebuild the entire presentation layer from scratch with PrimeVue 5 components only, preserving the existing router flow, stores, services, and validations.
version: 2.0
date_created: 2026-08-08
last_updated: 2026-08-08
owner: Storefront Team
status: 'Planned'
tags: [refactor, ui, primevue, storefront, full-coverage]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Store SPA (`app/Store/`) currently contains 36 `.vue` files (3 layouts, 7 shell components, 26 feature views/components) built incrementally with mixed native HTML and PrimeVue. The user directive is absolute: **drop all `.vue` files and rebuild the UI** with PrimeVue 5 as the only component system, following the canonical user flows. The non-presentation layers (Pinia stores, API services, types, validations, composables, router) are complete and verified (289 tests passing) and MUST NOT be touched. This plan is a deterministic, phase-ordered rewrite of the presentation layer only: delete first, then rebuild shell → catalog → identity → ordering → profile, then re-verify.

The rebuild must exercise the **full PrimeVue 5 component catalog**, not a subset: every applicable component from the installed library (verified against `app/legacy/llms.txt` and `app/Store/node_modules/primevue/`) is assigned a concrete home in the UI via the coverage matrix in REQ-009, and an automated gate (`scripts/check-primevue-coverage.sh`, TEST-005) fails the build if any matrix row is missing. Only components with no storefront application are excluded, with explicit rationale in REQ-009.

Canonical flows preserved by this plan (from `src/app/router/routes.ts` and feature route files; formal requirements in REQ-005 to REQ-008):

- Guest browse flow: `/` (home) → `/shop` (taxonomy filter + grid) → `/products/:slug` (detail + add-to-cart) → `/cart` (cart view) → `/checkout` (auth-guarded).
- Auth flow: `/login` · `/register` · `/forgot-password` · `/reset-password` (guest-only shell) and `/account/sessions`.
- Order flow: `/cart` → `/checkout` (5-step) → `/account/orders` → `/account/orders/:id`.
- Account flow: `/account/profile` · `/account/addresses` · `/account/wishlists` · `/account/notifications` · `/account/change-password` · `/account/preferences`.

## 1. Requirements & Constraints

- **REQ-001**: Delete every `.vue` file under `app/Store/src/` (36 files, listed in FILE-001 to FILE-005) and all view/component specs in FILE-006, then rebuild from scratch.
- **REQ-002**: Every interactive element must be a PrimeVue 5 component. Native interactive tags (`<button>`, `<input>`, `<select>`, `<textarea>`, `<a href>` for actions, `<label>` with for/onclick, native checkbox/radio) are forbidden anywhere in `src/**/*.vue`.
- **REQ-003**: Do not modify any non-presentation layer file: `src/features/*/stores`, `src/features/*/services`, `src/features/*/types`, `src/features/*/validations`, `src/shared/**`, `src/app/router/**`, `src/app/config/**`, `src/app/providers/**`.
- **REQ-004**: All 26 named routes and the 3-layout shell structure in `src/app/router/routes.ts` remain unchanged; every rebuilt view must be imported by its existing route exactly where it is today.
- **REQ-005**: Guest browse flow: `/` → `/shop` → `/products/:slug` → `/cart` → `/checkout` with guest-only guard on auth pages (per REQ-006).
- **REQ-006**: Auth flow: `/login` · `/register` · `/forgot-password` · `/reset-password` render inside `AuthLayout`; all identity forms use existing `src/features/identity/validations/auth.ts` schemas via vee-validate + zod.
- **REQ-007**: Order flow: cart → 5-step checkout (Stepper component, v5 successor of Steps) → order confirmation → `/account/orders` → `/account/orders/:id`, using `checkoutStore`, `cartStore`, `orderStore`, `usePayment`/`useStripe`.
- **REQ-008**: Account flow: all `/account/*` routes render inside `AccountLayout` with a persistent sidebar nav and use `profileStore`, `addressStore`, `wishlistStore`, `notificationApi`-backed stores.
- **SEC-001**: `requiresAuth` and `guestOnly` route guards in `src/app/router/guards.ts` must continue to function unchanged; checkout and all `/account/*` views must not render authenticated data without the store resolving the session.
- **SEC-002**: Password fields render with `InputPassword` (v5) with mask toggle and, where required, strength meter; passwords are never logged or echoed.
- **SEC-003**: Auth redirect after login uses existing `postLoginRedirect` util (REQ-006 flows only), no new redirect logic.
- **CON-001**: Tailwind CSS is used exclusively for layout, spacing, sizing, and responsive composition — never to re-implement interactive behavior.
- **CON-002**: No new npm dependencies may be added. Use installed packages only (see DEP-001 to DEP-005).
- **CON-003**: Quality gates before every commit: `pnpm run lint` (eslint + oxlint), `npx vue-tsc --build`, `npx vitest run`, `pnpm run build-only` — all must pass with zero errors.
- **CON-004**: Dark mode is handled by PrimeVue theme tokens + `useTheme` composable; no ad-hoc `dark:` Tailwind color logic inside components.
- **GUD-001**: Use PrimeVue Aura theme preset from `src/app/providers/primevue.ts` (unchanged) and `pi` icon classes from primeicons (`pi pi-search`, `pi pi-shopping-cart`, `pi pi-user`, `pi pi-chevron-*`, etc.).
- **GUD-002**: Notifications via `Toast` + `useNotify`; destructive confirmations via `ConfirmDialog` or `ConfirmPopup` registered in `App.vue`.
- **GUD-003**: Loading states via `Skeleton`/`ProgressSpinner` bound to store `loading` flags; empty states via `EmptyState`-style layout using `Tag`/`Message`/illustration and a `Button` CTA.
- **GUD-004**: All forms use `Form`/`Field` from `@primevue/forms` pattern where present, else `FloatLabel` + `InputText`/`InputPassword`/`Select`/`InputNumber`/`Textarea`/`InputMask` with vee-validate; validation messages render via `Message`/inline `small` (non-interactive).
- **GUD-005**: Amounts formatted with existing `useCurrency` util; dates with `src/shared/utils/date.ts`.
- **PAT-001**: Every SFC is `<script setup lang="ts">` + `<template>`; template root is a semantic native element (`main`, `section`, `div`, `Card`, etc.) allowed per native-HTML rules; component props/emits typed and named per existing store APIs.
- **PAT-002**: Feature-scoped structure retained: `src/features/{catalog,identity,ordering,profile}/{views,components}/`; shell components stay in `src/app/components/`.
- **PAT-003**: When a PrimeVue component exists for a need, use it first and customize via `class`/design tokens; never hand-roll: buttons, inputs, selects, checkboxes, radios, dialogs, drawers, menus, tabs, pagination, tables, trees, toasts, confirms, tooltips, file upload, carousels/galleries, steppers, timeline, rating, tags/badges, skeletons, sliders, switches, accordions, breadcrumbs, cards.
- **REQ-009**: Full-catalog coverage — every row of Table 1 (coverage matrix) MUST render its component as a real element in the designated file's `<template>` (match `<Component` or `<Component `). Verified by `scripts/check-primevue-coverage.sh` (TEST-005) which parses Table 1 from this plan. Excluded components (no storefront application or superseded in v5, listed with rationale): Chart (no analytics graphs), ColorPicker/InputColor (no color fields), DatePicker (no date inputs in any form), Dock (web anti-pattern), Editor (no rich text), ImageCompare (no before/after use case), InputOtp (no OTP flow), InputTags (no tag entry), KeyFilter/Mask (InputNumber/InputMask cover), Knob (no dial input), Listbox (Select covers), OrderList/PickList (no reorder lists), OrganizationChart (no org data), SpeedDial (no FAB pattern), Splitter (no resizable panes), StyleClass (no declarative toggles needed), Terminal (no CLI UI), TreeSelect/TreeTable (Tree covers taxonomy), VirtualScroller (Paginator covers), AvatarGroup (no user groups), Password (superseded by InputPassword), Steps (superseded by Stepper), Tooltip (directive-only API in v5 — used as v-tooltip in ProductCard), AnimateOnScroll (directive-only API in v5 — used as v-animateonscroll in HomeView), Badge (superseded by Tag + OverlayBadge/badge prop), Fieldset (filter groups covered by Panel/Accordion).
- **REQ-010**: Plan completion requires both audits green: native-interactive audit (TEST-002) returns zero files AND coverage audit (TEST-005) reports every Table 1 row present (TASK-048).
- **GUD-006**: `app/legacy/llms.txt` and https://primevue.dev/llms are the authoritative PrimeVue catalog; when a new UI need arises, consult it first and either use the component or record the exclusion rationale in REQ-009.

Table 1 — PrimeVue coverage matrix (parsed by `scripts/check-primevue-coverage.sh`; component = exact tag, file = path relative to `app/Store/src`):

| Component | File (relative to app/Store/src) | Task |
|-----------|----------------------------------|------|
| Accordion | features/catalog/components/ShopFilterPanel.vue | via TASK-017 |
| AutoComplete | app/components/layout/AppHeader.vue | via TASK-010 |
| Avatar | app/components/layout/AppHeader.vue | via TASK-010 |
| Breadcrumb | features/catalog/views/ProductDetailView.vue | via TASK-020 |
| Button | app/components/layout/AppHeader.vue | via TASK-010 |
| ButtonGroup | features/ordering/views/CheckoutView.vue | via TASK-033 |
| Card | app/layouts/AuthLayout.vue | via TASK-008 |
| Carousel | features/catalog/views/HomeView.vue | via TASK-016 |
| CascadeSelect | features/ordering/views/CheckoutView.vue | via TASK-033 |
| Checkbox | features/identity/views/LoginView.vue | via TASK-025 |
| CheckboxGroup | features/catalog/components/ShopFilterPanel.vue | via TASK-017 |
| Chip | features/catalog/components/ShopFilterPanel.vue | via TASK-017 |
| CommandMenu | features/catalog/components/SearchOverlay.vue | via TASK-019 |
| ConfirmDialog | App.vue | via TASK-038 |
| ConfirmPopup | features/identity/views/SessionsView.vue | via TASK-029 |
| ContextMenu | features/catalog/components/ProductCard.vue | via TASK-015 |
| DataTable | features/ordering/views/OrderListView.vue | via TASK-034 |
| DataView | features/ordering/views/CartView.vue | via TASK-032 |
| DeferredContent | features/catalog/views/HomeView.vue | via TASK-016 |
| Dialog | features/profile/views/AddressBookView.vue | via TASK-038 |
| Divider | features/identity/views/LoginView.vue | via TASK-025 |
| Drawer | features/ordering/components/CartDrawer.vue | via TASK-031 |
| FileUpload | features/catalog/views/VisualSearchView.vue | via TASK-022 |
| FloatLabel | features/identity/views/LoginView.vue | via TASK-025 |
| Fluid | app/layouts/AuthLayout.vue | via TASK-008 |
| Galleria | features/catalog/views/ProductDetailView.vue | via TASK-020 |
| Gallery | features/catalog/views/ProductDetailView.vue | via TASK-020 |
| IconField | features/catalog/components/ShopFilterPanel.vue | via TASK-017 |
| Image | features/catalog/components/ProductCard.vue | via TASK-015 |
| Inplace | features/profile/views/ProfileView.vue | via TASK-037 |
| InputGroup | features/ordering/views/CartView.vue | via TASK-032 |
| InputMask | features/profile/views/AddressBookView.vue | via TASK-038 |
| InputNumber | features/ordering/components/CartDrawer.vue | via TASK-031 |
| InputPassword | features/identity/views/LoginView.vue | via TASK-025 |
| InputText | features/identity/views/LoginView.vue | via TASK-025 |
| Label | features/profile/views/AddressBookView.vue | via TASK-038 |
| MegaMenu | app/components/layout/AppHeader.vue | via TASK-010 |
| Menu | app/components/layout/AppHeader.vue | via TASK-010 |
| Menubar | app/components/layout/AppHeader.vue | via TASK-010 |
| Message | features/catalog/views/ShopView.vue | via TASK-017 |
| MeterGroup | features/catalog/views/ProductDetailView.vue | via TASK-020 |
| MultiSelect | features/catalog/components/ShopFilterPanel.vue | via TASK-017 |
| OverlayBadge | app/components/layout/AppHeader.vue | via TASK-010 |
| Paginator | features/catalog/views/ShopView.vue | via TASK-017 |
| Panel | features/catalog/components/ShopFilterPanel.vue | via TASK-017 |
| PanelMenu | app/layouts/AccountLayout.vue | via TASK-009 |
| Popover | features/catalog/components/ProductCard.vue | via TASK-015 |
| ProgressBar | features/ordering/components/CartDrawer.vue | via TASK-031 |
| ProgressSpinner | features/catalog/views/VisualSearchView.vue | via TASK-022 |
| RadioButton | features/ordering/views/CheckoutView.vue | via TASK-033 |
| RadioButtonGroup | features/ordering/views/CheckoutView.vue | via TASK-033 |
| Rating | features/catalog/components/ProductCard.vue | via TASK-015 |
| ScrollPanel | features/catalog/views/TermsView.vue | via TASK-023 |
| ScrollTop | app/layouts/DefaultLayout.vue | via TASK-007 |
| Select | features/catalog/views/ShopView.vue | via TASK-017 |
| SelectButton | features/catalog/views/ShopView.vue | via TASK-017 |
| Sidebar | app/layouts/AccountLayout.vue | via TASK-009 |
| Skeleton | features/catalog/views/HomeView.vue | via TASK-016 |
| Slider | features/catalog/components/ShopFilterPanel.vue | via TASK-017 |
| SplitButton | features/catalog/views/ProductDetailView.vue | via TASK-020 |
| Stepper | features/ordering/views/CheckoutView.vue | via TASK-033 |
| Tabs | features/catalog/views/ProductDetailView.vue | via TASK-020 |
| Tag | features/ordering/views/OrderListView.vue | via TASK-034 |
| Textarea | features/profile/views/AddressBookView.vue | via TASK-038 |
| Timeline | features/ordering/views/OrderDetailView.vue | via TASK-035 |
| Toast | App.vue | via TASK-006 |
| ToggleButton | features/profile/views/PreferencesView.vue | via TASK-041 |
| ToggleSwitch | app/components/ThemeToggle.vue | via TASK-013 |
| Toolbar | features/catalog/views/ShopView.vue | via TASK-017 |
| Tree | features/catalog/components/TaxonTree.vue | via TASK-018 |

## 2. Implementation Steps

### Implementation Phase 1 — Teardown & data-layer verification

- GOAL-001: Delete the entire presentation layer and prove the remaining code (stores, services, router, api) type-checks and tests green without any `.vue` file.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete all 36 `.vue` files: FILE-001 (8 shell/layout files), FILE-002 (12 catalog files), FILE-003 (5 identity files), FILE-004 (5 ordering files), FILE-005 (6 profile files). | | |
| TASK-002 | Delete the 9 view/component spec files listed in FILE-006 (they import deleted components and will not compile). | | |
| TASK-003 | Run `npx vue-tsc --build` in `app/Store/`. The `*.vue` shim means missing module errors do NOT surface; verification is: (a) vue-tsc exits 0 (no error in stores/services/types/validations/composables/shared/router — any such error indicates an unintended deletion and blocks Phase 2), and (b) `git status --short` shows exactly the 36 deletions enumerated in FILE-001 to FILE-005 plus the 9 spec deletions from TASK-002. | | |
| TASK-004 | Run `npx vitest run` and `pnpm run lint`. All non-view tests (280+ cases across stores, validations, api interceptors, composables, querying types) must pass; zero lint errors. | | |

### Implementation Phase 2 — App shell & layouts

- GOAL-002: Rebuild `App.vue`, the 3 layouts, and the 5 shell components with PrimeVue 5 only.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-005 | Rewrite `src/main.ts` (FILE-007): import the new `App.vue`, keep Pinia + router setup and the existing PrimeVue provider exactly as before. | | |
| TASK-006 | Create `src/App.vue`: root `<div>` binding dark-mode class from `useTheme` (`.app-dark`); global `Toast` (position `bottom-right`), `ConfirmDialog`, and `<RouterView />`. No other markup. | | |
| TASK-007 | Create `src/app/layouts/DefaultLayout.vue`: `<div>` flex column min-h-screen → `AppHeader`, `<main class="flex-1">` with `<RouterView />`, `AppFooter`; `ScrollTop` (threshold 400, position bottom-right); `Skeleton` page-loader bar while route lazy chunks load (router `beforeResolve` + `useStoreEvents`). | | |
| TASK-008 | Create `src/app/layouts/AuthLayout.vue`: centered min-h-screen layout; `Card` with brand mark (`Image` or `i.pi`) + `<slot />` for the form wrapped in `Fluid` (full-width inputs); `Divider` between form and secondary links; footer line with `Button text` links to `/` and `/register` or `/login` (opposite of current route). No native interactive elements. | | |
| TASK-009 | Create `src/app/layouts/AccountLayout.vue`: responsive 2-column grid — left `aside` with vertical nav using `PanelMenu` (items: Profile, Addresses, Wishlists, Notifications, Change Password, Preferences, Orders — route `to` targets from REQ-008) plus `Tag` on Orders when `orderStore` has active orders; right column `<RouterView />`. On mobile (<lg, `useMediaQuery`), the same nav renders inside `Sidebar` (v5 compound nav panel, position left, `sidebar` variant) triggered by `Button` `pi pi-bars`. | | |
| TASK-010 | Create `src/app/components/layout/AppHeader.vue`: sticky header, blur backdrop. Content: brand `RouterLink`/`Button` to `/`; `Menubar` (Home, Shop, Collections, Visual Search) on `lg+`; `MegaMenu` catalog panel (taxonomies from `catalogStore.taxonomyGroups`, linking to `/shop?taxon=…`); `AutoComplete` search (suggestions from `useSearch`, opens `SearchOverlay` on select); `ToggleSwitch` bound to `useTheme().isDark` (replaces ThemeToggle usage); cart `Button` with `OverlayBadge` (count from `cartStore.items.length`) opening `CartDrawer`; authenticated `Avatar` + `Menu` (Account, Orders, Sign Out via `authStore.signOut()`) or `Button` "Sign In" → `/login`; `Button` hamburger (`pi pi-bars`) opening `MobileNav` drawer on <lg; `Tooltip` on all icon-only buttons. | | |
| TASK-011 | Create `src/app/components/layout/MobileNav.vue`: `Drawer` (position `left`) listing the same routes as AppHeader via `PanelMenu` + account actions; closes on route change (`watch(route)`). | | |
| TASK-012 | Create `src/app/components/layout/AppFooter.vue`: 4-column layout — brand + blurb, Shop links (`Button text` → catalog routes), Company links (`/about`, `/terms`, `/privacy`), newsletter `InputGroup` (`InputText` + `Button` `pi pi-send`, emits toast via `useNotify`); `Divider` before copyright line. No native interactive elements. | | |
| TASK-013 | Create `src/app/components/ThemeToggle.vue`: thin wrapper exposing `ToggleSwitch` bound to `useTheme`; used by AppHeader (TASK-010) and AccountLayout. | | |
| TASK-014 | Verify Phase 2: `npx vue-tsc --build` clean; `pnpm exec oxlint .` and `pnpm exec eslint .` clean; `npx vitest run` green; `src/app/router/routes.ts` unchanged (git diff empty for it). `pnpm run build-only` is NOT a Phase 2 gate — route lazy imports still reference views rebuilt in Phases 3-6; first green build is verified at TASK-045. | | |

### Implementation Phase 3 — Catalog (14 views/components)

- GOAL-003: Rebuild the catalog surface: product card, home, shop with taxonomy `Tree` filter, product detail, search overlay, collections, visual search, and 4 static pages.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-015 | Create `src/features/catalog/components/ProductCard.vue`: `Card` per product — `Image` (aspect-square, object-cover, `preview` for zoom), `Rating` (readonly, from rating avg), title, price line (`useCurrency`), promo `Badge`/`Tag`, wishlist `Button` (`pi pi-heart` / `pi pi-heart-fill`, `wishlistStore.toggleItem`) with `Tooltip` ("Add to wishlist"), quick-add `Button` (`pi pi-shopping-cart`, `useQuickAdd`) with `Tooltip`; `ContextMenu` (Quick view / Add to cart / Wishlist actions) on right-click; `Popover` "Quick view" (image + name + price + `Button` → detail). Whole card wrapped in `RouterLink` to `/products/:slug`. Emits typed `add-to-cart`/`toggle-wishlist`. | | |
| TASK-016 | Create `src/features/catalog/views/HomeView.vue`: hero `section` (`AnimateOnScroll` fade on load; headline + CTA `Button` → `/shop` + `Image` banner); "Featured" `Carousel` (responsive, one item per view on mobile / 4 on lg) of `ProductCard` from `catalogStore` featured products with `Skeleton` while loading, wrapped in `DeferredContent` (below-fold sections defer until scrolled into view); category `Tag` row linking to `/shop?taxon=…`; benefits strip (`i.pi` icons + text). | | |
| TASK-017 | Create `src/features/catalog/views/ShopView.vue`: 2-column grid — `aside` (sticky, `lg+`) with filter panel: top `IconField` + `InputIcon` (`pi pi-filter`) + `InputText` (search within taxons/options, filters the tree below), per-taxonomy `Accordion` (one tab per taxonomy group) containing `TaxonTree` (TASK-018), option-value `Panel`/`Fieldset` (toggleable) `CheckboxGroup` + `MultiSelect` groups from `catalogStore.optionTypes`, price `Slider` (range) + `InputNumber` min/max, active-filter `Chip` row (removable, `@remove` → clear that filter), "Clear all" `Button text` (`catalogStore.clearFilters()`), active-filter count `Tag`; main column: `Toolbar` (result count `Tag`, `SelectButton` grid/list layout toggle, `Select` sort from `catalogStore.sortOptions`), grid of `ProductCard`, `Paginator` bound to `productListStore` paging (`rows`, `totalRecords`, `first`, `@page`). Empty state: `Message` + `Button` "Clear filters". Mobile: filter toggle `Button` → `Drawer` containing the same panel. | | |
| TASK-018 | Create `src/features/catalog/components/TaxonTree.vue`: `Tree` from PrimeVue with `selection-mode="checkbox"`, `v-model:expanded-keys`, `v-model:selection-keys`, `:filter` + `filter-placeholder="Search..."` (renders `IconField` + `InputIcon` + `InputText` internally); map `TaxonTreeNode[]` → `TreeNode[]` (`key`=id, `label`=name, recursive `children`, `leaf`=!hasChildren); selection setter diffs against `catalogStore.selectedTaxonIds` and calls `catalogStore.toggleTaxon(id)`; `#default` node slot renders label + child-count `Tag`; roots expanded by default. No native elements anywhere. | | |
| TASK-019 | Create `src/features/catalog/components/SearchOverlay.vue`: `CommandMenu` (PrimeVue search-driven command palette) opened from AppHeader (TASK-010); commands = product results from `useSearch` (debounced via `useDebounce`), `@command` navigates to `/products/:slug`; header shows `IconField` + `InputIcon` (`pi pi-search`) + `InputText`; `Skeleton` while loading; empty `Message` "No products found"; footer command "View all results" → `/shop?q=…`. | | |
| TASK-020 | Create `src/features/catalog/views/ProductDetailView.vue`: `Breadcrumb` (home / shop / taxon / product, from `productDetailStore` breadcrumbs); 2-col grid: left `Galleria` (product images, thumbnails, responsive) with `Gallery` (fullscreen zoom/rotate/download) on image click; right: title, `Rating` + review count, `MeterGroup` (rating breakdown / stock level from `availabilityStore`), price (+compare-at strikethrough), `Tag` badges, variant `Select` (from `productDetailStore` options), `InputNumber` qty (min 1), `SplitButton` "Add to Cart" (default = add, `cartStore.addItem`; items = "Buy Now", "Add to Wishlist"), stock `Message` from `availabilityStore`; `Divider`; `Tabs` (Description / Details / Reviews — static content); `Accordion` (Shipping & Returns); related products grid from `productListStore`. | | |
| TASK-021 | Create `src/features/catalog/views/CollectionsView.vue`: grid of `Card` per collection (Image, name, product-count `Tag`) from `catalogStore.collections`, linking to `/shop?taxon=…`; `Skeleton` while loading. | | |
| TASK-022 | Create `src/features/catalog/views/VisualSearchView.vue`: `FileUpload` (mode basic, accept image/*, custom `:auto`, `@select` → `visualSearchStore.upload`), `ProgressSpinner` while embedding, result grid of `ProductCard` from `visualSearchStore.results`, similarity `Tag` per item; error via `Message`. | | |
| TASK-023 | Create `src/features/catalog/views/NotFoundView.vue`: centered `Message`/big `i.pi pi-exclamation-circle`, copy, `Button` → `/`; and static pages `AboutView.vue`, `TermsView.vue`, `PrivacyView.vue` as typographic `section`s (headings + paragraphs, `Card` prose blocks, `ScrollPanel` for the long `TermsView` body; no interactive elements). | | |
| TASK-024 | Verify Phase 3: `npx vue-tsc --build`, `pnpm run lint`, `pnpm run build-only` all clean; grep audit (TEST-002) shows zero native interactive tags in `src/features/catalog/**/*.vue`. | | |

### Implementation Phase 4 — Identity (5 views)

- GOAL-004: Rebuild login/register/forgot/reset and sessions management with PrimeVue forms.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-025 | Create `src/features/identity/views/LoginView.vue`: `Card` form in `Fluid` — `FloatLabel` + `InputText` (email), `FloatLabel` + `InputPassword` (mask toggle), `Checkbox` "Remember me", `Divider` ("or" separators), submit `Button` (loading state `:loading`), `Message` for API errors (`useApiErrorHandler`), `Message` for success, `Button text` links: Forgot password → `/forgot-password`, Create account → `/register`. Uses `authStore.signIn` + `postLoginRedirect` (SEC-003). | | |
| TASK-026 | Create `src/features/identity/views/RegisterView.vue`: same `Card`/`Fluid` form pattern — firstName, lastName, email, `InputPassword` ×2 (confirm, with strength meter on the first), `Checkbox` agree-to-terms (link via `Button text` → `/terms`), submit with `:loading`, validation via `RegisterFormSchema` (TASK-025 pattern), `Button text` → `/login`. | | |
| TASK-027 | Create `src/features/identity/views/ForgotPasswordView.vue`: `Card` with `FloatLabel` `InputText` email + submit `Button`; on success show `Message` success + `Button text` back to `/login`. | | |
| TASK-028 | Create `src/features/identity/views/ResetPasswordView.vue`: `Card` with `InputPassword` (new) + `InputPassword` (confirm, strength meter), submit `Button`, `ResetPasswordSchema` validation, `Message` error/success, link `/login` on success. | | |
| TASK-029 | Create `src/features/identity/views/SessionsView.vue`: `DataTable` of sessions (`sessionApi` via authStore) — columns: device/browser icon (`i.pi`), IP, last active (relative via `src/shared/utils/date.ts`), current `Tag` "This device"; row action `Button` "Revoke" (`Tooltip` "End this session", `ConfirmPopup` confirm → revoke); empty `Message`. | | |
| TASK-030 | Verify Phase 4: type-check + lint + build clean; grep audit zero native interactive tags in `src/features/identity/**`; login/register specs (TASK-044) recreated and passing. | | |

### Implementation Phase 5 — Ordering (5 files)

- GOAL-005: Rebuild cart drawer, cart page, 5-step checkout, order list and order detail.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-031 | Create `src/features/ordering/components/CartDrawer.vue`: `Drawer` (position right, `cartStore` bound) — items as compact list (Image thumb, name, `InputNumber` qty min-1, line total, remove `Button text` `pi pi-trash` with `Tooltip`), `Divider` between subtotal and totals, free-shipping progress (`ProgressBar` bound to threshold), footer with `Button` "Checkout" (→ `/checkout`) and `Button text` "View Cart" (→ `/cart`); empty state `Message` + `Button` "Continue Shopping" → `/shop`. | | |
| TASK-032 | Create `src/features/ordering/views/CartView.vue`: full-page — `DataView` (grid layout) of line items (Image, name/sku, unit price, `InputNumber` qty, line total, remove `Button` with `Tooltip`), summary `Card` (subtotal, shipping estimate from `shippingStore`, tax, total — `useCurrency`, `Divider` before total), promo `InputGroup` (`InputText` + `Button` apply, `cartStore.applyCoupon`) with applied-coupon `Chip` (removable), `Button` "Proceed to Checkout" primary; empty `Message` + CTA. | | |
| TASK-033 | Create `src/features/ordering/views/CheckoutView.vue`: `Stepper` (v5 wizard; 5 `Step` in `StepList` + `StepPanels`/`StepPanel`: Shipping → Delivery → Payment → Review → Confirmation) driven by `checkoutStore.step`; per-step panels: (1) address form (`CascadeSelect` country→state from `useLocationCascade` + `FloatLabel` `InputText` + `InputMask` phone, `checkoutStore` + `addressStore`), (2) shipping method `RadioButtonGroup` of `RadioButton` from `shippingStore`, (3) payment: Stripe card via `usePayment` (Stripe Elements container — no native form controls), (4) review: `DataTable` items + totals + `ButtonGroup` (Back / `Button` "Place Order" via `checkoutStore.placeOrder`, `:loading` with `ProgressSpinner`), (5) confirmation: success `Message`/`i.pi pi-check-circle`, order number, `Button` → `/account/orders`; guards: if `cartStore` empty and not confirm step, redirect `/cart`; `Message` error on any step with API failure. | | |
| TASK-034 | Create `src/features/ordering/views/OrderListView.vue`: `DataTable` of orders (`orderStore`) — order number (link), date, total, status `Tag` (severity map from `orderStore`), items count; `Paginator`; empty `Message` + `Button` → `/shop`. | | |
| TASK-035 | Create `src/features/ordering/views/OrderDetailView.vue`: header row (order number + status `Tag` + `Button` "Track" opening `Dialog` with `Timeline` of status events); items `DataTable`; summary `Card` (subtotal, shipping, tax, total); shipping address block; "Reorder" `Button` (`cartStore.reorder`). | | |
| TASK-036 | Verify Phase 5: type-check + lint + build clean; grep audit zero native interactive tags in `src/features/ordering/**`; cart/checkout store tests still green. | | |

### Implementation Phase 6 — Profile (6 views)

- GOAL-006: Rebuild the six account-area views.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-037 | Create `src/features/profile/views/ProfileView.vue`: `Card` "Personal Information" — `Inplace` editable rows (`FloatLabel` `InputText` for first/last name, email readonly), `Button` save (`profileStore.updateProfile`, `:loading`, success toast); `Card` "Preferences summary" with `Button text` → `/account/preferences`. | | |
| TASK-038 | Create `src/features/profile/views/AddressBookView.vue`: `DataTable` of addresses (`addressStore`) — label `Tag`, lines, default `Tag`; row actions `Button` edit (opens `Dialog` with `Fluid` + `FloatLabel` form: `InputText` fields, `Label`-paired inputs, `CascadeSelect` country→state from `useLocationCascade`, `InputMask` phone, `Textarea` notes), `Button` delete (`Tooltip`, `ConfirmDialog` → `addressStore.remove`), `Button text` "Set default"; top `Button` "Add Address" (same `Dialog`); empty `Message`. | | |
| TASK-039 | Create `src/features/profile/views/WishlistsView.vue`: `Tabs` (one tab per wishlist) with per-tab `DataView` of `ProductCard` + `Button` remove item; "New list" `Button` → `Dialog` with `InputText` name; `Tag` item counts. | | |
| TASK-040 | Create `src/features/profile/views/NotificationPrefsView.vue`: `Card` list of preference rows — label + description, `ToggleSwitch` bound per channel (`notificationApi`-backed store); `Button` "Save" with success toast. | | |
| TASK-041 | Create `src/features/profile/views/PreferencesView.vue`: `Card` with `Select` (currency, locale) + `ToggleSwitch` (dark mode via `useTheme`) + `ToggleButton` (e.g., email receipts on/off) persisted via `usePreferences`/`profileStore.preferences`; `Button` save. | | |
| TASK-042 | Create `src/features/profile/views/ChangePasswordView.vue`: `Card` in `Fluid` — current `InputPassword`, new `InputPassword` + confirm `InputPassword` (strength meter), `ChangePasswordSchema` validation, `Button` save (`profileStore.changePassword`, `:loading`), `Message` success + `Button text` back to profile. | | |
| TASK-043 | Verify Phase 6: type-check + lint + build clean; grep audit zero native interactive tags in `src/features/profile/**`. | | |

### Implementation Phase 7 — Tests, audit & completion

- GOAL-007: Recreate the component/view test suite, run the full verification matrix, and prove zero native interactive elements remain and the full PrimeVue catalog is exercised.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-044 | Recreate view/component specs (TEST-001) — one spec per rebuilt file that previously had one, using `@vue/test-utils` + `createTestingPinia`: `AppHeader`, `ThemeToggle`, `layouts`, `ProductCard`, `HomeView`, `ShopView`, `LoginView`, `RegisterView`, `CartDrawer`. Each spec mounts with PrimeVue auto-imports, asserts the flow behavior (e.g., ShopView toggles taxon → `catalogStore.toggleTaxon` called; LoginView invalid submit shows validation `Message`) and asserts no `button`/`input` native elements are rendered by the component. | | |
| TASK-045 | Run full verification matrix in `app/Store/`: `pnpm run lint` (eslint+oxlint), `npx vue-tsc --build`, `npx vitest run` (all suites), `pnpm run build-only`. All must pass. | | |
| TASK-046 | Run the native-interactive audit (TEST-002) via `rg -l "<(button|input|select|textarea|label)" src --glob "*.vue"` — must return zero files; if nonzero, convert the offending element to its PrimeVue equivalent (map in GUD-001/PAT-003) and re-run. | | |
| TASK-047 | Run repo-wide checks per AGENTS.md: `dotnet build` unaffected (no C# changes expected — verify), `bash scripts/check-feature-conventions.sh`, `bash scripts/check-cross-module-refs.sh` unaffected (should pass as before). | | |
| TASK-048 | Create `scripts/check-primevue-coverage.sh` (FILE-010) and run it (TEST-005): it parses Table 1 rows from this plan (`^\| [A-Z][A-Za-z]+ \| (app|features)/`) and fails if any `<Component` tag is absent from its assigned file under `app/Store/src`. Every row must pass. | | |
| TASK-049 | Commit: `refactor(store): rebuild UI on PrimeVue 5 with full component coverage` — single commit on `feature/implement-storefront` including all rebuilt files and `scripts/check-primevue-coverage.sh`; `git status` must show zero deletions of non-`.vue` files (REQ-003). | | |

## 3. Alternatives

- **ALT-001**: Incrementally patch existing `.vue` files instead of deleting. Rejected — the files accumulated mixed native/PrimeVue implementations and drift (39+ violations); a clean rewrite guarantees uniformity and satisfies the explicit user directive.
- **ALT-002**: Adopt a full PrimeVue template (Sakai/Apollo from primevue.org) wholesale. Rejected — those templates assume a different data layer and would conflict with the existing stores/services/router; we reuse only their component choices, not their code.
- **ALT-003**: Use a Tailwind component library (e.g., shadcn-vue) for missing primitives. Rejected — CON-001/GUD-001 mandate PrimeVue 5 as the only component system.
- **ALT-004**: Rebuild using only the narrow component subset of plan v1.0 (≈30 components) instead of the full catalog. Rejected — the user directive requires the whole PrimeVue library to be exercised; REQ-009 + TEST-005 make coverage a hard gate.

## 4. Dependencies

- **DEP-001**: PrimeVue `^5.0.0` + `@primevue/auto-import-resolver` (installed, verified in `components.d.ts`).
- **DEP-002**: `@primeuix/themes` (Aura) + `tailwindcss-primeui` tokens — configure only via existing `src/app/providers/primevue.ts`.
- **DEP-003**: primeicons `^8.0.0` for `pi` icon classes.
- **DEP-004**: vee-validate `^4.15.1` + `@vee-validate/zod` + existing schemas in `src/features/*/validations/`.
- **DEP-005**: Non-UI layers — Pinia stores, API services, composables (`useTheme`, `useNotify`, `useLocationCascade`, `usePayment`, `useSearch`, `useDebounce`, `useMediaQuery`, `useQuickAdd`, `useRecentlyViewed`, `usePagedQuery`), router — verified green in Phase 1 (TASK-004) and untouched thereafter.
- **DEP-006**: `app/legacy/llms.txt` + https://primevue.dev/llms — authoritative PrimeVue component catalog used for the REQ-009 coverage matrix; matrix rows verified against `app/Store/node_modules/primevue/` availability (all 74 rows confirmed installed in v5.0.0).

## 5. Files

- **FILE-001**: `src/App.vue`; `src/app/layouts/DefaultLayout.vue`, `AuthLayout.vue`, `AccountLayout.vue`; `src/app/components/layout/AppHeader.vue`, `AppFooter.vue`, `MobileNav.vue`; `src/app/components/ThemeToggle.vue` — delete in TASK-001, recreate in TASK-006 to TASK-013.
- **FILE-002**: `src/features/catalog/components/ProductCard.vue`, `SearchOverlay.vue`, `TaxonTree.vue`; `src/features/catalog/views/HomeView.vue`, `ShopView.vue`, `ProductDetailView.vue`, `CollectionsView.vue`, `VisualSearchView.vue`, `NotFoundView.vue`, `AboutView.vue`, `TermsView.vue`, `PrivacyView.vue` — delete in TASK-001, recreate in TASK-015 to TASK-023.
- **FILE-003**: `src/features/identity/views/LoginView.vue`, `RegisterView.vue`, `ForgotPasswordView.vue`, `ResetPasswordView.vue`, `SessionsView.vue` — delete in TASK-001, recreate in TASK-025 to TASK-029.
- **FILE-004**: `src/features/ordering/components/CartDrawer.vue`; `src/features/ordering/views/CartView.vue`, `CheckoutView.vue`, `OrderListView.vue`, `OrderDetailView.vue` — delete in TASK-001, recreate in TASK-031 to TASK-035.
- **FILE-005**: `src/features/profile/views/ProfileView.vue`, `AddressBookView.vue`, `WishlistsView.vue`, `NotificationPrefsView.vue`, `PreferencesView.vue`, `ChangePasswordView.vue` — delete in TASK-001, recreate in TASK-037 to TASK-042.
- **FILE-006**: `src/app/components/layout/__tests__/AppHeader.spec.ts`; `src/app/components/__tests__/ThemeToggle.spec.ts`; `src/app/layouts/__tests__/layouts.spec.ts`; `src/features/catalog/components/__tests__/ProductCard.spec.ts`; `src/features/catalog/views/__tests__/HomeView.spec.ts`; `src/features/catalog/views/__tests__/ShopView.spec.ts`; `src/features/identity/views/__tests__/LoginView.spec.ts`; `src/features/identity/views/__tests__/RegisterView.spec.ts`; `src/features/ordering/components/__tests__/CartDrawer.spec.ts` — delete in TASK-002, recreate in TASK-044.
- **FILE-007**: `src/main.ts` — rewritten in TASK-005 (imports new `App.vue` only).
- **FILE-008**: `components.d.ts` — auto-regenerated by the PrimeVue auto-import resolver on each build; commit it if `git status` shows changes (it is currently gitignored; keep gitignored).
- **FILE-009**: `plan/refactor-store-primevue-overhaul-1.md` — this plan; Table 1 is the single source of truth parsed by `scripts/check-primevue-coverage.sh` (FILE-010); referenced by TASK-046 for the native-element audit mapping.
- **FILE-010**: `scripts/check-primevue-coverage.sh` — created in TASK-048; parses Table 1 from FILE-009 and asserts each component tag appears in its assigned file. Exact script body:

```bash
#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
PLAN="$ROOT/plan/refactor-store-primevue-overhaul-1.md"
STORE="$ROOT/app/Store/src"
fail=0
total=0
while IFS=$'\t' read -r comp file; do
  [ -z "$comp" ] && continue
  total=$((total + 1))
  if ! grep -qE "<${comp}([ />]|$)" "$STORE/$file" 2>/dev/null; then
    echo "MISSING: <$comp> not found in $STORE/$file"
    fail=1
  fi
done < <(grep -E '^\| [A-Z][A-Za-z]+ \| [^|]+ \| via ' "$PLAN" | awk -F'|' '{gsub(/[ ]+/,"",$2); gsub(/[ ]+/,"",$3); if ($2 != "" && $3 != "") print $2 "\t" $3}')
if [ "$fail" -eq 0 ]; then echo "PrimeVue coverage OK: $total matrix rows verified."; else echo "PrimeVue coverage FAILED."; fi
exit $fail
```


## 6. Testing

- **TEST-001**: Recreate 9 specs (listed in FILE-006) in TASK-044, each asserting: component mounts and renders expected PrimeVue elements; primary flow action (taxon toggle, submit with invalid → validation `Message`, cart add, filter clear) triggers the expected store call; no native interactive tags present in rendered output.
- **TEST-002**: Native-interactive audit — `rg -l "<(button|input|select|textarea|label)" src --glob "*.vue"` returns zero files (TASK-046); enforced at every phase verification (TASK-014, TASK-024, TASK-030, TASK-036, TASK-043).
- **TEST-003**: `npx vitest run` — full suite (existing 289 + recreated view specs) passes with zero failures (TASK-045).
- **TEST-004**: Static gates — `npx vue-tsc --build`, `pnpm run lint`, `pnpm run build-only` all pass with zero errors (TASK-045); repo scripts per TASK-047 unaffected.
- **TEST-005**: PrimeVue coverage audit — `bash scripts/check-primevue-coverage.sh` exits 0 with all Table 1 rows verified (TASK-048); the matrix parses from this plan, so component→file assignments are single-sourced.

## 7. Risks & Assumptions

- **RISK-001**: PrimeVue v5 API drift (e.g., `Galleria`, `Steps`, `Tree` selection-key shapes) could cause subtle behavior bugs. Mitigation: consult `app/Store/node_modules/primevue/<component>/*.vue` source before use; the taxonomy `Tree` (TASK-018) already proves the pattern.
- **RISK-002**: Recreated specs (TEST-001) may be flaky against auto-imported PrimeVue components (transition/stub needs). Mitigation: stub `transition` and `teleport` in specs, use `createTestingPinia`, follow existing passing spec patterns (e.g., `CartDrawer.spec.ts` before deletion).
- **RISK-003**: Dark-mode and theme-token regressions if `useTheme` binding is mis-wired in `App.vue` (TASK-006). Mitigation: TASK-006 binds the exact class the existing provider token set uses (`.app-dark`), verified by existing `useTheme.spec.ts` which stays untouched.
- **RISK-004**: The coverage gate (TEST-005) may flag a matrix component whose API differs from expectations (e.g., v5 `MegaMenu`, `Sidebar` compound nav, `CommandMenu`). Mitigation: consult `app/Store/node_modules/primevue/<component>/*.vue` before use; the gate parses Table 1, so a genuine redesign updates the plan matrix rather than weakening the gate.
- **ASSUMPTION-004**: All 70 matrix components exist in the installed PrimeVue 5.0.0 build (verified against `node_modules/primevue/` before the matrix was written); auto-import resolver covers each (re-verified by `components.d.ts` regeneration during TASK-045 builds).
- **ASSUMPTION-001**: All non-presentation layers (stores, services, types, validations, composables, router, guards) are complete and correct as of commit `eab1adfd`; Phase 1 (TASK-004) verifies this before any rebuild begins.
- **ASSUMPTION-002**: No backend, API, or C# changes are required; the contract types in `src/features/*/types` are final.
- **ASSUMPTION-003**: PrimeVue auto-import resolver covers every component used (verified per component via `components.d.ts` regeneration in TASK-008/TASK-045 builds).

## 8. Related Specifications / Further Reading

- [PrimeVue 5 Tree documentation](https://primevue.dev/tree/)
- [PrimeVue 5 component library](https://primevue.dev/components)
- [PrimeVue LLMs catalog](https://primevue.dev/llms) — source of the REQ-009 coverage matrix; local mirror at `app/legacy/llms.txt`
- [Store SPA design specs](https://github.com/anomalyco/ReSys.Shop/blob/main/docs/superpowers/specs/2026-08-08-store-spa-cycle2-catalog-design.md)
- [Repo AGENTS.md — verification commands and UI rules](AGENTS.md)
- [Previous plan: storefront-catalog-experience-1.md](plan/storefront-catalog-experience-1.md)
