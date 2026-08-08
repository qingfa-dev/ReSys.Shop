<script setup lang="ts">
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
            <router-link to="/account/orders" class="block px-3 py-2 text-sm font-medium rounded-lg text-neutral-700 hover:bg-neutral-100">Orders</router-link>
            <router-link to="/account/addresses" class="block px-3 py-2 text-sm font-medium rounded-lg text-neutral-700 hover:bg-neutral-100">Addresses</router-link>
            <router-link to="/account/profile" class="block px-3 py-2 text-sm font-medium rounded-lg text-neutral-700 hover:bg-neutral-100">Profile</router-link>
            <router-link to="/account/sessions" class="block px-3 py-2 text-sm font-medium rounded-lg text-neutral-700 hover:bg-neutral-100">Sessions</router-link>
            <router-link to="/account/wishlists" class="block px-3 py-2 text-sm font-medium rounded-lg text-neutral-700 hover:bg-neutral-100">Wishlists</router-link>
            <router-link to="/account/notifications" class="block px-3 py-2 text-sm font-medium rounded-lg text-neutral-700 hover:bg-neutral-100">Notifications</router-link>
            <router-link to="/account/change-password" class="block px-3 py-2 text-sm font-medium rounded-lg text-neutral-700 hover:bg-neutral-100">Change Password</router-link>
            <router-link to="/account/preferences" class="block px-3 py-2 text-sm font-medium rounded-lg text-neutral-700 hover:bg-neutral-100">Preferences</router-link>
          </nav>
        </aside>
        <div class="flex-1 min-w-0">
          <router-view />
        </div>
      </div>
    </div>
  </div>
</template>
