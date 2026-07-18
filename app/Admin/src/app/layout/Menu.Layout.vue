<script setup lang="ts">
import AppMenuItem from './MenuItem.Layout.vue'
import { adminMenuConfig } from '@/app/config/admin-menu.config'
import { useAuthStore } from '@/features/auth/stores/auth.store'
import { computed } from 'vue'

const authStore = useAuthStore()

function groupHasVisibleItems(items: { permission?: string }[]): boolean {
  return items.some(item => {
    if (!item.permission) return true
    return authStore.session?.user?.permissions?.includes(item.permission) ?? false
  })
}

const visibleGroups = computed(() =>
  adminMenuConfig.filter(group => groupHasVisibleItems(group.items))
)
</script>

<template>
  <ul class="layout-menu">
    <template v-for="(item, i) in visibleGroups" :key="item.label">
      <AppMenuItem v-if="!item.separator" :item="item" :index="i" root />
      <li v-if="item.separator" class="menu-separator" />
    </template>
  </ul>
</template>
