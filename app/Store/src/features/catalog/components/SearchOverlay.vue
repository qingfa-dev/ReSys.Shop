<script setup lang="ts">
</script>
<template>
  <Teleport to="body">
    <div class="fixed inset-0 z-50 flex items-start justify-center pt-[10vh]">
      <div class="absolute inset-0 bg-black/50" />
      <div class="relative bg-white rounded-xl shadow-2xl w-full max-w-2xl mx-4 overflow-hidden">
        <div class="p-4">
          <input
            type="text"
            placeholder="Search products..."
            class="w-full text-lg px-4 py-3 border-0 outline-none"
            autofocus
          />
        </div>
        <div class="border-t border-neutral-100 p-4 text-sm text-neutral-500">
          Type to search products, collections, and more...
        </div>
      </div>
    </div>
  </Teleport>
</template>
