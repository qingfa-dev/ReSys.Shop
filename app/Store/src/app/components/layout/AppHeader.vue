<script setup lang="ts">
import { ref } from 'vue'
import { useRoute } from 'vue-router'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useCartStore } from '@/features/ordering/stores/cartStore'
import { useSearch } from '@/features/catalog/composables/useSearch'
import MobileNav from './MobileNav.vue'
import CartDrawer from '@/features/ordering/components/CartDrawer.vue'
import ThemeToggle from '@/app/components/ThemeToggle.vue'

const route = useRoute()
const auth = useAuthStore()
const cart = useCartStore()
const search = useSearch()
const mobileMenuOpen = ref(false)
const cartDrawerOpen = ref(false)

// NavLink: Active state detection by path prefix.
const isActive = (path: string) => route.path.startsWith(path)
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
        <div class="flex items-center gap-1">
          <Button icon="pi pi-search" text rounded aria-label="Search" @click="search.open()" />
          <div class="relative inline-flex">
            <Button icon="pi pi-shopping-cart" text rounded @click="cartDrawerOpen = true" />
            <Tag v-if="cart.itemCount > 0" :value="String(cart.itemCount)" severity="contrast" class="absolute -top-0.5 -right-0.5 min-w-[18px] h-[18px] text-[10px] p-0" />
          </div>
          <ThemeToggle />
          <template v-if="auth.isAuthenticated">
            <Button icon="pi pi-user" text rounded as="router-link" to="/account/orders" aria-label="Account" class="hidden md:flex" />
          </template>
          <template v-else>
            <Button label="Sign In" text size="small" as="router-link" to="/login" class="hidden md:inline-flex" />
          </template>
          <Button icon="pi pi-bars" text rounded class="md:hidden" @click="mobileMenuOpen = !mobileMenuOpen" />
        </div>
      </div>
    </div>
    <MobileNav v-if="mobileMenuOpen" @close="mobileMenuOpen = false" />
    <CartDrawer v-model:visible="cartDrawerOpen" />
  </header>
</template>
