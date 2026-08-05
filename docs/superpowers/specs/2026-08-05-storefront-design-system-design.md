# Storefront Design System — Fashion Boutique Theme

**Date**: 2026-08-05
**Scope**: PrimeVue 5 Aura token override — teal palette, Playfair Display + DM Sans fonts, dark mode, animations
**Depends on**: Nothing (foundation for Spec B — Feature Restoration)
**Status**: Approved

## Goal

Replace the monochrome gray Aura preset with a teal-based fashion boutique
palette. Migrate core semantic tokens and full color scales from the legacy
storefront design system into PrimeVue 5's `--p-*` CSS custom property
namespace. No component template changes except adding the theme toggle
button to `AppHeader.vue`.

## Design Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Primary color | Teal-700 (`#0f766e`) | Legacy palette, warm fashion feel |
| Surface scale | Warm stone neutral | Legacy surface hierarchy, better than slate-tinted Aura default |
| Typography | Playfair Display (headings) + DM Sans (body) | Legacy font pairing — editorial + modern |
| Font loading | Google Fonts CDN @import | Zero config, works offline after first cache |
| Dark mode strategy | `.app-dark` class on `<html>` | Matches existing PrimeVue config, no component changes needed |
| Dark mode logic | useTheme composable with localStorage + system preference | Singleton, no Pinia dependency |
| Animation | 4 @keyframes + utility classes | Legacy had these; used in overlay/transition patterns |
| Token scope | Core semantic + full color scales (~50 tokens) | Covers all PrimeVue component states without over-engineering |

## Architecture

### Token Namespace Strategy

PrimeVue 5 Aura emits one CSS custom property per semantic token as
`--p-{dotted-path->kebab}`. Override these in `:root` block in
`styles.scss`. PrimeVue components read them natively — zero component
changes needed for theme adoption.

Dark mode: `.app-dark` block overrides the same tokens with inverted
values. PrimeVue's `darkModeSelector: '.app-dark'` config (already set
in `primevue.ts`) causes components to re-read the CSS variables when
the class is present.

### Theme State Machine (`useTheme` composable)

```
                    +--> toggle()
                    |
    light <---> dark <---> system
      |           |           |
      v           v           v
   localStorage  localStorage  localStorage
   "light"       "dark"        "system"
       |           |               |
       v           v               v
   remove        add          respect OS
   .app-dark     .app-dark    prefers-color-scheme
```

Module-level singleton: first call to `useTheme()` reads localStorage,
applies class, registers `matchMedia('(prefers-color-scheme: dark)')`
listener. Subsequent calls share the same underlying state.

## File Inventory

| File | Action | Lines after | Description |
|------|--------|-------------|-------------|
| `src/assets/styles.scss` | Rewrite | ~220 | Full token map, dark mode, fonts, animations |
| `src/assets/tailwind.css` | Modify | 3 | Add Google Fonts @import |
| `src/app/providers/primevue.ts` | No change | 20 | Aura preset + darkModeSelector already configured |
| `src/App.vue` | Modify | 14 | Call useTheme() on mount |
| `src/shared/composables/useTheme.ts` | New | ~60 | Theme state machine |
| `src/app/components/ThemeToggle.vue` | New | ~15 | Sun/moon icon button |
| `src/app/components/layout/AppHeader.vue` | Modify | 100 | Add `<ThemeToggle />` in action bar |

### No Changes To

- `src/app/providers/pinia.ts`
- `src/app/router/*`
- `src/app/layouts/*`
- All feature components (they pick up tokens automatically via PrimeVue)

## Token Map

### Primary Scale (teal)

All tokens placed in `:root { ... }` block of `styles.scss`.

| Token | Value | Note |
|-------|-------|------|
| `--p-primary-color` | `#0f766e` | teal-700 — legacy `--color-primary` |
| `--p-primary-hover-color` | `#0d5d56` | Darkened from teal-700 — legacy `--color-primary-hover` |
| `--p-primary-active-color` | `#115e59` | teal-800 |
| `--p-primary-contrast-color` | `#ffffff` | White text on teal buttons |
| `--p-primary-50` | `#f0fdfa` | teal-50 |
| `--p-primary-100` | `#ccfbf1` | teal-100 |
| `--p-primary-200` | `#99f6e4` | teal-200 |
| `--p-primary-300` | `#5eead4` | teal-300 |
| `--p-primary-400` | `#2dd4bf` | teal-400 — legacy `--color-primary-light` |
| `--p-primary-500` | `#14b8a6` | teal-500 |
| `--p-primary-600` | `#0d9488` | teal-600 |
| `--p-primary-700` | `#0f766e` | teal-700 |
| `--p-primary-800` | `#115e59` | teal-800 |
| `--p-primary-900` | `#134e4a` | teal-900 |
| `--p-primary-950` | `#042f2e` | teal-950 |

### Surface Scale (warm stone)

Maps legacy surface hierarchy to PrimeVue 5 surface scale. Aura
default is slate-tinted; we override with warm neutral stone.

| Token | Value | Maps to legacy |
|-------|-------|---------------|
| `--p-surface-0` | `#ffffff` | `--color-surface` |
| `--p-surface-50` | `#fafaf9` | `--color-surface-ground` |
| `--p-surface-100` | `#f5f5f4` | `--color-border-light` |
| `--p-surface-200` | `#e7e5e4` | `--color-border` |
| `--p-surface-300` | `#d6d3d1` | — |
| `--p-surface-400` | `#a8a29e` | `--color-text-muted` |
| `--p-surface-500` | `#78716c` | — |
| `--p-surface-600` | `#57534e` | `--color-text-secondary` |
| `--p-surface-700` | `#44403c` | — |
| `--p-surface-800` | `#292524` | dark `--color-surface-elevated` |
| `--p-surface-900` | `#1c1917` | `--color-text` |
| `--p-surface-950` | `#0c0a09` | dark `--color-surface-ground` |

### Semantic Tokens

| Token | Value | Note |
|-------|-------|------|
| `--p-content-background` | `#ffffff` | Card + panel surfaces — legacy `--color-surface-elevated` |
| `--p-content-border-color` | `#e7e5e4` | Card/panel borders — legacy `--color-border` |
| `--p-text-color` | `#1c1917` | Primary text — legacy `--color-text` |
| `--p-text-muted-color` | `#a8a29e` | Secondary/muted text — legacy `--color-text-muted` |
| `--p-form-field-background` | `#ffffff` | Input backgrounds |
| `--p-form-field-border-color` | `#d6d3d1` | Input borders |
| `--p-form-field-focus-border-color` | `#0f766e` | Teal focus ring — legacy `--color-primary` |
| `--p-navigation-item-focus-background` | `#f0fdfa` | Teal-50 — hover state |
| `--p-navigation-item-active-background` | `#ccfbf1` | Teal-100 — active state |
| `--p-highlight-background` | `#ccfbf1` | Selection/active background |
| `--p-highlight-color` | `#0f766e` | Selection/active text |

### State Colors (unchanged from legacy)

| Token | Value | Usage |
|-------|-------|-------|
| `--p-success-color` | `#16a34a` | Green-600 |
| `--p-warning-color` | `#ca8a04` | Yellow-600 |
| `--p-danger-color` | `#dc2626` | Red-600 |
| `--p-info-color` | `#2563eb` | Blue-600 |

### Legacy v4 Aliases (compatibility)

| Token | Value | Note |
|-------|-------|------|
| `--p-surface-ground` | `#f9fafb` | Legacy PrimeVue v4 name; harmless in Aura 3 |
| `--p-surface-card` | `#ffffff` | Legacy PrimeVue v4 name; harmless in Aura 3 |

These aliases exist because old documentation and internal references
may use the v4 names. Aura 3 does not read them — components read
`--p-surface-50` and `--p-content-background` instead.

## Dark Mode

`.app-dark` class applied to `<html>` by `useTheme` composable. All
surface and text tokens invert. Primary teal scale inverts so lighter
teal shades become accent colors in dark mode (prevents dark teal from
disappearing against dark backgrounds).

Semantic state colors (success/warning/danger/info) are **not inverted**
— red is red in both modes, verified WCAG contrast maintained.

| Category | Light | Dark |
|----------|-------|------|
| Surface-0 | `#ffffff` | `#292524` |
| Surface-50 | `#fafaf9` | `#1c1917` |
| Surface-950 | `#0c0a09` | `#ffffff` |
| Content background | `#ffffff` | `#1c1917` |
| Text color | `#1c1917` | `#fafaf9` |
| Text muted | `#a8a29e` | `#a8a29e` (same — sufficient contrast on dark) |
| Primary color | `#0f766e` | `#14b8a6` (teal-500 — brighter on dark) |
| Primary-50 | `#f0fdfa` | `#042f2e` |
| Primary-950 | `#042f2e` | `#f0fdfa` |

## Typography

```scss
/* Google Fonts — imported in tailwind.css */
@import url('https://fonts.googleapis.com/css2?family=DM+Sans:opsz,wght@9..40,400;500;600;700&family=Playfair+Display:ital,wght@0,400;600;700;1,400&display=swap');

body {
  --p-font-family: 'DM Sans', -apple-system, BlinkMacSystemFont, sans-serif;
  font-family: var(--p-font-family);
  font-size: 16px;
  line-height: 1.5;
  background: var(--p-surface-50);
  color: var(--p-text-color);
  -webkit-font-smoothing: antialiased;
}

h1, h2, h3, h4, h5, h6 {
  font-family: 'Playfair Display', Georgia, serif;
  font-weight: 600;
  line-height: 1.25;
}
```

PrimeVue components inherit `--p-font-family` for body text (DM Sans).
Custom sections use Playfair Display for headings via the global h1-h6
override.

## Animations

```scss
@keyframes fadeIn {
  from { opacity: 0; }
  to { opacity: 1; }
}
@keyframes slideUp {
  from { opacity: 0; transform: translateY(20px); }
  to { opacity: 1; transform: translateY(0); }
}
@keyframes slideDown {
  from { opacity: 0; transform: translateY(-20px); }
  to { opacity: 1; transform: translateY(0); }
}
@keyframes scaleIn {
  from { opacity: 0; transform: scale(0.95); }
  to { opacity: 1; transform: scale(1); }
}

.animate-fadeIn    { animation: fadeIn 250ms ease-out; }
.animate-slideUp   { animation: slideUp 250ms ease-out; }
.animate-slideDown { animation: slideDown 250ms ease-out; }
.animate-scaleIn   { animation: scaleIn 250ms ease-out; }
```

All animations are 250ms ease-out. Honor `prefers-reduced-motion` via
existing media query in styles.scss.

## useTheme Composable API

```ts
// src/shared/composables/useTheme.ts

type ThemeMode = 'light' | 'dark' | 'system'

export function useTheme() {
  const mode: Ref<ThemeMode>        // current mode, persisted to localStorage
  const isDark: ComputedRef<boolean> // actual dark state (resolved)
  const toggle(): void              // cycle light -> dark -> system -> light
  const setMode(m: ThemeMode): void // direct set
}
```

Storage key: `theme-preference`. Dark class: `app-dark`. Module-level
singleton — safe to call from multiple components.

System preference listener: `window.matchMedia('(prefers-color-scheme: dark)')`.
Registered once on first composable call. Unregistered on app unmount.

## Risk Matrix

| Risk | Impact | Mitigation |
|------|--------|------------|
| Token name mismatch in `.app-dark` block | High — dark mode broken | Test manually: toggle class, verify surface/text invert |
| Google Fonts unreachable in dev | Medium — font fallback | Font family always includes system fallback |
| Playfair Display too wide for small headings | Low — visual | Use font-weight 600 not 700 for h4-h6 |
| `useTheme` matchMedia listener memory leak | Medium | Clean up in `onUnmounted` |

## Verification

1. `pnpm run type-check` — 0 errors (no new TypeScript files)
2. `pnpm run lint` — 0 violations
3. Open `localhost:5174` — all PrimeVue components render with teal primary
4. `document.querySelector('html').classList.add('app-dark')` — dark mode activates
5. `localStorage.setItem('theme-preference', 'dark')`, reload — dark mode persists
6. Check DevTools: h1-h6 font-family = Playfair Display; body = DM Sans
7. Click sun/moon icon in AppHeader — cycles through modes
8. Change OS to dark mode — storefront follows when mode = 'system'
9. `prefers-reduced-motion: reduce` in DevTools — animations disabled, skeleton pulse stops

## Out of Scope

- No component template changes (except ThemeToggle in AppHeader)
- No spacing, z-index, or line-height tokens (Tailwind handles layout, PrimeVue manages z-index)
- No SCSS partials or component-scoped SCSS files
- No custom component styling beyond tokens (covered in Spec B)
- No design token documentation page (tokens self-document in code)

## Related Specs

- **Spec B**: `2026-08-05-storefront-feature-restoration-design.md` — consumes these tokens
- **Spec C**: `2026-08-05-storefront-api-fixes-checkout-design.md` — independent API fixes
