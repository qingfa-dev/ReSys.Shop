<script setup lang="ts">
import Drawer from 'primevue/drawer'

const visible = defineModel<boolean>({ required: true })

withDefaults(defineProps<{
  header?: string
  position?: 'left' | 'right' | 'top' | 'bottom'
  width?: string
}>(), {
  position: 'right',
  width: '30rem',
})
</script>

<template>
  <Drawer v-model="visible" :header="header" :position="position" :style="{ width }">
    <slot />
  </Drawer>
</template>
