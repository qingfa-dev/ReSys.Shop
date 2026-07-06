<template>
  <aside
    class="flex-shrink-0 border-r border-surface-200 bg-surface-0 transition-all duration-300"
    :class="[collapsed ? 'w-16' : 'w-64']"
  >
    <div class="flex h-16 items-center justify-center border-b border-surface-200 px-4">
      <div v-if="!collapsed" class="flex items-center gap-2">
        <i class="pi pi-shopping-cart text-2xl text-primary-500" />
        <span class="text-lg font-semibold">{{ appName }}</span>
      </div>
      <i v-else class="pi pi-shopping-cart text-2xl text-primary-500" />
    </div>
    <nav class="overflow-y-auto p-3" style="height: calc(100vh - 4rem)">
      <MenuLayout :items="menuItems" :collapsed="collapsed" />
    </nav>
  </aside>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { APP_NAME } from '@/shared/config/app' // eslint-disable-line
import { useLayout } from './composables/layout.composable'
import MenuLayout from './menu.layout.vue'

const { sidebarCollapsed, menuItems } = useLayout()

const collapsed = computed(() => sidebarCollapsed.value)
const appName = APP_NAME
</script>
