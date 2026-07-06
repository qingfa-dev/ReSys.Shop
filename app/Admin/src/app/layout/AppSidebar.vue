<template>
  <aside :class="['w-60 border-r border-surface-200 p-3', sidebar.collapsed && 'w-16']">
    <Button :icon="sidebar.collapsed ? 'pi pi-angle-double-right' : 'pi pi-angle-double-left'" text rounded @click="sidebar.toggle()" />
    <nav class="mt-3 flex flex-col gap-1">
      <RouterLink
        v-for="item in items"
        :key="item.to"
        :to="item.to"
        class="rounded p-2 hover:bg-surface-100"
        active-class="bg-primary-100 text-primary-700"
      >
        <i :class="item.icon" class="mr-2" />
        <span v-if="!sidebar.collapsed">{{ item.label }}</span>
      </RouterLink>
    </nav>
  </aside>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useSidebarStore } from '@/app/stores/sidebar.store'

const sidebar = useSidebarStore()
const items = [
  { to: '/', label: 'Dashboard', icon: 'pi pi-home' },
  { to: '/identity/users', label: 'Users', icon: 'pi pi-users' },
]
</script>
