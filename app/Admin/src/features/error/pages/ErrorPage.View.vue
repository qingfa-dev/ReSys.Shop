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
        <rect x="30" y="30" width="140" height="80" rx="8" stroke="currentColor" stroke-width="2" />
        <path d="M60 55l40 40M100 55l-40 40" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
        <path d="M70 130h60" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-dasharray="4 4" />
        <path d="M100 110v-5" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
      </svg>

      <h1 class="text-4xl font-bold text-surface-900 dark:text-surface-0 mb-3">Something went wrong</h1>
      <p class="text-surface-500 dark:text-surface-400 mb-8 leading-relaxed">
        An unexpected error occurred. Please try again or contact support if the problem persists.
      </p>

      <div class="flex gap-4 justify-center">
        <Button
          :label="t('error.server_error.action_retry')"
          icon="pi pi-refresh"
          severity="secondary"
          @click="router.go(-1)"
        />
        <Button
          :label="t('error.server_error.action_home')"
          icon="pi pi-home"
          @click="router.push('/')"
        />
      </div>
    </div>
  </div>
</template>
