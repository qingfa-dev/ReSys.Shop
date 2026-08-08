<script setup lang="ts">
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import { useSearch } from '@/features/catalog/composables/useSearch'
import MobileNav from './MobileNav.vue'
import CartDrawer from '@/features/ordering/components/CartDrawer.vue'
import ThemeToggle from '@/app/components/ThemeToggle.vue'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()
const cart = useCartStore()
const search = useSearch()
const mobileMenuOpen = ref(false)
const cartDrawerOpen = ref(false)
const authDropdownOpen = ref(false)

// NavLink: Active state detection by path prefix.
const isActive = (path: string) => route.path.startsWith(path)

// AuthDropdown: Sign out and close dropdown.
const handleSignOut = async () => {
  authDropdownOpen.value = false
  await auth.logout()
  router.push('/')
}
</script>
<template>
  <header class="bg-white border-b border-neutral-200 sticky top-0 z-50">
    <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-14 gap-4">
        <router-link to="/" class="text-lg font-semibold tracking-tight text-neutral-900 shrink-0">ReSys.Shop</router-link>
        <nav class="hidden md:flex items-center gap-6">
          <router-link to="/shop" class="relative text-sm font-medium transition-colors after:absolute after:bottom-[-2px] after:left-0 after:h-[2px] after:w-0 after:bg-neutral-900 after:transition-all after:duration-300 hover:text-neutral-900 hover:after:w-full" :class="isActive('/shop') ? 'text-neutral-900 after:w-full' : 'text-neutral-600'">Shop</router-link>
          <router-link to="/collections" class="relative text-sm font-medium transition-colors after:absolute after:bottom-[-2px] after:left-0 after:h-[2px] after:w-0 after:bg-neutral-900 after:transition-all after:duration-300 hover:text-neutral-900 hover:after:w-full" :class="isActive('/collections') ? 'text-neutral-900 after:w-full' : 'text-neutral-600'">Collections</router-link>
          <router-link to="/recommendations" class="relative text-sm font-medium transition-colors after:absolute after:bottom-[-2px] after:left-0 after:h-[2px] after:w-0 after:bg-neutral-900 after:transition-all after:duration-300 hover:text-neutral-900 hover:after:w-full" :class="isActive('/recommendations') ? 'text-neutral-900 after:w-full' : 'text-neutral-600'">Visual Search</router-link>
        </nav>
        <div class="relative flex items-center gap-1">
          <!-- Search: Desktop with keyboard hint -->
          <div class="hidden md:flex items-center gap-1">
            <Button icon="pi pi-search" text rounded aria-label="Search" @click="search.open()" />
            <kbd class="text-[10px] font-mono text-neutral-400 border border-neutral-200 rounded px-1 py-0.5 leading-none">Ctrl+K</kbd>
          </div>
          <!-- Search: Mobile plain button -->
          <Button icon="pi pi-search" text rounded aria-label="Search" class="md:hidden" @click="search.open()" />
          <div class="relative inline-flex">
            <Button icon="pi pi-shopping-cart" text rounded @click="cartDrawerOpen = true" />
            <Tag v-if="cart.itemCount > 0" :value="String(cart.itemCount)" severity="contrast" class="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] text-[10px] p-0" />
          </div>
          <ThemeToggle />
          <!-- Auth: Logged in — dropdown -->
          <template v-if="auth.isAuthenticated">
            <Button icon="pi pi-user" text rounded aria-label="Account" class="hidden md:flex" @click="authDropdownOpen = !authDropdownOpen" />
            <Teleport to="body">
              <div v-if="authDropdownOpen" class="fixed inset-0 z-40" @click="authDropdownOpen = false" />
            </Teleport>
            <div v-if="authDropdownOpen" class="absolute right-0 top-full mt-1 z-50 w-48 bg-white border border-neutral-200 rounded-lg shadow-lg py-1">
              <router-link to="/account/orders" class="block px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-50" @click="authDropdownOpen = false">Orders</router-link>
              <router-link to="/account/profile" class="block px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-50" @click="authDropdownOpen = false">Profile</router-link>
              <hr class="my-1 border-neutral-100" />
              <button class="w-full text-left px-4 py-2 text-sm text-neutral-700 hover:bg-neutral-50" @click="handleSignOut">Sign Out</button>
            </div>
          </template>
          <!-- Auth: Logged out -->
          <template v-else>
            <Button label="Sign In" text size="small" as="router-link" to="/login" class="hidden md:inline-flex" />
          </template>
          <Button icon="pi pi-bars" text rounded class="md:hidden" @click="mobileMenuOpen = !mobileMenuOpen" />
        </div>
      </div>
    </div>
    <MobileNav v-model:open="mobileMenuOpen" />
    <CartDrawer v-model:visible="cartDrawerOpen" />
  </header>
</template>
