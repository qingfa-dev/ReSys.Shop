<script setup lang="ts">
import Times from '@primeicons/vue/times'
import Check from '@primeicons/vue/check'

interface Props {
  visible: boolean
  message: string
  header?: string
  icon?: string
  confirmLabel?: string
  cancelLabel?: string
}

withDefaults(defineProps<Props>(), {
  header: 'Confirm',
  icon: 'pi pi-exclamation-triangle',
  confirmLabel: 'Yes',
  cancelLabel: 'No',
})

const emit = defineEmits<{
  (e: 'update:visible', value: boolean): void
  (e: 'confirm'): void
  (e: 'cancel'): void
}>()
</script>

<template>
  <Dialog
    :visible="visible"
    :header="header"
    :modal="true"
    :style="{ width: '450px' }"
    @update:visible="emit('update:visible', $event)"
  >
    <div class="flex items-center gap-4">
      <i :class="icon" class="text-8xl" style="color: var(--p-amber-500)" />
      <span class="text-surface-600 dark:text-surface-0 text-lg">{{ message }}</span>
    </div>
    <template #footer>
      <Button :label="cancelLabel" variant="text" @click="emit('cancel')">
        <Times />
      </Button>
      <Button :label="confirmLabel" @click="emit('confirm')">
        <Check />
      </Button>
    </template>
  </Dialog>
</template>
