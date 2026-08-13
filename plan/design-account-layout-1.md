---
goal: Redesign the Store SPA account area (/account) into a coherent "account console" — a defined navigation panel with a profile summary, a consistent page-header + content column — nested inside the shop's DefaultLayout so account pages share the AppHeader, cart, mobile nav, and footer — using PrimeVue components and Tailwind, preserving the existing layout test contracts.
version: 1.1
date_created: 2026-08-12
last_updated: 2026-08-12
owner: Store SPA team
status: 'In progress'
tags: [feature, design, store, layout, primevue, tailwind]
---

# Introduction

![Status: Planned](https://img.shields.io/badge/status-Planned-blue)

The `/account` area renders with no coherent layout: `SidebarLayout`/`SidebarMain`
(the PrimeVue Sidebar compound) are headless wrappers that add no structure, so
the real layout is a bare Tailwind grid. The desktop nav is an undefined
`bg-surface-50` column holding a raw `PanelMenu`, and the routed views are
disconnected Cards floating on the same background with no page-level title.
There is no signed-in profile context anywhere. This plan rebuilds the shell as
a defined two-zone account console — a surfaced nav panel (profile summary +
navigation) beside a content column with a consistent page header — while
keeping the seven existing layout tests green.

## Design direction

**Subject:** the shopper's personal account hub. **Audience:** a signed-in
customer managing profile, addresses, wishlists, notifications, and orders.
**Single job:** make every account task findable and give the area a sense of
place (who is signed in, where am I, what can I do here).

**Signature:** the profile summary strip at the top of the nav panel — an
Avatar monogram of the user's initials, display name, and email — which makes
the console feel personal rather than a generic settings page. The rest stays
quiet: teal active states (existing `bg-highlight`/`text-brand` tokens), a
white nav card on the `surface-50` page, and a page-header band in the content
column that names the current section.

**Palette (all existing tokens, no new colors):** page `surface-50` (#fafafa),
nav/content cards `surface-0` (#ffffff), borders `surface-200` (#e5e5e5),
active nav `highlight` (#e0f5f5) + `brand` (#0d7377), body `text-heading`
(#171717), muted `text-muted` (#737373).

**Typography (existing Inter stack):** page header `text-2xl font-bold`
(matching `DefaultLayout`'s h1 convention); nav labels `text-sm`; profile
meta `text-sm` / `text-xs text-muted`.

**Structure encodes truth:** the nav order follows the shopper's journey
(identity → saved places → lists → communication → security → settings →
orders), and the profile strip names the person the console belongs to.

## 1. Requirements & Constraints

- **REQ-001**: The account shell must render a navigation panel containing exactly the 7 existing nav items (Profile, Addresses, Wishlists, Notifications, Change Password, Preferences, Orders) with the active item marked `aria-current="page"`.
- **REQ-002**: Each nav item must keep the `data-pc-section="headerlink"` attribute so the existing `layouts.spec.ts` queries still match.
- **REQ-003**: The nav panel must include a profile summary strip (Avatar monogram + display name + email) derived from `authStore.user`.
- **REQ-004**: The content column must render a page header (section title from the active route's `meta.title`, plus an "My Account" eyebrow) above the `<RouterView />`.
- **REQ-005**: Below the `lg` breakpoint, the nav must collapse into the existing PrimeVue `Sidebar` drawer opened by a "Menu" button (keep the `[data-pc-name="sidebar"]` contract).
- **REQ-006**: The Orders nav item must keep its active-order-count badge (`activeOrderCount` from `useOrders`).
- **REQ-007**: The layout must not add native `input`, `select`, `label`, or `textarea` elements of its own.
- **REQ-008**: Preserve the unauthenticated fallback: when `authStore.isAuthenticated` is false, render the session-expired message instead of the shell.
- **SEC-001**: Display the user's name/email only from `authStore.user`; render nothing personal when the user object is null.
- **CON-001**: `SidebarLayout` / `SidebarMain` / `SidebarAside` / `SidebarPanel` / `SidebarHeader` / `SidebarContent` are headless — the redesign must NOT rely on them for structural styling; structure comes from explicit Tailwind layout classes.
- **CON-002**: PrimeVue components used: `PanelMenu` (nav), `Avatar` (profile monogram), `Button` (mobile menu + sign-out), `Tag` (order badge), `Message` (session-expired fallback), `Sidebar` (mobile drawer). All auto-imported.
- **CON-003**: Comments follow the Store AGENTS.md standard (`// Label: Sentence.` script; `<!-- Section: Title — purpose -->` template).
- **CON-004**: Warnings-as-errors; `pnpm run build-only`, `pnpm run lint`, and the layout spec must pass.
- **GUD-001**: Reuse existing semantic color tokens (`surface-*`, `text-heading`, `text-muted`, `bg-highlight`, `text-brand`); introduce no new palette.
- **GUD-002**: Keep the redesign to `AccountLayout.vue` and its spec; do NOT rewrite the routed account views (their per-Card `<template #title>` remains as section labels).
- **GUD-003**: One memorable element (the profile summary strip); keep everything else disciplined.
- **PAT-001**: Match `DefaultLayout`'s h1 convention (`text-2xl font-bold`) for the page header.
- **PAT-002**: Keep the `PanelMenu` `#item` slot rendering a `RouterLink` with `data-pc-section="headerlink"`, exactly as today, so tests and the router active-state logic are unchanged.

## 2. Implementation Steps

### Implementation Phase 1: Desktop nav panel with profile summary

- GOAL-001: Turn the bare `aside` into a defined nav card with a profile summary strip and the existing PanelMenu nav.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-001 | In `app/Store/src/app/layouts/AccountLayout.vue`, replace the desktop `aside` (lines 114-139) with a nav panel: a `Card`-like column (`flex w-full flex-col gap-4 rounded-xl border border-surface-200 bg-surface-0 p-4`) containing (a) a profile summary strip and (b) the existing `PanelMenu` block. | |  |
| TASK-002 | Add the profile summary strip: an `Avatar` (initials derived from `authStore.user?.userName` — first letters of first two whitespace-separated parts, uppercased; fall back to `'U'` when null) sized `size="large"`, shaped `shape="circle"`, with class `bg-brand text-on-brand`, followed by a column with the display name (`text-sm font-semibold text-heading`, `truncate`) and email (`text-xs text-muted`, `truncate`). Guard: when `authStore.user` is null render only the Avatar + "My Account" label. | |  |
| TASK-003 | Add the Avatar import to the script block (`Avatar` is auto-imported by `PrimeVueResolver` — no explicit import needed; verify against the existing auto-import convention). Keep the `navItems` computed and `isItemActive` function unchanged. | |  |

### Implementation Phase 2: Content column page header

- GOAL-002: Give the routed views a consistent home with a page-header band.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-004 | In `AccountLayout.vue`, above the `<RouterView />` inside the `<main>` element, add a page-header block: a `div` with `mb-6` containing an eyebrow (`text-xs font-medium uppercase tracking-wide text-muted` → "My Account") and an `<h1 class="text-2xl font-bold text-heading">{{ pageTitle }}</h1>`. | |  |
| TASK-005 | Add `const pageTitle = computed(() => typeof route.meta.title === 'string' ? route.meta.title : 'Account')` to the script (the `route` ref already exists). Add a `// Title: Section heading derived from the active route meta.` comment. | |  |

### Implementation Phase 3: Mobile drawer consistency

- GOAL-003: Keep the mobile Sidebar drawer but surface the profile summary at its top for continuity.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-006 | In the mobile `Sidebar` block, replace the static `Account` label in `SidebarHeader` (line 73) with the same profile summary markup as the desktop strip (reuse the Avatar + name + email block). Keep `SidebarHeader`, `SidebarContent`, and the `PanelMenu` block as-is. | |  |
| TASK-007 | Ensure the mobile "Menu" button (line 110) still toggles `mobileNavOpen`; keep its `label="Menu"` text so the test's `findAll('button').find(b => b.text().includes('Menu'))` still matches. | |  |

### Implementation Phase 4: Layout spec updates

- GOAL-004: Update `layouts.spec.ts` to cover the new structure without breaking the existing contracts.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-008 | In `app/Store/src/app/layouts/__tests__/layouts.spec.ts`, extend the AccountLayout describe block with a test asserting the profile summary renders when signed in: after `signIn(wrapper)`, expect the Avatar (`[data-pc-name="avatar"]`) to exist and the text to contain `ada@test.dev` and the user's display name. | |  |
| TASK-009 | Add a test asserting the page header renders: after `signIn(wrapper)` on `/account/profile`, expect an `h1` whose text is `Profile` and the eyebrow `My Account`. | |  |
| TASK-010 | Verify all 7 existing AccountLayout tests still pass unchanged (nav items count, `aria-current`, routed stub, order badge, mobile drawer open, no-native-elements, session-expired fallback). | |  |

### Implementation Phase 5: Verification

- GOAL-005: Prove the redesign builds, lints, and passes tests.

| Task | Description | Completed | Date |
|------|-------------|-----------|------|
| TASK-011 | Run `pnpm run build-only` in `app/Store` — passes with zero warnings. | |  |
| TASK-012 | Run `npx vitest run src/app/layouts/__tests__/layouts.spec.ts` in `app/Store` — all tests pass (existing 7 + 2 new). | |  |
| TASK-013 | Run `pnpm run lint` in `app/Store` and confirm zero errors in `AccountLayout.vue` and `layouts.spec.ts` (repo-wide pre-existing errors in unrelated files are out of scope). | |  |

## 3. Alternatives

- **ALT-001**: Rely on the PrimeVue Sidebar compound (`SidebarAside`/`SidebarPanel`/etc.) for the layout structure — rejected: these components are headless (render only `as`/`class` wrappers with no layout CSS); the current broken look is precisely because the layout delegated structure to them. Explicit Tailwind layout is the reliable structure.
- **ALT-002**: Redesign all 8 routed account views (add per-view page headers, restructure their Cards) — rejected: larger blast radius, touches their tests, and the per-Card `<template #title>` already labels sections; the layout-level page header is the single fix that makes every account page coherent at once.
- **ALT-003**: Introduce a new shared `PageHeader` component — rejected: only the account area needs it today; keeping it inline in `AccountLayout` avoids premature abstraction (YAGNI). Extract later if a second consumer appears.
- **ALT-004**: Add the profile summary as a standalone card above the content column — rejected: it belongs in the nav panel (the console's identity is a property of the nav, not the page body), and putting it in the nav keeps the content column purely about the current task.

## 4. Dependencies

- **DEP-001**: PrimeVue `PanelMenu` — nav rendering; must keep the `#item` slot + `data-pc-section="headerlink"` contract.
- **DEP-002**: PrimeVue `Avatar` — profile monogram; auto-imported via `PrimeVueResolver`.
- **DEP-003**: PrimeVue `Sidebar` — mobile drawer; `data-pc-name="sidebar"` + `data-state` contract.
- **DEP-004**: `useAuthStore` — `user` (name/email) and `isAuthenticated` for the profile strip and fallback.
- **DEP-005**: `useOrders` — `activeOrderCount` for the Orders badge.
- **DEP-006**: `useMediaQuery` — `(max-width: 1023px)` breakpoint for mobile/desktop switch.
- **DEP-007**: Tailwind v4 + `tailwindcss-primeui` plugin — semantic tokens (`surface-*`, `highlight`, `brand`, `text-heading`, `text-muted`).

## 5. Files

- **FILE-001**: `app/Store/src/app/layouts/AccountLayout.vue` — the shell redesign (nav panel + profile strip + page header + mobile drawer).
- **FILE-002**: `app/Store/src/app/layouts/__tests__/layouts.spec.ts` — two new tests (profile summary, page header) plus verification of the existing seven.

## 6. Testing

- **TEST-001**: Existing 7 AccountLayout tests in `layouts.spec.ts` remain green (nav count, `aria-current`, routed stub, order badge, mobile drawer, no-native-elements, session-expired fallback).
- **TEST-002**: New — profile summary renders Avatar + name + email when signed in.
- **TEST-003**: New — page header renders the route `meta.title` as an `h1` with the "My Account" eyebrow.
- **TEST-004**: Build gate — `pnpm run build-only` zero warnings.
- **TEST-005**: Lint gate — no errors in the two changed files.

## 7. Risks & Assumptions

- **RISK-001**: The existing tests assert `data-pc-section="headerlink"` on nav links and `[data-pc-name="sidebar"]` on the drawer. Any restructure that drops these breaks the suite. Mitigated: TASK-001/TASK-006 keep both contracts verbatim.
- **RISK-002**: `route.meta.title` may be absent on some account routes (e.g. order-detail). Mitigated: `pageTitle` falls back to `'Account'` (TASK-005).
- **RISK-003**: Adding the profile summary to the mobile drawer changes its height; the existing drawer test only checks `data-state` and text, so it is safe.
- **RISK-004**: `Avatar` initials derivation edge cases (single-part name, empty `userName`). Mitigated: split-and-uppercase with `'U'` fallback (TASK-002).
- **ASSUMPTION-001**: `authStore.user.userName` is populated for authenticated users (the `login`/`init` flows set it).
- **ASSUMPTION-002**: The routed account views are NOT changed by this plan; their internal Cards and `<template #title>` labels remain as the section content.

## 8. Related Specifications / Further Reading

- [Store SPA AGENTS.md — comment standard](app/Store/AGENTS.md)
- [Store SPA layout test contracts](app/Store/src/app/layouts/__tests__/layouts.spec.ts)
- [PrimeVue PanelMenu](https://primevue.org/panelmenu/)
- [PrimeVue Avatar](https://primevue.org/avatar/)
- [PrimeVue Sidebar](https://primevue.org/sidebar/)
- [DefaultLayout h1 convention](app/Store/src/app/layouts/DefaultLayout.vue)
