<script setup lang="ts">
</script>
<template>
  <footer class="bg-white border-t border-neutral-200 mt-auto">
    <div class="max-w-[1440px] mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div class="grid grid-cols-2 md:grid-cols-4 gap-8">
        <div>
          <h3 class="text-sm font-semibold text-neutral-900 mb-4">ReSys.Shop</h3>
          <p class="text-sm text-neutral-500">AI-powered fashion e-commerce platform.</p>
        </div>
        <div>
          <h3 class="text-sm font-semibold text-neutral-900 mb-4">Shop</h3>
          <ul class="space-y-2">
            <li><router-link to="/shop" class="text-sm text-neutral-500 hover:text-neutral-900">All Products</router-link></li>
            <li><router-link to="/collections" class="text-sm text-neutral-500 hover:text-neutral-900">Collections</router-link></li>
            <li><router-link to="/recommendations" class="text-sm text-neutral-500 hover:text-neutral-900">Visual Search</router-link></li>
          </ul>
        </div>
        <div>
          <h3 class="text-sm font-semibold text-neutral-900 mb-4">Help</h3>
          <ul class="space-y-2">
            <li><a href="#" class="text-sm text-neutral-500 hover:text-neutral-900">Help Center</a></li>
            <li><a href="#" class="text-sm text-neutral-500 hover:text-neutral-900">Shipping Info</a></li>
            <li><a href="#" class="text-sm text-neutral-500 hover:text-neutral-900">Returns</a></li>
            <li><a href="#" class="text-sm text-neutral-500 hover:text-neutral-900">Size Guide</a></li>
          </ul>
        </div>
        <div>
          <h3 class="text-sm font-semibold text-neutral-900 mb-4">Company</h3>
          <ul class="space-y-2">
            <li><router-link to="/about" class="text-sm text-neutral-500 hover:text-neutral-900">About</router-link></li>
            <li><router-link to="/terms" class="text-sm text-neutral-500 hover:text-neutral-900">Terms</router-link></li>
            <li><router-link to="/privacy" class="text-sm text-neutral-500 hover:text-neutral-900">Privacy</router-link></li>
          </ul>
        </div>
      </div>
      <div class="mt-8 pt-8 border-t border-neutral-200">
        <p class="text-sm text-neutral-500">&copy; {{ new Date().getFullYear() }} ReSys.Shop. All rights reserved.</p>
      </div>
    </div>
  </footer>
</template>
