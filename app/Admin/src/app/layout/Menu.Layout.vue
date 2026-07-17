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
