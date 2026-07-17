<script setup lang="ts">
import { useLayout } from '../composables/layout.composable'
const { toggleDarkMode } = useLayout()
</script>
<template>
  <div class="floating-configurator">
    <button @click="toggleDarkMode" class="floating-config-button">
      <i class="pi pi-palette" />
    </button>
  </div>
</template>
