<template>
  <Dialog
    :visible="visible"
    :header="title"
    :modal="true"
    :style="{ width: width }"
    @update:visible="$emit('update:visible', $event)"
  >
    <slot />
    <template #footer>
      <slot name="footer" />
    </template>
  </Dialog>
</template>

<script setup lang="ts">
defineProps<{
  visible: boolean
  title: string
  width?: string
}>()

defineEmits<{ 'update:visible': [v: boolean] }>()
</script>
