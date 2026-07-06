<template>
  <Drawer
    :visible="visible"
    :header="title"
    position="right"
    @update:visible="$emit('update:visible', $event)"
  >
    <slot />
  </Drawer>
</template>

<script setup lang="ts">
defineProps<{ visible: boolean; title: string }>()
defineEmits<{ 'update:visible': [v: boolean] }>()
</script>
