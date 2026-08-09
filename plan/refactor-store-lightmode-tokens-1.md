---
goal: Remove dark mode + theme toggle from the Store SPA (light mode only) and unify design-token usage across all components, views, and layouts
version: 1.0
date_created: 2026-08-09
owner: storefront-ui
status: 'Planned'
tags: [refactor, design, frontend, tokens, lightmode]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The Store SPA (`app/Store`) was rebuilt on PrimeVue 5 with a dark/light theme system: a `useTheme` composable toggles an `app-dark` class, a `darkModeSelector` primes the Aura preset, a header `ToggleSwitch` and a Preferences row expose the toggle, and 23 `.vue` files carry `dark:` variant classes plus a `.app-dark` block in `styles.scss`. This plan deletes that entire dark-mode surface, leaving a deliberate light-only identity, and then fixes token inconsistency: the same semantic role (muted text, borders, placeholder tiles, brand accents) is currently expressed with different `surface-*`/`primary-*` steps per file, and two views leak raw Tailwind palette colors (`text-green-500`, `text-white/80`). The plan introduces a single canonical semantic token layer (Tailwind v4 `@theme inline` mapped to PrimeVue's `--p-*` variables) and normalizes every component and view against it, enforced by a new audit script so the consistency is durable.

## 1. Requirements & Constraints

- **REQ-001**: Remove every piece of the dark-mode runtime: `useTheme` composable, `ThemeToggle.vue`, the `app-dark` class, PrimeVue `darkModeSelector`, and the `resys_theme` localStorage key.
- **REQ-002**: Light mode only — the app must render correctly with no `dark:` variant anywhere, no `.app-dark` selector, and no `prefers-color-scheme` handling in `app/Store/src`.
- **REQ-003**: Establish one canonical semantic token layer in `app/Store/src/assets/tailwind.css` (Tailwind v4 `@theme inline`) mapping semantic roles to the Aura `--p-*` variables; every text/border/status color in a `.vue` file must resolve to a token from that layer, the `primary-*` scale, or the `surface-*` scale.
- **REQ-004**: Zero raw-palette leakage — no Tailwind default palette classes (`green-`, `red-`, `gray-`, `white-`, `slate-`, `zinc-`, `neutral-`, `stone-`, `blue-`, `yellow-`, `sky-`, `indigo-`, `violet-`, `purple-`, `pink-`, `rose-`, `orange-`, `lime-`, `cyan-`, `fuchsia-`, `amber-`, `emerald-`, `teal-`) and no hex values (`#rgb`/`#rrggbb`) inside `.vue` files.
- **REQ-005**: Preserve the deliberate brand moments that are dark-by-design in light mode: the HomeView hero banner panel (`from-primary-500 via-primary-700 to-primary-950` gradient) and the AuthLayout brand panel (`from-primary-950 via-primary-900 to-primary-600` gradient). These are brand accents, not theme-dependent surfaces; their on-brand text tokens are kept.
- **SEC-001**: Styling-only change. No modification to auth, checkout, cart, order, search, or filter behavior; no changes to store logic, API contracts, or route names.
- **CON-001**: PrimeVue 5 + Aura preset remains the theme base. Only the `theme.options` object (drop `darkModeSelector`) and the CSS override layer (`styles.scss`) change; `tailwindcss-primeui` plugin and its `primary-*`/`surface-*` Tailwind mapping stay in place.
- **CON-002**: No new npm dependencies, no new Vue components, no new routes. One new shell audit script under `scripts/`.
- **CON-003**: Follow the repository Code Commenting Standard v3.0 (`app/Store/AGENTS.md`): keep every `// Label:` script comment and `<!-- Section: ... -->` template comment intact; when a task edits a commented block, update the comment text to match the new behaviour (no stale comments, AP-5).
- **CON-004**: Warnings-as-errors. All gates must stay green after every phase: `npx vue-tsc --build`, `pnpm exec oxlint .`, `pnpm exec eslint .`, `npx vitest run --test-timeout=60000`, `pnpm run build-only`, and from repo root `bash scripts/check-primevue-coverage.sh` (70 matrix rows).
- **GUD-001**: Apply the frontend-design skill to the light-only identity: warm-neutral Aura surfaces (`surface-0` white → `surface-50` `#fafafa`), deep-teal brand (`#0d7377`), editorial serif accents (`Newsreader`) and monospace prices (`JetBrains Mono`) stay exactly as defined in `styles.scss` `:root`; the design is disciplined around the two retained dark brand blocks (REQ-005) rather than scattered dark surfaces.
- **GUD-002**: One semantic role maps to exactly one token value. The canonical table in TASK-016 is authoritative; do not introduce a second `surface-*` step for a role that already has a token.
- **GUD-003**: Keep utility-only changes in the template section of each `.vue` file (class attribute edits); never restructure template DOM, slots, or PrimeVue props as part of this plan.
- **PAT-001**: Tailwind v4 `@theme inline { --color-<role>: var(--p-<var>); }` pattern for semantic tokens (already the mechanism `tailwindcss-primeui` uses for `primary-*`/`surface-*`).
- **PAT-002**: Delete-then-verify ordering: Phase 1–3 remove dark-mode surface, Phase 4 adds the semantic layer, Phase 5 normalizes against it, Phase 6 guards and verifies. Each phase is independently verifiable (see phase GOALs).

## 2. Implementation Steps

### Implementation Phase 1 — Remove dark-mode runtime

- GOAL-001: Delete the dark-mode engine (`useTheme` + `ThemeToggle`), its PrimeVue wiring, its storage key, and its App.vue binding; the SPA no longer applies or reacts to a dark class.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | Delete `app/Store/src/shared/composables/useTheme.ts` and `app/Store/src/shared/composables/__tests__/useTheme.spec.ts`. Edit `app/Store/src/shared/composables/index.ts` to remove the line `export { useTheme } from './useTheme'`. Completion: `git rm` of both files, index.ts has no `useTheme` reference, `rg -n "useTheme" app/Store/src --glob "*.ts"` returns zero rows. | | |
| TASK-002 | Delete `app/Store/src/app/components/ThemeToggle.vue` and `app/Store/src/app/components/__tests__/ThemeToggle.spec.ts`. The component is dead code — its only references in the codebase are its own spec (verified: AppHeader renders an inline `ToggleSwitch`, it does not import `ThemeToggle`). Completion: both files removed, `rg -n "ThemeToggle" app/Store/src` returns zero rows. | | |
| TASK-003 | Edit `app/Store/src/app/providers/primevue.ts`: inside `theme.options`, remove the `darkModeSelector: '.app-dark',` property. Leave `preset: Aura` unchanged. Completion: `theme` object is `{ preset: Aura, options: {} }` or the empty `options` block removed; `rg -n "darkModeSelector" app/Store/src` returns zero rows. | | |
| TASK-004 | Edit `app/Store/src/shared/constants/storage.ts`: remove the `THEME: 'resys_theme',` entry. Completion: `rg -n "resys_theme|THEME" app/Store/src/shared/constants/storage.ts` returns zero rows and no other file reads `THEME` (verified pre-change: only `useTheme.ts` consumed it). | | |
| TASK-005 | Edit `app/Store/src/App.vue`: remove `import { useTheme } from '@/shared/composables/useTheme'`, remove `const { isDark, init } = useTheme()`, remove `init()` from the `onMounted` callback, and remove the `:class="{ 'app-dark': isDark }"` binding on the root `<div>` (keep the `Toast`, `ConfirmDialog`, and `RouterView` children). Keep the Ctrl+K global handler and its `onMounted`/`onUnmounted` listener wiring. Completion: App.vue has no `useTheme`, no `isDark`, no `app-dark`; vue-tsc passes. | | |
| TASK-006 | Edit `app/Store/src/app/components/layout/AppHeader.vue`: remove `import { useTheme } ...`, remove `const { isDark, toggle } = useTheme()`, and remove the `<!-- Theme Toggle: ... -->` ToggleSwitch block (the `ToggleSwitch` with `:model-value="isDark"` / `@update:model-value="toggle"`, lines 145–151). Completion: AppHeader has no `useTheme`, no `isDark`, no `toggle`; the header still contains the mobile-nav trigger, brand, Menubar, MegaMenu, AutoComplete, search trigger, cart buttons, and account menu. | | |

### Implementation Phase 2 — Remove theme UI rows

- GOAL-002: Remove the user-facing dark-mode affordances (Preferences toggle row, Profile summary row) so no view exposes theme control.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-007 | Edit `app/Store/src/features/profile/views/PreferencesView.vue`: remove `import { useTheme } ...`, remove `const { isDark, toggle } = useTheme()`, and remove the entire "Dark mode" row block (`<div class="flex items-center justify-between gap-4 border-t border-surface-100 pt-4 dark:border-surface-800">...` containing the `Dark mode` label + `ToggleSwitch`). On the remaining "Email receipts" row, change `border-t border-surface-100 pt-4 dark:border-surface-800` to `border-t border-surface-100 pt-4` (drop the `dark:` variant only). Keep the currency/language selects, the email-receipts ToggleButton, the error Message, and the Save button. Completion: PreferencesView has no `useTheme`, no `Dark mode`, no `dark:`; the `toggle`/`isDark` identifiers are gone. | | |
| TASK-008 | Edit `app/Store/src/features/profile/views/ProfileView.vue`: remove `import { useTheme } ...`, remove `const { isDark } = useTheme()`, and remove the "Theme" summary row in the Preferences summary Card (the `<div class="flex items-center justify-between">` containing `Theme` / `{{ isDark ? 'Dark' : 'Light' }}`). Keep the Currency and Language rows, the Divider, and the "Edit preferences" button. Completion: ProfileView has no `useTheme`, no `Theme`, no `isDark`; the Preferences card still shows currency and language. | | |

### Implementation Phase 3 — Strip dark variants and collapse paired tokens

- GOAL-003: Delete the `.app-dark` CSS block and every `dark:` variant class across the 23 affected `.vue` files, collapsing each `light dark:dark` pair to the light token; the codebase is light-mode-only.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-009 | Edit `app/Store/src/assets/styles.scss`: delete the entire `.app-dark { ... }` block (currently lines 65–104). Update the file-header comment (line 1) from "Storefront Aura theme overrides — Farfetch-inspired luxury fashion + teal." to state that the file is the single light-mode token source for the Store SPA. Keep all `:root` tokens unchanged (primary scale, surface scale, semantic tokens, state colors, and the `--p-surface-ground`/`--p-surface-card` legacy aliases). Keep the Typography, editorial/mono font, animations, and `prefers-reduced-motion` sections. Completion: `rg -n "app-dark" app/Store/src/assets/styles.scss` returns zero rows and the `:root` block is untouched. | | |
| TASK-010 | Strip `dark:` variants from the 6 shell/layout files. For each, delete the `dark:` token from every paired class and keep the light token, per the canonical light values: (a) `app/Store/src/app/components/layout/AppHeader.vue` — `border-b border-surface-200 bg-surface-0/80 backdrop-blur dark:border-surface-800 dark:bg-surface-950/80` → `border-b border-surface-200 bg-surface-0/80 backdrop-blur`; avatar badge `bg-primary-100 text-primary-900 dark:bg-primary-900 dark:text-primary-100` → `bg-primary-100 text-primary-900`. (b) `app/Store/src/app/components/layout/AppFooter.vue` — `bg-surface-50 dark:bg-surface-950`, `border-t border-surface-200 dark:border-surface-800`, headings `text-surface-900 dark:text-surface-100` → `text-surface-900`, body `text-surface-500 dark:text-surface-400` → `text-surface-500`. (c) `app/Store/src/app/components/layout/MobileNav.vue` — active `bg-primary-50 font-semibold text-primary-800 dark:bg-primary-950 dark:text-primary-200` → `bg-primary-50 font-semibold text-primary-800`; inactive `text-surface-700 dark:text-surface-300` → `text-surface-700`. (d) `app/Store/src/app/layouts/AccountLayout.vue` — auth-fallback `bg-surface-50 p-8 dark:bg-surface-950` → `bg-surface-50 p-8`; aside `border-surface-200 ... lg:bg-surface-50 dark:border-surface-800 dark:bg-surface-950` → `... lg:bg-surface-50`; sidebar "Account" header `text-surface-900 dark:text-surface-100` → `text-surface-900`; both nav RouterLink ternaries → keep `bg-primary-50 ... text-primary-800` active and `text-surface-700` inactive; mobile top-bar `border-surface-200 dark:border-surface-800` → `border-surface-200`. (e) `app/Store/src/app/layouts/AuthLayout.vue` — no `dark:` variants exist; verify none were introduced. Completion: `rg -n "dark:"` over these 5 files returns zero rows. | | |
| TASK-011 | Strip `dark:` variants from the 10 catalog files, keeping the light token in each pair: `HomeView.vue` (hero gradient `from-primary-50 via-surface-0 to-primary-100 dark:from-primary-950 dark:via-surface-950 dark:to-primary-900` → drop the three `dark:from/via/to` classes; headings `text-surface-900 dark:text-surface-100` → `text-surface-900`; body `text-surface-500 dark:text-surface-400` → `text-surface-500`; benefits strip `border-surface-200 bg-surface-50 dark:border-surface-800 dark:bg-surface-950` → `border-surface-200 bg-surface-50`), `CollectionsView.vue` (heading `text-surface-900 dark:text-surface-100`, body `text-surface-400` pair, tile `bg-surface-50 dark:bg-surface-800` → `bg-surface-50`), `ProductDetailView.vue` (headings/titles `text-surface-900 dark:text-surface-100` → `text-surface-900`, body `text-surface-400` pair, tile `bg-surface-50 dark:bg-surface-800` → `bg-surface-50`, `border-surface-100 dark:border-surface-800` → `border-surface-100`), `ProductCard.vue` (price `text-primary-600 dark:text-primary-400` → `text-primary-600`, tile `bg-surface-50 dark:bg-surface-800` → `bg-surface-50`), `ShopFilterPanel.vue` (`text-surface-600 dark:text-surface-300` → `text-surface-600`, `text-surface-700 dark:text-surface-200` → `text-surface-700`), `VisualSearchView.vue`, `AboutView.vue`, `TermsView.vue`, `PrivacyView.vue` (each: heading `text-surface-900 dark:text-surface-100` → `text-surface-900`, body/`text-surface-400`/`text-surface-500` dark pairs → drop `dark:`), `NotFoundView.vue` (verify none). Completion: `rg -n "dark:"` over these 10 files returns zero rows. | | |
| TASK-012 | Strip `dark:` variants from the 8 ordering + remaining profile files, keeping the light token in each pair: `CheckoutView.vue` (`border-surface-300 dark:border-surface-700` → `border-surface-300`, `text-surface-400` pair), `CartView.vue` (`border-surface-200 dark:border-surface-800` → `border-surface-200`, `bg-surface-50 dark:bg-surface-800` → `bg-surface-50`, `text-surface-400` pair), `CartDrawer.vue` (`bg-surface-50 dark:bg-surface-800` → `bg-surface-50`, `text-surface-400` pair), `OrderDetailView.vue` (`text-surface-400` pair), `OrderListView.vue` (`text-primary-700 dark:text-primary-300` → `text-primary-700`), `WishlistsView.vue` (`bg-surface-100 text-surface-500 dark:bg-surface-800` → `bg-surface-100 text-surface-500`, `border-surface-100 dark:border-surface-800` → `border-surface-100`), `AddressBookView.vue` (`border-surface-200 dark:border-surface-800` → `border-surface-200`, `text-surface-400` pair), `NotificationPrefsView.vue` (`border-surface-100 dark:border-surface-800` → `border-surface-100`). Completion: `rg -n "dark:"` over these 8 files returns zero rows. | | |
| TASK-013 | Audit the whole SPA: run `rg -n "dark:" app/Store/src --glob "*.vue" --glob "*.scss" --glob "*.css"` from repo root and confirm zero matches across all files. Also run `rg -n "prefers-color-scheme|app-dark|resys_theme" app/Store/src` and confirm zero matches. Completion: both commands return zero rows and exit non-zero (no matches). | | |

### Implementation Phase 4 — Canonical semantic token layer

- GOAL-004: Add a single semantic token layer to `tailwind.css` (Tailwind v4 `@theme inline`) so text roles and status colors resolve to the Aura `--p-*` variables and are consistent everywhere.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-014 | Edit `app/Store/src/assets/tailwind.css`: keep the Google Fonts `@import`, `@import 'tailwindcss'`, and `@plugin 'tailwindcss-primeui'` lines, and append this `@theme inline` block (exact content): `@theme inline { --color-heading: var(--p-text-color); --color-body: var(--p-text-color); --color-muted: var(--p-text-muted-color); --color-subtle: var(--p-surface-400); --color-placeholder: var(--p-surface-300); --color-brand: var(--p-primary-color); --color-on-brand: var(--p-primary-contrast-color); --color-success: var(--p-success-color); --color-danger: var(--p-danger-color); --color-warning: var(--p-warning-color); --color-info: var(--p-info-color); }`. This makes `text-heading`, `text-body`, `text-muted`, `text-subtle`, `text-placeholder`, `text-brand`, `text-on-brand`, `text-success`, `text-danger`, `text-warning`, `text-info` (plus `bg-*`/`border-*` variants) available. Completion: `rg -n "--color-(heading|body|muted|subtle|placeholder|brand|on-brand|success|danger|warning|info)" app/Store/src/assets/tailwind.css` returns the 11 rows. | | |
| TASK-015 | Edit `app/Store/src/assets/styles.scss`: after TASK-009 the file is the sole light token source. Add a short `// --- Semantic role map (consumed by tailwind.css @theme inline) ---` comment block above the `--p-text-color` definition listing the role→variable mapping from TASK-014 (heading/body→`--p-text-color`, muted→`--p-text-muted-color`, subtle→`--p-surface-400`, placeholder→`--p-surface-300`, brand→`--p-primary-color`, on-brand→`--p-primary-contrast-color`, success/danger/warning/info→their `--p-*` vars). No token values change. Completion: the comment exists and `:root` values are byte-identical to the pre-change file. | | |

### Implementation Phase 5 — Normalize token usage across components and views

- GOAL-005: Replace the inconsistent `text-surface-*`/`text-primary-*` steps and the raw palette leaks with the canonical semantic tokens from TASK-014 across every component, view, and layout.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-016 | Normalize text tokens across all `.vue` files to the canonical role table (applies after Phase 3 stripping). Authoritative mapping: headings and strong titles → `text-heading` (replaces `text-surface-900`); inactive nav links and emphasis body copy → `text-body` (replaces `text-surface-700`); secondary labels, descriptions, helper text, and line-through compare-at → `text-muted` (replaces `text-surface-500` and `text-surface-600` in label/caption positions); faint hints and keyboard shortcuts → `text-subtle` (replaces `text-surface-400`); image-placeholder icons (`pi-image` in placeholder tiles) → `text-placeholder` (replaces `text-surface-300`); brand accents — prices, icons, links, active nav states → `text-brand` (replaces `text-primary-400/500/600/700/800`), except on the two dark brand blocks where light text stays `text-on-brand` (replaces `text-surface-0` and `text-white/80` on the HomeView banner panel and the AuthLayout brand panel; `text-primary-100` accents on those panels stay as primary-scale tint accents). Files to normalize (19): AppHeader.vue, AppFooter.vue, MobileNav.vue, AccountLayout.vue, AuthLayout.vue, HomeView.vue, CollectionsView.vue, ProductDetailView.vue, ProductCard.vue, ShopFilterPanel.vue, VisualSearchView.vue, AboutView.vue, TermsView.vue, PrivacyView.vue, NotFoundView.vue, CartDrawer.vue, CartView.vue, CheckoutView.vue, OrderDetailView.vue, OrderListView.vue, WishlistsView.vue, AddressBookView.vue, NotificationPrefsView.vue, PreferencesView.vue, ProfileView.vue, SearchOverlay.vue. Completion: `rg -nE "text-surface-[1-9][0-9]*" app/Store/src --glob "*.vue"` returns zero rows (only `text-surface-0` may remain for text directly on a pure-white on-dark chip if needed). | | |
| TASK-017 | Replace the two raw-palette leaks: (a) `app/Store/src/features/ordering/views/CheckoutView.vue` line ~431 `text-green-500` → `text-success` on the success-check icon; (b) `app/Store/src/features/catalog/views/HomeView.vue` line ~73 `text-white/80` → `text-on-brand/80` on the banner-panel sparkle icon. Completion: `rg -nE "class=\"[^\"]*(green-|red-|gray-|white-|black-|slate-|zinc-|neutral-|stone-|blue-|yellow-|sky-|indigo-|violet-|purple-|pink-|rose-|orange-|lime-|cyan-|fuchsia-|amber-|emerald-|teal-)[0-9]" app/Store/src --glob "*.vue"` returns zero rows. | | |
| TASK-018 | Normalize border and placeholder-tile tokens across all `.vue` files: every `border-surface-100` and `border-surface-300` divider/card border → `border-surface-200` (canonical, equals `--p-content-border-color`); every image/empty-state placeholder tile that used `bg-surface-50` → `bg-surface-100` (matches the tile already used in `WishlistsView.vue`), keeping the `text-placeholder` icon inside it. Layout surfaces that are page backgrounds (`bg-surface-50` on `body`, AccountLayout aside, AppFooter, HomeView benefits strip, auth fallback) stay `bg-surface-50`. Completion: `rg -nE "border-surface-(100|300)" app/Store/src --glob "*.vue"` and `rg -n "bg-surface-50" app/Store/src --glob "*.vue"` show only the allowed page-background instances listed above (verify by reading each match). | | |

### Implementation Phase 6 — Guard and verify

- GOAL-006: Add a durable audit script that fails on dark-mode remnants and non-canonical tokens, and run the full verification battery.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-019 | Create `scripts/check-store-lightmode-tokens.sh` (new file, executable `chmod +x`, `set -euo pipefail`), mirroring the style of `scripts/check-primevue-coverage.sh`. From repo root it must `exit 1` with a labelled message on any of: (1) `dark:` in `app/Store/src` (`.vue`/`.scss`/`.css`/`.ts`); (2) `app-dark`, `prefers-color-scheme`, `useTheme`, or `resys_theme` in `app/Store/src`; (3) raw Tailwind palette classes (the list in REQ-004) inside any `.vue` `class="..."` attribute; (4) hex color literals `#[0-9a-fA-F]{3,8}` in `.vue` files; (5) `text-surface-[1-9][0-9]*` and `border-surface-(100|300)` in `.vue` files. Allowed exemptions: hex literals inside `app/Store/src/assets/styles.scss` and `app/Store/src/assets/tailwind.css` (the token sources). On success print `Light-mode token audit OK` and `exit 0`. Completion: `bash scripts/check-store-lightmode-tokens.sh` exits 0 on the post-normalization tree and exits 1 on a deliberately injected `dark:`/raw-palette line (verify once each way). | | |
| TASK-020 | Run the full gate battery and fix any failure before closing: from `app/Store`: `npx vue-tsc --build` (exit 0), `pnpm exec oxlint .` (zero), `pnpm exec eslint .` (zero), `npx vitest run --test-timeout=60000` (48 files / 381 tests minus the 2 deleted theme specs; all pass), `pnpm run build-only` (exit 0); from repo root: `bash scripts/check-primevue-coverage.sh` (70 matrix rows OK) and `bash scripts/check-store-lightmode-tokens.sh` (exit 0). Verify no dangling imports remain from deleted files (vue-tsc catches them). Completion: every command in this task exits 0 / prints its green marker. | | |

## 3. Alternatives

- **ALT-001**: Keep `useTheme` but force it light-only (`applyTheme(false)` always). Rejected: retains a dead runtime, the `app-dark` CSS block, and the storage key for no user value, and still requires deleting the toggle UI — strictly more work with the same end state.
- **ALT-002**: Remove `tailwindcss-primeui` and hand-roll the full palette in `tailwind.css`. Rejected: `primary-*`/`surface-*` are already wired to the Aura `--p-*` variables by the plugin; duplicating them breaks the single-source-of-truth rule and risks drift from the preset.
- **ALT-003**: Keep the per-component inline tokens as-is (status quo). Rejected: that is precisely the inconsistency being fixed — `surface-400/500/600` all meaning "muted", `border-surface-100/200/300` all meaning "divider", `text-primary-400/600/700` all meaning "accent".
- **ALT-004**: Apply the same light-mode-only change to the Admin SPA (`app/Admin`, Sakai theme with `_dark.scss` and `useLayout.ts`). Rejected: out of scope — Admin is a separate legacy-theme app with its own layout system; documented as a follow-up, not this plan.
- **ALT-005**: Define semantic classes by hand in `styles.scss` (`.text-muted {}` etc.) instead of Tailwind `@theme inline`. Rejected: hand-rolled classes require manual cascade order, fight Tailwind utilities, and are not IDE-autocompleted; `@theme inline` generates first-class `text-*`/`bg-*`/`border-*` utilities that compose with variants and opacity modifiers (`text-on-brand/80`).

## 4. Dependencies

- **DEP-001**: Tailwind v4 `@theme inline` support (already used; `tailwindcss@^4.3.3` + `@tailwindcss/vite` in `app/Store/package.json`). No version change.
- **DEP-002**: PrimeVue 5 Aura preset semantic CSS variables — `--p-text-color`, `--p-text-muted-color`, `--p-primary-color`, `--p-primary-contrast-color`, `--p-surface-300/400`, and `--p-success/danger/warning/info-color`. The last four are already defined and consumed by the app via the `styles.scss` `:root` overrides (the current build and coverage gate pass), so they resolve at runtime.
- **DEP-003**: `tailwindcss-primeui@^0.6.1` v4 plugin — remains in `tailwind.css` via `@plugin`; its `primary-*`/`surface-*` mapping is additive-compatible with the new `@theme inline` semantic tokens (no name collisions: the semantic layer introduces `heading/body/muted/subtle/placeholder/brand/on-brand/success/danger/warning/info`, none of which overlap the plugin's `primary`/`surface` namespaces).
- **DEP-004**: No new npm packages. The audit script uses only `rg` and standard POSIX shell, matching `scripts/check-primevue-coverage.sh`.

## 5. Files

- **FILE-001**: `app/Store/src/shared/composables/useTheme.ts` — deleted (TASK-001).
- **FILE-002**: `app/Store/src/shared/composables/__tests__/useTheme.spec.ts` — deleted (TASK-001).
- **FILE-003**: `app/Store/src/app/components/ThemeToggle.vue` — deleted (TASK-002).
- **FILE-004**: `app/Store/src/app/components/__tests__/ThemeToggle.spec.ts` — deleted (TASK-002).
- **FILE-005**: `app/Store/src/shared/composables/index.ts` — drop `useTheme` export (TASK-001).
- **FILE-006**: `app/Store/src/app/providers/primevue.ts` — drop `darkModeSelector` (TASK-003).
- **FILE-007**: `app/Store/src/shared/constants/storage.ts` — drop `THEME` key (TASK-004).
- **FILE-008**: `app/Store/src/App.vue` — drop theme binding + init (TASK-005).
- **FILE-009**: `app/Store/src/app/components/layout/AppHeader.vue` — drop toggle + `dark:` variants; normalize tokens (TASK-006/010/016).
- **FILE-010**: `app/Store/src/features/profile/views/PreferencesView.vue` — drop dark-mode row; normalize tokens (TASK-007/016).
- **FILE-011**: `app/Store/src/features/profile/views/ProfileView.vue` — drop theme row; normalize tokens (TASK-008/016).
- **FILE-012**: `app/Store/src/assets/styles.scss` — delete `.app-dark` block; add role-map comment (TASK-009/015).
- **FILE-013**: `app/Store/src/assets/tailwind.css` — add `@theme inline` semantic tokens (TASK-014).
- **FILE-014**: `app/Store/src/app/components/layout/AppFooter.vue` — strip `dark:`; normalize tokens (TASK-010/016).
- **FILE-015**: `app/Store/src/app/components/layout/MobileNav.vue` — strip `dark:`; normalize tokens (TASK-010/016).
- **FILE-016**: `app/Store/src/app/layouts/AccountLayout.vue` — strip `dark:`; normalize tokens (TASK-010/016).
- **FILE-017**: `app/Store/src/app/layouts/AuthLayout.vue` — verify no `dark:`; normalize on-brand tokens (TASK-010/016).
- **FILE-018**: `app/Store/src/features/catalog/views/HomeView.vue` — strip `dark:` (hero gradient); `text-white/80`→`text-on-brand/80`; normalize tokens (TASK-011/016/017).
- **FILE-019**: `app/Store/src/features/catalog/views/CollectionsView.vue` — strip `dark:`; normalize tokens (TASK-011/016).
- **FILE-020**: `app/Store/src/features/catalog/views/ProductDetailView.vue` — strip `dark:`; normalize tokens (TASK-011/016).
- **FILE-021**: `app/Store/src/features/catalog/components/ProductCard.vue` — strip `dark:`; price→`text-brand`; normalize tokens (TASK-011/016).
- **FILE-022**: `app/Store/src/features/catalog/components/ShopFilterPanel.vue` — strip `dark:`; normalize tokens (TASK-011/016).
- **FILE-023**: `app/Store/src/features/catalog/views/VisualSearchView.vue` — strip `dark:`; normalize tokens (TASK-011/016).
- **FILE-024**: `app/Store/src/features/catalog/views/AboutView.vue` — strip `dark:`; normalize tokens (TASK-011/016).
- **FILE-025**: `app/Store/src/features/catalog/views/TermsView.vue` — strip `dark:`; normalize tokens (TASK-011/016).
- **FILE-026**: `app/Store/src/features/catalog/views/PrivacyView.vue` — strip `dark:`; normalize tokens (TASK-011/016).
- **FILE-027**: `app/Store/src/features/catalog/views/NotFoundView.vue` — normalize `text-primary-400` → `text-brand` (TASK-016).
- **FILE-028**: `app/Store/src/features/ordering/views/CheckoutView.vue` — strip `dark:`; `text-green-500`→`text-success`; normalize tokens (TASK-012/016/017).
- **FILE-029**: `app/Store/src/features/ordering/views/CartView.vue` — strip `dark:`; normalize tokens (TASK-012/016).
- **FILE-030**: `app/Store/src/features/ordering/components/CartDrawer.vue` — strip `dark:`; normalize tokens (TASK-012/016).
- **FILE-031**: `app/Store/src/features/ordering/views/OrderDetailView.vue` — strip `dark:`; normalize tokens (TASK-012/016).
- **FILE-032**: `app/Store/src/features/ordering/views/OrderListView.vue` — strip `dark:`; normalize tokens (TASK-012/016).
- **FILE-033**: `app/Store/src/features/profile/views/WishlistsView.vue` — strip `dark:`; normalize tokens (TASK-012/016).
- **FILE-034**: `app/Store/src/features/profile/views/AddressBookView.vue` — strip `dark:`; normalize tokens (TASK-012/016).
- **FILE-035**: `app/Store/src/features/profile/views/NotificationPrefsView.vue` — strip `dark:`; normalize tokens (TASK-012/016).
- **FILE-036**: `app/Store/src/features/catalog/components/SearchOverlay.vue` — normalize `text-surface-500` → `text-muted` (TASK-016).
- **FILE-037**: `scripts/check-store-lightmode-tokens.sh` — new audit script (TASK-019).

## 6. Testing

- **TEST-001**: Unit-suite integrity — the two deleted spec files (`useTheme.spec.ts`, `ThemeToggle.spec.ts`) leave no dangling imports; `npx vitest run --test-timeout=60000` passes with the full remaining suite (48 files / 381 tests minus the 2 removed files). No other spec asserted theme UI (verified pre-change: ProfileView, PreferencesView, AppHeader, MobileNav, and layouts specs contain no `Theme`/`Dark`/`toggle` assertions).
- **TEST-002**: Dark-mode remnant audit — from repo root, `rg -n "dark:" app/Store/src --glob "*.vue" --glob "*.scss" --glob "*.css"` and `rg -n "app-dark|prefers-color-scheme|useTheme|resys_theme" app/Store/src` both return zero matches (TASK-013).
- **TEST-003**: Token-consistency audit — `bash scripts/check-store-lightmode-tokens.sh` exits 0 on the final tree; positive control: injecting one `dark:bg-surface-800` line or one `text-green-500` class into a `.vue` file makes it exit 1, then the injection is reverted (TASK-019).
- **TEST-004**: Full gate battery — `npx vue-tsc --build`, `pnpm exec oxlint .`, `pnpm exec eslint .`, `npx vitest run --test-timeout=60000`, `pnpm run build-only`, `bash scripts/check-primevue-coverage.sh` (70 rows), and `bash scripts/check-store-lightmode-tokens.sh` all pass (TASK-020).
- **TEST-005**: Visual regression (manual, `pnpm dev` under Aspire): verify in light mode that the home page (hero + banner panel), shop + filters, product detail, cart/checkout, account profile/preferences, and auth pages all render with no invisible text, no washed-out chips, and the two dark brand blocks (HomeView banner, AuthLayout brand panel) retain contrast; theme control is absent from the header and Preferences.

## 7. Risks & Assumptions

- **RISK-001**: Removing the only theme affordance (header toggle + Preferences row) eliminates the user's ability to switch themes. If dark mode is later required, the deleted `useTheme.ts` and `.app-dark` block are recoverable from git history (commit `4eaa3206` and prior). Accepted per REQ-001/REQ-002.
- **RISK-002**: The `--p-success-color`/`--p-danger-color`/`--p-warning-color`/`--p-info-color` variables must resolve for `text-success` etc. to render. They are already defined in `styles.scss` `:root` and the current build/coverage gate pass, so they resolve; if a runtime check shows a severity token is absent, the executor must define the missing variable in `styles.scss` `:root` using the canonical Aura value (`--p-success-color: #16a34a`, `--p-danger-color: #dc2626`, `--p-warning-color: #ca8a04`, `--p-info-color: #2563eb`) rather than adding inline hex to a `.vue` file.
- **RISK-003**: The `text-heading`/`text-body` pair both resolve to `--p-text-color` (`#171717`); this is intentional role clarity, not a conflict. Executors must not "deduplicate" them into one token, as the audit and role table depend on both names.
- **ASSUMPTION-001**: Scope is the Store SPA (`app/Store`) only. The Admin SPA (`app/Admin`, Sakai theme, `useLayout.ts` dark handling) is untouched; removing its dark mode is a separate follow-up.
- **ASSUMPTION-002**: No server contract or persisted user data depends on the removed `resys_theme` localStorage key — it is UI-only. The `theme: string | null` field in the frontend `ProfilePreferences` type (`app/Store/src/features/profile/types/preferences.ts`) is a dormant DTO field with no consumer and no backend counterpart; it is left untouched to avoid server-contract churn.
- **ASSUMPTION-003**: The canonical light palette (`--p-surface-*` neutral scale, `--p-primary-*` deep-teal scale, `--p-text-muted-color` `#737373`) stays as-is; this plan changes token *usage* for consistency, not token *values*.
- **ASSUMPTION-004**: PrimeVue components (not our classes) that carry their own severity colors (e.g., `Tag severity="danger"`, `Message severity="error"`) are outside this plan's normalization scope — the plan targets our Tailwind class usage only.

## 8. Related Specifications / Further Reading

- [Store SPA agent guide](app/Store/AGENTS.md) — Code Commenting Standard v3.0 applied by CON-003
- [ReSys.Shop architecture](docs/codebase/ARCHITECTURE.md)
- [Frontend design skill](https://github.com/anthropics/claude-code/tree/main/skills/frontend-design) — GUD-001 direction
- [tailwindcss-primeui v4](https://www.npmjs.com/package/tailwindcss-primeui) — plugin token mapping (DEP-003)
- [PrimeVue Aura theme](https://primevue.org/theming/styled/) — `--p-*` semantic variables (DEP-002)
