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
        width="140"
        height="140"
        viewBox="0 0 140 140"
        fill="none"
        xmlns="http://www.w3.org/2000/svg"
      >
        <circle cx="70" cy="70" r="60" stroke="currentColor" stroke-width="2" />
        <circle cx="70" cy="55" r="18" stroke="currentColor" stroke-width="2" />
        <path d="M42 100c0-16 28-16 28-16s28 0 28 16" stroke="currentColor" stroke-width="2" stroke-linecap="round" />
        <rect x="56" y="80" width="28" height="20" rx="4" fill="currentColor" opacity="0.2" />
        <rect x="61" y="83" width="18" height="4" rx="2" fill="currentColor" opacity="0.4" />
      </svg>

      <h1 class="text-4xl font-bold text-surface-900 dark:text-surface-0 mb-3">Access denied</h1>
      <p class="text-surface-500 dark:text-surface-400 mb-8 leading-relaxed">
        You do not have the required permissions to access this page. Contact your administrator if you believe this is a mistake.
      </p>

      <Button
        :label="t('error.access_denied.action')"
        icon="pi pi-home"
        @click="router.push('/')"
      />
    </div>
  </div>
</template>
