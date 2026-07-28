<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router'
import { useToast } from 'primevue/usetoast'
import AppMenuItem from './AppMenuItem.vue';
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

interface MenuItem {
  separator?: boolean
  label?: string
  path?: string
  items?: MenuItem[]
  icon?: string
  to?: string
  class?: string
  url?: string
  target?: string
}

const router = useRouter()
const toast = useToast()
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
]);

async function handleLogout() {
  await authStore.logout()
  toast.add({ severity: 'info', summary: 'Logged out', life: 3000 })
  router.replace({ name: 'login' })
}
</script>

<template>
    <ul class="layout-menu">
        <template v-for="(item, i) in model" :key="item">
            <app-menu-item v-if="!item.separator" :item="item" :index="i"></app-menu-item>
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
        <li class="menu-separator"></li>
        <li>
            <a class="logout-item flex align-items-center px-3 py-2 cursor-pointer border-round"
               :class="{ 'pointer-events-none opacity-50': authStore.isLoggingOut }"
               @click="handleLogout">
                <i class="pi pi-sign-out layout-menuitem-icon"></i>
                <span class="layout-menuitem-text">Logout</span>
            </a>
        </li>
    </ul>
</template>

<style lang="scss" scoped></style>
