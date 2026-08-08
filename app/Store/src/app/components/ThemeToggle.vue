<script setup lang="ts">
import { useTheme } from '@/shared/composables/useTheme'

// Theme: Shared singleton keeps dark mode state consistent across consumers.
const { isDark, toggle } = useTheme()
</script>

<template>
  <!-- Section: Theme Toggle — switch bound to the shared dark-mode state -->
  <ToggleSwitch
    :model-value="isDark"
    aria-label="Toggle dark mode"
    v-tooltip.bottom="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
    @update:model-value="toggle"
  />
</template>
