<template>
  <Button
    icon="pi pi-ellipsis-v"
    text
    rounded
    severity="secondary"
    aria-haspopup="true"
    @click="menu?.toggle($event)"
  />
  <Menu ref="menu" :model="items" popup />
</template>

<script setup lang="ts">
import { ref } from 'vue';
import Button from 'primevue/button';
import Menu from 'primevue/menu';
import type { MenuItem } from 'primevue/menuitem';

defineProps<{ items: MenuItem[] }>();
const menu = ref();
</script>
