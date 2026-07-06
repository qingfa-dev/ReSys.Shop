<template>
  <footer class="flex items-center justify-between border-t border-surface-200 bg-surface-0 px-6 py-3">
    <span class="text-sm text-color-secondary">
      &copy; {{ year }} {{ appName }}
    </span>
    <span class="text-sm text-color-secondary">
      v{{ version }}
    </span>
  </footer>
</template>

<script setup lang="ts">
import { APP_NAME, APP_VERSION } from '@/shared/config/app' // eslint-disable-line

const year = new Date().getFullYear()
const appName = APP_NAME
const version = APP_VERSION
</script>
