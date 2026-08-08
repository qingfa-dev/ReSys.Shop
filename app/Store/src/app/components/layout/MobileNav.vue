<script setup lang="ts">
import { useAuthStore } from '@/features/identity/stores/authStore'

// Model: Two-way binding for open/close state.
const open = defineModel<boolean>('open', { required: true })
const auth = useAuthStore()

// Close: Set open to false.
const close = () => {
  open.value = false
}
</script>
<template>
  <Teleport to="body">
    <Transition name="mobile-nav">
      <div v-if="open" class="fixed inset-0 z-50 md:hidden">
        <div class="absolute inset-0 bg-black/50" @click="close" />
        <Transition name="mobile-panel" appear>
          <div v-if="open" class="absolute right-0 top-0 h-full w-72 bg-white shadow-xl p-6">
            <div class="flex justify-between items-center mb-8">
              <span class="text-lg font-semibold">Menu</span>
              <Button icon="pi pi-times" text rounded @click="close" />
            </div>
            <nav class="space-y-4">
              <router-link to="/shop" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="close">Shop</router-link>
              <router-link to="/collections" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="close">Collections</router-link>
              <router-link to="/recommendations" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="close">Visual Search</router-link>
              <router-link to="/cart" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="close">Cart</router-link>
              <template v-if="auth.isAuthenticated">
                <router-link to="/account/orders" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="close">My Orders</router-link>
                <router-link to="/account/profile" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="close">Profile</router-link>
              </template>
              <template v-else>
                <router-link to="/login" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="close">Sign In</router-link>
                <router-link to="/register" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="close">Register</router-link>
              </template>
            </nav>
          </div>
        </Transition>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped>
.mobile-nav-enter-active,
.mobile-nav-leave-active {
  transition: opacity 200ms ease;
}
.mobile-nav-enter-from,
.mobile-nav-leave-to {
  opacity: 0;
}

.mobile-panel-enter-active {
  transition: transform 250ms ease;
}
.mobile-panel-leave-active {
  transition: transform 200ms ease;
}
.mobile-panel-enter-from,
.mobile-panel-leave-to {
  transform: translateX(100%);
}

@media (prefers-reduced-motion: reduce) {
  .mobile-nav-enter-active,
  .mobile-nav-leave-active,
  .mobile-panel-enter-active,
  .mobile-panel-leave-active {
    transition-duration: 0ms;
  }
}
</style>
