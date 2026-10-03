<template>
  <Dialog
    :visible="visible"
    :header="title"
    :modal="true"
    :style="{ width: width }"
    :draggable="false"
    class="mx-4"
    @update:visible="emit('update:visible', $event)"
  >
    <div class="py-1">
      <slot />
    </div>
    <template v-if="$slots.footer" #footer>
      <slot name="footer" />
    </template>
  </Dialog>
</template>

<script setup lang="ts">
import Dialog from 'primevue/dialog'

defineOptions({ name: 'AppModal' })

withDefaults(defineProps<{ visible: boolean; title?: string; width?: string }>(), { width: '32rem' })

const emit = defineEmits<{ 'update:visible': [boolean] }>()
</script>
