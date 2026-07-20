<script setup lang="ts">
import { ref, watch } from 'vue'
import Drawer from 'primevue/drawer'

const props = withDefaults(defineProps<{
  visible: boolean
  header?: string
  position?: 'left' | 'right' | 'top' | 'bottom'
  width?: string
}>(), {
  position: 'right',
  width: '30rem',
})

const emit = defineEmits<{
  'update:visible': [value: boolean]
}>()

const localVisible = ref(props.visible)
watch(() => props.visible, (v) => { localVisible.value = v })
watch(localVisible, (v) => { emit('update:visible', v) })
</script>

<template>
  <Drawer v-model:visible="localVisible" :header="header" :position="position" :style="{ width }">
    <slot />
  </Drawer>
</template>
