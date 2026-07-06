<template>
  <header class="flex items-center justify-between border-b border-surface-200 p-3">
    <h1 class="text-lg font-semibold">{{ appName }}</h1>
    <div class="flex items-center gap-2">
      <Button :icon="theme.isDark ? 'pi pi-sun' : 'pi pi-moon'" text rounded @click="theme.toggle()" />
      <RouterLink to="/login">
        <Button icon="pi pi-sign-out" text rounded />
      </RouterLink>
    </div>
  </header>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
import { useThemeStore } from '@/app/stores/theme.store'
import { APP_NAME } from '@/shared/config/app'

const theme = useThemeStore()
const appName = APP_NAME
</script>
