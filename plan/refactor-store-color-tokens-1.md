---
goal: Rebuild Store SPA styling from scratch — delete assets folder, set up PrimeVue Tailwind per docs, fix MegaMenu dark colors (remove it), fix Accordion colors, fix taxonomy tree rendering, fix carousel card count, redesign shop grid, eliminate all raw palette violations, align everything to teal Aura light theme
version: 2.0
date_created: 2026-08-09
last_updated: 2026-08-09
owner: ng
status: 'Planned'
tags: ['refactor', 'design-tokens', 'tailwind', 'aura', 'store-spa', 'bug-fix', 'ui-redesign']
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Store SPA has **8 distinct issues** causing visual inconsistency and broken UX:
(1) MegaMenu renders with dark default colors in light mode — remove it entirely;
(2) Accordion in ShopFilterPanel has wrong colors — add token overrides;
(3) Taxonomy tree nested nodes don't render — fix Tree component binding;
(4) Carousel shows only 1 product card on desktop — fix responsive config;
(5) Shop grid layout is poorly designed — tighten spacing and card surfaces;
(6) 13 raw palette violations across 6 files bypass the semantic token layer;
(7) `assets/` contains 2 ad-hoc CSS files that should be rebuilt from scratch
following PrimeVue's official Tailwind integration guide;
(8) PrimeVue config is missing `darkModeSelector: false`. This plan deletes the
entire `assets/` folder, rebuilds Tailwind + PrimeVue theming per
https://primevue.dev/tailwind/ and https://primevue.dev/theming/styled/, fixes
all component-level issues, and eliminates every raw palette violation.

## 1. Requirements & Constraints

- **REQ-001**: Delete `app/Store/src/assets/` entirely and recreate with a single
  `main.css` (or `main.scss`) that follows PrimeVue's official Tailwind v4 integration:
  `@import 'tailwindcss'; @plugin 'tailwindcss-primeui';` plus `@theme inline` semantic
  tokens mapped to Aura `--p-*` vars.
- **REQ-002**: The PrimeVue Aura preset must be configured with teal as the primary color
  palette. Use `@primeuix/themes/aura` with `{ primary: 'teal' }` or equivalent palette
  config. The `:root` overrides in `styles.scss` (primary scale, surface scale, semantic
  roles) must be preserved in the new stylesheet.
- **REQ-003**: Remove the `<MegaMenu>` component from `AppHeader.vue` (line 113) and all
  related dead code (`catalogItems` computed, `toTaxonItem` helper, `TaxonTreeNode` import).
  Replace with a simple "Catalog" `<Button as="router-link" to="/shop">` link.
- **REQ-004**: Fix Accordion colors in `ShopFilterPanel.vue` by adding `--p-accordion-*`
  token overrides to the `:root` block. Also add `--p-dialog-background` for the mobile
  filter Drawer overlay.
- **REQ-005**: Fix taxonomy tree nested node rendering in `TaxonTree.vue`. Investigate
  and resolve why `Tree` component doesn't expand nested children (likely `leaf` prop or
  `expandedKeys` initialization issue).
- **REQ-006**: Fix `Carousel` in `HomeView.vue` to show 4 product cards on desktop by
  default (`numVisible="4"`), 3 at `1024px`, 2 at `768px`, 1 at `560px`.
- **REQ-007**: Redesign shop grid layout — tighten `gap-6` to `gap-4`, add Card surface
  token overrides (`--p-card-background`, `--p-card-border-color`, `--p-card-border-radius`).
- **REQ-008**: Replace all 13 raw palette violations with semantic tokens:
  - 9 class-based: `bg-primary-50` → `bg-highlight`, `text-primary-100` → `text-brand-subtle`,
    `bg-primary-500/10` → `bg-brand/10`, `bg-primary-100 text-primary-900` →
    `bg-brand-subtle text-brand-muted`.
  - 4 var-based: `var(--p-red-400)` → `var(--p-danger-color)`, etc.
- **REQ-009**: Set `darkModeSelector: false` in `primevue.ts` options to explicitly
  disable dark mode.
- **CON-001**: No `dark:` variants — the SPA is light-mode only (per
  `plan/refactor-store-lightmode-tokens-1.md`).
- **CON-002**: Warnings-as-errors globally; `vue-tsc`, `oxlint`, `eslint`, `vitest`,
  `build-only` must all pass with zero errors.
- **CON-003**: The auth brand panel gradient (`from-primary-950 via-primary-900 to-primary-600`)
  is decorative and intentional — keep it as-is. The `text-primary-100` on that dark
  panel is the correct light-on-dark treatment for the brand panel only.
- **CON-004**: Self-host Google Fonts (Inter, Newsreader, JetBrains Mono) as local
  `.woff2` files with `@font-face` declarations to eliminate the external runtime
  dependency.
- **GUD-001**: Follow the token role map: heading/body → `text-heading`/`text-body`;
  muted → `text-muted`; subtle → `text-subtle`; brand → `text-brand`/`bg-brand`;
  on-brand → `text-on-brand`; success/danger/warning/info → their semantic token.
- **PAT-001**: PrimeVue Tailwind integration must follow https://primevue.dev/tailwind/:
  `@import 'tailwindcss'` + `@plugin 'tailwindcss-primeui'` + `@theme inline` block.
  Theme configuration must follow https://primevue.dev/theming/styled/ with the Aura
  preset and teal color palette.

## 2. Implementation Steps

### Phase 1 — Delete Assets & Rebuild Tailwind + PrimeVue Theme Foundation

- GOAL-001: Delete `app/Store/src/assets/` entirely, recreate with a single `main.css`
  that follows PrimeVue's official Tailwind v4 integration guide, and preserve all
  existing `:root` token values plus component-level overrides.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `app/Store/src/assets/` directory entirely (`rm -rf app/Store/src/assets/`). This removes `tailwind.css` (717B) and `styles.scss` (3.7KB). | | |
| TASK-002 | Create `app/Store/src/assets/main.css` with the following structure in order: (1) `@import url(...)` for self-hosted fonts (Phase 4 placeholder — will be replaced in TASK-013); (2) `@import 'tailwindcss';` (3) `@plugin 'tailwindcss-primeui';` (4) `@theme inline` block with 11 semantic colors + 3 new tokens (see below); (5) `:root` block with all PrimeVue Aura overrides from the old `styles.scss`; (6) Typography rules (`body`, `h1-h6`); (7) Utility classes (`.font-editorial`, `.font-mono-price`, animations); (8) Accessibility `@media (prefers-reduced-motion)`. | | |
| TASK-003 | The `@theme inline` block must contain these 14 semantic colors: `--color-heading: var(--p-text-color);`, `--color-body: var(--p-text-color);`, `--color-muted: var(--p-text-muted-color);`, `--color-subtle: var(--p-surface-400);`, `--color-placeholder: var(--p-surface-300);`, `--color-brand: var(--p-primary-color);`, `--color-on-brand: var(--p-primary-contrast-color);`, `--color-success: var(--p-success-color);`, `--color-danger: var(--p-danger-color);`, `--color-warning: var(--p-warning-color);`, `--color-info: var(--p-info-color);`, `--color-highlight: var(--p-primary-50);`, `--color-brand-subtle: var(--p-primary-100);`, `--color-brand-muted: var(--p-primary-900);`. | | |
| TASK-004 | The `:root` block must contain ALL existing overrides from the old `styles.scss:8-68`: primary scale (`--p-primary-color` through `--p-primary-950`), surface scale (`--p-surface-0` through `--p-surface-950`), semantic roles (`--p-content-background`, `--p-text-color`, `--p-text-muted-color`, etc.), state colors (`--p-success-color`, `--p-warning-color`, `--p-danger-color`, `--p-info-color`), legacy aliases (`--p-surface-ground`, `--p-surface-card`). Copy verbatim — do not modify any hex values. | | |
| TASK-005 | Add NEW component-level token overrides to the `:root` block (these fix issues 2, 5, 7): Accordion: `--p-accordion-header-background: transparent;`, `--p-accordion-header-color: var(--p-text-color);`, `--p-accordion-header-hover-background: var(--p-surface-100);`, `--p-accordion-header-active-background: var(--p-primary-50);`, `--p-accordion-content-background: var(--p-surface-0);`, `--p-accordion-content-border-color: var(--p-surface-200);`. Card: `--p-card-background: var(--p-surface-0);`, `--p-card-border-color: var(--p-surface-200);`, `--p-card-border-radius: 0.75rem;`. Dialog/Drawer: `--p-dialog-background: var(--p-surface-0);`. | | |
| TASK-006 | Update `app/Store/src/main.ts` to import the new file: replace `import '@/assets/tailwind.css'` and `import '@/assets/styles.scss'` with single `import '@/assets/main.css'`. | | |
| TASK-007 | Verify the build compiles: `npx vue-tsc --build` (0), `pnpm exec oxlint .` (0), `pnpm exec eslint .` (0), `pnpm run build-only` (0) — all from `app/Store/`. | | |

### Phase 2 — Configure PrimeVue Dark Mode Off + Verify Theme

- GOAL-002: Explicitly disable dark mode in PrimeVue config and verify the Aura teal
  preset renders correctly.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | In `app/Store/src/app/providers/primevue.ts:13`, change `options: {}` to `options: { darkModeSelector: false }`. This tells PrimeVue 5 to never inject `.dark` class rules. | | |
| TASK-009 | Verify `pnpm run build-only` still exits 0 after the config change. | | |

### Phase 3 — Remove MegaMenu (Dark Color Fix)

- GOAL-003: Remove the MegaMenu from AppHeader that renders with dark default colors
  in light mode. Replace with a simple "Catalog" link to `/shop`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-010 | In `app/Store/src/app/components/layout/AppHeader.vue`: delete line 7 (`import type { TaxonTreeNode }`), lines 52-58 (`toTaxonItem` function), lines 61-69 (`catalogItems` computed), line 113 (`<MegaMenu .../>`). | | |
| TASK-011 | In the same file, add a "Catalog" `<Button>` as a router-link after the Menubar (line ~111): `<Button as="router-link" to="/shop" label="Catalog" variant="text" class="hidden lg:flex" />`. This preserves desktop catalog navigation without the broken MegaMenu. | | |
| TASK-012 | In `app/Store/src/app/components/layout/AppHeader.spec.ts`, remove the test case `renders MegaMenu tabs for taxonomy roots with children` (lines ~90-125). Add a test that verifies the "Catalog" button links to `/shop`. | | |

### Phase 4 — Fix Taxonomy Tree Rendering

- GOAL-004: Fix nested taxonomy tree nodes that don't expand/render in
  `TaxonTree.vue`.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-013 | In `app/Store/src/features/catalog/components/TaxonTree.vue`, investigate the `leaf` property issue. The `toTreeNode` function (line 14-21) sets `leaf: !node.hasChildren`. PrimeVue v5 Tree uses `leaf` to decide whether to render the expand toggle. If `hasChildren` is `false` but children exist (or vice versa), nodes won't expand. Fix: ensure `leaf` is only set when `children` is `undefined`. Change line 18 to: `leaf: node.children.length === 0 ? true : undefined` (PrimeVue treats `undefined` as "not a leaf"). | | |
| TASK-014 | Also in `TaxonTree.vue`, the `expandedKeys` watcher (lines 27-37) only seeds root-level expansion. When a user expands a child node and the component re-renders, the seed may override user toggles. Fix: change the merge order to `{ ...seed, ...expandedKeys.value }` → only seed keys not already toggled. Add `deep: true` to the watcher if needed. | | |
| TASK-015 | In `app/Store/src/features/catalog/components/ShopFilterPanel.vue:181`, the Accordion wraps TaxonTree. The `filter` prop on Tree (line 75 of TaxonTree) shows a search input but the filtering is internal state only. If the search input breaks nested display, remove `filter` and `filter-placeholder` props and use the existing `IconField` search above the Accordion instead. | | |
| TASK-016 | Verify taxonomy data loads: check `catalogStore.ts` `buildTree` function. The `buildTree` filters `i.taxonomyId === taxonomyId && i.parentId === parentId`. If the API returns items with missing `parentId` for root items (null vs undefined vs ''), the filter may exclude them. Verify and fix if needed. | | |

### Phase 5 — Fix Carousel Card Count

- GOAL-005: Fix the home page Carousel to show 4 product cards on desktop.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-017 | In `app/Store/src/features/catalog/views/HomeView.vue:96-101`, change the Carousel props from `:numVisible="1" :numScroll="1"` to `:numVisible="4" :numScroll="1"`. Update `responsiveOptions` to: `[{ breakpoint: '1024px', numVisible: 3, numScroll: 1 }, { breakpoint: '768px', numVisible: 2, numScroll: 1 }, { breakpoint: '560px', numVisible: 1, numScroll: 1 }]`. | | |

### Phase 6 — Fix Shop Grid Layout

- GOAL-006: Tighten the product grid spacing and card surface tokens.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-018 | In `app/Store/src/features/catalog/views/ShopView.vue:28`, change `gap-6` to `gap-4` in the grid class. Do the same for line 29 (`gap-6` → `gap-4` in list mode). | | |
| TASK-019 | In `app/Store/src/features/catalog/views/ShopView.vue:85`, change the loading skeleton grid `gap-6` to `gap-4` to match. | | |

### Phase 7 — Replace Raw Palette Violations in Templates

- GOAL-007: Replace all 9 class-based raw palette violations with semantic tokens.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-020 | `app/Store/src/app/layouts/AuthLayout.vue:23` — `text-primary-100` → `text-brand-subtle` (sparkles icon in dark brand panel). | | |
| TASK-021 | `app/Store/src/app/layouts/AuthLayout.vue:27` — `text-primary-100` → `text-brand-subtle` (tagline paragraph). | | |
| TASK-022 | `app/Store/src/app/layouts/AuthLayout.vue:38` — `bg-primary-500/10` → `bg-brand/10` (mobile brand mark bg). | | |
| TASK-023 | `app/Store/src/app/layouts/AccountLayout.vue:83` — `bg-primary-50` → `bg-highlight` (mobile drawer active nav). | | |
| TASK-024 | `app/Store/src/app/layouts/AccountLayout.vue:122` — `bg-primary-50` → `bg-highlight` (desktop sidebar active nav). | | |
| TASK-025 | `app/Store/src/app/components/layout/MobileNav.vue:50` — `bg-primary-50` → `bg-highlight` (mobile drawer active nav). | | |
| TASK-026 | `app/Store/src/app/components/layout/AppHeader.vue:180` — `bg-primary-100` → `bg-brand-subtle`; `text-primary-900` → `text-brand-muted` (avatar badge). | | |
| TASK-027 | `app/Store/src/features/catalog/views/HomeView.vue:53` — `from-primary-50 via-surface-0 to-primary-100` → `from-highlight via-surface-0 to-brand-subtle` (hero section gradient). | | |
| TASK-028 | `app/Store/src/features/catalog/views/HomeView.vue:72` — `from-primary-500 via-primary-700 to-primary-950` → keep as-is. This is a decorative dark gradient panel (like the auth brand panel) — it uses Aura's primary scale intentionally. Add a comment documenting this exception. | | |

### Phase 8 — Replace Raw Palette Vars in TypeScript

- GOAL-008: Replace all 4 `var(--p-{color}-{shade})` references with semantic tokens.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-029 | `app/Store/src/features/identity/composables/usePasswordStrength.ts:4` — `var(--p-red-400)` → `var(--p-danger-color)`. | | |
| TASK-030 | `app/Store/src/features/identity/composables/usePasswordStrength.ts:5` — `var(--p-amber-400)` → `var(--p-warning-color)`. | | |
| TASK-031 | `app/Store/src/features/identity/composables/usePasswordStrength.ts:6` — `var(--p-blue-400)` → `var(--p-info-color)`. | | |
| TASK-032 | `app/Store/src/features/identity/composables/usePasswordStrength.ts:7` — `var(--p-emerald-400)` → `var(--p-success-color)`. | | |

### Phase 9 — Self-Host Google Fonts

- GOAL-009: Eliminate the external Google Fonts `@import url(...)` runtime dependency
  by self-hosting Inter, Newsreader, and JetBrains Mono as local `.woff2` files.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-033 | Download `.woff2` files for Inter (400, 500, 600, 700), Newsreader (400, 400i, 600, 600i), JetBrains Mono (500). Place in `app/Store/src/assets/fonts/`. Use `google-webfonts-helper` or direct Google Fonts CSS API with `&subset=latin&text=` to minimize file sizes. | | |
| TASK-034 | In `app/Store/src/assets/main.css`, replace the `@import url(...)` placeholder (TASK-002) with `@font-face` declarations for each font file. Use `font-display: swap` and `url('./fonts/{file}.woff2')` for each. | | |
| TASK-035 | Verify `pnpm run build-only` resolves the font URLs (Vite handles `url()` in CSS automatically). | | |

### Phase 10 — Audit & Gate

- GOAL-010: Verify zero raw palette violations remain and all gates pass.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-036 | Run raw-palette audit: `rg -n 'class="[^"]*primary-[0-9]' app/Store/src/ --glob '*.vue'` → only `AuthLayout.vue:19` (the allowed gradient per CON-003). | | |
| TASK-037 | Run var audit: `rg -n 'var\(--p-(red\|amber\|blue\|emerald)-' app/Store/src/ --glob '*.ts' --glob '*.vue'` → zero results. | | |
| TASK-038 | Run light-mode token audit: `bash scripts/check-store-lightmode-tokens.sh` → `Light-mode token audit OK`. | | |
| TASK-039 | Run full gate battery from `app/Store/`: `npx vue-tsc --build` (0), `pnpm exec oxlint .` (0), `pnpm exec eslint .` (0), `npx vitest run --test-timeout=60000` (all green), `pnpm run build-only` (0). | | |
| TASK-040 | From repo root: `bash scripts/check-primevue-coverage.sh` → 70 matrix rows OK. | | |
| TASK-041 | Visual spot-check: (a) AppHeader shows "Catalog" button, no MegaMenu; (b) ShopFilterPanel Accordion renders with correct light colors; (c) TaxonTree nested nodes expand/collapse; (d) Home carousel shows 4 cards on desktop; (e) Shop grid has tight gap-4 spacing; (f) All semantic tokens render correctly. | | |

## 3. Alternatives

- **ALT-001**: Fix MegaMenu colors by adding `--p-megamenu-*` token overrides instead of
  removing it. Rejected: the MegaMenu is a complex component with 20+ internal tokens;
  fixing them all is fragile and the "Catalog" link is simpler and more maintainable.
- **ALT-002**: Keep the two-file CSS setup (`tailwind.css` + `styles.scss`). Rejected:
  the user explicitly asked to delete assets and rebuild from scratch.
- **ALT-003**: Use PrimeVue's `definePreset` to customize the Aura theme at the JS level.
  Rejected: `styles.scss` `:root` overrides are simpler, more transparent, and already
  proven to work (per the predecessor light-mode plan).
- **ALT-004**: Add MegaMenu-specific CSS overrides (`--p-megamenu-panel-background`,
  etc.) to fix the dark color issue. Rejected: the MegaMenu's dark defaults come from
  Aura's `overlay.navigation.background` token; overriding it globally would affect
  other overlay components.

## 4. Dependencies

- **DEP-001**: `tailwindcss` v4 with `@theme inline` support (already installed).
- **DEP-002**: `tailwindcss-primeui` plugin (already installed).
- **DEP-003**: `@primeuix/themes/aura` preset (already installed).
- **DEP-004**: Google Fonts API access for downloading `.woff2` files (one-time in TASK-033).
- **DEP-005**: Vite's built-in CSS `url()` handling for font resolution.

## 5. Files

- **FILE-001**: `app/Store/src/assets/` — DELETE entire directory, recreate with `main.css` + `fonts/`.
- **FILE-002**: `app/Store/src/assets/main.css` — new single entry point (Tailwind + tokens + typography + fonts).
- **FILE-003**: `app/Store/src/assets/fonts/*.woff2` — self-hosted font files (9 files).
- **FILE-004**: `app/Store/src/main.ts` — update import to `@/assets/main.css`.
- **FILE-005**: `app/Store/src/app/providers/primevue.ts` — add `darkModeSelector: false`.
- **FILE-006**: `app/Store/src/app/components/layout/AppHeader.vue` — remove MegaMenu, add Catalog button.
- **FILE-007**: `app/Store/src/app/components/layout/AppHeader.spec.ts` — remove MegaMenu test, add Catalog test.
- **FILE-008**: `app/Store/src/app/layouts/AuthLayout.vue` — 3 token replacements (TASK-020..022).
- **FILE-009**: `app/Store/src/app/layouts/AccountLayout.vue` — 2 token replacements (TASK-023..024).
- **FILE-010**: `app/Store/src/app/components/layout/MobileNav.vue` — 1 token replacement (TASK-025).
- **FILE-011**: `app/Store/src/features/catalog/components/TaxonTree.vue` — fix leaf/expansion (TASK-013..015).
- **FILE-012**: `app/Store/src/features/catalog/components/ShopFilterPanel.vue` — Accordion uses fixed tokens.
- **FILE-013**: `app/Store/src/features/catalog/views/HomeView.vue` — fix carousel + 2 token replacements.
- **FILE-014**: `app/Store/src/features/catalog/views/ShopView.vue` — tighten grid gap.
- **FILE-015**: `app/Store/src/features/identity/composables/usePasswordStrength.ts` — 4 var replacements.

## 6. Testing

- **TEST-001**: `rg -n 'class="[^"]*primary-[0-9]' app/Store/src/ --glob '*.vue'` → only AuthLayout.vue:19.
- **TEST-002**: `rg -n 'var\(--p-(red|amber|blue|emerald)-' app/Store/src/` → zero results.
- **TEST-003**: `bash scripts/check-store-lightmode-tokens.sh` → `Light-mode token audit OK`.
- **TEST-004**: `npx vue-tsc --build` → exit 0.
- **TEST-005**: `pnpm exec oxlint . && pnpm exec eslint .` → 0 warnings/errors.
- **TEST-006**: `npx vitest run --test-timeout=60000` → all tests pass.
- **TEST-007**: `pnpm run build-only` → exit 0 (verifies font url() resolution).
- **TEST-008**: `bash scripts/check-primevue-coverage.sh` → 70 matrix rows OK.
- **TEST-009**: Visual: AppHeader has no MegaMenu, shows "Catalog" link; ShopFilterPanel Accordion renders with correct light colors; TaxonTree nested nodes expand; Home carousel shows 4 cards; Shop grid uses tight spacing.

## 7. Risks & Assumptions

- **RISK-001**: Taxonomy tree fix (TASK-013) may not fully resolve if the issue is API
  data quality (missing `parentId` on root items). Mitigation: TASK-016 verifies the
  data shape; if API data is the root cause, document and escalate.
- **RISK-002**: Tailwind v4 `@import 'tailwindcss'` + `@plugin` inside `.css` (not `.scss`)
  may not work with the existing Vite plugin. Mitigation: `.css` is the standard Tailwind
  v4 entry format; if issues arise, use `.scss` with the Tailwind Vite plugin configured.
- **RISK-003**: Self-hosted `.woff2` files increase repo size by ~200-400 KB. Acceptable
  trade-off for eliminating the external runtime dependency.
- **ASSUMPTION-001**: The `tailwindcss-primeui` plugin correctly bridges `--p-*` CSS vars
  to Tailwind utility classes (verified in the predecessor light-mode plan).
- **ASSUMPTION-002**: The 3 new semantic tokens (`highlight`, `brand-subtle`, `brand-muted`)
  do not conflict with existing Tailwind or PrimeVue color names.
- **ASSUMPTION-003**: PrimeVue v5 Accordion component respects the `--p-accordion-*`
  token overrides (standard pattern per PrimeVue theming docs).

## 8. Related Specifications / Further Reading

- `plan/refactor-store-lightmode-tokens-1.md` — predecessor plan that established the
  semantic token layer and stripped dark mode.
- https://primevue.dev/tailwind/ — PrimeVue v5 official Tailwind integration guide.
- https://primevue.dev/theming/styled/ — PrimeVue styled theming with color palette.
- `app/Store/src/app/providers/primevue.ts` — PrimeVue Aura preset configuration.
- `scripts/check-store-lightmode-tokens.sh` — CI audit gate for light-mode token compliance.
