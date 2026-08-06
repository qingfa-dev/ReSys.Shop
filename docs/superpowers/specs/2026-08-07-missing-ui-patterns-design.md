# Missing UI Patterns Design Spec

## Summary

Add 4 missing UI patterns from legacy storefronts: grid/list toggle, mobile filter drawer, cart drawer, notification dropdown.

## Patterns

### 1. Grid/List View Toggle

**Current:** `ShopView.vue` only shows grid layout.

**New:** Toggle button switching between grid and list views.

**Design:**
- `SelectButton` with two options: grid icon + list icon
- State: `viewMode: ref<'grid' | 'list'>` in ShopView
- Grid: existing 2/3/4-column responsive grid
- List: horizontal card layout (image left, info right)
- Persist preference in localStorage

**Files:** `features/catalog/views/ShopView.vue`, `features/catalog/components/ProductCard.vue`

### 2. Mobile Filter Drawer

**Current:** `FilterSidebar.vue` hidden on mobile (`hidden md:block`).

**New:** Slide-in drawer for filters on mobile.

**Design:**
- PrimeVue `Drawer` with `position="left"`
- Trigger: "Filters" button in shop toolbar (visible on mobile only)
- Contains same `FilterSidebar` content
- Close on filter apply or X button
- Overlay backdrop

**Files:** `features/catalog/views/ShopView.vue`, `features/catalog/components/FilterSidebar.vue`

### 3. Cart Drawer

**Current:** Cart is full page only (`/cart`).

**New:** Slide-in cart panel without leaving current page.

**Design:**
- PrimeVue `Drawer` with `position="right"`
- Trigger: Cart icon in header (opens drawer instead of navigating)
- Contains cart items list + order summary
- "View Full Cart" link to `/cart`
- "Proceed to Checkout" button
- Badge shows item count

**Files:** `app/components/layout/AppHeader.vue`, new `features/ordering/components/CartDrawer.vue`

### 4. Notification Bell Dropdown

**Current:** `NotificationBell.vue` shows popover with preferences link only.

**New:** Dropdown with notification list.

**Design:**
- Keep existing `Popover` but enhance content
- Show notification list (icon, title, message, time-ago)
- "Mark all as read" action
- "View all" link to notification preferences
- Unread count badge on bell icon

**Files:** `app/components/layout/AppHeader.vue`, `features/catalog/components/NotificationBell.vue`

## Verification

- [ ] Grid/list toggle switches layout
- [ ] Mobile filter drawer opens/closes
- [ ] Cart drawer shows items without page navigation
- [ ] Notification dropdown shows list
- [ ] All responsive on mobile
- [ ] All 257 unit tests pass
