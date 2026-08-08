<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import AppHeader from '../components/layout/AppHeader.vue'

// Loader: Show the skeleton bar while the router resolves lazy route chunks.
const router = useRouter()
const loading = ref(false)

router.beforeEach((_to, _from, next) => {
  loading.value = true
  next()
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

    <!-- Header: Sticky shell bar; MobileNav (Task 11) and CartDrawer (Task 31) bind its events -->
    <AppHeader />

    <main class="flex-1">
      <RouterView />
    </main>

    <!-- AppFooter (Task 12) -->

    <ScrollTop :threshold="400" icon="pi pi-arrow-up" />
  </div>
</template>
