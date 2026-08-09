<script setup lang="ts">
import { computed, ref, watch } from 'vue'
import { useRoute } from 'vue-router'
import type { MenuItem } from 'primevue/menuitem'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useOrderStore } from '@/features/ordering/stores/orderStore'
import { useMediaQuery } from '@/shared/composables/useMediaQuery'

// Breakpoint: Below lg the nav moves into the overlay Sidebar drawer.
const { matches: isMobile } = useMediaQuery('(max-width: 1023px)')

const route = useRoute()
const authStore = useAuthStore()
const orderStore = useOrderStore()

const mobileNavOpen = ref(false)

// Count: Non-terminal orders (Draft or Placed) badge for the Orders nav item.
const activeOrderCount = computed(
  () => orderStore.items.filter(o => o.status !== 'Canceled' && o.status !== 'Expired').length,
)

// Highlight: Mark the nav item matching the current route (covers order-detail too).
function isItemActive(to: string): boolean {
  return route.path === to || route.path.startsWith(`${to}/`)
}

// Model: Account nav targets from REQ-008; PanelMenu v5 has no built-in router
// support, so each row renders a RouterLink via the #item slot.
const navItems = computed<MenuItem[]>(() => [
  { label: 'Profile', icon: 'pi pi-user', to: '/account/profile' },
  { label: 'Addresses', icon: 'pi pi-map-marker', to: '/account/addresses' },
  { label: 'Wishlists', icon: 'pi pi-heart', to: '/account/wishlists' },
  { label: 'Notifications', icon: 'pi pi-bell', to: '/account/notifications' },
  { label: 'Change Password', icon: 'pi pi-key', to: '/account/change-password' },
  { label: 'Preferences', icon: 'pi pi-sliders-h', to: '/account/preferences' },
  { label: 'Orders', icon: 'pi pi-shopping-bag', to: '/account/orders', badge: activeOrderCount.value },
])

// Close: Dismiss the mobile drawer once navigation reaches the target route.
watch(
  () => route.path,
  () => {
    mobileNavOpen.value = false
  },
)
</script>

<template>
  <!-- Section: Auth Fallback — defensive guard; guards normally redirect before this -->
  <div v-if="!authStore.isAuthenticated" class="flex min-h-svh items-center justify-center bg-surface-50 p-8">
    <Message severity="warn" :closable="false">Your session has expired. Please sign in again to continue.</Message>
  </div>

  <!-- Section: Account Shell — mobile drawer + desktop grid of nav and routed views -->
  <SidebarLayout v-else>
    <!-- Mobile Drawer: Same PanelMenu model slides in over the content below lg -->
    <SidebarBackdrop v-if="isMobile" />
    <Sidebar
      v-if="isMobile"
      id="account-nav"
      side="left"
      variant="sidebar"
      collapsible="offcanvas"
      overlay
      v-model:open="mobileNavOpen"
      width="16rem"
    >
      <SidebarSpacer />
      <SidebarAside>
        <SidebarPanel>
          <SidebarHeader>
            <span class="px-1 text-sm font-semibold text-heading">Account</span>
          </SidebarHeader>
          <SidebarContent>
            <PanelMenu :model="navItems">
              <template #item="{ item }">
                <RouterLink
                  :to="item.to"
                  :class="[
                    'flex w-full items-center gap-3 px-3 py-2 text-sm',
                    isItemActive(item.to)
                      ? 'rounded-lg bg-highlight font-semibold text-brand'
                      : 'text-body',
                  ]"
                  :aria-current="isItemActive(item.to) ? 'page' : undefined"
                  data-pc-section="headerlink"
                >
                  <i :class="item.icon" />
                  <span class="flex-1">{{ item.label }}</span>
                  <Tag
                    v-if="item.badge !== undefined && item.badge > 0"
                    :value="String(item.badge)"
                    severity="secondary"
                    rounded
                  />
                </RouterLink>
              </template>
            </PanelMenu>
          </SidebarContent>
        </SidebarPanel>
      </SidebarAside>
    </Sidebar>

    <!-- Desktop Grid: Sticky nav aside beside the routed views -->
    <SidebarMain>
      <div class="flex min-h-svh flex-col">
        <!-- Mobile Top Bar: Opens the drawer below lg -->
        <header class="flex items-center border-b border-surface-200 px-4 py-3 lg:hidden">
          <Button icon="pi pi-bars" label="Menu" severity="secondary" variant="text" @click="mobileNavOpen = true" />
        </header>

        <div class="grid flex-1 lg:grid-cols-[16rem_1fr] lg:items-start">
          <aside class="hidden border-r border-surface-200 p-4 lg:sticky lg:top-0 lg:block lg:h-svh lg:overflow-y-auto lg:bg-surface-50">
            <PanelMenu :model="navItems">
              <template #item="{ item }">
                <RouterLink
                  :to="item.to"
                  :class="[
                    'flex w-full items-center gap-3 px-3 py-2 text-sm',
                    isItemActive(item.to)
                      ? 'rounded-lg bg-highlight font-semibold text-brand'
                      : 'text-body',
                  ]"
                  :aria-current="isItemActive(item.to) ? 'page' : undefined"
                  data-pc-section="headerlink"
                >
                  <i :class="item.icon" />
                  <span class="flex-1">{{ item.label }}</span>
                  <Tag
                    v-if="item.badge !== undefined && item.badge > 0"
                    :value="String(item.badge)"
                    severity="secondary"
                    rounded
                  />
                </RouterLink>
              </template>
            </PanelMenu>
          </aside>

          <main class="min-w-0 p-4 lg:p-8">
            <RouterView />
          </main>
        </div>
      </div>
    </SidebarMain>
  </SidebarLayout>
</template>
