<script setup lang="ts">
import { computed, watch } from 'vue'
import { useRoute } from 'vue-router'
import type { MenuItem } from 'primevue/menuitem'
import { useAuthStore } from '@/features/identity/stores/authStore'

// Visible: Parent layout owns the open state via v-model:visible.
const visible = defineModel<boolean>('visible', { required: true })

const route = useRoute()
const authStore = useAuthStore()

// Nav: Same primary routes as AppHeader's Menubar, plus account links when signed in.
const navItems = computed<MenuItem[]>(() => [
  { label: 'Home', icon: 'pi pi-home', to: '/' },
  { label: 'Shop', icon: 'pi pi-shopping-bag', to: '/shop' },
  { label: 'Collections', icon: 'pi pi-images', to: '/collections' },
  { label: 'Visual Search', icon: 'pi pi-camera', to: '/recommendations' },
  ...(authStore.isAuthenticated
    ? [
        { label: 'Profile', icon: 'pi pi-user', to: '/account/profile' },
        { label: 'Orders', icon: 'pi pi-shopping-bag', to: '/account/orders' },
      ]
    : []),
])

// Close: Dismiss the drawer once navigation reaches the target route.
watch(
  () => route.path,
  () => {
    visible.value = false
  },
)
</script>

<template>
  <!-- Section: Mobile Drawer — off-canvas PanelMenu nav for small screens -->
  <Drawer v-model:visible="visible" position="left" class="w-72">
    <template #header>
      <span class="px-1 text-lg font-semibold tracking-tight">Menu</span>
    </template>

    <PanelMenu :model="navItems">
      <template #item="{ item }">
        <RouterLink
          :to="item.to"
          :class="[
            'flex w-full items-center gap-3 px-3 py-2 text-sm',
            route.path === item.to
              ? 'rounded-lg bg-primary-50 font-semibold text-primary-800'
              : 'text-surface-700',
          ]"
          :aria-current="route.path === item.to ? 'page' : undefined"
          data-pc-section="headerlink"
        >
          <i :class="item.icon" />
          <span class="flex-1">{{ item.label }}</span>
        </RouterLink>
      </template>
    </PanelMenu>

    <!-- Sign In: Fallback CTA for guests below the nav items -->
    <Button
      v-if="!authStore.isAuthenticated"
      as="router-link"
      to="/login"
      label="Sign In"
      icon="pi pi-sign-in"
      iconPos="right"
      class="mt-4 w-full"
    />
  </Drawer>
</template>
