<script setup lang="ts">
import { usePageTitle } from '@/shared/composables/usePageTitle'

usePageTitle('Page Not Found')
</script>

<template>
  <!-- Section: 404 Page — centered error content with home link -->
  <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-24">
    <div class="text-center">
      <p class="text-6xl font-bold text-neutral-200">404</p>
      <h1 class="mt-4 text-2xl font-bold text-neutral-900">Page not found</h1>
      <p class="mt-2 text-neutral-500">
        The page you are looking for does not exist or has been moved.
      </p>
      <router-link to="/">
        <Button class="mt-8" severity="info" style="background: #14b8a6; border-color: #14b8a6">
          Back to Home
        </Button>
      </router-link>
    </div>
  </div>
</template>
