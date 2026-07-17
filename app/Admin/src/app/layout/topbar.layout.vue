<script setup lang="ts">
defineEmits<{ 'toggle-sidebar': []; 'toggle-dark-mode': [] }>()
</script>
<template>
  <div class="layout-topbar">
    <button @click="$emit('toggle-sidebar')" class="topbar-menu-button">
      <i class="pi pi-bars" />
    </button>
    <div class="topbar-left">
      <span class="topbar-title">ReSys.Shop</span>
    </div>
    <div class="topbar-right">
      <button @click="$emit('toggle-dark-mode')" class="topbar-action-button">
        <i class="pi pi-moon" />
      </button>
    </div>
  </div>
</template>
