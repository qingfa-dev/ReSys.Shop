<template>
  <ul class="m-0 list-none p-0">
    <MenuItemLayout
      v-for="(item, index) in items"
      :key="index"
      :item="item"
      :collapsed="collapsed"
    />
  </ul>
</template>

<script setup lang="ts">
import type { MenuItem } from './composables/layout.composable'
import MenuItemLayout from './menu-item.layout.vue'

defineProps<{
  items: MenuItem[]
  collapsed: boolean
}>()
</script>
