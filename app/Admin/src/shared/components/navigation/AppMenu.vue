<script setup lang="ts">
import { ref } from 'vue';
import AppMenuItem from './AppMenuItem.vue';
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
</script>

<template>
    <ul class="layout-menu">
        <template v-for="(item, i) in model" :key="item">
            <app-menu-item v-if="!item.separator" :item="item" :index="i"></app-menu-item>
            <li v-if="item.separator" class="menu-separator"></li>
        </template>
    </ul>
</template>

<style lang="scss" scoped></style>
