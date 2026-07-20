<script setup lang="ts">
withDefaults(defineProps<{
  entityName: string
  warningText?: string
  loading?: boolean
  visible: boolean
}>(), {
  warningText: 'This action cannot be undone.',
  loading: false,
})

const emit = defineEmits<{
  confirm: []
  cancel: []
  'update:visible': [value: boolean]
}>()

function onCancel() {
  emit('cancel')
  emit('update:visible', false)
}
</script>

<template>
  <Dialog
    :visible="visible"
    :modal="true"
    :closable="!loading"
    header="Delete Confirmation"
    :style="{ width: '450px' }"
    @update:visible="emit('update:visible', $event)"
    @hide="onCancel"
  >
    <div class="flex flex-col gap-4">
      <div class="flex items-center gap-3">
        <i class="pi pi-exclamation-triangle text-2xl" style="color: var(--p-yellow-500)" />
        <p class="m-0 text-sm" style="color: var(--p-text-color)">
          Are you sure you want to delete <strong>{{ entityName }}</strong>?
        </p>
      </div>
      <p class="m-0 text-sm" style="color: var(--p-text-muted-color)">{{ warningText }}</p>
    </div>
    <template #footer>
      <div class="flex gap-2 justify-end">
        <Button
          label="Cancel"
          severity="secondary"
          text
          :disabled="loading"
          @click="onCancel"
        />
        <Button
          :label="loading ? 'Deleting...' : 'Delete'"
          severity="danger"
          :loading="loading"
          :disabled="loading"
          @click="$emit('confirm')"
        />
      </div>
    </template>
  </Dialog>
</template>
