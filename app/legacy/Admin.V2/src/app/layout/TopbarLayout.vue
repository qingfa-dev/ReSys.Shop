<script setup lang="ts">
import { useLayout } from '@/app/composables/layout.composable'
import GlobalSearch from './components/GlobalSearch.vue'
import ConfiguratorLayout from './ConfiguratorLayout.vue'
import NotificationBell from '@/shared/components/layout/NotificationBell.vue'

const { toggleMenu, toggleDarkMode, isDarkTheme, toggleConfigSidebar } = useLayout()
</script>

<template>
  <div class="layout-topbar">
    <div class="layout-topbar-logo-container">
      <button class="layout-menu-button layout-topbar-action" @click="toggleMenu">
        <i class="pi pi-bars" />
      </button>
      <router-link to="/" class="layout-topbar-logo">
        <span class="topbar-brand-text">ReSys.Shop</span>
      </router-link>
    </div>

    <GlobalSearch />

    <div class="layout-topbar-actions">
      <NotificationBell />
      <div class="layout-config-menu">
        <button type="button" class="layout-topbar-action" @click="toggleDarkMode">
          <i :class="['pi', { 'pi-moon': isDarkTheme, 'pi-sun': !isDarkTheme }]" />
        </button>
        <div class="relative">
          <button type="button" class="layout-topbar-action" @click="toggleConfigSidebar">
            <i class="pi pi-cog" />
          </button>
          <ConfiguratorLayout />
        </div>
      </div>
    </div>
  </div>
</template>
