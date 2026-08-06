<script setup lang="ts">
import AppHeader from '@/app/components/layout/AppHeader.vue'
import AppFooter from '@/app/components/layout/AppFooter.vue'
import { useRoute } from 'vue-router'

const route = useRoute()

const navItems = [
  { label: 'Orders', to: '/account/orders', icon: 'pi pi-shopping-bag' },
  { label: 'Addresses', to: '/account/addresses', icon: 'pi pi-map-marker' },
  { label: 'Profile', to: '/account/profile', icon: 'pi pi-user' },
  { label: 'Sessions', to: '/account/sessions', icon: 'pi pi-shield' },
  { label: 'Wishlists', to: '/account/wishlists', icon: 'pi pi-heart' },
  { label: 'Notifications', to: '/account/notifications', icon: 'pi pi-bell' },
  { label: 'Change Password', to: '/account/change-password', icon: 'pi pi-key' },
]
</script>
<template>
  <!-- Section: Account Shell -->
  <div class="min-h-screen flex flex-col bg-stone-50">
    <AppHeader />
    <!-- Section: Account Body -->
    <div class="flex-1 max-w-7xl mx-auto w-full px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex gap-8">
        <!-- Section: Sidebar Navigation -->
        <nav class="w-56 shrink-0 hidden md:block">
          <ul class="space-y-1">
            <li v-for="item in navItems" :key="item.to">
              <router-link
                :to="item.to"
                class="flex items-center gap-3 px-4 py-2 rounded-lg text-sm font-medium transition-colors"
                :class="route.path === item.to
                  ? 'bg-stone-900 text-white'
                  : 'text-stone-600 hover:bg-stone-100'"
              >
                <i :class="item.icon" />
                {{ item.label }}
              </router-link>
            </li>
          </ul>
        </nav>
        <!-- Section: Content Area -->
        <div class="flex-1 min-w-0">
          <router-view />
        </div>
      </div>
    </div>
    <ConfirmDialog />
    <AppFooter />
  </div>
</template>
