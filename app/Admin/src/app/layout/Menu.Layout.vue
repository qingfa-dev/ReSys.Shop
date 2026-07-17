<script setup lang="ts">
import AppMenuItem from './MenuItem.Layout.vue'
import { useRouter } from 'vue-router'

const router = useRouter()

const menuItems = [
  { label: 'Dashboard', icon: 'pi pi-home', command: () => router.push({ name: 'reports.dashboard' }) },
  {
    label: 'Catalog', icon: 'pi pi-book', items: [
      { label: 'Dashboard', command: () => router.push({ name: 'catalog.dashboard' }) },
      { label: 'Products', command: () => router.push({ name: 'catalog.products' }) },
      { label: 'Taxonomies', command: () => router.push({ name: 'catalog.taxonomies' }) },
      { label: 'Option Types', command: () => router.push({ name: 'catalog.option-types' }) },
      { label: 'Property Types', command: () => router.push({ name: 'catalog.property-types' }) },
    ]
  },
  {
    label: 'Inventory', icon: 'pi pi-warehouse', items: [
      { label: 'Stock Locations', command: () => router.push({ name: 'inventory.stock-locations' }) },
      { label: 'Stock Items', command: () => router.push({ name: 'inventory.stock-items' }) },
      { label: 'Stock Transfers', command: () => router.push({ name: 'inventory.stock-transfers' }) },
    ]
  },
  {
    label: 'Orders', icon: 'pi pi-shopping-cart', items: [
      { label: 'All Orders', command: () => router.push({ name: 'ordering.orders' }) },
      { label: 'Fulfillment Queue', command: () => router.push({ name: 'ordering.fulfillment' }) },
    ]
  },
  {
    label: 'Users', icon: 'pi pi-users', items: [
      { label: 'Admin Users', command: () => router.push({ name: 'users.admin' }) },
      { label: 'Customers', command: () => router.push({ name: 'users.customers' }) },
      { label: 'Roles', command: () => router.push({ name: 'users.roles' }) },
      { label: 'Permissions', command: () => router.push({ name: 'users.permissions' }) },
    ]
  },
  {
    label: 'Locations', icon: 'pi pi-map-marker', items: [
      { label: 'Countries', command: () => router.push({ name: 'location.countries' }) },
      { label: 'States', command: () => router.push({ name: 'location.states' }) },
    ]
  },
  { label: 'Profile', icon: 'pi pi-user', command: () => router.push({ name: 'profile' }) },
]
</script>
<template>
  <ul class="layout-menu">
    <AppMenuItem v-for="item of menuItems" :key="item.label" :item="item" />
  </ul>
</template>
