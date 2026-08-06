# Missing UI Patterns Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add 4 missing UI patterns: grid/list toggle, mobile filter drawer, cart drawer, notification dropdown.

**Architecture:** Each pattern is an independent task. Uses PrimeVue Drawer, SelectButton, Popover components.

**Tech Stack:** Vue 3, Pinia, PrimeVue 5, Tailwind CSS v4

## Global Constraints

- Warnings-as-errors: any TypeScript/lint warning fails build
- PrimeVue components auto-import via `PrimeVueResolver`
- Mobile-first responsive design
- Run `pnpm run lint` and `pnpm run test:unit` after each task

---

## File Structure

| File | Action | Purpose |
|------|--------|---------|
| `features/catalog/views/ShopView.vue` | MODIFY | Add grid/list toggle + mobile filter drawer |
| `features/catalog/components/ProductCard.vue` | MODIFY | Add list layout variant |
| `app/components/layout/AppHeader.vue` | MODIFY | Add cart drawer trigger |
| `features/ordering/components/CartDrawer.vue` | CREATE | Slide-in cart panel |
| `features/catalog/components/NotificationBell.vue` | MODIFY | Add notification list dropdown |

---

## Tasks

### Task 1: Grid/list view toggle

**Files:**
- Modify: `app/Store/src/features/catalog/views/ShopView.vue`
- Modify: `app/Store/src/features/catalog/components/ProductCard.vue`

**Interfaces:**
- Consumes: Same product data
- Produces: `viewMode` state switching between grid and list

- [ ] **Step 1: Read ShopView.vue**

Read the file. Find the product grid section.

- [ ] **Step 2: Add viewMode state**

```typescript
const viewMode = ref<'grid' | 'list'>('grid')
```

- [ ] **Step 3: Add toggle button**

In the toolbar section, add:

```vue
<SelectButton v-model="viewMode" :options="[
  { icon: 'pi pi-th-large', value: 'grid' },
  { icon: 'pi pi-list', value: 'list' }
]" option-label="icon" option-value="value" class="ml-auto" />
```

- [ ] **Step 4: Conditional grid class**

Change the product grid container:

```vue
<div :class="viewMode === 'grid' ? 'grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4' : 'space-y-4'">
```

- [ ] **Step 5: Add list prop to ProductCard**

In `ProductCard.vue`, add prop:

```typescript
const props = defineProps<{ product: StoreProductListItemResponse; loading?: boolean; viewMode?: 'grid' | 'list' }>()
```

- [ ] **Step 6: Conditional layout in ProductCard**

```vue
<div :class="viewMode === 'list' ? 'flex gap-4' : ''">
  <div :class="viewMode === 'list' ? 'w-32 shrink-0' : ''">
    <!-- image -->
  </div>
  <div :class="viewMode === 'list' ? 'flex-1' : ''">
    <!-- info -->
  </div>
</div>
```

- [ ] **Step 7: Pass viewMode from ShopView**

```vue
<ProductCard :product="item" :view-mode="viewMode" />
```

- [ ] **Step 8: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 9: Commit**

```bash
cd app/Store && git add src/features/catalog/views/ShopView.vue src/features/catalog/components/ProductCard.vue
git commit -m "feat(catalog): add grid/list view toggle to shop page"
```

### Task 2: Mobile filter drawer

**Files:**
- Modify: `app/Store/src/features/catalog/views/ShopView.vue`

**Interfaces:**
- Consumes: Same filter data
- Produces: Drawer with filters on mobile

- [ ] **Step 1: Read ShopView.vue**

Read the file. Find the filter sidebar section.

- [ ] **Step 2: Add drawer state**

```typescript
const filterDrawerOpen = ref(false)
```

- [ ] **Step 3: Add mobile filter button**

In toolbar, add (visible on mobile only):

```vue
<Button label="Filters" icon="pi pi-filter" class="md:hidden" @click="filterDrawerOpen = true" />
```

- [ ] **Step 4: Add Drawer component**

```vue
<Drawer v-model:visible="filterDrawerOpen" header="Filters" position="left">
  <FilterSidebar
    :taxonomy-groups="taxonomyGroups"
    :option-types="optionTypes"
    :selected-taxon-ids="catalog.selectedTaxonIds"
    :selected-option-value-ids="catalog.selectedOptionValueIds"
    @toggle-taxon="handleToggleTaxon"
    @toggle-option-value="handleToggleOptionValue"
    @clear="catalog.clearFilters(); applyFilters()"
  />
</Drawer>
```

- [ ] **Step 5: Hide desktop sidebar on mobile**

Add `hidden md:block` to the existing sidebar container.

- [ ] **Step 6: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 7: Commit**

```bash
cd app/Store && git add src/features/catalog/views/ShopView.vue
git commit -m "feat(catalog): add mobile filter drawer"
```

### Task 3: Cart drawer

**Files:**
- Create: `app/Store/src/features/ordering/components/CartDrawer.vue`
- Modify: `app/Store/src/app/components/layout/AppHeader.vue`

**Interfaces:**
- Consumes: `useCartStore`
- Produces: Slide-in cart panel

- [ ] **Step 1: Read AppHeader.vue**

Read the file. Find the cart icon section.

- [ ] **Step 2: Add drawer state**

```typescript
const cartDrawerOpen = ref(false)
```

- [ ] **Step 3: Change cart icon to open drawer**

Replace router-link on cart icon with:

```vue
<button @click="cartDrawerOpen = true" class="relative">
  <i class="pi pi-shopping-cart text-xl" />
  <Badge v-if="cart.itemCount > 0" :value="cart.itemCount" class="absolute -top-2 -right-2" />
</button>
```

- [ ] **Step 4: Create CartDrawer.vue**

Create `app/Store/src/features/ordering/components/CartDrawer.vue`:

```vue
<script setup lang="ts">
import { useCartStore } from '../stores/cartStore'
import CartItem from './CartItem.vue'
import OrderSummary from './OrderSummary.vue'

defineProps<{ visible: boolean }>()
const emit = defineEmits<{ 'update:visible': [value: boolean] }>()
const cart = useCartStore()
</script>
<template>
  <Drawer :visible="visible" @update:visible="emit('update:visible', $event)" header="Shopping Cart" position="right" class="w-96">
    <div v-if="cart.isEmpty" class="text-center py-8">
      <i class="pi pi-shopping-cart text-4xl text-stone-300 mb-4" />
      <p class="text-stone-500">Your cart is empty</p>
      <router-link to="/shop" class="text-teal-600 hover:underline mt-2 inline-block">Continue Shopping</router-link>
    </div>
    <div v-else class="space-y-4">
      <CartItem v-for="item in cart.items" :key="item.id" :item="item" />
    </div>
    <template #footer>
      <OrderSummary v-if="!cart.isEmpty" />
    </template>
  </Drawer>
</template>
```

- [ ] **Step 5: Add CartDrawer to AppHeader**

```vue
<CartDrawer v-model:visible="cartDrawerOpen" />
```

- [ ] **Step 6: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 7: Commit**

```bash
cd app/Store && git add src/features/ordering/components/CartDrawer.vue src/app/components/layout/AppHeader.vue
git commit -m "feat(ordering): add cart drawer slide-in panel"
```

### Task 4: Notification bell dropdown

**Files:**
- Modify: `app/Store/src/features/catalog/components/NotificationBell.vue`

**Interfaces:**
- Consumes: Notification data
- Produces: Dropdown with notification list

- [ ] **Step 1: Read NotificationBell.vue**

Read the file. Note the current Popover usage.

- [ ] **Step 2: Enhance Popover content**

Replace the Popover content with:

```vue
<Popover ref="popover">
  <div class="w-80">
    <div class="flex items-center justify-between mb-3">
      <h3 class="font-semibold">Notifications</h3>
      <Button text size="small" label="Mark all read" @click="markAllRead" />
    </div>
    <div v-if="notifications.length === 0" class="text-center py-4 text-stone-500">
      No notifications
    </div>
    <div v-else class="space-y-2 max-h-64 overflow-y-auto">
      <div v-for="notif in notifications" :key="notif.id" class="flex gap-3 p-2 rounded-lg hover:bg-stone-50">
        <i :class="getIcon(notif.type)" class="text-teal-600 mt-0.5" />
        <div class="flex-1 min-w-0">
          <p class="text-sm font-medium text-stone-900 truncate">{{ notif.title }}</p>
          <p class="text-xs text-stone-500 truncate">{{ notif.message }}</p>
          <p class="text-xs text-stone-400 mt-1">{{ timeAgo(notif.createdAt) }}</p>
        </div>
      </div>
    </div>
    <router-link to="/account/notifications" class="block text-center text-sm text-teal-600 hover:underline mt-3 pt-3 border-t">
      View all
    </router-link>
  </div>
</Popover>
```

- [ ] **Step 3: Add helper functions**

```typescript
function getIcon(type: string) {
  const icons: Record<string, string> = {
    order: 'pi pi-shopping-bag',
    promotion: 'pi pi-tag',
    system: 'pi pi-info-circle',
  }
  return icons[type] ?? 'pi pi-bell'
}

function timeAgo(date: string) {
  const seconds = Math.floor((Date.now() - new Date(date).getTime()) / 1000)
  if (seconds < 60) return 'just now'
  if (seconds < 3600) return `${Math.floor(seconds / 60)}m ago`
  if (seconds < 86400) return `${Math.floor(seconds / 3600)}h ago`
  return `${Math.floor(seconds / 86400)}d ago`
}
```

- [ ] **Step 4: Run lint**

```bash
cd app/Store && pnpm run lint
```

Expected: PASS

- [ ] **Step 5: Commit**

```bash
cd app/Store && git add src/features/catalog/components/NotificationBell.vue
git commit -m "feat(catalog): enhance NotificationBell with dropdown list"
```
