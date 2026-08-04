<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import MobileNav from '@/app/components/layout/MobileNav.vue'
import ThemeToggle from '@/app/components/ThemeToggle.vue'
import { useAuthStore } from '@/features/identity/stores/authStore'
import { useCartStore } from '@/features/ordering/stores/cartStore'

const router = useRouter()
const auth = useAuthStore()
const cart = useCartStore()

const searchQuery = ref('')
const mobileMenuOpen = ref(false)

// Trigger: Execute keyword search
function onSearch(): void {
  if (searchQuery.value.trim()) {
    router.push({ path: '/shop', query: { search: searchQuery.value } })
  }
}
</script>
<template>
  <!-- Section: Header Bar -->
  <header class="bg-white border-b border-gray-200 sticky top-0 z-50">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex items-center justify-between h-16 gap-4">
        <!-- Section: Logo -->
        <router-link to="/" class="text-xl font-bold text-gray-900 shrink-0">
          ReSys.Shop
        </router-link>

        <!-- Section: Search Bar (desktop) -->
        <form class="hidden md:flex flex-1 max-w-lg" @submit.prevent="onSearch">
          <span class="p-input-icon-left w-full">
            <i class="pi pi-search" />
            <InputText
              v-model="searchQuery"
              placeholder="Search products..."
              class="w-full"
            />
          </span>
        </form>

        <!-- Section: Header Actions -->
        <div class="flex items-center gap-3">
          <!-- Cart Icon -->
          <router-link
            to="/cart"
            class="relative p-2 text-gray-600 hover:text-gray-900 transition-colors"
          >
            <i class="pi pi-shopping-cart text-xl" />
          <span
            v-if="cart.itemCount > 0"
            class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center"
          >
            {{ cart.itemCount }}
          </span>
        </router-link>

        <!-- Section: Theme Toggle -->
        <ThemeToggle />

        <!-- Section: User Menu / Sign In -->
        <template v-if="auth.isAuthenticated">
          <router-link
            to="/account/orders"
            class="hidden md:flex items-center gap-2 text-sm text-gray-600 hover:text-gray-900"
          >
            <i class="pi pi-user" />
            {{ auth.user?.userName ?? 'Account' }}
          </router-link>
          <Button
            label="Logout"
            size="small"
            severity="secondary"
            @click="auth.logout()"
            class="hidden md:inline-flex"
          />
        </template>
        <template v-else>
          <router-link to="/login">
            <Button label="Sign In" size="small" severity="secondary" />
          </router-link>
        </template>

          <!-- Mobile Menu Toggle -->
          <Button
            icon="pi pi-bars"
            severity="secondary"
            text
            class="md:hidden"
            @click="mobileMenuOpen = !mobileMenuOpen"
          />
        </div>
      </div>
    </div>

    <!-- Section: Mobile Navigation -->
    <MobileNav v-if="mobileMenuOpen" @close="mobileMenuOpen = false" />
  </header>
</template>
