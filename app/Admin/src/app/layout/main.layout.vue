<template>
  <div class="flex h-screen flex-col">
    <TopbarLayout />
    <div class="flex flex-1 overflow-hidden">
      <SidebarLayout />
      <main class="flex flex-1 flex-col overflow-auto bg-surface-50">
        <router-view />
      </main>
    </div>
    <FooterLayout />
    <ConfiguratorLayout v-model:visible="configuratorVisible" />
    <ConfirmDialog />
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import TopbarLayout from './topbar.layout.vue'
import SidebarLayout from './sidebar.layout.vue'
import FooterLayout from './footer.layout.vue'
import ConfiguratorLayout from './configurator.layout.vue'

const configuratorVisible = ref(false)
</script>
