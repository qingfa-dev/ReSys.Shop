<script setup lang="ts">
import { ref } from 'vue'

interface MenuItem {
  label: string
  icon?: string
  command?: () => unknown
  items?: MenuItem[]
}

defineProps<{ item: MenuItem }>()
const expanded = ref(false)
</script>
<template>
  <li class="menu-item">
    <a v-if="item.command" @click="item.command" class="menu-link">
      <i :class="item.icon" />
      <span>{{ item.label }}</span>
    </a>
    <a v-else @click="expanded = !expanded" class="menu-link">
      <i :class="item.icon" />
      <span>{{ item.label }}</span>
      <i class="pi pi-chevron-down menu-submenu-icon" :class="{ 'rotated': expanded }" />
    </a>
    <ul v-if="item.items && item.items.length" v-show="expanded" class="menu-submenu">
      <AppMenuItem v-for="child of item.items" :key="child.label" :item="child" />
    </ul>
  </li>
</template>
