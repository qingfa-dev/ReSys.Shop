<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import PanelMenu from 'primevue/panelmenu'
import { useNotify } from '@/shared/composables/useNotify'
import { useAuthStore } from '@/features/auth/stores/authStore'
import { dashboardMenuItems } from '@/features/dashboard/routes'
import { catalogMenuItems } from '@/features/catalog/routes'
import { identityMenuItems } from '@/features/identity/routes'
import { inventoryMenuItems } from '@/features/inventory/routes'
import { locationMenuItems } from '@/features/location/routes'
import { orderingMenuItems } from '@/features/ordering/routes'
import { paymentMenuItems } from '@/features/payment/routes'
import { profileMenuItems } from '@/features/profile/routes'
import { shippingMenuItems } from '@/features/shipping/routes'
import SignOut from '@primeicons/vue/sign-out'

interface MenuItem {
  separator?: boolean
  label?: string
  path?: string
  items?: MenuItem[]
  icon?: string
  route?: string
  to?: string
  class?: string
  url?: string
  target?: string
}

const router = useRouter()
const notify = useNotify()
const authStore = useAuthStore()

const model = ref<MenuItem[]>([
  ...dashboardMenuItems,
  ...catalogMenuItems,
  ...identityMenuItems,
  ...inventoryMenuItems,
  ...locationMenuItems,
  ...orderingMenuItems,
  ...paymentMenuItems,
  ...profileMenuItems,
  ...shippingMenuItems,
])

async function handleLogout() {
  await authStore.logout()
  notify.info('Logged out')
  router.replace({ name: 'login' })
}
</script>

<template>
  <ul class="layout-menu">
    <li>
      <PanelMenu :model="model" multiple class="w-full" />
    </li>
    <li class="menu-separator"></li>
    <li>
      <a class="logout-item flex align-items-center px-3 py-2 cursor-pointer border-round"
         :class="{ 'pointer-events-none opacity-50': authStore.isLoggingOut }"
         @click="handleLogout">
        <SignOut class="layout-menuitem-icon" />
        <span class="layout-menuitem-text">Logout</span>
      </a>
    </li>
  </ul>
</template>

<style lang="scss" scoped>
/* PanelMenu: remove default panel borders to fit sidebar */
:deep(.p-panelmenu) {
  border: none;
}

:deep(.p-panelmenu-panel) {
  border: none;
}

:deep(.p-panelmenu-header) {
  border: none;
}

:deep(.p-panelmenu-content) {
  border: none;
}
</style>
