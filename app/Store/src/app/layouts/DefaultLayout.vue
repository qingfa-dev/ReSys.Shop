<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import AppHeader from '../components/layout/AppHeader.vue'
import MobileNav from '../components/layout/MobileNav.vue'
import AppFooter from '../components/layout/AppFooter.vue'
import CartDrawer from '@/features/ordering/components/CartDrawer.vue'

// Loader: Show the skeleton bar while the router resolves lazy route chunks.
const router = useRouter()
const loading = ref(false)

// Nav: Mobile drawer state driven by the AppHeader hamburger (Task 11).
const mobileNavOpen = ref(false)

// Cart: Drawer visibility driven by the AppHeader cart button (Task 31).
const cartOpen = ref(false)

router.beforeEach(() => {
  loading.value = true
})

router.afterEach(() => {
  loading.value = false
})
</script>

<template>
  <!-- Section: Root shell — header, scrollable main, footer -->
  <div class="flex min-h-screen flex-col">
    <!-- Route Loader: Full-width skeleton bar while lazy route chunks load -->
    <Skeleton v-if="loading" class="w-full rounded-none" height="0.5rem" />

    <!-- Header: Sticky shell bar; CartDrawer (Task 31) binds its open-cart event -->
    <AppHeader @open-mobile-nav="mobileNavOpen = true" @open-cart="cartOpen = true" />

    <!-- Mobile Nav: Drawer below lg; closes itself on route change -->
    <MobileNav :visible="mobileNavOpen" @update:visible="mobileNavOpen = $event" />

    <!-- Cart Drawer: Slide-in cart panel opened from the header cart button -->
    <CartDrawer v-model:visible="cartOpen" />

    <main class="flex-1">
      <RouterView />
    </main>

    <!-- Footer: Brand, links and newsletter across four columns -->
    <AppFooter />

    <ScrollTop :threshold="400" icon="pi pi-arrow-up" />
  </div>
</template>
