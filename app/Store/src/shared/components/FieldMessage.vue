<script setup lang="ts">
// Props: Optional error text; renders nothing when absent.
defineProps<{ error?: string | null }>()
</script>

<template>
  <!-- Section: Field Error — inline validation or API field message -->
  <Message v-if="error" severity="error" size="small" variant="simple">{{ error }}</Message>
</template>
