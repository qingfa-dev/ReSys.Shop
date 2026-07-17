<script setup lang="ts">
defineProps<{
  size?: number
}>()
</script>

<template>
  <svg
    :width="size ?? 72"
    :height="size ?? 72"
    viewBox="0 0 72 72"
    fill="none"
    xmlns="http://www.w3.org/2000/svg"
    class="text-copper-500"
  >
    <rect x="4" y="4" width="64" height="64" rx="16" stroke="currentColor" stroke-width="3" />
    <path
      d="M22 22h18c6.5 0 12 4 12 11s-5.5 11-12 11H22V22zM22 44l18 24"
      stroke="currentColor"
      stroke-width="3"
      stroke-linecap="round"
      stroke-linejoin="round"
    />
  </svg>
</template>
