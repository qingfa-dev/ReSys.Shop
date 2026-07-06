<template>
  <AppProviders>
    <RouterView v-slot="{ Component, route }">
      <component :is="Component" v-if="Component" :key="route.fullPath" />
    </RouterView>
  </AppProviders>
</template>

<script setup lang="ts">
import { RouterView } from 'vue-router'
import AppProviders from './providers/AppProviders.vue'
</script>
