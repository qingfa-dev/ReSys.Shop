<script setup lang="ts">
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'

const { t } = useI18n()
const router = useRouter()
</script>

<template>
  <div class="flex items-center justify-center min-h-screen bg-surface-50 dark:bg-surface-950 p-8">
    <div class="text-center max-w-md">
      <svg
        class="mx-auto mb-8 text-surface-300 dark:text-surface-700"
        width="200"
        height="140"
        viewBox="0 0 200 140"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <rect x="40" y="50" width="120" height="80" rx="8" stroke="currentColor" stroke-width="2" />
        <path d="M40 70h120M65 50V34M135 50V34" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
        <circle cx="70" cy="95" r="6" fill="currentColor" opacity="0.3" />
        <circle cx="100" cy="95" r="6" fill="currentColor" opacity="0.3" />
        <circle cx="130" cy="95" r="6" fill="currentColor" opacity="0.3" />
        <text x="100" y="115" text-anchor="middle" font-size="10" fill="currentColor" opacity="0.5">404</text>
      </svg>

      <h1 class="text-4xl font-bold text-surface-900 dark:text-surface-0 mb-3">Page not found</h1>
      <p class="text-surface-500 dark:text-surface-400 mb-8 leading-relaxed">
        The page you are looking for might have been removed, had its name changed, or is temporarily unavailable.
      </p>

      <Button
        :label="t('error.not_found.action')"
        icon="pi pi-home"
        @click="router.push('/')"
      />
    </div>
  </div>
</template>
