<template>
  <footer class="border-t border-surface-200 p-3 text-sm text-color-secondary">
    © {{ year }} {{ appName }} v{{ version }}
  </footer>
</template>

<script setup lang="ts">
import { APP_NAME, APP_VERSION } from '@/shared/config/app'

const year = new Date().getFullYear()
const appName = APP_NAME
const version = APP_VERSION
</script>
