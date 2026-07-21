<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/features/auth/store/auth.store'
import { useLayout } from './composables/layout.composable'
import GlobalSearch from './components/GlobalSearch.vue'
import AppConfigurator from './ConfiguratorLayout.vue'

const router = useRouter()
const authStore = useAuthStore()
const { toggleMenu, toggleDarkMode, isDarkTheme, toggleConfigSidebar } = useLayout()
const profileMenu = ref()
const profileMenuItems = ref([
  { label: 'My Profile', icon: 'pi pi-user', command: () => router.push({ name: 'profile' }) },
  { separator: true },
  { label: 'Logout', icon: 'pi pi-sign-out', command: () => authStore.logout() },
])

const toggleProfileMenu = (event: Event) => {
  profileMenu.value?.toggle(event)
}
</script>

<template>
  <div class="layout-topbar">
    <div class="layout-topbar-logo-container">
      <button class="layout-menu-button layout-topbar-action" @click="toggleMenu">
        <i class="pi pi-bars"></i>
      </button>
      <router-link to="/" class="layout-topbar-logo">
        <span class="topbar-brand-text">ReSys.Shop</span>
      </router-link>
    </div>

    <GlobalSearch />

    <div class="layout-topbar-actions">
      <div class="layout-config-menu">
        <button type="button" class="layout-topbar-action" @click="toggleDarkMode">
          <i :class="['pi', { 'pi-moon': isDarkTheme, 'pi-sun': !isDarkTheme }]"></i>
        </button>
        <div class="relative">
          <button
            v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
            type="button"
            class="layout-topbar-action"
          >
            <i class="pi pi-palette"></i>
          </button>
          <AppConfigurator />
        </div>
      </div>

      <button
        class="layout-topbar-menu-button layout-topbar-action"
        v-styleclass="{ selector: '@next', enterFromClass: 'hidden', enterActiveClass: 'animate-scalein', leaveToClass: 'hidden', leaveActiveClass: 'animate-fadeout', hideOnOutsideClick: true }"
      >
        <i class="pi pi-ellipsis-v"></i>
      </button>

      <div class="layout-topbar-menu hidden lg:block">
        <div class="layout-topbar-menu-content">
          <button
            type="button"
            class="layout-topbar-action"
            @click="toggleProfileMenu"
            v-tooltip.bottom="'Profile'"
          >
            <i class="pi pi-user"></i>
            <span>Profile</span>
          </button>
          <Menu ref="profileMenu" :model="profileMenuItems" :popup="true" />
        </div>
      </div>
    </div>
  </div>
</template>
