# Store SPA Rebuild — Cycle 1: Layouts + Shell

Date: 2026-08-08
Scope: Layout system and shell components for the Store SPA
Tier: 1 of 3 (Layouts → Catalog → Identity/Ordering/Profile)

## Visual Direction

Minimal clean e-commerce. Neutral palette, generous white space, subtle borders.
Reference aesthetic: Everlane, Aesop, Muji — structure over decoration.

## Brand Tokens

### Color Palette

| Token | Value | Usage |
|-------|-------|-------|
| Page bg | `neutral-50` (`#fafafa`) | DefaultLayout, AuthLayout backgrounds |
| Surface | `white` (`#ffffff`) | Cards, header, footer |
| Border | `neutral-200` (`#e5e5e5`) | Dividers, card borders |
| Text primary | `neutral-900` (`#171717`) | Headings, important labels |
| Text secondary | `neutral-500` (`#737373`) | Metadata, hints, footer links |
| Accent primary | `#0d7377` (teal) | CTAs, links, selected states only |
| Accent dark | `#2ecfd3` (cyan) | Primary in dark mode |

### Typography

| Role | Family | Weight | Usage |
|------|--------|--------|-------|
| Body | Inter | 400, 500, 600 | All UI text, nav, labels, inputs |
| Editorial | Newsreader italic | 400, 600 | Hero headlines only, never in navigation |
| Price | JetBrains Mono | 500 | Currency/pricing displays only |

### Layout Rhythm

| Token | Desktop | Mobile |
|-------|---------|--------|
| Max content width | 1440px | 100% |
| Section padding y | `py-12` | `py-8` |
| Section padding x | `px-8` (lg), `px-6` (sm), `px-4` | same |
| Card radius | `rounded-lg` (8px) | same |
| Card border | `border border-neutral-200` | same |

### Signature Element

Content boundary containment. The header/footer borders visually stop at the max-width
container, not full-width. This creates a contained, editorial frame around the content —
distinctive without being loud.

## Component Specifications

### DefaultLayout

- `min-h-screen flex flex-col bg-neutral-50`
- `<AppHeader />` (sticky)
- `<main class="flex-1">` wraps `<router-view />`
- `<AppFooter />` (mt-auto)
- No changes to structure — only enhances existing

### AuthLayout

- `min-h-screen flex items-center justify-center bg-neutral-50 px-4`
- Centered card: `w-full max-w-md`
- Logo: "ReSys.Shop" in serif (Newsreader italic, 2xl)
- `<router-view />` renders the actual form
- No changes to structure

### AccountLayout

- Sticky mini-header: `ReSys.Shop / Account` breadcrumb bar
- Two-column flex: sidebar (w-56, sticky) + content (flex-1)
- Sidebar: 8 nav links with active indicator (left border + bold)
- Content: `<router-view />`
- No changes to structure

### AppHeader

Sticky top bar, `z-50`, `bg-white` (light) / `neutral-900` (dark).

Desktop layout:
```
[ReSys.Shop]     Shop · Collections · Visual Search    Ctrl+K  🛒(3) ☀  👤
```

- Logo: `text-lg font-semibold tracking-tight` (Inter)
- Nav links: `text-sm font-medium text-neutral-600`, hover → `text-neutral-900` + border-b animation
- Search hint: `text-xs text-neutral-400` next to search icon (desktop only)
- Cart: `Button icon="pi pi-shopping-cart"` text rounded + `Tag` badge (hidden at count 0)
  - Fix: badge `absolute` positioning needs parent `relative` — the `Button` wrapper must have `class="relative"`
- ThemeToggle: `Button` with sun/moon icon (see ThemeToggle section)
- Auth: When logged in → user icon `Button` + dropdown (Orders, Profile, Sign Out). When logged out → "Sign In" text link.
- Mobile: hamburger `pi-bars` replaces desktop nav; badge fix same as desktop

Scroll behavior:
- On scroll past 20px: add `backdrop-blur-sm bg-white/95` via `useScroll` or IntersectionObserver
- Smooth transition via Tailwind `transition-colors duration-200`

Cart badge fix (current bug):
Current code has `<Tag>` after `<Button>` with `class="absolute -top-0.5 -right-0.5"` but the
`Button` wrapper lacks `relative`. Move the `relative` class from no parent to the outer
container div wrapping Button + Tag.

### AppFooter

4-column grid, `bg-white border-t border-neutral-200 mt-auto`.

```
ReSys.Shop          Shop              Help              Company
AI-powered          All Products      Help Center       About
fashion             Collections       Shipping Info     Terms
e-commerce         Visual Search     Returns           Privacy
                                        Size Guide

© 2026 ReSys.Shop. All rights reserved.
```

- Logo column: brand name + tagline
- Link columns: `ul > li > a` with `text-sm text-neutral-500 hover:text-neutral-900`
- Divider before copyright: `border-t border-neutral-200`
- No social media icons — remove placeholder Facebook/Twitter/Instagram buttons

### MobileNav

Slide-in overlay from right. No structural changes.
- Backdrop: `bg-black/50`, click to close
- Panel: `w-72 h-full bg-white shadow-xl`
- Links: Shop, Collections, Visual Search, Cart
- Auth-aware: logged in → My Orders, Profile; logged out → Sign In, Register
- Close button: `pi-times` top-right

### CartDrawer

Already fully functional. One fix needed:
- The `<Transition name="slide">` slide animation uses custom CSS. Ensure it respects
  `prefers-reduced-motion` by checking `window.matchMedia('(prefers-reduced-motion: reduce)').matches`
  and disabling the transition if true.

### ThemeToggle

- `Button` with `pi-sun` (dark mode icon) or `pi-moon` (light mode icon)
- `severity="secondary" text rounded`
- Calls `useTheme().toggle()`

Dark mode fix:
- `useTheme.ts` currently toggles `documentElement.classList.toggle('dark', ...)`
- Change to `toggle('app-dark', ...)` to match PrimeVue Aura `darkModeSelector: '.app-dark'`
- `styles.scss` already uses `.app-dark` selector
- One-line fix, no other files affected

## Dark Mode Theme

| Element | Light | Dark |
|---------|-------|------|
| Page bg | `neutral-50` | `neutral-950` |
| Surface (cards, header) | `white` | `neutral-900` |
| Border | `neutral-200` | `neutral-800` |
| Text primary | `neutral-900` | `neutral-100` |
| Text secondary | `neutral-500` | `neutral-400` |
| Accent (links, CTAs) | `#0d7377` | `#2ecfd3` |

All defined in existing `styles.scss` under `.app-dark {}` block. No CSS changes needed.

## Implementation Tasks

### 1. Fix Dark Mode (critical bug)
- `useTheme.ts`: change `'dark'` → `'app-dark'` in `classList.toggle()`

### 2. Fix Cart Badge Positioning
- `AppHeader.vue`: add `relative` to the container that wraps the cart `Button` + `Tag`

### 3. Polish DefaultLayout
- Smooth `transition-colors duration-200` on header
- (Optional) Add scroll-aware `backdrop-blur-sm bg-white/95` to header — defer to post-Cycle-3 polish

### 4. Polish AppHeader
- Add `Ctrl+K` hint text next to search icon (desktop only, `hidden md:inline`)
- Add underline-on-hover animation for nav links
- Add active-route styling (bold + underline for current route)
- Add auth dropdown (when logged in): Orders, Profile, Sign Out
- Remove `v-if="mobileMenuOpen"` from MobileNav — switch to `v-show` or transition for smoother toggle
- Hide cart badge when count is 0

### 5. Polish AppFooter
- Remove social media icon buttons (placeholders with no real links)
- Add Size Guide link in Help column

### 6. Polish AccountLayout
- Add active-route indicator on sidebar: `border-l-2 border-neutral-900 font-semibold` for current route
- Remove "ReSys.Shop / Account" inline header — move to a shared breadcrumb pattern

### 7. Polish AuthLayout
- Add subtle fade-in animation on mount (opacity transition)

### 8. CartDrawer — Reduced Motion Respect
- Check `prefers-reduced-motion` and disable slide transition when true

### 9. ThemeToggle — No changes needed (bug fix is in useTheme.ts)

## Testing

Add 6 layout smoke tests:

1. **DefaultLayout** renders header + footer + router-view outlet
2. **AuthLayout** renders centered card with logo + router-view
3. **AccountLayout** renders sidebar with 8 nav links + router-view
4. **AppHeader** shows Sign In when logged out, user icon when logged in
5. **CartDrawer** opens/closes with v-model toggle
6. **ThemeToggle** toggles `.app-dark` class on `<html>` element

## Non-Scope

- Catalog views (Cycle 2)
- Identity forms (Cycle 2)
- Ordering pages (Cycle 3)
- Profile pages (Cycle 3)
- SearchOverlay wiring (Cycle 2 — belongs to catalog)
- Sticky header scroll detection exact implementation (optional polish, may defer)
