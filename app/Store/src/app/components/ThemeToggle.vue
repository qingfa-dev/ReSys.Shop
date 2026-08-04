<script setup lang="ts">
import { useTheme } from '@/shared/composables/useTheme'

const { isDark, toggle } = useTheme()
</script>
<template>
  <Button
    :icon="isDark ? 'pi pi-sun' : 'pi pi-moon'"
    severity="secondary"
    text
    rounded
    :aria-label="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
    @click="toggle"
  />
</template>
