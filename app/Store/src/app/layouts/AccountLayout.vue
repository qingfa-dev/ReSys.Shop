<script setup lang="ts">
import { useRoute } from 'vue-router'

// Route: Track current route for active sidebar state.
const route = useRoute()

const navItems = [
  { to: '/account/orders', label: 'Orders' },
  { to: '/account/addresses', label: 'Addresses' },
  { to: '/account/profile', label: 'Profile' },
  { to: '/account/sessions', label: 'Sessions' },
  { to: '/account/wishlists', label: 'Wishlists' },
  { to: '/account/notifications', label: 'Notifications' },
  { to: '/account/change-password', label: 'Change Password' },
  { to: '/account/preferences', label: 'Preferences' },
]

// Nav: Determine active state for a nav link.
function isActive(path: string): boolean {
  return route.path === path
}

// Nav: Resolve CSS classes based on active state.
function linkClass(path: string): string {
  const base = 'block rounded-r-lg border-l-2 px-3 py-2 text-sm transition-colors'
  if (isActive(path)) {
    return `${base} border-neutral-900 text-neutral-900 font-semibold bg-neutral-100`
  }
  return `${base} border-transparent text-neutral-700 hover:bg-neutral-100 hover:text-neutral-900 font-medium`
}
</script>
<template>
  <div class="min-h-screen bg-neutral-50">
    <header class="bg-white border-b border-neutral-200 sticky top-0 z-40">
      <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center h-14 gap-4">
          <router-link to="/" class="text-lg font-semibold tracking-tight text-neutral-900 shrink-0">ReSys.Shop</router-link>
          <span class="text-neutral-300">/</span>
          <span class="text-sm font-medium text-neutral-600">Account</span>
        </div>
      </div>
    </header>
    <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div class="flex flex-col md:flex-row gap-8">
        <aside class="w-full md:w-56 shrink-0">
          <nav class="space-y-1">
            <router-link
              v-for="item in navItems"
              :key="item.to"
              :to="item.to"
              :class="linkClass(item.to)"
            >
              {{ item.label }}
            </router-link>
          </nav>
        </aside>
        <div class="flex-1 min-w-0">
          <router-view />
        </div>
      </div>
    </div>
  </div>
</template>
