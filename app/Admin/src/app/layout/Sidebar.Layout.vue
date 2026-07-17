<script setup lang="ts">
import AppMenu from './Menu.Layout.vue'
defineProps<{ active: boolean }>()
defineEmits<{ close: [] }>()
</script>
<template>
  <div class="layout-sidebar" :class="{ 'layout-sidebar-active': active }">
    <div class="sidebar-header">
      <span class="sidebar-brand">ReSys.Shop</span>
      <button @click="$emit('close')" class="sidebar-close-button">
        <i class="pi pi-times" />
      </button>
    </div>
    <AppMenu />
  </div>
</template>
