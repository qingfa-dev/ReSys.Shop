<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'

const confirm = useConfirm()

const props = withDefaults(defineProps<{
  message: string
  header?: string
  icon?: string
  severity?: 'danger' | 'warn' | 'info'
  acceptLabel?: string
  rejectLabel?: string
  disabled?: boolean
  loading?: boolean
}>(), {
  icon: 'pi pi-trash',
  severity: 'danger',
})

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()

function onClick() {
  confirm.require({
    message: props.message,
    header: props.header ?? 'Confirm',
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: props.rejectLabel ?? 'Cancel',
    acceptProps: {
      label: props.acceptLabel ?? (props.severity === 'danger' ? 'Delete' : 'Confirm'),
      severity: props.severity as any,
    },
    accept: () => emit('confirm'),
    reject: () => emit('cancel'),
  })
}
</script>

<template>
  <Button
    :icon="icon"
    :severity="severity"
    rounded
    text
    :disabled="disabled"
    :loading="loading"
    @click="onClick"
  />
</template>
