<script setup lang="ts">
defineProps<{ icon?: string; message: string; actionLabel?: string; actionTo?: string }>()
</script>
<template>
  <div class="text-center py-16">
    <i v-if="icon" :class="icon" class="text-4xl text-stone-300 mb-4 block" />
    <p class="text-stone-500">{{ message }}</p>
    <router-link v-if="actionLabel && actionTo" :to="actionTo" class="mt-4 inline-block">
      <Button :label="actionLabel" severity="secondary" />
    </router-link>
  </div>
</template>
