<script setup lang="ts">
defineEmits<{ 'toggle-dark-mode': [] }>()
</script>
<template>
  <div class="layout-configurator">
    <button @click="$emit('toggle-dark-mode')" class="configurator-button">
      <i class="pi pi-palette" />
    </button>
  </div>
</template>
