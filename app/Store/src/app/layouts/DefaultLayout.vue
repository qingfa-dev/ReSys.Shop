<script setup lang="ts">
import AppHeader from '@/app/components/layout/AppHeader.vue'
import AppFooter from '@/app/components/layout/AppFooter.vue'
</script>
<template>
  <div class="min-h-screen flex flex-col bg-neutral-50">
    <AppHeader />
    <main class="flex-1">
      <router-view />
    </main>
    <AppFooter />
  </div>
</template>
