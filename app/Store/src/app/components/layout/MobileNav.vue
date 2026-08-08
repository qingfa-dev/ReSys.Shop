<script setup lang="ts">
import { useAuthStore } from '@/features/identity/stores/authStore'

const emit = defineEmits<{ close: [] }>()
const auth = useAuthStore()
</script>
<template>
  <div class="fixed inset-0 z-50 md:hidden">
    <div class="absolute inset-0 bg-black/50" @click="emit('close')" />
    <div class="absolute right-0 top-0 h-full w-72 bg-white shadow-xl p-6">
      <div class="flex justify-between items-center mb-8">
        <span class="text-lg font-semibold">Menu</span>
        <Button icon="pi pi-times" text rounded @click="emit('close')" />
      </div>
      <nav class="space-y-4">
        <router-link to="/shop" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="emit('close')">Shop</router-link>
        <router-link to="/collections" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="emit('close')">Collections</router-link>
        <router-link to="/recommendations" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="emit('close')">Visual Search</router-link>
        <router-link to="/cart" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="emit('close')">Cart</router-link>
        <template v-if="auth.isAuthenticated">
          <router-link to="/account/orders" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="emit('close')">My Orders</router-link>
          <router-link to="/account/profile" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="emit('close')">Profile</router-link>
        </template>
        <template v-else>
          <router-link to="/login" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="emit('close')">Sign In</router-link>
          <router-link to="/register" class="block text-sm font-medium text-neutral-700 hover:text-neutral-900" @click="emit('close')">Register</router-link>
        </template>
      </nav>
    </div>
  </div>
</template>
