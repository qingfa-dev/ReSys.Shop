<script setup lang="ts">
import { useLayout } from './composables/layout.composable'
import AppTopbar from './topbar.layout.vue'
import AppSidebar from './sidebar.layout.vue'
import AppFooter from './footer.layout.vue'
import { RouterView } from 'vue-router'
const { isDarkMode, toggleDarkMode, sidebarActive, toggleSidebar } = useLayout()
</script>
<template>
  <div class="layout-wrapper" :class="{ 'layout-dark': isDarkMode, 'layout-sidebar-active': sidebarActive }">
    <AppTopbar @toggle-sidebar="toggleSidebar" @toggle-dark-mode="toggleDarkMode" />
    <AppSidebar :active="sidebarActive" @close="toggleSidebar" />
    <div class="layout-main">
      <router-view />
      <AppFooter />
    </div>
  </div>
</template>
