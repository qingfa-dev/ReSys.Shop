<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'

// Title: Browser tab title for the 404 page
usePageTitle('Page Not Found')
</script>

<template>
  <div class="mx-auto max-w-7xl px-4 py-24 sm:px-6 lg:px-8">
    <!-- Section: Error Content — centered icon, message and home CTA -->
    <div class="flex flex-col items-center gap-6 text-center">
      <i class="pi pi-exclamation-circle text-7xl text-primary-400" />
      <Message severity="warn" :closable="false" class="max-w-md">
        The page you are looking for does not exist or has been moved.
      </Message>
      <Button as="router-link" to="/" label="Back to Home" icon="pi pi-home" />
    </div>
  </div>
</template>
