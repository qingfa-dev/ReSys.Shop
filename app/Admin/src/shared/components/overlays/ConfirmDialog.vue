<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'

const confirm = useConfirm()

const props = withDefaults(defineProps<{
  icon?: string
  severity?: string
  header: string
  message: string
  acceptLabel?: string
  rejectLabel?: string
  loading?: boolean
}>(), {
  icon: 'pi pi-trash',
  severity: 'danger',
  acceptLabel: 'Confirm',
  rejectLabel: 'Cancel',
  loading: false,
})

const emit = defineEmits<{
  confirm: []
  cancel: []
}>()

function open() {
  confirm.require({
    message: props.message,
    header: props.header,
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: props.rejectLabel,
    acceptProps: {
      label: props.acceptLabel,
      severity: props.severity,
    },
    rejectProps: {
      label: props.rejectLabel,
      severity: 'secondary',
      outlined: true,
    },
    accept: () => emit('confirm'),
    reject: () => emit('cancel'),
  })
}
</script>

<template>
  <ConfirmDialogPrime />
  <Button :icon="icon" :severity="severity" :loading="loading" rounded text @click="open">
    <slot />
  </Button>
</template>
