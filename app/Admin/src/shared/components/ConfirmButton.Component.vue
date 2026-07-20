<script setup lang="ts">
import { useConfirm } from 'primevue/useconfirm'

const confirm = useConfirm()

const props = withDefaults(defineProps<{
  icon?: string
  severity?: string
  rounded?: boolean
  text?: boolean
  header: string
  message: string
  acceptLabel?: string
  rejectLabel?: string
  loading?: boolean
}>(), {
  icon: 'pi pi-trash',
  severity: 'danger',
  rounded: true,
  text: true,
})

const emit = defineEmits<{
  confirm: []
}>()

function onClick() {
  confirm.require({
    message: props.message,
    header: props.header,
    icon: 'pi pi-exclamation-triangle',
    rejectLabel: props.rejectLabel,
    acceptProps: {
      label: props.acceptLabel,
      severity: props.severity as any,
    },
    accept: () => emit('confirm'),
  })
}
</script>

<template>
  <Button
    :icon="icon"
    :severity="severity"
    :rounded="rounded"
    :text="text"
    :loading="loading"
    @click="onClick"
  />
</template>
