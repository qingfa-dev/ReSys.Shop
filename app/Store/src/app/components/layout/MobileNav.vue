<script setup lang="ts">
defineEmits<{ close: [] }>()
</script>
<template>
  <!-- Section: Mobile Slide-out Nav -->
  <div class="fixed inset-0 z-50 md:hidden">
    <div class="absolute inset-0 bg-black/50" @click="$emit('close')" />
    <nav class="absolute top-0 right-0 bottom-0 w-64 bg-white shadow-lg p-6">
      <div class="flex justify-between items-center mb-6">
        <span class="font-semibold">Menu</span>
        <Button icon="pi pi-times" text severity="secondary" @click="$emit('close')" />
      </div>
      <ul class="space-y-3">
        <li><router-link to="/shop" class="text-gray-700 hover:text-gray-900" @click="$emit('close')">Shop</router-link></li>
        <li><router-link to="/cart" class="text-gray-700 hover:text-gray-900" @click="$emit('close')">Cart</router-link></li>
        <li><router-link to="/account/orders" class="text-gray-700 hover:text-gray-900" @click="$emit('close')">Orders</router-link></li>
      </ul>
    </nav>
  </div>
</template>
