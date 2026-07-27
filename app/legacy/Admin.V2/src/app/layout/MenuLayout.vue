<script setup lang="ts">
import AppMenuItem from './MenuItemLayout.vue'
import { adminMenuConfig } from '@/app/config/admin-menu.config'

defineOptions({ name: 'AppMenu' })

const visibleGroups = adminMenuConfig
</script>

<template>
  <ul class="layout-menu">
    <template v-for="(group, i) in visibleGroups" :key="group.label">
      <AppMenuItem
        v-if="!group.separator"
        :item="{ label: group.label, items: group.items, icon: group.icon }"
        :index="i"
        root
      />
      <li v-if="group.separator" class="menu-separator" />
    </template>
  </ul>
</template>
