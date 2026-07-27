<script setup lang="ts">
interface Props {
  title: string
}

defineProps<Props>()
</script>

<template>
  <div class="card">
    <div class="font-semibold text-xl mb-4">{{ title }}</div>
    <slot />
  </div>
</template>
