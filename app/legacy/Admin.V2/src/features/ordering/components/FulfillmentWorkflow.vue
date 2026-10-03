<script setup lang="ts">
import { ref, computed } from 'vue'
import { useI18n } from 'vue-i18n'
import { useConfirm } from '@/shared/composables/useConfirm'
import { useToast } from '@/shared/composables/useToast'
import { OrderApi } from '../api'

const props = defineProps<{
  orderId: string
  status: string
}>()

const emit = defineEmits<{
  'status-changed': []
}>()

const { t } = useI18n()
const { confirmDelete } = useConfirm()
const toastService = useToast()
const transitioning = ref(false)

const STATUSES = [
  { label: t('ordering.workflow.statuses.pending'), value: 'pending' },
  { label: t('ordering.workflow.statuses.approved'), value: 'approved' },
  { label: t('ordering.workflow.statuses.processing'), value: 'processing' },
  { label: t('ordering.workflow.statuses.completed'), value: 'completed' },
  { label: t('ordering.workflow.statuses.shipped'), value: 'shipped' },
  { label: t('ordering.workflow.statuses.delivered'), value: 'delivered' },
]

const terminalStatuses: string[] = ['completed', 'cancelled', 'returned']

const activeStep = computed(() => {
  const idx = STATUSES.findIndex((s) => s.value === props.status)
  return idx >= 0 ? idx : 0
})

const canApprove = computed(() => props.status === 'pending')
const canComplete = computed(() => props.status === 'approved' || props.status === 'processing')
const canCancel = computed(() => !terminalStatuses.includes(props.status))

async function transition(action: 'approve' | 'complete' | 'cancel' | 'resume') {
  if (action === 'cancel') {
    confirmDelete({
      target: t('ordering.workflow.confirm.cancel'),
      onAccept: () => executeTransition(action),
    })
    return
  }
  await executeTransition(action)
}

async function executeTransition(action: 'approve' | 'complete' | 'cancel' | 'resume') {
  transitioning.value = true
  try {
    const result = await OrderApi[action](props.orderId)
    if (result.isSuccess) {
      toastService.success(t(`ordering.orders.messages.${action}_success`))
      emit('status-changed')
    } else {
      console.error(result.message)
      toastService.error(result.message ?? t('ordering.orders.messages.save_failed'))
    }
  } catch (err) {
    console.error(err)
    toastService.error(t('ordering.orders.messages.save_failed'))
  }
  transitioning.value = false
}
</script>

<template>
  <div class="flex flex-col gap-4">
    <Steps :model="STATUSES" :active-step="activeStep" />

    <div v-if="!terminalStatuses.includes(props.status)" class="flex items-center gap-2">
      <Button
        v-if="canApprove"
        :label="t('ordering.workflow.confirm_order')"
        icon="pi pi-check"
        severity="success"
        :loading="transitioning"
        @click="transition('approve')"
      />
      <Button
        v-if="canComplete"
        :label="t('ordering.workflow.mark_complete')"
        icon="pi pi-check-circle"
        severity="success"
        :loading="transitioning"
        @click="transition('complete')"
      />
      <Button
        v-if="canCancel"
        :label="t('ordering.workflow.cancel_order')"
        severity="danger"
        icon="pi pi-times"
        :loading="transitioning"
        outlined
        @click="transition('cancel')"
      />
    </div>
    <div v-else class="text-sm text-surface-500">
      {{ t('ordering.workflow.terminal_note', { status: props.status }) }}
    </div>
  </div>
</template>
