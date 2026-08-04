<script setup lang="ts">
import AppHeader from '@/app/components/layout/AppHeader.vue'
import AppFooter from '@/app/components/layout/AppFooter.vue'
</script>
<template>
  <!-- Section: Page Shell -->
  <div class="min-h-screen flex flex-col bg-gray-50">
    <AppHeader />
    <!-- Section: Main Content -->
    <main class="flex-1">
      <router-view />
    </main>
    <AppFooter />
  </div>
</template>
