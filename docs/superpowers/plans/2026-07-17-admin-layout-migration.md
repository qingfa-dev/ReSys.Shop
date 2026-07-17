# Admin Layout Shell Migration — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the broken admin layout shell in `app/Admin` with a ported, full-featured Sakai admin shell from `app/lagacy/Admin`.

**Architecture:** Atomic file swap — delete all 11 new Admin layout files and 1 provider file, rewrite the composable with cleaner internals but preserved API, then port each legacy component to PascalCase naming in the new Admin directory. Feature pages are untouched.

**Tech Stack:** Vue 3.5, PrimeVue 4.5, vue-router 5.1, vue-i18n 11, Pinia 3, TailwindCSS 4, SCSS (Sakai theme), TypeScript 6

## Global Constraints

- TreatWarningsAsErrors: true (oxlint + eslint must pass with zero warnings)
- PascalCase file naming: `*.Layout.vue`, `*.Component.vue`, `*.Type.ts`
- All imports use `@/` alias; no relative paths beyond `./` for same-directory files
- `vue-i18n` + JSON locale files replace legacy TypeScript `*.locales.ts`

---

## Task 1: Delete all existing layout files and the broken AppProviders

**Files:**
- Delete: `app/Admin/src/app/layout/Main.Layout.vue`
- Delete: `app/Admin/src/app/layout/Topbar.Layout.vue`
- Delete: `app/Admin/src/app/layout/Sidebar.Layout.vue`
- Delete: `app/Admin/src/app/layout/Menu.Layout.vue`
- Delete: `app/Admin/src/app/layout/MenuItem.Layout.vue`
- Delete: `app/Admin/src/app/layout/Footer.Layout.vue`
- Delete: `app/Admin/src/app/layout/Configurator.Layout.vue`
- Delete: `app/Admin/src/app/layout/composables/layout.composable.ts`
- Delete: `app/Admin/src/app/layout/components/FloatingConfigurator.Component.vue`
- Delete: `app/Admin/src/app/layout/components/GlobalSearch.Component.vue`
- Delete: `app/Admin/src/app/providers/AppProviders.Component.vue`
- Delete: `app/Admin/src/app/layout/components/` directory

**Interfaces:**
- Consumes: nothing
- Produces: clean slate for porting

- [ ] **Step 1: Delete files**

```bash
rm app/Admin/src/app/layout/Main.Layout.vue
rm app/Admin/src/app/layout/Topbar.Layout.vue
rm app/Admin/src/app/layout/Sidebar.Layout.vue
rm app/Admin/src/app/layout/Menu.Layout.vue
rm app/Admin/src/app/layout/MenuItem.Layout.vue
rm app/Admin/src/app/layout/Footer.Layout.vue
rm app/Admin/src/app/layout/Configurator.Layout.vue
rm app/Admin/src/app/layout/composables/layout.composable.ts
rm app/Admin/src/app/layout/components/FloatingConfigurator.Component.vue
rm app/Admin/src/app/layout/components/GlobalSearch.Component.vue
rm app/Admin/src/app/providers/AppProviders.Component.vue
rmdir app/Admin/src/app/layout/components/
```

- [ ] **Step 2: Commit**

```bash
git add -A && git commit -m "chore(admin): remove broken layout shell files"
```

---

## Task 2: Rewrite layout composable with individual refs, preserved API

**Files:**
- Create: `app/Admin/src/app/layout/composables/layout.composable.ts`

**Interfaces:**
- Consumes: nothing (module-level refs, no deps)
- Produces:
  - `useLayout()` → `{ layoutConfig, layoutState, isDarkTheme, toggleDarkMode, toggleMenu, toggleConfigSidebar, hideMobileMenu, changeMenuMode, isDesktop, hasOpenOverlay }`
  - `layoutConfig: { preset: string, primary: string, surface: string | null, darkTheme: boolean, menuMode: string }`
  - `layoutState: { staticMenuInactive: boolean, overlayMenuActive: boolean, profileSidebarVisible: boolean, configSidebarVisible: boolean, sidebarExpanded: boolean, menuHoverActive: boolean, activeMenuItem: string | null, activePath: string | null, mobileMenuActive: boolean, anchored: boolean }`

- [ ] **Step 1: Create the composable**

```typescript
import { computed, ref } from 'vue'

const preset = ref('Aura')
const primary = ref('emerald')
const surface = ref<string | null>(null)
const darkTheme = ref(false)
const menuMode = ref('static')
const staticMenuInactive = ref(false)
const overlayMenuActive = ref(false)
const profileSidebarVisible = ref(false)
const configSidebarVisible = ref(false)
const sidebarExpanded = ref(false)
const menuHoverActive = ref(false)
const activeMenuItem = ref<string | null>(null)
const activePath = ref<string | null>(null)
const mobileMenuActive = ref(false)
const anchored = ref(false)

export function useLayout() {
  const layoutConfig = {
    get preset() { return preset.value },
    set preset(v: string) { preset.value = v },
    get primary() { return primary.value },
    set primary(v: string) { primary.value = v },
    get surface() { return surface.value },
    set surface(v: string | null) { surface.value = v },
    get darkTheme() { return darkTheme.value },
    set darkTheme(v: boolean) { darkTheme.value = v },
    get menuMode() { return menuMode.value },
    set menuMode(v: string) { menuMode.value = v },
  }

  const layoutState = {
    get staticMenuInactive() { return staticMenuInactive.value },
    set staticMenuInactive(v: boolean) { staticMenuInactive.value = v },
    get overlayMenuActive() { return overlayMenuActive.value },
    set overlayMenuActive(v: boolean) { overlayMenuActive.value = v },
    get profileSidebarVisible() { return profileSidebarVisible.value },
    set profileSidebarVisible(v: boolean) { profileSidebarVisible.value = v },
    get configSidebarVisible() { return configSidebarVisible.value },
    set configSidebarVisible(v: boolean) { configSidebarVisible.value = v },
    get sidebarExpanded() { return sidebarExpanded.value },
    set sidebarExpanded(v: boolean) { sidebarExpanded.value = v },
    get menuHoverActive() { return menuHoverActive.value },
    set menuHoverActive(v: boolean) { menuHoverActive.value = v },
    get activeMenuItem() { return activeMenuItem.value },
    set activeMenuItem(v: string | null) { activeMenuItem.value = v },
    get activePath() { return activePath.value },
    set activePath(v: string | null) { activePath.value = v },
    get mobileMenuActive() { return mobileMenuActive.value },
    set mobileMenuActive(v: boolean) { mobileMenuActive.value = v },
    get anchored() { return anchored.value },
    set anchored(v: boolean) { anchored.value = v },
  }

  const isDarkTheme = computed(() => darkTheme.value)

  function toggleDarkMode() {
    if (!document.startViewTransition) {
      darkTheme.value = !darkTheme.value
      document.documentElement.classList.toggle('app-dark', darkTheme.value)
      return
    }
    document.startViewTransition(() => {
      darkTheme.value = !darkTheme.value
      document.documentElement.classList.toggle('app-dark', darkTheme.value)
    })
  }

  const isDesktop = () => window.innerWidth > 991

  function toggleMenu() {
    if (isDesktop()) {
      if (menuMode.value === 'static') {
        staticMenuInactive.value = !staticMenuInactive.value
      }
      if (menuMode.value === 'overlay') {
        overlayMenuActive.value = !overlayMenuActive.value
      }
    } else {
      mobileMenuActive.value = !mobileMenuActive.value
    }
  }

  function toggleConfigSidebar() {
    configSidebarVisible.value = !configSidebarVisible.value
  }

  function hideMobileMenu() {
    mobileMenuActive.value = false
  }

  function changeMenuMode(mode: string) {
    menuMode.value = mode
    staticMenuInactive.value = false
    mobileMenuActive.value = false
    sidebarExpanded.value = false
    menuHoverActive.value = false
    anchored.value = false
  }

  const hasOpenOverlay = computed(() => overlayMenuActive.value)

  return {
    layoutConfig,
    layoutState,
    isDarkTheme,
    toggleDarkMode,
    toggleConfigSidebar,
    toggleMenu,
    hideMobileMenu,
    changeMenuMode,
    isDesktop,
    hasOpenOverlay,
  }
}
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/layout/composables/layout.composable.ts
git commit -m "feat(admin): rewrite layout composable with individual refs"
```

---

## Task 3: Create Footer.Layout.vue

**Files:**
- Create: `app/Admin/src/app/layout/Footer.Layout.vue`

**Interfaces:**
- Consumes: nothing
- Produces: AppFooter component with copyright text

- [ ] **Step 1: Create the file**

```vue
<template>
  <div class="layout-footer">
    <span>ReSys.Shop &copy; {{ new Date().getFullYear() }} — All rights reserved</span>
  </div>
</template>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/layout/Footer.Layout.vue
git commit -m "feat(admin): port footer layout component"
```

---

## Task 4: Create MenuItem.Layout.vue

**Files:**
- Create: `app/Admin/src/app/layout/MenuItem.Layout.vue`

**Interfaces:**
- Consumes: `useLayout()` from `./composables/layout.composable.ts`, `useRoute()` from `vue-router`
- Produces: `AppMenuItem` component (named export via `defineOptions`), `MenuItem` interface (exported)
- Props: `item: MenuItem`, `index?: number`, `root?: boolean`
- Children import: `import AppMenuItem from './MenuItem.Layout.vue'`

- [ ] **Step 1: Create the file**

```vue
<script setup lang="ts">
import { useLayout } from './composables/layout.composable'
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'

export interface MenuItem {
  label?: string
  icon?: string
  to?: string | object
  url?: string
  target?: string
  items?: MenuItem[]
  separator?: boolean
  visible?: boolean
  disabled?: boolean
  class?: string
  command?: (event: { originalEvent: Event; item: MenuItem }) => void
}

const route = useRoute()
const { layoutState } = useLayout()

defineOptions({
  name: 'AppMenuItem',
})

const props = defineProps<{
  item: MenuItem
  index?: number
  root?: boolean
}>()

const active = ref(false)

const isActive = computed(() => {
  if (props.item.to && typeof props.item.to === 'string' && route.path === props.item.to) return true
  if (props.item.to && typeof props.item.to === 'object' && 'name' in props.item.to) {
    return route.name === props.item.to.name
  }
  if (props.item.items) {
    return props.item.items.some(child => {
      if (typeof child.to === 'string' && child.to === route.path) return true
      if (child.to && typeof child.to === 'object' && 'name' in child.to && route.name === child.to.name) return true
      if (child.items) return child.items.some(sub => {
        if (typeof sub.to === 'string' && sub.to === route.path) return true
        if (sub.to && typeof sub.to === 'object' && 'name' in sub.to && route.name === sub.to.name) return true
        return false
      })
      return false
    })
  }
  return false
})

watch(() => route.path, (newPath) => {
  if (props.item.items) {
    const hasActiveChild = props.item.items.some(child => {
      if (typeof child.to === 'string' && child.to === newPath) return true
      if (child.to && typeof child.to === 'object' && 'name' in child.to && route.name === child.to.name) return true
      if (child.items) return child.items.some(sub => {
        if (typeof sub.to === 'string' && sub.to === newPath) return true
        if (sub.to && typeof sub.to === 'object' && 'name' in sub.to && route.name === sub.to.name) return true
        return false
      })
      return false
    })
    if (hasActiveChild) active.value = true
  }
}, { immediate: true })

const itemClick = (event: Event, item: MenuItem) => {
  if (item.disabled) {
    event.preventDefault()
    return
  }
  if (item.command) {
    item.command({ originalEvent: event, item })
  }
  if (item.items) {
    active.value = !active.value
  }
  if (item.to || item.url) {
    layoutState.overlayMenuActive = false
    layoutState.mobileMenuActive = false
  }
}
</script>

<template>
  <li :class="{ 'layout-root-menuitem': root, 'active-menuitem': active || isActive }">
    <div v-if="root && item.visible !== false" class="layout-menuitem-root-text">
      {{ item.label }}
    </div>

    <a
      v-if="!root && item.items && item.visible !== false"
      :href="item.url"
      @click="itemClick($event, item)"
      :class="[item.class, { 'active-route': isActive }]"
      :target="item.target"
      tabindex="0"
    >
      <i v-if="item.icon" :class="item.icon" class="layout-menuitem-icon" />
      <span class="layout-menuitem-text">{{ item.label }}</span>
      <i class="pi pi-fw pi-angle-down layout-submenu-toggler" />
    </a>

    <router-link
      v-if="!root && item.to && !item.items && item.visible !== false"
      @click="itemClick($event, item)"
      exactActiveClass="active-route"
      :class="[item.class]"
      tabindex="0"
      :to="item.to"
    >
      <i v-if="item.icon" :class="item.icon" class="layout-menuitem-icon" />
      <span class="layout-menuitem-text">{{ item.label }}</span>
    </router-link>

    <Transition v-if="item.items && item.visible !== false" name="layout-submenu">
      <ul v-show="root ? true : active || isActive" class="layout-submenu">
        <AppMenuItem
          v-for="(child, i) in item.items"
          :key="child.label + '_' + i"
          :item="child"
          :index="i"
          :root="false"
        />
      </ul>
    </Transition>
  </li>
</template>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/layout/MenuItem.Layout.vue
git commit -m "feat(admin): port menu item layout component"
```

---

## Task 5: Create Menu.Layout.vue

**Files:**
- Create: `app/Admin/src/app/layout/Menu.Layout.vue`

**Interfaces:**
- Consumes: `import AppMenuItem from './MenuItem.Layout.vue'`, `import type { MenuItem } from './MenuItem.Layout.vue'`
- Produces: AppMenu component rendering `<ul class="layout-menu">`

- [ ] **Step 1: Create the file**

```vue
<script setup lang="ts">
import { ref } from 'vue'
import AppMenuItem from './MenuItem.Layout.vue'
import type { MenuItem } from './MenuItem.Layout.vue'

const model = ref<MenuItem[]>([
  {
    label: 'Home',
    items: [
      { label: 'Dashboard', icon: 'pi pi-fw pi-home', to: { name: 'reports.dashboard' } },
      { label: 'My Profile', icon: 'pi pi-fw pi-user', to: { name: 'profile' } },
    ],
  },
  {
    label: 'Catalog',
    items: [
      { label: 'Dashboard', icon: 'pi pi-fw pi-th-large', to: { name: 'catalog.dashboard' } },
      {
        label: 'Products',
        icon: 'pi pi-fw pi-shopping-bag',
        items: [
          { label: 'List', icon: 'pi pi-fw pi-list', to: { name: 'catalog.products.list' } },
          { label: 'Add New', icon: 'pi pi-fw pi-plus-circle', to: { name: 'catalog.products.create' } },
        ],
      },
      {
        label: 'Taxonomies',
        icon: 'pi pi-fw pi-sitemap',
        items: [
          { label: 'Manager', icon: 'pi pi-fw pi-sitemap', to: { name: 'catalog.taxonomies.list' } },
          { label: 'Categories', icon: 'pi pi-fw pi-tags', to: { name: 'catalog.taxa.list' } },
        ],
      },
      {
        label: 'Option Types',
        icon: 'pi pi-fw pi-list',
        items: [
          { label: 'Manager', icon: 'pi pi-fw pi-list', to: { name: 'catalog.option-types.list' } },
          { label: 'Values', icon: 'pi pi-fw pi-th-large', to: { name: 'catalog.option-values.list' } },
        ],
      },
      {
        label: 'Property Types',
        icon: 'pi pi-fw pi-tags',
        items: [
          { label: 'List', icon: 'pi pi-fw pi-list', to: { name: 'catalog.property-types.list' } },
          { label: 'Add New', icon: 'pi pi-fw pi-plus-circle', to: { name: 'catalog.property-types.create' } },
        ],
      },
    ],
  },
  {
    label: 'Inventory',
    items: [
      { label: 'Stock Items', icon: 'pi pi-fw pi-box', to: { name: 'inventory.stocks.list' } },
      { label: 'Stock Units', icon: 'pi pi-fw pi-cubes', to: { name: 'inventory.units.list' } },
      { label: 'Locations', icon: 'pi pi-fw pi-building', to: { name: 'inventory.locations.list' } },
      { label: 'Transfers', icon: 'pi pi-fw pi-arrow-right-arrow-left', to: { name: 'inventory.transfers.list' } },
    ],
  },
  {
    label: 'Sales',
    items: [
      { label: 'All Orders', icon: 'pi pi-fw pi-shopping-cart', to: { name: 'ordering.orders.list' } },
      { label: 'Fulfillment', icon: 'pi pi-fw pi-box', to: { name: 'ordering.fulfillment.queue' } },
      { label: 'Reports', icon: 'pi pi-fw pi-chart-bar', to: { name: 'reports.dashboard' } },
    ],
  },
  {
    label: 'Identity & Access',
    items: [
      { label: 'Staff', icon: 'pi pi-fw pi-id-card', to: { name: 'admin-users' } },
      { label: 'Customers', icon: 'pi pi-fw pi-users', to: { name: 'customer-users' } },
      { label: 'Roles', icon: 'pi pi-fw pi-shield', to: { name: 'roles-list' } },
      { label: 'Permissions', icon: 'pi pi-fw pi-key', to: { name: 'permissions-list' } },
    ],
  },
])
</script>

<template>
  <ul class="layout-menu">
    <template v-for="(item, i) in model" :key="item.label">
      <AppMenuItem v-if="!item.separator" :item="item" :index="i" root></AppMenuItem>
      <li v-if="item.separator" class="menu-separator"></li>
    </template>
  </ul>
</template>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/layout/Menu.Layout.vue
git commit -m "feat(admin): port menu layout component"
```

---

## Task 6: Create Sidebar.Layout.vue

**Files:**
- Create: `app/Admin/src/app/layout/Sidebar.Layout.vue`

**Interfaces:**
- Consumes: `import AppMenu from './Menu.Layout.vue'`
- Produces: AppSidebar component wrapping `<div class="layout-sidebar"><AppMenu /></div>`

- [ ] **Step 1: Create the file**

```vue
<script setup lang="ts">
import AppMenu from './Menu.Layout.vue'
</script>

<template>
  <div class="layout-sidebar">
    <AppMenu />
  </div>
</template>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/layout/Sidebar.Layout.vue
git commit -m "feat(admin): port sidebar layout component"
```

---

## Task 7: Create GlobalSearch.Component.vue

**Files:**
- Create: `app/Admin/src/app/layout/components/GlobalSearch.Component.vue`

**Interfaces:**
- Consumes: `useRouter()` from `vue-router`, `useI18n()` from `vue-i18n` (replaces legacy `generalLocales`)
- Produces: GlobalSearch component with InputText + OverlayPanel for menu page search

- [ ] **Step 1: Create directory**

```bash
mkdir -p app/Admin/src/app/layout/components
```

- [ ] **Step 2: Create the file**

```vue
<script setup lang="ts">
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const router = useRouter()

const searchQuery = ref('')
const overlayVisible = ref(false)

interface SearchResult {
  label: string
  description: string
  to: string
}

const searchResults = ref<SearchResult[]>([])

const allPages: SearchResult[] = [
  { label: 'Dashboard', description: 'Overview and statistics', to: '/' },
  { label: 'Users', description: 'Manage user accounts', to: '/identity/users' },
]

watch(searchQuery, (query) => {
  if (!query.trim()) {
    searchResults.value = []
    return
  }
  const q = query.toLowerCase()
  searchResults.value = allPages.filter(
    (page) =>
      page.label.toLowerCase().includes(q) || page.description.toLowerCase().includes(q),
  )
})

function goTo(to: string) {
  overlayVisible.value = false
  searchQuery.value = ''
  router.push(to)
}
</script>

<template>
  <span class="p-input-icon-left relative">
    <i class="pi pi-search" />
    <InputText
      v-model="searchQuery"
      :placeholder="t('layout.search')"
      class="w-64 rounded-border"
      @focus="overlayVisible = true"
      @keydown.escape="overlayVisible = false"
    />
    <OverlayPanel ref="op" :visible="overlayVisible" @hide="overlayVisible = false">
      <div class="flex flex-col gap-2" style="min-width: 280px">
        <div
          v-for="result in searchResults"
          :key="result.to"
          class="cursor-pointer rounded p-2 hover:bg-surface-100"
          @click="goTo(result.to)"
        >
          <span class="font-medium">{{ result.label }}</span>
          <span class="ml-2 text-sm text-color-secondary">{{ result.description }}</span>
        </div>
        <div v-if="searchResults.length === 0 && searchQuery.length > 0" class="p-2 text-sm text-color-secondary">
          {{ t('layout.noResults') }}
        </div>
      </div>
    </OverlayPanel>
  </span>
</template>
```

- [ ] **Step 3: Commit**

```bash
git add app/Admin/src/app/layout/components/GlobalSearch.Component.vue
git commit -m "feat(admin): port global search component with vue-i18n"
```

---

## Task 8: Create Configurator.Layout.vue

**Files:**
- Create: `app/Admin/src/app/layout/Configurator.Layout.vue`

**Interfaces:**
- Consumes: `useLayout()` from `./composables/layout.composable.ts`
- Produces: AppConfigurator component — theme preset/color/surface/menu-mode configurator sidebar panel

- [ ] **Step 1: Create the file**

```vue
<script setup lang="ts">
import { useLayout } from './composables/layout.composable'

interface Preset {
  label: string
  value: string
}

interface ColorOption {
  label: string
  value: string
}

const { layoutConfig, layoutState, changeMenuMode, toggleConfigSidebar } = useLayout()

const presets: Preset[] = [
  { label: 'Aura', value: 'Aura' },
  { label: 'Lara', value: 'Lara' },
  { label: 'Nora', value: 'Nora' },
]

const surfaceOptions: ColorOption[] = [
  { label: 'Slate', value: 'slate' },
  { label: 'Gray', value: 'gray' },
  { label: 'Zinc', value: 'zinc' },
  { label: 'Neutral', value: 'neutral' },
  { label: 'Stone', value: 'stone' },
  { label: 'Soho', value: 'soho' },
  { label: 'Viva', value: 'viva' },
  { label: 'Owl', value: 'owl' },
]

const primaryColors: ColorOption[] = [
  { label: 'Emerald', value: 'emerald' },
  { label: 'Green', value: 'green' },
  { label: 'Lime', value: 'lime' },
  { label: 'Orange', value: 'orange' },
  { label: 'Amber', value: 'amber' },
  { label: 'Yellow', value: 'yellow' },
  { label: 'Teal', value: 'teal' },
  { label: 'Cyan', value: 'cyan' },
  { label: 'Sky', value: 'sky' },
  { label: 'Blue', value: 'blue' },
  { label: 'Indigo', value: 'indigo' },
  { label: 'Violet', value: 'violet' },
  { label: 'Purple', value: 'purple' },
  { label: 'Fuchsia', value: 'fuchsia' },
  { label: 'Pink', value: 'pink' },
  { label: 'Rose', value: 'rose' },
]

const menuModes = [
  { label: 'Static', value: 'static', icon: 'pi pi-bars' },
  { label: 'Overlay', value: 'overlay', icon: 'pi pi-window-maximize' },
]
</script>

<template>
  <div class="layout-config-sidebar" :class="{ 'layout-config-sidebar-active': layoutState.configSidebarVisible }">
    <div class="layout-config-sidebar-header">
      <span class="text-lg font-semibold">Theme Config</span>
      <button class="p-link layout-config-close" @click="toggleConfigSidebar">
        <i class="pi pi-times"></i>
      </button>
    </div>

    <div class="layout-config-sidebar-content">
      <div class="config-section">
        <h4>Preset</h4>
        <div class="config-options">
          <button
            v-for="preset in presets"
            :key="preset.value"
            class="config-option"
            :class="{ 'config-option-active': layoutConfig.preset === preset.value }"
            @click="layoutConfig.preset = preset.value"
          >
            {{ preset.label }}
          </button>
        </div>
      </div>

      <div class="config-section">
        <h4>Primary Color</h4>
        <div class="config-colors">
          <button
            v-for="color in primaryColors"
            :key="color.value"
            class="config-color"
            :style="{ backgroundColor: `var(--p-${color.value}-500)` }"
            :class="{ 'config-color-active': layoutConfig.primary === color.value }"
            :title="color.label"
            @click="layoutConfig.primary = color.value"
          />
        </div>
      </div>

      <div class="config-section">
        <h4>Surface</h4>
        <div class="config-colors">
          <button
            v-for="surface in surfaceOptions"
            :key="surface.value"
            class="config-color"
            :style="{ backgroundColor: `var(--p-${surface.value}-500)` }"
            :class="{ 'config-color-active': layoutConfig.surface === surface.value }"
            :title="surface.label"
            @click="layoutConfig.surface = surface.value"
          />
        </div>
      </div>

      <div class="config-section">
        <h4>Menu Mode</h4>
        <div class="config-options">
          <button
            v-for="mode in menuModes"
            :key="mode.value"
            class="config-option"
            :class="{ 'config-option-active': layoutConfig.menuMode === mode.value }"
            @click="changeMenuMode(mode.value)"
          >
            <i :class="mode.icon"></i>
            {{ mode.label }}
          </button>
        </div>
      </div>
    </div>

    <div class="layout-config-sidebar-footer">
      <p class="text-xs text-surface-500">Changes apply immediately</p>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/layout/Configurator.Layout.vue
git commit -m "feat(admin): port configurator layout component"
```

---

## Task 9: Create Topbar.Layout.vue

**Files:**
- Create: `app/Admin/src/app/layout/Topbar.Layout.vue`

**Interfaces:**
- Consumes: `useLayout()` from `./composables/layout.composable.ts`, `import GlobalSearch from './components/GlobalSearch.Component.vue'`, `import AppConfigurator from './Configurator.Layout.vue'`
- Produces: AppTopbar component with logo, search bar, dark toggle, configurator button, user dropdown

- [ ] **Step 1: Create the file**

```vue
<script setup lang="ts">
import { useLayout } from './composables/layout.composable'
import GlobalSearch from './components/GlobalSearch.Component.vue'
import AppConfigurator from './Configurator.Layout.vue'

const { toggleMenu, toggleDarkMode, isDarkTheme } = useLayout()
</script>

<template>
  <div class="layout-topbar">
    <div class="layout-topbar-logo-container">
      <button class="layout-menu-button layout-topbar-action" @click="toggleMenu">
        <i class="pi pi-bars"></i>
      </button>
      <router-link to="/" class="layout-topbar-logo">
        <span class="topbar-brand-text">ReSys.Shop</span>
      </router-link>
    </div>

    <GlobalSearch />

    <div class="layout-topbar-actions">
      <div class="layout-config-menu">
        <button type="button" class="layout-topbar-action" @click="toggleDarkMode">
          <i :class="['pi', { 'pi-moon': isDarkTheme, 'pi-sun': !isDarkTheme }]"></i>
        </button>
        <div class="relative">
          <button
            v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
            type="button"
            class="layout-topbar-action"
          >
            <i class="pi pi-palette"></i>
          </button>
          <AppConfigurator />
        </div>
      </div>

      <button
        class="layout-topbar-menu-button layout-topbar-action"
        v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
      >
        <i class="pi pi-ellipsis-v"></i>
      </button>

      <div class="layout-topbar-menu hidden lg:block">
        <div class="layout-topbar-menu-content">
          <button type="button" class="layout-topbar-action">
            <i class="pi pi-calendar"></i>
            <span>Calendar</span>
          </button>
          <button type="button" class="layout-topbar-action">
            <i class="pi pi-inbox"></i>
            <span>Messages</span>
          </button>
          <button type="button" class="layout-topbar-action">
            <i class="pi pi-user"></i>
            <span>Profile</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/layout/Topbar.Layout.vue
git commit -m "feat(admin): port topbar layout component"
```

---

## Task 10: Create FloatingConfigurator.Component.vue

**Files:**
- Create: `app/Admin/src/app/layout/components/FloatingConfigurator.Component.vue`

**Interfaces:**
- Consumes: `useLayout()` from `@/app/layout/composables/layout.composable`, `import AppConfigurator from '../Configurator.Layout.vue'`
- Produces: FloatingConfigurator component — fixed floating buttons (dark toggle + palette)

- [ ] **Step 1: Create the file**

```vue
<script setup lang="ts">
import AppConfigurator from '../Configurator.Layout.vue'
import { useLayout } from '../composables/layout.composable'

const { toggleDarkMode, isDarkTheme } = useLayout()
</script>

<template>
  <div class="fixed flex gap-4 top-8 right-8 z-50">
    <Button
      type="button"
      @click="toggleDarkMode"
      rounded
      :icon="isDarkTheme ? 'pi pi-moon' : 'pi pi-sun'"
      severity="secondary"
    />
    <div class="relative">
      <Button
        icon="pi pi-palette"
        v-styleclass="{
          selector: '@next',
          enterFromClass: 'hidden',
          enterActiveClass: 'animate-scalein',
          leaveToClass: 'hidden',
          leaveActiveClass: 'animate-fadeout',
          hideOnOutsideClick: true,
        }"
        type="button"
        rounded
      />
      <AppConfigurator />
    </div>
  </div>
</template>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/layout/components/FloatingConfigurator.Component.vue
git commit -m "feat(admin): port floating configurator component"
```

---

## Task 11: Create Main.Layout.vue

**Files:**
- Create: `app/Admin/src/app/layout/Main.Layout.vue`

**Interfaces:**
- Consumes: `useLayout()` from `./composables/layout.composable.ts`, all child layout components, `AppBreadcrumb` from `@/shared/components/Breadcrumb.Component.vue`, `RouterView` from `vue-router`, `ConfirmDialog` from `primevue/confirmdialog`
- Produces: AppLayout shell with sidebar, topbar, breadcrumb, router-view, footer, floating configurator, mobile mask, confirm dialog

- [ ] **Step 1: Create the file**

```vue
<script setup lang="ts">
import { useLayout } from './composables/layout.composable'
import { computed, watch, ref } from 'vue'
import { RouterView } from 'vue-router'
import ConfirmDialog from 'primevue/confirmdialog'
import AppTopbar from './Topbar.Layout.vue'
import AppFooter from './Footer.Layout.vue'
import AppSidebar from './Sidebar.Layout.vue'
import AppBreadcrumb from '@/shared/components/Breadcrumb.Component.vue'
import FloatingConfigurator from './components/FloatingConfigurator.Component.vue'

const { layoutConfig, layoutState, isDarkTheme, hideMobileMenu } = useLayout()

const containerClass = computed(() => ({
  'layout-overlay': layoutConfig.menuMode === 'overlay',
  'layout-static': layoutConfig.menuMode === 'static',
  'layout-static-inactive': layoutState.staticMenuInactive && layoutConfig.menuMode === 'static',
  'layout-overlay-active': layoutState.overlayMenuActive,
  'layout-mobile-active': layoutState.mobileMenuActive,
  'layout-dark': isDarkTheme.value,
}))

const outsideClickListener = ref<((event: MouseEvent) => void) | null>(null)

watch(() => layoutState.mobileMenuActive, (newVal) => {
  if (newVal) {
    bindOutsideClickListener()
  } else {
    unbindOutsideClickListener()
  }
})

const bindOutsideClickListener = () => {
  if (!outsideClickListener.value) {
    outsideClickListener.value = (event: MouseEvent) => {
      if (isOutsideClicked(event)) {
        hideMobileMenu()
      }
    }
    document.addEventListener('click', outsideClickListener.value)
  }
}

const unbindOutsideClickListener = () => {
  if (outsideClickListener.value) {
    document.removeEventListener('click', outsideClickListener.value)
    outsideClickListener.value = null
  }
}

const isOutsideClicked = (event: MouseEvent) => {
  const sidebarEl = document.querySelector('.layout-sidebar')
  const topbarEl = document.querySelector('.layout-menu-button')
  return !(
    sidebarEl?.isSameNode(event.target as Node) ||
    sidebarEl?.contains(event.target as Node) ||
    topbarEl?.isSameNode(event.target as Node) ||
    topbarEl?.contains(event.target as Node)
  )
}
</script>

<template>
  <div class="layout-wrapper" :class="containerClass">
    <AppTopbar />
    <AppSidebar />
    <div class="layout-main-container">
      <div class="layout-main">
        <AppBreadcrumb />
        <router-view />
      </div>
      <AppFooter />
    </div>
    <FloatingConfigurator />
    <div class="layout-mask animate-fadein" @click="hideMobileMenu" />
  </div>
  <ConfirmDialog />
</template>

<style lang="scss" scoped>
:global(.p-toast.p-component.p-toast-top-right),
:global(.p-toast.p-component.p-toast-top-left),
:global(.p-toast.p-component.p-toast-top-center) {
  top: 5rem;
}
</style>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/layout/Main.Layout.vue
git commit -m "feat(admin): port main layout shell component"
```

---

## Task 12: Create AppProviders.Component.vue

**Files:**
- Create: `app/Admin/src/app/providers/AppProviders.Component.vue`

**Interfaces:**
- Consumes: `ConfirmDialog` from `primevue/confirmdialog`, `Toast` from `primevue/toast`
- Produces: AppProviders wrapper with slot, ConfirmDialog, and Toast

- [ ] **Step 1: Create the file**

```vue
<script setup lang="ts">
</script>

<template>
  <slot />
  <ConfirmDialog />
  <Toast />
</template>
```

- [ ] **Step 2: Commit**

```bash
git add app/Admin/src/app/providers/AppProviders.Component.vue
git commit -m "feat(admin): restore confirm dialog and toast in app providers"
```

---

## Task 13: Verify build, lint, and unit tests

**Files:**
- None created or modified

**Interfaces:**
- Consumes: all files from Tasks 1-12
- Produces: verification gate (pass/fail)

- [ ] **Step 1: Run lint**

```bash
cd app/Admin && pnpm run lint
```
Expected: zero warnings, exit code 0.

- [ ] **Step 2: Run unit tests**

```bash
cd app/Admin && pnpm run test:unit
```
Expected: all 8+ existing test files pass.

- [ ] **Step 3: Run typecheck (if available)**

```bash
cd app/Admin && npx vue-tsc --noEmit 2>&1 || true
```
Fix any type errors. Expected: no errors related to layout/ directory.

- [ ] **Step 4: Verify dev server starts**

```bash
cd app/Admin && timeout 10 pnpm run dev 2>&1 || true
```
Expected: no compile errors in terminal output.

- [ ] **Step 5: Commit if verification passes**

```bash
git add -A && git commit -m "chore(admin): verify layout migration build passes"
```

---

## Task 14: Brand refresh

**Files:**
- Modify: `app/Admin/src/app/layout/Topbar.Layout.vue` — already done in Task 9 (used "ReSys.Shop" text instead of RESYS SVG)
- Delete: `app/Admin/src/assets/sekai/` directory

**Interfaces:**
- Consumes: nothing new
- Produces: cleaned up assets, updated brand text

- [ ] **Step 1: Delete duplicate sekai SCSS directory**

```bash
rm -rf app/Admin/src/assets/sekai
```

- [ ] **Step 2: Commit**

```bash
git add -A && git commit -m "chore(admin): remove duplicate sekai scss, apply brand text"
```

---

## Task 15: Smoke test checklist

Manual verification against the smoke test items from the spec.

- [ ] **Step 1: Start dev server and verify each item**

```bash
cd app/Admin && pnpm run dev
```

Check each item in a browser:
- [ ] Page loads without white screen or console errors
- [ ] Sidebar renders with full menu tree (Home, Catalog, Inventory, Sales, Identity & Access)
- [ ] Clicking menu items navigates to correct pages
- [ ] Dark/light mode toggle works (sun/moon icon swaps in topbar)
- [ ] Floating dark mode toggle button works (bottom-right corner)
- [ ] Configurator panel opens/closes via palette button in topbar
- [ ] Configurator panel opens/closes via floating palette button
- [ ] Mobile viewport (resize to < 992px): hamburger opens sidebar, mask click closes it
- [ ] Breadcrumb shows active page path below topbar
- [ ] Search input in topbar renders and opens OverlayPanel on focus
- [ ] Footer visible at bottom of every page

- [ ] **Step 2: Commit if all pass**

```bash
git add -A && git commit -m "chore(admin): smoke test layout migration complete"
```

---

## Debug Phase (out of scope for this plan)

After this plan completes, the debug phase begins. See the design spec at `docs/superpowers/specs/2026-07-17-admin-layout-migration-design.md` section "Debug Phase Process" for the triage workflow:

1. Smoke test every feature; document broken items
2. Triage each as CSS bug / JS bug / missing feature
3. Fix CSS first, JS second
4. Remove unfixable features and clean up associated SCSS
5. Final SCSS cleanup (remove dead rules)
