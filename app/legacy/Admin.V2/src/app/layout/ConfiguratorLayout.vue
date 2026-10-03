<script setup lang="ts">
import { useLayout } from '@/app/composables/layout.composable'

const { layoutConfig, layoutState, changeMenuMode } = useLayout()
</script>

<template>
  <div
    v-if="layoutState.configSidebarVisible"
    class="config-panel absolute top-[3.25rem] right-0 w-64 p-4 bg-surface-0 dark:bg-surface-900 border border-surface rounded-lg origin-top shadow-lg z-50"
  >
    <div class="flex flex-col gap-4">
      <div class="flex flex-col gap-2">
        <span class="text-sm text-muted-color font-semibold">Menu Mode</span>
        <div class="flex gap-2">
          <button
            v-for="mode in ['static', 'overlay']"
            :key="mode"
            type="button"
            :class="[
              'px-3 py-1.5 text-sm rounded-md border transition-colors',
              layoutConfig.menuMode === mode
                ? 'bg-primary border-primary text-primary-contrast'
                : 'border-surface-300 dark:border-surface-600 text-muted-color hover:border-primary',
            ]"
            @click="changeMenuMode(mode)"
          >
            {{ mode.charAt(0).toUpperCase() + mode.slice(1) }}
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
