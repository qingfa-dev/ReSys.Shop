<script setup lang="ts">
import AppMenuItem from './AppMenuItem.Layout.vue'
import type { MenuItem } from './AppMenuItem.Layout.vue'

const model: MenuItem[] = [
  {
    label: 'Home',
    items: [
      {
        label: 'Dashboard',
        icon: 'pi pi-fw pi-home',
        to: '/',
      },
    ],
  },
  {
    label: 'Catalog',
    items: [
      {
        label: 'Products',
        icon: 'pi pi-fw pi-tag',
        to: '/catalog/products',
      },
      {
        label: 'Taxonomies',
        icon: 'pi pi-fw pi-sitemap',
        to: '/catalog/taxonomies',
      },
      {
        label: 'Categories',
        icon: 'pi pi-fw pi-folder',
        to: '/catalog/categories',
      },
    ],
  },
  {
    label: 'Orders',
    items: [
      {
        label: 'Orders',
        icon: 'pi pi-fw pi-shopping-cart',
        to: '/ordering/orders',
      },
      {
        label: 'Fulfillment',
        icon: 'pi pi-fw pi-truck',
        to: '/ordering/fulfillment',
      },
    ],
  },
  {
    label: 'Inventory',
    items: [
      {
        label: 'Stock Items',
        icon: 'pi pi-fw pi-box',
        to: '/inventory/stocks',
      },
      {
        label: 'Movements',
        icon: 'pi pi-fw pi-arrow-right-arrow-left',
        to: '/inventory/movements',
      },
    ],
  },
  {
    label: 'Users',
    items: [
      {
        label: 'Staff',
        icon: 'pi pi-fw pi-users',
        to: '/users/staff',
      },
      {
        label: 'Customers',
        icon: 'pi pi-fw pi-id-card',
        to: '/users/customers',
      },
      {
        label: 'Roles',
        icon: 'pi pi-fw pi-shield',
        to: '/roles',
      },
    ],
  },
  {
    label: 'Settings',
    items: [
      {
        label: 'Shipping',
        icon: 'pi pi-fw pi-paper-plane',
        to: '/shipping/methods',
      },
      {
        label: 'Payments',
        icon: 'pi pi-fw pi-credit-card',
        to: '/payments',
      },
      {
        label: 'Locations',
        icon: 'pi pi-fw pi-map-marker',
        to: '/locations',
      },
    ],
  },
]
</script>

<template>
  <ul class="layout-menu">
    <template v-for="(item, i) in model" :key="i">
      <AppMenuItem v-if="!item.separator" :item="item" :index="i" />
      <li v-if="item.separator" class="menu-separator"></li>
    </template>
  </ul>
</template>
